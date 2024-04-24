// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BatchParameterEditor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ParameterEditor.BatchParameterEditor
{
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the component <see cref="BatchParameterEditor" />
    /// </summary>
    public partial class BatchParameterEditor
    {
        /// <summary>
        /// Gets or sets the <see cref="IBatchParameterEditorViewModel" />
        /// </summary>
        [Parameter]
        public IBatchParameterEditorViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.ParameterTypeSelector.SelectedParameterType).Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
