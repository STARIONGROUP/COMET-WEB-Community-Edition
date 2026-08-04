// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometButtonStyleExtensions.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Extensions
{
    using COMET.Web.Common.Enumerations;

    using DevExpress.Blazor;

    /// <summary>
    /// Extension methods for <see cref="CometButtonStyle" />
    /// </summary>
    public static class CometButtonStyleExtensions
    {
        /// <summary>
        /// Maps a <see cref="CometButtonStyle" /> to the corresponding <see cref="ButtonRenderStyle" />.
        /// </summary>
        /// <param name="style">The <see cref="CometButtonStyle" /> to map.</param>
        /// <returns>The mapped <see cref="ButtonRenderStyle" />.</returns>
        public static ButtonRenderStyle ToButtonRenderStyle(this CometButtonStyle style)
        {
            return style switch
            {
                CometButtonStyle.Primary => ButtonRenderStyle.Primary,
                CometButtonStyle.Secondary => ButtonRenderStyle.Secondary,
                CometButtonStyle.Danger => ButtonRenderStyle.Danger,
                CometButtonStyle.Success => ButtonRenderStyle.Success,
                CometButtonStyle.Warning => ButtonRenderStyle.Warning,
                CometButtonStyle.Info => ButtonRenderStyle.Info,
                CometButtonStyle.Edit => ButtonRenderStyle.Info,
                CometButtonStyle.Light => ButtonRenderStyle.Light,
                CometButtonStyle.Dark => ButtonRenderStyle.Dark,
                CometButtonStyle.Link => ButtonRenderStyle.Link,
                _ => ButtonRenderStyle.Primary
            };
        }
    }
}
