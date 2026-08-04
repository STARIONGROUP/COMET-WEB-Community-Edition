// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ReconnectStateBanner.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Shared.Reconnection
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component representing an individual state banner in the reconnect modal
    /// </summary>
    public partial class ReconnectStateBanner
    {
        /// <summary>
        /// Gets or sets the CSS class for the state container
        /// </summary>
        [Parameter]
        public string StateClass { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the badge type (e.g. "warning", "danger")
        /// </summary>
        [Parameter]
        public string BadgeType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether a spinner is used instead of a static dot indicator
        /// </summary>
        [Parameter]
        public bool IsSpinner { get; set; }

        /// <summary>
        /// Gets or sets the title text
        /// </summary>
        [Parameter]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description message text
        /// </summary>
        [Parameter]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional action button text
        /// </summary>
        [Parameter]
        public string ActionText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action type
        /// </summary>
        [Parameter]
        public ReconnectActionType ActionType { get; set; } = ReconnectActionType.None;

        /// <summary>
        /// Gets the JavaScript handler string for the action link
        /// </summary>
        public string ActionOnClick => this.ActionType switch
        {
            ReconnectActionType.Reconnect => "Blazor.reconnect()",
            _ => string.Empty
        };

        /// <summary>
        /// Gets additional CSS class for the action link
        /// </summary>
        public string ActionClass => this.ActionType switch
        {
            ReconnectActionType.Reload => "reload",
            _ => string.Empty
        };
    }
}
