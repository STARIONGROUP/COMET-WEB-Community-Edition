// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesComponent.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Viewer.PropertiesPanel
{
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;
    
    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// The properties component used for displaying data about the selected primitive
    /// </summary>
    public partial class PropertiesComponent
    {
        /// <summary>
        /// Gets or sets the <see cref="IPropertiesComponentViewModel"/>
        /// </summary>
        [Parameter]
        public IPropertiesComponentViewModel ViewModel { get; set; }
        
        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.WhenAnyValue(x => x.ViewModel.IsVisible,
            x => x.ViewModel.SelectedParameter,
            x => x.ViewModel.ParameterHaveChanges).Subscribe(_ => this.InvokeAsync(this.StateHasChanged));
        }
    }
}
