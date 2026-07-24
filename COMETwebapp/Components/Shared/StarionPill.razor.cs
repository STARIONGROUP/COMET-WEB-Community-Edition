// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StarionPill.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Shared
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders a single "pill" badge, the small rounded tag used throughout the application to display an
    /// owner Domain Of Expertise, a Category, or any other short piece of information. The pill's background
    /// colour is derived from <see cref="ColorSeed" /> via <see cref="COMETwebapp.Utilities.TagColorGenerator" />,
    /// so that pills sharing the same seed always share the same colour.
    /// </summary>
    public partial class StarionPill
    {
        /// <summary>
        /// Gets or sets the seed passed to <see cref="COMETwebapp.Utilities.TagColorGenerator.GetBackgroundColor(string)" />
        /// to compute the pill's background colour. When <see langword="null" /> or empty, no background-color
        /// style is rendered, so uncoloured pills (e.g. the state-count or switch-value pills in a parameter card)
        /// keep their default appearance.
        /// </summary>
        [Parameter]
        public string ColorSeed { get; set; }

        /// <summary>
        /// Gets or sets the text rendered as the pill's <c>title</c> attribute. When <see langword="null" /> or
        /// empty, no <c>title</c> attribute is rendered.
        /// </summary>
        [Parameter]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets an optional extra inline style, appended after the computed background-color style, used
        /// to preserve per-site tweaks such as spacing or text wrapping.
        /// </summary>
        [Parameter]
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets the content rendered inside the pill.
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets the computed inline <c>style</c> attribute value, combining the background colour derived from
        /// <see cref="ColorSeed" /> (when set) with any additional <see cref="Style" />. Returns
        /// <see langword="null" /> when both are empty so no <c>style</c> attribute is rendered.
        /// </summary>
        private string ComputedStyle
        {
            get
            {
                var backgroundStyle = string.IsNullOrEmpty(this.ColorSeed)
                    ? string.Empty
                    : $"background-color:{Utilities.TagColorGenerator.GetBackgroundColor(this.ColorSeed)} !important;";

                var combinedStyle = $"{backgroundStyle}{this.Style}";
                return string.IsNullOrEmpty(combinedStyle) ? null : combinedStyle;
            }
        }

        /// <summary>
        /// Gets the <see cref="Title" />, or <see langword="null" /> when it is empty so no <c>title</c> attribute
        /// is rendered.
        /// </summary>
        private string ComputedTitle => string.IsNullOrEmpty(this.Title) ? null : this.Title;
    }
}
