// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServerConnexionService.cs" company="RHEA System S.A.">
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
namespace COMET.Web.Common.Services.ServerConnexionService
{
    using System;
    using System.Text.Json;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Utilities;

    using Microsoft.Extensions.Options;

    /// <summary>
    /// Service that holds the text data from the configuration file
    /// </summary>
    public class ServerConnexionService : IServerConnexionService
    {
        /// <summary>
        /// The json file that contains the server configuration
        /// </summary>
        private readonly string ServerConfigurationFile;

        /// <summary>
        /// The Server Address to use
        /// </summary>
        public string ServerAddress { get; private set; }

        /// <summary>
        /// The <see cref="HttpClient"/> used to retrieve the json
        /// </summary>
        private readonly HttpClient http;

        /// <summary>
        /// Value to assert that the service has been initialized
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Creates a new instance of type <see cref="ServerConnexionService"/>
        /// </summary>
        /// <param name="options">the <see cref="IOptions{GlobalOptions}"/></param>
        /// <param name="httpClient">the <see cref="HttpClient"/></param>
        public ServerConnexionService(IOptions<GlobalOptions> options, HttpClient httpClient)
        {
            this.ServerConfigurationFile = options.Value.ServerConfigurationFile ?? "server_configuration.json";
            this.http = httpClient;
        }

        /// <summary>
        /// Initializes the <see cref="ServerConnexionService"/>
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public async Task InitializeService()
        {
            Dictionary<string, string> configurations = new Dictionary<string, string>();

            if (this.isInitialized)
            {
                return;
            }

            try
            {
                var path = ContentPathBuilder.BuildPath(this.ServerConfigurationFile);
                var jsonContent = await this.http.GetStreamAsync(path);
                configurations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                ServerAddress = configurations["ServerAddress"];
            }
            catch (Exception e)
            {
                return;
            }

            this.isInitialized = true;
        }
    }
}
