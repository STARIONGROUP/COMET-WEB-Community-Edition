﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTab.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.Common
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.Common.OpenTab;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component used to open an <see cref="EngineeringModel" /> tab
    /// </summary>
    public partial class OpenTab : OpenModel
    {
        /// <summary>
        /// The <see cref="IOpenTabViewModel" />
        /// </summary>
        [Inject]
        public new IOpenTabViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed when the component is closed. If not set, the cancel button will be hidden
        /// </summary>
        [Parameter]
        public Action OnCancel { get; set; }

        /// <summary>
        /// Gets the condition to check if the selected application view model inherits from
        /// <see cref="ISingleEngineeringModelApplicationBaseViewModel" />
        /// </summary>
        private bool IsEngineeringModelView => this.ViewModel.SelectedApplication?.ViewModelType.IsAssignableTo(typeof(ISingleEngineeringModelApplicationBaseViewModel)) == true;

        /// <summary>
        /// Gets the condition to check if the selected application view model inherits from
        /// <see cref="ISingleIterationApplicationBaseViewModel" />
        /// </summary>
        private bool IsIterationView => this.ViewModel.SelectedApplication?.ViewModelType.IsAssignableTo(typeof(ISingleIterationApplicationBaseViewModel)) == true;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Verifies that all required field are selected
        /// </summary>
        /// <returns>True if all required field are selected</returns>
        protected override bool AreRequiredFieldSelected()
        {
            if (!this.IsEngineeringModelView && !this.IsIterationView)
            {
                return this.ViewModel.SelectedApplication != null;
            }

            return base.AreRequiredFieldSelected() && this.ViewModel.SelectedApplication != null;
        }

        /// <summary>
        /// Opens a model and navigates to the selected application/view
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OpenModelAndNavigateToView()
        {
            await this.ViewModel.OpenSession();
            await this.InvokeAsync(this.StateHasChanged);
        }
    }
}