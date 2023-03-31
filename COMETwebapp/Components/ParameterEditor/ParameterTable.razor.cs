// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTable.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// Class for the component <see cref="ParameterTable" />
    /// </summary>
    public partial class ParameterTable
    {
        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel" />
        /// </summary>
        [Parameter]
        public IParameterTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnEditMode)
                            .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
            
            this.Disposables.Add(this.ViewModel.Rows.CountChanged.Subscribe( _ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
