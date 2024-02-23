// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="INamingConventionService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Services.NamingConventionService
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The <see cref="INamingConventionService{TEnum}" /> provides static information based on defined naming convention, like for names of
    /// <see cref="Category" /> to use for example
    /// </summary>
    /// <typeparam name="TEnum">Any type of enumeration that will contain the different types of naming conventions</typeparam>
    public interface INamingConventionService<in TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Initializes this service
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task InitializeService();

        /// <summary>
        /// Gets the value for naming convention
        /// </summary>
        /// <param name="namingConventionKey">The naming convention key</param>
        /// <returns>The defined naming convention, if exists</returns>
        string GetNamingConventionValue(string namingConventionKey);

        /// <summary>
        /// Gets the value for naming convention
        /// </summary>
        /// <param name="namingConventionKind">The <typeparamref name="TEnum" /></param>
        /// <returns>The defined naming convention, if exists</returns>
        string GetNamingConventionValue(TEnum namingConventionKind);
    }
}
