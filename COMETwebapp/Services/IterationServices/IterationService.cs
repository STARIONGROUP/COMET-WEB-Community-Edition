// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.IterationServices
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model;

    /// <summary>
    /// Service to access iteration data
    /// </summary>
    public class IterationService : IIterationService
    {
        /// <summary>
        /// Save updates changes to avoid highlights after validation
        /// Save changes for each domain available in the opened session
        /// </summary>
        public Dictionary<DomainOfExpertise, List<ParameterSubscriptionViewModel>> ValidatedUpdates { get; set; } = new();

        /// <summary>
        /// Save Thing Iid with edit changes in the web application
        /// </summary>
        public List<Guid> NewUpdates { get; set; } = new();

        /// <summary>
        /// Get all <see cref="ParameterSubscription" /> of the given domain and for the given element
        /// </summary>
        /// <param name="element">The <see cref="ElementBase"> to get the subscriptions</param>
        /// <param name="currentDomainOfExpertise">The current <see cref="DomainOfExpertise" /> of the iteration</param>
        /// <returns>List of all <see cref="ParameterSubscription" /> for this element </returns>
        public List<ParameterSubscription> GetParameterSubscriptionsByElement(ElementBase element, DomainOfExpertise? currentDomainOfExpertise)
        {
            var parameterSubscriptions = new List<ParameterSubscription>();

            switch (element)
            {
                case ElementDefinition definition:
                    parameterSubscriptions.AddRange(definition.Parameter.SelectMany(p => p.ParameterSubscription).Where(p => p.Owner == currentDomainOfExpertise));
                    break;
                case ElementUsage elementUsage when elementUsage.ParameterOverride.Count == 0:
                    parameterSubscriptions.AddRange(elementUsage.ElementDefinition.Parameter.SelectMany(p => p.ParameterSubscription).Where(p => p.Owner == currentDomainOfExpertise));
                    break;
                case ElementUsage elementUsage:
                {
                    var associatedParameters = new List<Parameter>();

                    elementUsage.ParameterOverride.ForEach(p =>
                    {
                        p.ParameterSubscription.ForEach(s =>
                        {
                            if (s.Owner == currentDomainOfExpertise)
                            {
                                parameterSubscriptions.Add(s);
                                associatedParameters.Add(p.Parameter);
                            }
                        });
                    });

                    parameterSubscriptions.AddRange(
                        elementUsage.ElementDefinition.Parameter.Where(p => !associatedParameters.Contains(p))
                            .SelectMany(p => p.ParameterSubscription).Where(p => p.Owner == currentDomainOfExpertise));

                    break;
                }
            }

            return parameterSubscriptions.OrderBy(p => p.ParameterType.Name).ToList();
        }

        /// <summary>
        /// Gets number of updates in the iteration after a session refresh
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to get number of updates</param>
        /// <param name="currentDomainOfExpertise">The <see cref="DomainOfExpertise" /></param>
        /// <returns></returns>
        public int GetNumberUpdates(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise)
        {
            var subscribedParameters = new List<ParameterSubscription>();

            if (iteration is not null)
            {
                if (iteration.TopElement != null)
                {
                    subscribedParameters.AddRange(this.GetParameterSubscriptionsByElement(iteration.TopElement, currentDomainOfExpertise));
                }

                iteration.Element.SelectMany(e => e.ContainedElement).ToList().ForEach(e => { subscribedParameters.AddRange(this.GetParameterSubscriptionsByElement(e, currentDomainOfExpertise)); });
            }

            var numberUpdates = 0;

            subscribedParameters.SelectMany(p => p.ValueSet).ToList().ForEach(parameterSubscriptionValueSet =>
            {
                var isUpdated = parameterSubscriptionValueSet.SubscribedValueSet.Revisions.Count != 0
                                 && parameterSubscriptionValueSet.SubscribedValueSet.RevisionNumber != parameterSubscriptionValueSet.SubscribedValueSet.Revisions.Last().Value.RevisionNumber;

                if (currentDomainOfExpertise != null && this.ValidatedUpdates.TryGetValue(currentDomainOfExpertise, out var list))
                {
                    var existingValidatedParameter = list.Find(p => p.Iid == parameterSubscriptionValueSet.Iid);

                    if (existingValidatedParameter != null && parameterSubscriptionValueSet.RevisionNumber == existingValidatedParameter.RevisionNumber && parameterSubscriptionValueSet.ValueSwitch != ParameterSwitchKind.COMPUTED)
                    {
                        isUpdated = false;
                    }

                    if (existingValidatedParameter != null && parameterSubscriptionValueSet.SubscribedValueSet.RevisionNumber == existingValidatedParameter.SubscribedRevisionNumber)
                    {
                        isUpdated = false;
                    }
                }

                if (isUpdated)
                {
                    numberUpdates += 1;
                }
            });

            return numberUpdates;
        }

        /// <summary>
        /// Gets list of parameter types used in the given iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> for which the <see cref="ParameterType" />s list is created</param>
        /// <returns>All <see cref="ParameterType" />s used in the iteration</returns>
        public List<ParameterType> GetParameterTypes(Iteration? iteration)
        {
            var parameterTypes = new List<ParameterType>();
            iteration?.Element.ForEach(element => { element.Parameter.ForEach(parameter => { parameterTypes.Add(parameter.ParameterType); }); });
            return parameterTypes.Distinct().ToList();
        }
    }
}
