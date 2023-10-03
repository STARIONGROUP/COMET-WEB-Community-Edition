// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfigurationService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.WebAssembly.Services.ConfigurationService
{
    using System.Net;
    using System.Text.Json;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Utilities;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Service that holds the configuration for the application
    /// </summary>
    public class ConfigurationService : BaseConfigurationService
    {
        /// <summary>
        /// The <see cref="HttpClient" /> used to retrieve the json
        /// </summary>
        private readonly HttpClient http;

        /// <summary>
        /// The json file that contains the server configuration
        /// </summary>
        private readonly string serverConfigurationFile;

        /// <summary>
        /// Gets the <see cref="ILogger{T}"/>
        /// </summary>
        private readonly ILogger<ConfigurationService> logger;

        /// <summary>
        /// Creates a new instance of type <see cref="ConfigurationService" />
        /// </summary>
        /// <param name="options">the <see cref="IOptions{GlobalOptions}" /></param>
        /// <param name="httpClient">the <see cref="HttpClient" /></param>
        /// <param name="logger">The <see cref="ILogger{T}"/></param>
        public ConfigurationService(IOptions<GlobalOptions> options, HttpClient httpClient, ILogger<ConfigurationService> logger)
        {
            this.serverConfigurationFile = options.Value.ServerConfigurationFile ?? "server_configuration.json";
            this.http = httpClient;
            this.logger = logger;
        }

        /// <summary>
        /// Initializes the <see cref="ConfigurationService" />
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public override async Task InitializeService()
        {
            if (this.IsInitialized)
            {
                return;
            }

            try
            {
                var path = ContentPathBuilder.BuildPath(this.serverConfigurationFile);
                var response = await this.http.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStreamAsync();
                    var configurations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                    this.ServerAddress = configurations["ServerAddress"];
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogError("Server configuration file not found at {path}", path);
                    return;
                }
                else
                {
                    this.logger.LogError("Error fetching server configuration. Status code: {response}", response.StatusCode);
                    return;
                }
            }
            catch (Exception e)
            {
                this.logger.LogCritical("Exception has been raised : {message}", e.Message);
                return;
            }

            this.IsInitialized = true;
        }
    }
}
