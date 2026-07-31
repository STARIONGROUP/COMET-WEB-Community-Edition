// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometIcon.razor.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders a single glyph of the consolidated COMET WEB icon system. The glyph is drawn as a CSS mask so it
    /// inherits the surrounding text colour through <c>currentColor</c> and scales with the font size, with no
    /// icon font shipped. The per-glyph mask is emitted once at runtime by the <c>CometIconStyles</c> component
    /// from the vendored Feather icon library, so no SVG files are stored in the repository. This is the single
    /// entry point for icons in markup; the equivalent CSS class for the <c>IconCssClass</c> slot of a DevExpress
    /// button or menu item is produced by <see cref="IconNameExtensions.GetCssClass(IconName)" />.
    /// </summary>
    public partial class CometIcon
    {
        /// <summary>
        /// Gets or sets the glyph to render.
        /// </summary>
        [Parameter]
        public IconName Icon { get; set; }

        /// <summary>
        /// Gets or sets an optional explicit size, in pixels, overriding the default of one em. Used for the large
        /// icons on the application tiles and tabs; when <see langword="null" /> the glyph scales with the font size.
        /// </summary>
        [Parameter]
        public int? Size { get; set; }

        /// <summary>
        /// Gets or sets an optional accessible label rendered as the <c>title</c> attribute. When
        /// <see langword="null" /> or empty, no <c>title</c> attribute is rendered and the glyph stays purely decorative.
        /// </summary>
        [Parameter]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets any extra CSS classes appended after the icon classes, for per-site tweaks such as spacing.
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets the computed <c>class</c> attribute value, combining the mask-based icon classes for
        /// <see cref="Icon" /> with any additional <see cref="CssClass" />.
        /// </summary>
        private string ComputedCssClass => string.IsNullOrEmpty(this.CssClass)
            ? this.Icon.GetCssClass()
            : $"{this.Icon.GetCssClass()} {this.CssClass}";

        /// <summary>
        /// Gets the computed inline <c>style</c> attribute value that pins the glyph to <see cref="Size" /> pixels,
        /// or <see langword="null" /> when no size is set so the glyph scales with the font size.
        /// </summary>
        private string ComputedStyle => this.Size is { } size
            ? $"width:{size}px !important;height:{size}px !important;"
            : null;

        /// <summary>
        /// Gets the <see cref="Title" />, or <see langword="null" /> when it is empty so no <c>title</c> attribute
        /// is rendered.
        /// </summary>
        private string ComputedTitle => string.IsNullOrEmpty(this.Title) ? null : this.Title;
    }
}
