// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BaseStringTableService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Services.StringTableService
{
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Base Service implementation that holds the text data from the configuration file
    /// </summary>
    public abstract class BaseStringTableService : IStringTableService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStringTableService" /> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{T}" /></param>
        protected BaseStringTableService(ILogger<BaseStringTableService> logger)
        {
            this.Logger = logger;
        }

        /// <summary>
        /// The <see cref="ILogger{T}" />
        /// </summary>
        protected ILogger<BaseStringTableService> Logger { get; }

        /// <summary>
        /// Gets the file path to load the service
        /// </summary>
        protected string FilePath { get; set; }

        /// <summary>
        /// The dictionary that contains the map between the configuration and the value
        /// </summary>
        protected Dictionary<string, string> Configurations { get; set; } = new();

        /// <summary>
        /// Value to assert that the service has been initialized
        /// </summary>
        protected bool IsInitialized { get; set; }

        /// <summary>
        /// Initializes the <see cref="IStringTableService" />
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public abstract Task InitializeService();

        /// <summary>
        /// Gets the text asociated to a key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the text asociated to the key</returns>
        public string GetText(string key)
        {
            if (!this.IsInitialized)
            {
                throw new InvalidOperationException("The Service hasn't been initialized");
            }

            this.Configurations.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Gets the text asociated to a key
        /// </summary>
        /// <param name="configurationKind">the <see cref="TextConfigurationKind" /> key</param>
        /// <returns>the text asociated to the key</returns>
        public string GetText(TextConfigurationKind configurationKind)
        {
            return this.GetText(configurationKind.ToString());
        }
    }
}
