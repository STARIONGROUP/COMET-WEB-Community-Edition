// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTable.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Class for the component <see cref="ParameterTable"/>
    /// </summary>
    public partial class ParameterTable
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// All <see cref="ElementUsage"/> and the Top <see cref="ElementDefinition"/> of the iteration
        /// </summary>
        [Parameter]
        public List<ElementBase> Elements { get; set; }

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        [Parameter]
        public bool IsOwnedParameters { get; set; }

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        [Parameter]
        public string ParameterTypeSelected { get; set; }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        [Parameter]
        public string OptionSelected { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        [Parameter]
        public string StateSelected { get; set; }

        /// <summary>
        /// Filters <see cref="Parameter"/> for the selected owner and the selected type
        /// </summary>
        /// <param name="parameters"><see cref="Parameter"/> to filter</param>
        public IEnumerable<Parameter> FilterParameters(List<Parameter> parameters)
        {
            var filteredParameters = new List<Parameter>();
            
            if (this.IsOwnedParameters == true)
            {
                filteredParameters.AddRange(parameters.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)));
            }
            else
            {
                filteredParameters.AddRange(parameters);
            }
            
            if (this.ParameterTypeSelected != null)
            {
                filteredParameters.RemoveAll(p => p.ParameterType.Name != this.ParameterTypeSelected);
            }

            return filteredParameters.OrderBy(p => p.ParameterType.Name);
        }

        /// <summary>
        /// Filters <see cref="ParameterOverride"/> for the selected owner and the selected type
        /// </summary>
        /// <param name="parameters"><see cref="ParameterOverride"/> to filter</param>
        public IEnumerable<ParameterOverride> FilterParameterOverrides(List<ParameterOverride> parameters)
        {
            var filteredParameters = new List<ParameterOverride>();
            
            if (this.IsOwnedParameters == true)
            {
                filteredParameters.AddRange(parameters.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)));
            }
            else
            {
                filteredParameters.AddRange(parameters);
            }
            
            if (this.ParameterTypeSelected != null)
            {
                filteredParameters.RemoveAll(p => p.ParameterType.Name != this.ParameterTypeSelected);
            }

            return filteredParameters.OrderBy(p => p.ParameterType.Name);
        }

        /// <summary>
        /// Filters <see cref="ParameterValueSetBase"/>s for the selected option and the selected state
        /// </summary>
        /// <param name="isOptionDependent">if the <see cref="Parameter"/> is option dependant</param>
        /// <param name="parameterValueSets">the <see cref="ParameterValueSet"/> to filter</param>
        /// <returns>the filtered result</returns>
        public IEnumerable<ParameterValueSetBase> FilterParameterValueSetBase(bool isOptionDependent, List<ParameterValueSetBase> parameterValueSets)
        {
            var filteredParameterValueSets = new List<ParameterValueSetBase>();
            
            if (this.OptionSelected != null && isOptionDependent)
            {
                filteredParameterValueSets.AddRange(parameterValueSets.FindAll(p => p.ActualOption.Name.Equals(this.OptionSelected)));
            }
            else
            {
                filteredParameterValueSets.AddRange(parameterValueSets);
            }
            
            if (this.StateSelected != null)
            {
                var parameterValueSetsToRemove = new List<ParameterValueSetBase>();
                
                filteredParameterValueSets.ForEach(p =>
                {
                    if (p.ActualState is null || !p.ActualState.Name.Equals(this.StateSelected))
                    {
                        parameterValueSetsToRemove.Add(p);
                    }
                });
                
                filteredParameterValueSets.RemoveAll(p => parameterValueSetsToRemove.Contains(p));
            }

            return filteredParameterValueSets.OrderBy(p => p.ModelCode());
        }
    }
}
