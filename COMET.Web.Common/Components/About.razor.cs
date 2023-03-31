// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="About.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components
{
    using COMET.Web.Common.Services.VersionService;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component used to provide content about the software
    /// </summary>
    public partial class About
    {
        /// <summary>
        /// The Git url
        /// </summary>
        private const string GitUrl = "https://raw.githubusercontent.com/RHEAGROUP/COMET-WEB-Community-Edition/master/LICENSE";

        /// <summary>
        /// The Version of the COMET-WEB CE application
        /// </summary>
        private string cometWebVersion;

        /// <summary>
        /// The license text as retrieved from the GitHub repository
        /// </summary>
        private string license = string.Empty;

        /// <summary>
        /// The <see cref="HttpClient" />
        /// </summary>
        [Inject]
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// The <see cref="IVersionService" />
        /// </summary>
        [Inject]
        public IVersionService VersionService { get; set; }

        /// <summary>
        /// Initializes the component
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            this.cometWebVersion = this.VersionService.GetVersion();
            this.license = await this.HttpClient.GetStringAsync(GitUrl);
        }
    }
}
