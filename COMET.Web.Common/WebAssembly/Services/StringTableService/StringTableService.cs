// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StringTableService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.WebAssembly.Services.StringTableService
{
    using System.Text.Json;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Utilities;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Service that holds the text data from the configuration file
    /// </summary>
    public class StringTableService : BaseStringTableService
    {
        /// <summary>
        /// The <see cref="HttpClient" /> used to retrieve the json
        /// </summary>
        private readonly HttpClient http;

        /// <summary>
        /// Creates a new instance of type <see cref="StringTableService" />
        /// </summary>
        /// <param name="options">the <see cref="IOptions{GlobalOptions}" /></param>
        /// <param name="httpClient">the <see cref="HttpClient" /></param>
        /// <param name="logger">the <see cref="ILogger{T}" /></param>
        public StringTableService(IOptions<GlobalOptions> options, HttpClient httpClient, ILogger<StringTableService> logger):base( logger)
        {
            this.FilePath = options.Value.JsonConfigurationFile ?? "DefaultTextConfiguration.json";
            this.http = httpClient;
        }

        /// <summary>
        /// Initializes the <see cref="StringTableService" />
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
                var path = ContentPathBuilder.BuildPath(this.FilePath);
                var jsonContent = await this.http.GetStreamAsync(path);
                this.Configurations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Error while getting the configuration file.");
                return;
            }

            this.IsInitialized = true;
        }
    }
}
