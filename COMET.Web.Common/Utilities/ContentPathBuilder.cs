﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ContentPathBuilder.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMET.Web.Common.Utilities
{
    /// <summary>
    /// Utility class that enables to create path for content inside the assembly
    /// </summary>
    internal static class ContentPathBuilder
    {
        /// <summary>
        /// The name of the Nuget PackageId
        /// </summary>
        internal const string PackageId = "CDP4.WEB.Common";

        /// <summary>
        /// Builds the path to access a content inside the assembly
        /// </summary>
        /// <param name="pathOfContent">A relative path to access the content</param>
        /// <returns>The full builded path</returns>
        internal static string BuildPath(string pathOfContent)
        {
            return Path.Combine("_content", PackageId, pathOfContent);
        }
    }
}
