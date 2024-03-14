// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Services.SessionManagement
{
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Exceptions;

    using CDP4ServicesDal;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model.DTO;

    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// The purpose of the <see cref="AuthenticationService" /> is to authenticate against
    /// a E-TM-10-25 Annex C.2 data source
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </summary>
        private readonly AuthenticationStateProvider authStateProvider;

        /// <summary>
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The <see cref="HttpClient" /> used to retrieve data
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        /// <param name="sessionService">
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </param>
        /// <param name="authenticationStateProvider">
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="httpClient">The <see cref="HttpClient"/></param>
        public AuthenticationService(ISessionService sessionService, AuthenticationStateProvider authenticationStateProvider, ICDPMessageBus messageBus, HttpClient httpClient)
        {
            this.messageBus = messageBus;
            this.authStateProvider = authenticationStateProvider;
            this.sessionService = sessionService;
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Login (authenticate) with authentication information to a data source
        /// </summary>
        /// <param name="authenticationDto">
        /// The authentication information with data source, username and password
        /// </param>
        /// <returns>
        /// <see cref="AuthenticationStateKind.Success" /> when the authentication is done and the ISession opened
        /// </returns>
        public async Task<AuthenticationStateKind> Login(AuthenticationDto authenticationDto)
        {
            if (authenticationDto.SourceAddress != null)
            {
                var uri = new Uri(authenticationDto.SourceAddress);
                var dal = new CdpServicesDal(this.httpClient);
                var credentials = new Credentials(authenticationDto.UserName, authenticationDto.Password, uri);

                this.sessionService.Session = new Session(dal, credentials, this.messageBus);
            }
            else
            {
                return AuthenticationStateKind.Fail;
            }

            try
            {
                await this.sessionService.Session.Open();
                this.sessionService.IsSessionOpen = this.sessionService.GetSiteDirectory() != null;
                ((CometWebAuthStateProvider)this.authStateProvider).NotifyAuthenticationStateChanged();

                return this.sessionService.IsSessionOpen ? AuthenticationStateKind.Success : AuthenticationStateKind.Fail;
            }
            catch (DalReadException)
            {
                this.sessionService.IsSessionOpen = false;
                return AuthenticationStateKind.Fail;
            }
            catch (HttpRequestException)
            {
                this.sessionService.IsSessionOpen = false;
                return AuthenticationStateKind.ServerFail;
            }
        }

        /// <summary>
        /// Logout from the data source
        /// </summary>
        /// <returns>
        /// a <see cref="Task" />
        /// </returns>
        public async Task Logout()
        {
            if (this.sessionService.Session != null)
            {
                await this.sessionService.Close();
            }

            ((CometWebAuthStateProvider)this.authStateProvider).NotifyAuthenticationStateChanged();
        }
    }
}
