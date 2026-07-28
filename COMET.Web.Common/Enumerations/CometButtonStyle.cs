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
        /// Primary button style intent.
        /// </summary>
        Primary = 0,

        /// <summary>
        /// Secondary button style intent.
        /// </summary>
        Secondary = 1,

        /// <summary>
        /// Danger button style intent.
        /// </summary>
        Danger = 2,

        /// <summary>
        /// Success button style intent.
        /// </summary>
        Success = 3,

        /// <summary>
        /// Warning button style intent.
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Info button style intent.
        /// </summary>
        Info = 5,

        /// <summary>
        /// Light button style intent.
        /// </summary>
        Light = 6,

        /// <summary>
        /// Dark button style intent.
        /// </summary>
        Dark = 7,

        /// <summary>
        /// Link button style intent.
        /// </summary>
        Link = 8
    }
}
