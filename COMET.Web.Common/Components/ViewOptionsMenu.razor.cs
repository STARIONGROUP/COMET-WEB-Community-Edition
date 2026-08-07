// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewOptionsMenu.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A reusable "View" cog: a light button carrying the settings icon that opens a dropdown holding the
    /// display and filter options of a page. Gives every application page the same, predictable place - the
    /// upper-right corner of its toolbar - to reach its view settings. Callers supply the option controls
    /// through <see cref="ChildContent" />.
    /// </summary>
    public partial class ViewOptionsMenu
    {
        /// <summary>
        /// The random suffix appended to <see cref="ButtonId" /> to guarantee a unique DOM id even when
        /// multiple <see cref="ViewOptionsMenu" /> instances share the same <see cref="ButtonId" /> prefix.
        /// </summary>
        private readonly string idSuffix = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets or sets the id prefix given to the trigger button. The component appends a random suffix so
        /// that several menus with the same prefix can coexist on the same page. Useful as a stable selector
        /// prefix in tests (e.g. <c>[id^='myButton']</c>).
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string ButtonId { get; set; }

        /// <summary>
        /// Gets or sets the option controls rendered inside the dropdown.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the text shown on the trigger button.
        /// </summary>
        [Parameter]
        public string ButtonText { get; set; } = "View";

        /// <summary>
        /// Gets or sets the tooltip shown on the trigger button.
        /// </summary>
        [Parameter]
        public string ButtonTitle { get; set; } = "Display and filter options";

        /// <summary>
        /// Gets or sets a value indicating whether the dropdown is currently open.
        /// </summary>
        private bool IsOpen { get; set; }

        /// <summary>
        /// Gets the unique DOM id for the trigger button, formed by combining <see cref="ButtonId" /> with
        /// <see cref="idSuffix" />.
        /// </summary>
        private string UniqueId => $"{this.ButtonId}-{this.idSuffix}";

        /// <summary>
        /// Gets the CSS selector that targets the trigger button by its unique DOM id, used as the
        /// dropdown's <c>PositionTarget</c>.
        /// </summary>
        private string UniqueIdSelector => $"#{this.UniqueId}";
    }
}
