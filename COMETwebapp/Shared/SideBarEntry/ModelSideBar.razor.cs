// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelSideBar.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Shared.SideBarEntry
{
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    /// <summary>
    /// Side bar entry to list the model actions
    /// </summary>
    public partial class ModelSideBar : ModelMenu
    {
        /// <summary>
        /// The value to check if the dropdown should be expanded
        /// </summary>
        public bool Expanded { get; private set; }

        /// <summary>
        /// The <see cref="IJSRuntime" /> used to move keyboard focus into and out of the model drop-down
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Expands the dropdown present in the navbar
        /// </summary>
        public void ExpandDropdown()
        {
            this.Expanded = true;
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Moves keyboard focus into the model drop-down once it is shown, so a keyboard user can reach its menu (the
        /// drop-down renders in a body-level portal that Tab order would otherwise never reach, see issue #885).
        /// </summary>
        /// <param name="eventArgs">The <see cref="DropDownShownEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnDropdownShown(DropDownShownEventArgs eventArgs)
        {
            await this.JsRuntime.InvokeVoidAsync("cometKeyboard.focusFirstIn", "#model-dropdown-body");
        }

        /// <summary>
        /// Returns keyboard focus to the model entry when its drop-down closes, so focus is not lost.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DropDownClosedEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnDropdownClosed(DropDownClosedEventArgs eventArgs)
        {
            await this.JsRuntime.InvokeVoidAsync("cometKeyboard.focusElement", "#model-entry");
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Disposables.Add(this.ViewModel.SessionService.OpenIterations.CountChanged.SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
