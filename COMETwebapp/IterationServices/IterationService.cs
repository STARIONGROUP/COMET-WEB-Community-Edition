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
    using COMETwebapp.SessionManagement;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Service to access the opened iteration data
    /// </summary>
    public class IterationService
    {
        /// <summary>
        /// The actual opened iteration
        /// </summary>
        private readonly Iteration OpenedIteration;

        /// <summary>
        /// Initialize the service withe the opened iteration
        /// </summary>
        /// <param name="iteration">The iteration to read data</param>
        public IterationService(Iteration iteration)
        {
            OpenedIteration = iteration;
        }

        /// <summary>
        /// Get all <see cref="ParameterValueSet"/> of the opened iteration
        /// </summary>
        /// <returns>All <see cref="ParameterValueSet"/></returns>
        public List<ParameterValueSet> GetParameterValueSets()
        {
            List<ParameterValueSet> result = new List<ParameterValueSet>();
            this.OpenedIteration.Element.ForEach(e => e.Parameter.ForEach(p => result.AddRange(p.ValueSet)));
            return result;
        }

        /// <summary>
        /// Get the nested elements of the opened iteration for all options
        /// </summary>
        /// <returns></returns>
        public List<NestedElement> GetNestedElements()
        {
            NestedElementTreeGenerator nestedElementTreeGenerator = new NestedElementTreeGenerator();
            List<NestedElement> nestedElements = new List<NestedElement>();
            if(this.OpenedIteration.TopElement != null)
            {
                this.OpenedIteration.Option.ToList().ForEach(option => nestedElements.AddRange(nestedElementTreeGenerator.Generate(option)));
            }
            return nestedElements;
        }

        /// <summary>
        /// Get the nested parameters from the given option
        /// </summary>
        /// <param name="optionIid">The Iid of the option</param>
        /// <returns>All <see cref="NestedParameter"/> of the given option</returns>
        public List<NestedParameter> GetNestedParameters(Guid? optionIid)
        {
            NestedElementTreeGenerator nestedElementTreeGenerator = new NestedElementTreeGenerator();
            List<NestedParameter> nestedParameters = new List<NestedParameter>();
            var option = this.OpenedIteration.Option.ToList().Find(o => o.Iid == optionIid);
            if (option != null && this.OpenedIteration.TopElement != null)
            {
                nestedParameters.AddRange(nestedElementTreeGenerator.GetNestedParameters(option));
            }
            return nestedParameters;
        }

        /// <summary>
        /// Get unused elements defintion of the opened iteration
        /// An unused element is an element not used in an option
        /// </summary>
        /// <returns>All unused <see cref="ElementDefinition"/></returns>
        public List<ElementDefinition> GetUnusedElementDefinitions()
        {
            List<NestedElement> nestedElements = this.GetNestedElements();

            List<ElementDefinition> associatedElements = new List<ElementDefinition>();
            nestedElements.ForEach(element => {
                element.ElementUsage.ToList().ForEach(e => associatedElements.Add(e.ElementDefinition));
             });
            associatedElements = associatedElements.Distinct().ToList();

            List<ElementDefinition> unusedElementDefinitions = new List<ElementDefinition>();
            unusedElementDefinitions.AddRange(this.OpenedIteration.Element);

            unusedElementDefinitions.RemoveAll(e => associatedElements.Contains(e));

            return unusedElementDefinitions;
        }

        /// <summary>
        /// Get all the unreferenced element definitions in the opened iteration
        /// An unreferenced element is an element with no associated ElementUsage
        /// </summary>
        /// <returns>All unreferenced <see cref="ElementDefinition"/></returns>
        public List<ElementDefinition> GetUnreferencedElements()
        {
            List<ElementUsage> elementUsages = new List<ElementUsage>();
            this.OpenedIteration.Element.ForEach(e => elementUsages.AddRange(e.ContainedElement));

            List<ElementDefinition> associatedElementDefinitions = new List<ElementDefinition>();
            elementUsages.ForEach(e => associatedElementDefinitions.Add(e.ElementDefinition));

            List<ElementDefinition> unreferencedElementDefinitions = new List<ElementDefinition>();
            unreferencedElementDefinitions.AddRange(this.OpenedIteration.Element);

            unreferencedElementDefinitions.RemoveAll(e => associatedElementDefinitions.Contains(e));

            return unreferencedElementDefinitions;
        }
    }
}
