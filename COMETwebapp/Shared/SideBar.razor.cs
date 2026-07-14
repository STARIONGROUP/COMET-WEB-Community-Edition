// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SideBar.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Shared
{
    using COMET.Web.Common.Services.RegistrationService;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component used for the side bar
    /// </summary>
    public partial class SideBar
    {
        /// <summary>
        /// The <see cref="IRegistrationService" />
        /// </summary>
        [Inject]
        public IRegistrationService RegistrationService { get; set; }

        /// <summary>
        /// Backing field for <see cref="IsNarrowViewport" />
        /// </summary>
        private bool isNarrowViewport;

        /// <summary>
        /// The collapsed state that the user explicitly chose with the toggle, or null while they have not overruled the state
        /// that the viewport width implies
        /// </summary>
        private bool? userCollapsed;

        /// <summary>
        /// Gets or sets a value indicating whether the viewport is too narrow to afford the full width menu, for instance when
        /// the browser only takes a quarter of the screen. Crossing that threshold drops any earlier manual override, so that
        /// the side bar collapses on its own when the window shrinks and expands again when it grows.
        /// </summary>
        public bool IsNarrowViewport
        {
            get => this.isNarrowViewport;

            set
            {
                if (this.isNarrowViewport == value)
                {
                    return;
                }

                this.isNarrowViewport = value;
                this.userCollapsed = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the sidebar is rendered collapsed. A narrow viewport collapses it by default, but the
        /// user can always overrule that with the toggle, in either direction (issue #742)
        /// </summary>
        public bool IsCollapsed => this.userCollapsed ?? this.IsNarrowViewport;

        /// <summary>
        /// Toggles the collapsed state of the side bar
        /// </summary>
        public void ToggleCollapsed()
        {
            this.userCollapsed = !this.IsCollapsed;
        }
    }
}
