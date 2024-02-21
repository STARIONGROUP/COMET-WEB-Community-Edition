// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="About.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Shared
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
        /// The <see cref="IHttpClientFactory" />
        /// </summary>
        [Inject]
        public IHttpClientFactory HttpClientFactory { get; set; }

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
            var httpClient = this.HttpClientFactory.CreateClient("About");
            this.license = await httpClient.GetStringAsync(GitUrl);
        }
    }
}
