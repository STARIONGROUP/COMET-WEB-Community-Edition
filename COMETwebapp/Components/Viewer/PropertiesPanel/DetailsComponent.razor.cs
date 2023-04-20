// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsComponent.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Components.Viewer.PropertiesPanel
{
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;
    
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The component used for showing the details of the current selected <see cref="Model.SceneObject"/>
    /// </summary>
    public partial class DetailsComponent
    {
        /// <summary>
        /// Gets or sets the <see cref="IDetailsComponentViewModel"/>
        /// </summary>
        [Parameter]
        public IDetailsComponentViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (this.ViewModel.ParameterEditorSelector.HaveValueSetViewModel is CompoundParameterTypeEditorViewModel { IsOnEditMode: false } compoundParameterTypeEditorViewModel)
            {
                compoundParameterTypeEditorViewModel.IsOnEditMode = true;
            }
        }
    }
}
