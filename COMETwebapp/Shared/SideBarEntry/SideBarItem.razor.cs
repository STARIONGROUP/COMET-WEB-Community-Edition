// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SideBarItem.razor.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Enumerations;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    /// <summary>
    /// The component that handles all entries for the side bar
    /// </summary>
    public partial class SideBarItem
    {
        /// <summary>
        /// Gets or sets the action to be executed when the component is clicked
        /// </summary>
        [Parameter]
        public Action OnClick { get; set; }

        /// <summary>
        /// Gets or sets the value to check if the item is enabled
        /// </summary>
        [Parameter]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Value to check if the component is a dropdown selector
        /// </summary>
        [Parameter]
        public bool DropdownSelector { get; set; }

        /// <summary>
        /// Gets or sets the icon css class to be displayed
        /// </summary>
        [Parameter]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the icon to be displayed, overriding the selected <see cref="IconCssClass" />, if set
        /// </summary>
        [Parameter]
        public IconName? Icon { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed
        /// </summary>
        [Parameter]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets css class to apply to the component
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entry is the currently active one, so it is announced to assistive
        /// technologies with <c>aria-current="page"</c> (see issue #885)
        /// </summary>
        [Parameter]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the current component html id
        /// </summary>
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the render fragment to display inside the component
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Executes the onclick action if the component is enabled
        /// </summary>
        private void ExecuteAction()
        {
            if (this.Enabled)
            {
                this.OnClick?.Invoke();
            }
        }

        /// <summary>
        /// Activates the entry from the keyboard, so the side bar navigation can be reached and operated without a mouse,
        /// mirroring the native behaviour of a button on <c>Enter</c> and <c>Space</c>.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyboardEventArgs" /> of the key that was pressed</param>
        private void HandleKeyDown(KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Key is "Enter" or " " or "Spacebar")
            {
                this.ExecuteAction();
            }
        }
    }
}
