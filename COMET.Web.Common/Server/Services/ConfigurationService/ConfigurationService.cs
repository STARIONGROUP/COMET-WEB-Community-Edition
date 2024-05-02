﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfigurationService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Server.Services.ConfigurationService
{
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Service that holds the configuration for the application
    /// </summary>
    public class ConfigurationService : BaseConfigurationService
    {
        /// <summary>
        /// Gets the ServerConfiguration section key
        /// </summary>
        public const string ServerConfigurationSection = "ServerConfiguration";

        /// <summary>
        /// Gets the <see cref="IConfiguration" />
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationService" />
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration" /></param>
        public ConfigurationService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Initializes the <see cref="IConfigurationService" />
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public override Task InitializeService()
        {
            if (this.IsInitialized)
            {
                return Task.CompletedTask;
            }

            this.ServerConfiguration = this.configuration.GetSection(ServerConfigurationSection).Get<ServerConfiguration>();
            this.IsInitialized = true;
            return Task.CompletedTask;
        }
    }
}
