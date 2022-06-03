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

    /// <summary>
    /// Service to access iteration data
    /// </summary>
    public class IterationService : IIterationService
    {
        /// <summary>
        /// Get all <see cref="ParameterValueSet"/> of the given iteration
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ParameterValueSet"/>s list is created
        /// </param>
        /// <returns>All <see cref="ParameterValueSet"/></returns>
        public List<ParameterValueSet> GetParameterValueSets(Iteration iteration)
        {
            List<ParameterValueSet> result = new List<ParameterValueSet>();
            iteration.Element.ForEach(e => e.Parameter.ForEach(p => result.AddRange(p.ValueSet)));
            return result;
        }

        /// <summary>
        /// Get all <see cref="NestedElement"/> of the given iteration for all options
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="NestedElement"/>s list is created
        /// </param>
        /// <returns>All <see cref="NestedElement"/></returns>
        public List<NestedElement> GetNestedElements(Iteration iteration)
        {
            NestedElementTreeGenerator nestedElementTreeGenerator = new NestedElementTreeGenerator();
            List<NestedElement> nestedElements = new List<NestedElement>();
            if(iteration.TopElement != null)
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
        public List<NestedParameter> GetNestedParameters(Iteration iteration, Guid? optionIid)
        {
            NestedElementTreeGenerator nestedElementTreeGenerator = new NestedElementTreeGenerator();
            List<NestedParameter> nestedParameters = new List<NestedParameter>();
            var option = iteration.Option.ToList().Find(o => o.Iid == optionIid);
            if (option != null && iteration.TopElement != null)
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
        public List<ElementDefinition> GetUnusedElementDefinitions(Iteration iteration)
        {
            List<NestedElement> nestedElements = this.GetNestedElements(iteration);

            List<ElementDefinition> associatedElements = new List<ElementDefinition>();
            nestedElements.ForEach(element => {
                element.ElementUsage.ToList().ForEach(e => associatedElements.Add(e.ElementDefinition));
             });
            associatedElements = associatedElements.Distinct().ToList();

            List<ElementDefinition> unusedElementDefinitions = new List<ElementDefinition>();
            unusedElementDefinitions.AddRange(iteration.Element);

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
        public List<ElementDefinition> GetUnreferencedElements(Iteration iteration)
        {
            List<ElementUsage> elementUsages = new List<ElementUsage>();
            iteration.Element.ForEach(e => elementUsages.AddRange(e.ContainedElement));

            List<ElementDefinition> associatedElementDefinitions = new List<ElementDefinition>();
            elementUsages.ForEach(e => associatedElementDefinitions.Add(e.ElementDefinition));

            List<ElementDefinition> unreferencedElementDefinitions = new List<ElementDefinition>();
            unreferencedElementDefinitions.AddRange(iteration.Element);

            unreferencedElementDefinitions.RemoveAll(e => associatedElementDefinitions.Contains(e));

            return unreferencedElementDefinitions;
        }

        /// <summary>
        /// Get all subcribed <see cref="Parameter"/> by the given domain in the given iteration 
        /// </summary>
        /// <param name="iteration">The opened <see cref="Iteration"/></param>
        /// <param name="currentDomainOfExpertise">The current <see cref="DomainOfExpertise"/> of the iteration</param>
        /// <returns>List of all subcribed <see cref="Parameter"/></returns>
        public List<Parameter> GetParametersSubscribed(Iteration iteration, DomainOfExpertise? currentDomainOfExpertise)
        {
            List<Parameter> subscribedParameters = new List<Parameter>();

            iteration.Element.ForEach(element =>
            {
                subscribedParameters.AddRange(
                    element.Parameter.FindAll(parameter => parameter.ParameterSubscription.Count != 0 &&
                         parameter.ParameterSubscription.FindAll(p => p.Owner.Equals(currentDomainOfExpertise)).Count != 0)
                );
            });

            return subscribedParameters;
        }
    }
}
