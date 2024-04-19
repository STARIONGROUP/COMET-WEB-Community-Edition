// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LoginViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMET.Web.Common.ViewModels.Components
{
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;

    using FluentResults;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables the user to login against a COMET Server
    /// </summary>
    public class LoginViewModel : ReactiveObject, ILoginViewModel
    {
        /// <summary>
        /// The <see cref="IAuthenticationService" />
        /// </summary>
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// Gets the <see cref="IConfigurationService" />
        /// </summary>
        public IConfigurationService serverConnectionService { get; }

        /// <summary>
        /// Backing field for <see cref="isLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Backing field for <see cref="AuthenticationResult" />
        /// </summary>
        private Result authenticationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel" /> class.
        /// </summary>
        /// <param name="authenticationService">The <see cref="IAuthenticationService" /></param>
        /// <param name="serverConnectionService">The <see cref="IConfigurationService"/></param>
        public LoginViewModel(IAuthenticationService authenticationService, IConfigurationService serverConnectionService)
        {
            this.serverConnectionService = serverConnectionService;
            this.authenticationService = authenticationService;
        }

        /// <summary>
        /// Gets or sets the loading state
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Result" /> of an authentication
        /// </summary>
        public Result AuthenticationResult
        {
            get => this.authenticationResult;
            set => this.RaiseAndSetIfChanged(ref this.authenticationResult, value);
        }

        /// <summary>
        /// The <see cref="AuthenticationDto" /> used for perfoming a login
        /// </summary>
        public AuthenticationDto AuthenticationDto { get; private set; } = new();

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExecuteLogin()
        {
            this.IsLoading = true;

            if(!string.IsNullOrEmpty(this.serverConnectionService.ServerConfiguration.ServerAddress))
            {
                this.AuthenticationDto.SourceAddress = this.serverConnectionService.ServerConfiguration.ServerAddress;
            }

            this.AuthenticationResult = await this.authenticationService.Login(this.AuthenticationDto);

            if (this.AuthenticationResult.IsSuccess)
            {
                this.AuthenticationDto = new AuthenticationDto();
            }

            this.IsLoading = false;
        }
    }
}
