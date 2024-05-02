// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NamingConventionService.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Server.Services.NamingConventionService
{
    using System.Text.Json;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.NamingConventionService;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="NamingConventionService" /> provides static information based on defined naming convention, like for names of
    /// <see cref="Category" /> to use for example
    /// </summary>
    public class NamingConventionService<TEnum> : BaseNamingConventionService<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<INamingConventionService<TEnum>> logger;

        /// <summary>
        /// Initializes a new instance of <see cref="NamingConventionService{TEnum}" />
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public NamingConventionService(ILogger<INamingConventionService<TEnum>> logger) : base(logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets the naming convention configuration
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{TKey,TValue}" /> of the naming convention configuration</returns>
        protected override async Task<IReadOnlyDictionary<string, string>> GetNamingConventionConfiguration()
        {
            try
            {
                var namingConvention = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(Path.Combine("Resources", "configuration", "naming_convention.json")))!;
                return new Dictionary<string, string>(namingConvention, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error while getting the naming convention configuration file.");
                throw;
            }
        }
    }
}
