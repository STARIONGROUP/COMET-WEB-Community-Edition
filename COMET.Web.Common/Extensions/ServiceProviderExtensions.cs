// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BuilderExtension.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Extensions
{
    using COMET.Web.Common.Services.StringTableService;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension class for <see cref="IServiceProvider" />
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Initialize the neccesary services of the CDP4 Comet webapp
        /// </summary>
        /// <param name="serviceProvider">the <see cref="IServiceProvider" /></param>
        /// <returns>an asynchronous operation</returns>
        public static async Task InitializeCdp4CometCommonServices(this IServiceProvider serviceProvider)
        {
            var stringTableService = serviceProvider.GetRequiredService<IStringTableService>();
            
            if (stringTableService != null)
            {
                await stringTableService.InitializeService();
            }
        }
    }
}
