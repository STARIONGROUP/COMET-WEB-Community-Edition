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
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;

    using FluentResults;

    using Microsoft.Extensions.Configuration;

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
        /// Backing field for <see cref="AuthenticationResult" />
        /// </summary>
        private Result authenticationResult = new();

        /// <summary>
        /// Backing field for <see cref="isLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel" /> class.
        /// </summary>
        /// <param name="authenticationService">The <see cref="IAuthenticationService" /></param>
        /// <param name="configuration">The <see cref="IConfiguration" /></param>
        public LoginViewModel(IAuthenticationService authenticationService, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.authenticationService = authenticationService;
        }

        /// <summary>
        /// Gets the <see cref="IConfiguration" />
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the <see cref="ServerConfiguration"/> value from application settings
        /// </summary>
        public ServerConfiguration ServerConfiguration => this.Configuration.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>();

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

            if (!string.IsNullOrEmpty(this.ServerConfiguration.ServerAddress))
            {
                this.AuthenticationDto.SourceAddress = this.ServerConfiguration.ServerAddress;
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
