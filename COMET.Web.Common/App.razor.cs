// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="App.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common
{
    using System.Diagnostics.CodeAnalysis;

    using COMET.Web.Common.Services.RegistrationService;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.AspNetCore.WebUtilities;

    /// <summary>
    /// Main application component
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class App
    {
        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// The <see cref="IRegistrationService" />
        /// </summary>
        [Inject]
        internal IRegistrationService RegistrationService { get; set; }

        /// <summary>
        /// Redirect to the home page with the redirect parameter set
        /// </summary>
        private void RedirectToHomePage()
        {
            var requestedUrl = this.NavigationManager.Uri.Replace(this.NavigationManager.BaseUri, string.Empty);
            var targetUrl = QueryHelpers.AddQueryString("/", "redirect", requestedUrl);
            this.NavigationManager.NavigateTo(targetUrl);
        }

        /// <summary>
        /// Handles the OnNavigate event. Will revoke the authorization based on registered applications
        /// </summary>
        /// <param name="navigationContext">The <see cref="NavigationContext" /></param>
        private void OnNavigate(NavigationContext navigationContext)
        {
            switch (navigationContext.Path)
            {
                case "":
                case "/":
                case "/Logout":
                    break;
                default:
                    if (this.RegistrationService.RegisteredApplications.All(x => !navigationContext.Path.StartsWith(x.Url)))
                    {
                        this.NavigationManager.NavigateTo("/");
                    }

                    break;
            }
        }
    }
}
