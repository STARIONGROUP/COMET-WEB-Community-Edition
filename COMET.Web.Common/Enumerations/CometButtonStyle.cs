// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometButtonStyle.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Enumerations
{
    /// <summary>
    /// Represents the style intent of a button component in COMET WEB applications.
    /// </summary>
    public enum CometButtonStyle
    {
        /// <summary>
        /// Primary button style intent. Used for primary actions, main call-to-actions, and confirmation/submit buttons.
        /// </summary>
        Primary = 0,

        /// <summary>
        /// Secondary button style intent. Used for neutral actions, cancellation, secondary options, and simple selections.
        /// </summary>
        Secondary = 1,

        /// <summary>
        /// Danger button style intent. Used for destructive or irreversible actions, such as delete or remove operations.
        /// </summary>
        Danger = 2,

        /// <summary>
        /// Success button style intent. Used for positive confirmation, successful operations, or completion actions.
        /// </summary>
        Success = 3,

        /// <summary>
        /// Warning button style intent. Used for cautionary actions or operations requiring user attention.
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Info button style intent. Used for informational actions, help triggers, or guidance prompts.
        /// </summary>
        Info = 5,

        /// <summary>
        /// Light button style intent. Used for subtle actions on dark backgrounds or low-emphasis controls.
        /// </summary>
        Light = 6,

        /// <summary>
        /// Dark button style intent. Used for high-contrast actions on light backgrounds or dark-themed controls.
        /// </summary>
        Dark = 7,

        /// <summary>
        /// Link button style intent. Used for inline navigation, hyperlink-style actions, or minimal footprint triggers.
        /// </summary>
        Link = 8
    }
}
