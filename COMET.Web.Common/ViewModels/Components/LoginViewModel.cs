// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LoginViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Enumerations;
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
        /// Backing field for <see cref="AuthenticationDto" />
        /// </summary>
        private AuthenticationDto authenticationDto;

        /// <summary>
        /// Backing field for <see cref="AuthenticationResult" />
        /// </summary>
        private Result authenticationResult = new();

        /// <summary>
        /// Backing field for <see cref="AuthenticationSchemeResponseResult" />
        /// </summary>
        private Result<AuthenticationSchemeResponse> authenticationSchemeResponseResult;

        /// <summary>
        /// Backing field for <see cref="isLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel" /> class.
        /// </summary>
        /// <param name="authenticationService">The <see cref="IAuthenticationService" /></param>
        /// <param name="serverConnectionService">The <see cref="IConfigurationService" /></param>
        public LoginViewModel(IAuthenticationService authenticationService, IConfigurationService serverConnectionService)
        {
            this.ServerConnectionService = serverConnectionService;
            this.authenticationService = authenticationService;
            this.ResetAuthenticationDto();
        }

        /// <summary>
        /// Gets the <see cref="Result{TValue}" /> for a <see cref="AuthenticationSchemeResponse" /> that provides supported scheme by a
        /// </summary>
        public Result<AuthenticationSchemeResponse> AuthenticationSchemeResponseResult
        {
            get => this.authenticationSchemeResponseResult;
            set => this.RaiseAndSetIfChanged(ref this.authenticationSchemeResponseResult, value);
        }

        /// <summary>
        /// Gets the <see cref="IConfigurationService" />
        /// </summary>
        public IConfigurationService ServerConnectionService { get; }

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
        public AuthenticationDto AuthenticationDto
        {
            get => this.authenticationDto;
            private set => this.RaiseAndSetIfChanged(ref this.authenticationDto, value);
        }

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExecuteLogin()
        {
            this.IsLoading = true;

            if (!string.IsNullOrEmpty(this.ServerConnectionService.ServerConfiguration.ServerAddress))
            {
                this.AuthenticationDto.SourceAddress = this.ServerConnectionService.ServerConfiguration.ServerAddress;
            }

            this.AuthenticationResult = await this.authenticationService.Login(this.AuthenticationDto);

            if (this.AuthenticationResult.IsSuccess)
            {
                this.ResetAuthenticationDto();
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <param name="authenticationSchemeKind">The <see cref="AuthenticationSchemeKind" /> that should be used</param>
        /// <param name="authenticationInformation">The <see cref="AuthenticationInformation" /> that should be used for authentication</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExecuteLoginAsync(AuthenticationSchemeKind authenticationSchemeKind, AuthenticationInformation authenticationInformation)
        {
            this.IsLoading = true;
            this.AuthenticationResult = await this.authenticationService.LoginAsync(authenticationSchemeKind, authenticationInformation);
            this.IsLoading = false;
        }

        /// <summary>
        /// Request supported <see cref="AuthenticationSchemeKind" /> by the server that we want to reach
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        public async Task RequestAvailableAuthenticationSchemeAsync()
        {
            this.IsLoading = true;

            if (!string.IsNullOrEmpty(this.ServerConnectionService.ServerConfiguration.ServerAddress))
            {
                this.AuthenticationDto.SourceAddress = this.ServerConnectionService.ServerConfiguration.ServerAddress;
            }

            this.AuthenticationSchemeResponseResult = await this.authenticationService.RequestAvailableAuthenticationSchemeAsync(this.AuthenticationDto.SourceAddress, this.AuthenticationDto.FullTrust);

            this.IsLoading = false;
        }
        
        /// <summary>
        /// Resets the <see cref="AuthenticationDto" /> property to a new object with the default parameters
        /// </summary>
        private void ResetAuthenticationDto()
        {
            this.AuthenticationDto = new AuthenticationDto
            {
                FullTrust = this.ServerConnectionService.ServerConfiguration?.FullTrustConfiguration?.IsTrusted == FullTrustTrustedKind.FullTrust
            };
        }
    }
}
