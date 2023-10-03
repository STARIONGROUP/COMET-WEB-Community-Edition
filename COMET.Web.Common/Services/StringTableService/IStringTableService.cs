// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IStringTableService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Services.StringTableService
{
    using COMET.Web.Common.Enumerations;

    /// <summary>
    /// Service that holds the text data from the configuration file
    /// </summary>
    public interface IStringTableService
    {
        /// <summary>
        /// Initializes the <see cref="IStringTableService"/>
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        Task InitializeService();

        /// <summary>
        /// Gets the text asociated to a key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the text asociated to the key</returns>
        string GetText(string key);

        /// <summary>
        /// Gets the text asociated to a key
        /// </summary>
        /// <param name="configurationKind">the <see cref="TextConfigurationKind"/> key</param>
        /// <returns>the text asociated to the key</returns>
        string GetText(TextConfigurationKind configurationKind);
    }
}
