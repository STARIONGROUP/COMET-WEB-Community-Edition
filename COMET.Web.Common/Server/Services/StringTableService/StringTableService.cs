// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StringTableService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Server.Services.StringTableService
{
    using System.Text.Json;

    using COMET.Web.Common.Services.StringTableService;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service implementation that holds the text data from the configuration file
    /// </summary>
    public class StringTableService : BaseStringTableService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringTableService" /> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration" /></param>
        /// <param name="logger">The <see cref="ILogger{T}" /></param>
        public StringTableService(IConfiguration configuration, ILogger<StringTableService> logger) : base(logger)
        {
            this.FilePath = configuration["StringTablePath"];
        }

        /// <summary>
        /// Initializes the <see cref="IStringTableService" />
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
                var jsonContent = await File.ReadAllTextAsync(this.FilePath);
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
