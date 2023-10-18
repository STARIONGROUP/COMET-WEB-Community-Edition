// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NamingConventionService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Services.NamingConventionService
{
    using System.Text.Json;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="NamingConventionService" /> provides static information based on defined naming convention, like for names of
    /// <see cref="Category" /> to use for example
    /// </summary>
    public class NamingConventionService<TEnum> : INamingConventionService<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}" /> that holds the defined naming convention
        /// </summary>
        private readonly Dictionary<string, string> definedNaming = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<INamingConventionService<TEnum>> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamingConventionService" /> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public NamingConventionService(ILogger<INamingConventionService<TEnum>> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Initializes this service
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task InitializeService()
        {
            Dictionary<string, string> namingConvention;

            try
            {
                namingConvention = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(Path.Combine("Resources", "configuration", "naming_convention.json")))!;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error while getting the naming convention configuration file.");
                return;
            }

            namingConvention = new Dictionary<string, string>(namingConvention, StringComparer.OrdinalIgnoreCase);

            foreach (var namingConventionKind in Enum.GetValues(typeof(TEnum)))
            {
                if (namingConvention.TryGetValue(namingConventionKind.ToString(), out var namingConventionValue))
                {
                    this.definedNaming[namingConventionKind.ToString()] = namingConventionValue;
                }
                else
                {
                    this.logger.LogWarning("{namingConventionKind} is missing from the Naming Convention configuration file", namingConventionKind.ToString());
                }
            }
        }

        /// <summary>
        /// Gets the value for naming convention
        /// </summary>
        /// <param name="namingConventionKey">The naming convention key</param>
        /// <returns>The defined naming convention, if exists</returns>
        public string GetNamingConventionValue(string namingConventionKey)
        {
            return this.definedNaming.TryGetValue(namingConventionKey, out var namingConventionValue) ? namingConventionValue : string.Empty;
        }
    
        /// <summary>
        /// Gets the value for naming convention
        /// </summary>
        /// <param name="namingConventionKind">The <see cref="NamingConventionKind" /></param>
        /// <returns>The defined naming convention, if exists</returns>
        public string GetNamingConventionValue(TEnum namingConventionKind)
        {
            return this.GetNamingConventionValue(namingConventionKind.ToString());
        }
    }
}
