// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometIconStyles.razor.cs" company="Starion Group S.A.">
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
    using System;
    using System.Text;

    using BlazorBlueprint.Icons.Feather.Data;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;

    /// <summary>
    /// Emits a single <c>&lt;style&gt;</c> block holding one <c>comet-icon-*</c> mask rule per
    /// <see cref="IconName" />. Each rule's <c>mask-image</c> is a data URI built at runtime from the vendored
    /// Feather icon library, so the glyphs the application uses are drawn from the library rather than from SVG
    /// files committed to the repository. Rendered once, near the top of the application layout.
    /// </summary>
    public partial class CometIconStyles
    {
        /// <summary>
        /// The generated <c>&lt;style&gt;</c> block. Built once because it depends only on the fixed
        /// <see cref="IconName" /> set and the static icon library.
        /// </summary>
        private static readonly string StyleBlock = BuildStyleBlock();

        /// <summary>
        /// Builds the <c>&lt;style&gt;</c> block containing a mask rule for every <see cref="IconName" />.
        /// </summary>
        /// <returns>The style block markup.</returns>
        private static string BuildStyleBlock()
        {
            var builder = new StringBuilder();
            builder.Append("<style>");

            foreach (var icon in Enum.GetValues<IconName>())
            {
                var inner = FeatherIconData.GetIcon(icon.ToFeatherName());

                if (string.IsNullOrEmpty(inner))
                {
                    continue;
                }

                var svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>{inner.Replace("currentColor", "black")}</svg>";
                var url = $"url(\"data:image/svg+xml,{Uri.EscapeDataString(svg)}\")";
                builder.Append($".comet-icon-{icon.ToCssSuffix()}{{-webkit-mask-image:{url};mask-image:{url};}}");
            }

            builder.Append("</style>");
            return builder.ToString();
        }
    }
}
