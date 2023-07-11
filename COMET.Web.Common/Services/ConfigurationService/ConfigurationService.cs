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
namespace COMET.Web.Common.Services.ConfigurationService
{
    using System.Text.Json;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Utilities;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    
    /// <summary>
    /// Service that holds the text data from the configuration file
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        /// <summary>
        /// The json file that contains the configuration
        /// </summary>
        private readonly string jsonFile;

        /// <summary>
        /// The <see cref="HttpClient"/> used to retrieve the json
        /// </summary>
        private readonly HttpClient http;

        /// <summary>
        /// The <see cref="ILogger{T}"/>
        /// </summary>
        private ILogger<ConfigurationService> logger;

        /// <summary>
        /// The dictionary that contains the map between the configuration and the value
        /// </summary>
        private Dictionary<string, string> configurations = new();

        /// <summary>
        /// Value to assert that the service has been initialized
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Creates a new instance of type <see cref="ConfigurationService"/>
        /// </summary>
        /// <param name="options">the <see cref="IOptions{GlobalOptions}"/></param>
        /// <param name="httpClient">the <see cref="HttpClient"/></param>
        /// <param name="logger">the <see cref="ILogger{T}"/></param>
        public ConfigurationService(IOptions<GlobalOptions> options, HttpClient httpClient, ILogger<ConfigurationService> logger)
        {
            this.jsonFile = options.Value.JsonConfigurationFile ?? "DefaultTextConfiguration.json";
            this.http = httpClient;
            this.logger = logger;
        }

        /// <summary>
        /// Initializes the <see cref="ConfigurationService"/>
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public async Task InitializeService()
        {
            if (this.isInitialized)
            {
                return;
            }
            
            try
            {
                var path = ContentPathBuilder.BuildPath(this.jsonFile);
                var jsonContent = await this.http.GetStreamAsync(path);
                this.configurations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error while getting the configuration file.");
                return;
            }

            this.isInitialized = true;
        }

        /// <summary>
        /// Gets the text asociated to a key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the text asociated to the key</returns>
        public string GetText(string key)
        {
            if (!this.isInitialized)
            {
                throw new InvalidOperationException("The Service hasn't been initialized");
            }

            this.configurations.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Gets the text asociated to a key
        /// </summary>
        /// <param name="configurationKind">the <see cref="TextConfigurationKind"/> key</param>
        /// <returns>the text asociated to the key</returns>
        public string GetText(TextConfigurationKind configurationKind)
        {
            return this.GetText(configurationKind.ToString());
        }
    }
}
