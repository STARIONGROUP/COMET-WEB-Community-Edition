﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Pages
{
    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.VersionService;

    /// <summary>
    /// View Model that handles the home page
    /// </summary>
    public class IndexViewModel : IIndexViewModel
    {
        /// <summary>
        /// The <see cref="IAuthenticationService" />
        /// </summary>
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexViewModel" /> class.
        /// </summary>
        /// <param name="versionService">The <see cref="IVersionService" /></param>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="authenticationService">The <see cref="IAuthenticationService" /></param>
        public IndexViewModel(IVersionService versionService, ISessionService sessionService, IAuthenticationService authenticationService)
        {
            this.SessionService = sessionService;
            this.authenticationService = authenticationService;
            this.Version = versionService.GetVersion();
        }

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }

        /// <summary>
        /// The version of the running application
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Close the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task Logout()
        {
            return this.authenticationService.Logout();
        }
    }
}