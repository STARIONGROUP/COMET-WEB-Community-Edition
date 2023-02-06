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

    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the component <see cref="ParameterTable"/>
    /// </summary>
    public partial class ParameterTable
    {
        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel"/>
        /// </summary>
        [Inject]
        public IParameterTableViewModel ViewModel { get; set; }

        /// <summary>
        /// All <see cref="ElementUsage"/> and the Top <see cref="ElementDefinition"/> of the iteration
        /// </summary>
        [Parameter]
        public SourceList<ElementBase> Elements { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            
            if (firstRender)
            {
                this.ViewModel.InitializeViewModel(this.Elements.Items);
                this.WhenAnyValue(x => x.ViewModel.Rows.CountChanged).Subscribe(_ => this.InvokeAsync(this.StateHasChanged));
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
