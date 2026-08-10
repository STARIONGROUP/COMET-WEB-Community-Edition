// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionSideBar.razor.cs" company="Starion Group S.A.">
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
    using CDP4Dal;

    using COMET.Web.Common.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    /// <summary>
    /// Side bar entry to access to the <see cref="ISession" /> content
    /// </summary>
    public partial class SessionSideBar : SessionMenu
    {
        /// <summary>
        /// The <see cref="IJSRuntime" /> used to move keyboard focus into and out of the session drop-down
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
        /// Moves keyboard focus into the session drop-down once it is shown, so a keyboard user can reach its buttons
        /// (the drop-down renders in a body-level portal that Tab order would otherwise never reach, see issue #885).
        /// </summary>
        /// <param name="eventArgs">The <see cref="DropDownShownEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnDropdownShown(DropDownShownEventArgs eventArgs)
        {
            await this.JsRuntime.InvokeVoidAsync("cometKeyboard.focusFirstIn", "#session-dropdown-body");
        }

        /// <summary>
        /// Returns keyboard focus to the session entry when its drop-down closes, so focus is not lost.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DropDownClosedEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnDropdownClosed(DropDownClosedEventArgs eventArgs)
        {
            await this.JsRuntime.InvokeVoidAsync("cometKeyboard.focusElement", "#session-side-bar-item");
        }
    }
}
