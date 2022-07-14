// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationService.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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
    using CDP4Common.Helpers;
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
        public Dictionary<DomainOfExpertise, List<ParameterSubscriptionViewModel>> ValidatedUpdates { get; set; } = new Dictionary<DomainOfExpertise, List<ParameterSubscriptionViewModel>>();

        /// <summary>
        /// Get all <see cref="ParameterValueSet"/> of the given iteration
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ParameterValueSet"/>s list is created
        /// </param>
        /// <returns>All <see cref="ParameterValueSet"/></returns>
        public List<ParameterValueSet> GetParameterValueSets(Iteration? iteration)
        {
            var result = new List<ParameterValueSet>();
            iteration?.Element.ForEach(e => e.Parameter.ForEach(p => result.AddRange(p.ValueSet)));
            return result;
        }

        /// <summary>
        /// Get all <see cref="NestedElement"/> of the given iteration for all options
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="NestedElement"/>s list is created
        /// </param>
        /// <returns>All <see cref="NestedElement"/></returns>
        public List<NestedElement> GetNestedElements(Iteration? iteration)
        {
            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = new List<NestedElement>();
            if(iteration?.TopElement != null)
            {
                iteration.Option.ToList().ForEach(option => nestedElements.AddRange(nestedElementTreeGenerator.Generate(option)));
            }
            return nestedElements;
        }

        /// <summary>
        /// Get the nested parameters from the given option
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="NestedParameter"/>s list is created
        /// </param>
        /// <param name="optionIid">
        /// The Iid of the option for which the <see cref="NestedParameter"/>s list is created
        /// </param>
        /// <returns>All<see cref="NestedParameter"/> of the given option</returns>
        public List<NestedParameter> GetNestedParameters(Iteration? iteration, Guid? optionIid)
        {
            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedParameters = new List<NestedParameter>();
            var option = iteration?.Option.ToList().Find(o => o.Iid == optionIid);
            if (option != null && iteration?.TopElement != null)
            {
                nestedParameters.AddRange(nestedElementTreeGenerator.GetNestedParameters(option));
            }
            return nestedParameters;
        }

        /// <summary>
        /// Get unused elements defintion of the opened iteration
        /// An unused element is an element not used in an option
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ElementDefinition"/>s list is created
        /// </param>
        /// <returns>All unused <see cref="ElementDefinition"/></returns>
        public List<ElementDefinition> GetUnusedElementDefinitions(Iteration? iteration)
        {
            var nestedElements = this.GetNestedElements(iteration);
            var associatedElements = new List<ElementDefinition>();
            nestedElements.ForEach(element => {
                element.ElementUsage.ToList().ForEach(e => associatedElements.Add(e.ElementDefinition));
             });
            associatedElements = associatedElements.Distinct().ToList();

            var unusedElementDefinitions = new List<ElementDefinition>();
            if (iteration is not null)
            {
                unusedElementDefinitions.AddRange(iteration.Element);
            }
            unusedElementDefinitions.RemoveAll(e => associatedElements.Contains(e));

            return unusedElementDefinitions;
        }

        /// <summary>
        /// Get all the unreferenced element definitions in the opened iteration
        /// An unreferenced element is an element with no associated ElementUsage
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ElementDefinition"/>s list is created
        /// </param>
        /// <returns>All unreferenced <see cref="ElementDefinition"/></returns>
        public List<ElementDefinition> GetUnreferencedElements(Iteration? iteration)
        {
            var elementUsages = new List<ElementUsage>();
            iteration?.Element.ForEach(e => elementUsages.AddRange(e.ContainedElement));

            var associatedElementDefinitions = new List<ElementDefinition>();
            elementUsages.ForEach(e => associatedElementDefinitions.Add(e.ElementDefinition));

            var unreferencedElementDefinitions = new List<ElementDefinition>();
            if(iteration is not null)
            {
                unreferencedElementDefinitions.AddRange(iteration.Element);
            }
            unreferencedElementDefinitions.RemoveAll(e => associatedElementDefinitions.Contains(e));

            return unreferencedElementDefinitions;
        }

        /// <summary>
        /// Get all <see cref="ParameterSubscription"/> by the given domain in the given iteration 
        /// </summary>
        /// <param name="iteration">The opened <see cref="Iteration"/></param>
        /// <param name="currentDomainOfExpertise">The current <see cref="DomainOfExpertise"/> of the iteration</param>
        /// <returns>List of all <see cref="ParameterSubscription"/></returns>
        public List<ParameterSubscription> GetParameterSubscriptions(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise)
        {
            var subscribedParameters = new List<ParameterSubscription>();
            iteration?.Element.ForEach(element =>
            {
                element?.Parameter.ForEach(parameter =>
                            subscribedParameters.AddRange(parameter.ParameterSubscription.FindAll(p => p.Owner.Equals(currentDomainOfExpertise)))
                );
            });
            return subscribedParameters;
        }

        /// <summary>
        /// Gets all <see cref="Parameter"/> owned by the given <see cref="DomainOfExpertise"/> and subscribed by other <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to get <see cref="Parameter"/></param>
        /// <param name="currentDomainOfExpertise">The <see cref="DomainOfExpertise"/></param>
        /// <returns>Subscribed <see cref="Parameter"/> owned by the given <see cref="DomainOfExpertise"/></returns>
        public List<Parameter> GetCurrentDomainSubscribedParameters(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise)
        {
            var subscribedParameters = new List<Parameter>();
            iteration?.Element.FindAll(element => element.Owner == currentDomainOfExpertise).ForEach(element =>
            {
                subscribedParameters.AddRange(element.Parameter.FindAll(parameter => parameter.ParameterSubscription.Count != 0));
            });
            return subscribedParameters;
        }

        /// <summary>
        /// Gets number of updates in the iteration after a session refresh
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to get number of updates</param>
        /// <param name="currentDomainOfExpertise">The <see cref="DomainOfExpertise"/></param>
        /// <returns></returns>
        public int GetNumberUpdates(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise)
        {
            var subscribedParameters = this.GetParameterSubscriptions(iteration, currentDomainOfExpertise);
            var numberUpdates = 0;

            subscribedParameters.ForEach(subscribedparameter =>
            {
                subscribedparameter.ValueSet.ForEach(parameterSubscriptionValueSet => 
                {
                    var parameterSubscriptionValueSetRevisions = parameterSubscriptionValueSet?.Revisions;
                    var isParameterSubscriptionValueUpdated = false;

                    //if change in manual or reference value, revisionNumber changes
                    if (parameterSubscriptionValueSetRevisions?.LongCount() != (long)0
                        && parameterSubscriptionValueSet?.RevisionNumber != parameterSubscriptionValueSetRevisions?.Last().Value.RevisionNumber)
                    {
                        isParameterSubscriptionValueUpdated = true;
                    }

                    var paramererValueSets = this.GetParameterValueSets(iteration);

                    var associatedParameterValueSet = paramererValueSets?.Find(p => p.Iid == parameterSubscriptionValueSet?.SubscribedValueSet.Iid);
                    var associatedElement = iteration?.Element.Find(element => element.Parameter.Find(p => p.ValueSet.Contains(associatedParameterValueSet)) != null);
                    var associatedParameter = associatedElement?.Parameter.Find(p => p.ValueSet.Contains(associatedParameterValueSet));

                    var associatedParameterValueSetRevisions = associatedParameterValueSet?.Revisions;
                    var isAssociatedParameterValueUpdated = false;

                    //if any change, revisionNumber changes
                    if (associatedParameterValueSetRevisions?.LongCount() != (long)0
                        && associatedParameterValueSet?.RevisionNumber != associatedParameterValueSetRevisions?.Last().Value.RevisionNumber)
                    {
                        isAssociatedParameterValueUpdated = true;
                    }

                    //check if changes already validated
                    if (currentDomainOfExpertise != null && this.ValidatedUpdates.TryGetValue(currentDomainOfExpertise, out var list))
                    {
                        var existingValidatedParameter = list.Find(element => element.Iid == parameterSubscriptionValueSet?.Iid);
                        if (existingValidatedParameter != null && parameterSubscriptionValueSet?.RevisionNumber == existingValidatedParameter.RevisionNumber)
                        {
                            isParameterSubscriptionValueUpdated = false;
                        }
                        if (existingValidatedParameter != null && associatedParameterValueSet?.RevisionNumber == existingValidatedParameter.SubscribedRevisionNumber)
                        {
                            isAssociatedParameterValueUpdated = false;
                        }
                    }

                    if (isAssociatedParameterValueUpdated && parameterSubscriptionValueSet?.ValueSwitch == ParameterSwitchKind.COMPUTED)
                    {
                        isParameterSubscriptionValueUpdated = true;
                    }

                    if(isAssociatedParameterValueUpdated || isParameterSubscriptionValueUpdated)
                    {
                        numberUpdates += 1;
                    }
                });
            });

            return numberUpdates;
        }

        /// <summary>
        /// Gets list of parameter types used in the given iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> for which the <see cref="ParameterType"/>s list is created</param>
        /// <returns>All <see cref="ParameterType"/>s used in the iteration</returns>
        public List<ParameterType> GetParameterTypes(Iteration? iteration)
        {
            var parameterTypes = new List<ParameterType>();
            iteration?.Element.ForEach(element =>
            {
                element.Parameter.ForEach(parameter =>
                {
                    parameterTypes.Add(parameter.ParameterType);
                });
            });
            return parameterTypes.Distinct().ToList();
        }

        /// Get all <see cref="ParameterValueSet"/> of the given iteration for one given parameter type
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ParameterValueSet"/>s list is created
        /// </param>
        /// <param name="parameterType">
        /// The name of <see cref="ParameterType"/> for which the <see cref="ParameterValueSet"/>s list is created
        /// </param>
        /// <returns>All <see cref="ParameterValueSet" for the given parameter type/></returns>
        public List<ParameterValueSet> GetParameterValueSetsByParameterType(Iteration? iteration, string? parameterTypeName)
        {
            var result = new List<ParameterValueSet>();
            if (parameterTypeName != null && iteration != null)
            {
                iteration.Element.ForEach(e => e.Parameter.FindAll(p => p.ParameterType.Name.Equals(parameterTypeName)).ForEach(p => result.AddRange(p.ValueSet)));
            }
            return result;
        }

        /// <summary>
        /// Gets all <see cref="ElementUsage"/> in the given iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> for which the <see cref="ElementUsage"/>s list is created</param>
        public IEnumerable<ElementUsage> GetElementUsages(Iteration? iteration)
        {
            var topElement = iteration?.TopElement;
            var allElementsUsages = new List<ElementUsage>();
            var newElementUsages = new List<ElementUsage>();
            if (topElement != null)
            {
                allElementsUsages.AddRange(topElement.ContainedElement);
            }
            var IsNextLevel = topElement?.ContainedElement.Count != 0;

            while (IsNextLevel)
            {
                allElementsUsages.ForEach(e =>
                {
                    newElementUsages.AddRange(e.ElementDefinition.ContainedElement.FindAll(newElementUsage => !allElementsUsages.Contains(newElementUsage)));
                }
            );

                IsNextLevel = newElementUsages.Count != 0;
                allElementsUsages.AddRange(newElementUsages.DistinctBy(e => e.Iid));
                newElementUsages.Clear();
            }
            return allElementsUsages.DistinctBy(e => e.Iid);
        }
    }
}
