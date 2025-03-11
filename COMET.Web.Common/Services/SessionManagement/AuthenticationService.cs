// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationService.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Services.SessionManagement
{
    using System.Net;

    using Blazored.SessionStorage;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Utilities;

    using CDP4DalCommon.Authentication;

    using CDP4Web.Extensions;

    using COMET.Web.Common.Model.DTO;

    using FluentResults;

    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// The purpose of the <see cref="AuthenticationService" /> is to authenticate against
    /// a E-TM-10-25 Annex C.2 data source
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Gets the name of the key of the server url that is store within the session storage
        /// </summary>
        private const string ServerUrlKey = "cdp4-comet-url";

        /// <summary>
        /// Gets the name of the key of the access token value that is store within the session storage
        /// </summary>
        private const string AccessTokenKey = "access_token";

        /// <summary>
        /// Gets the name of the key of the refresh token value that is store within the session storage
        /// </summary>
        private const string RefreshTokenKey = "refresh_token";

        /// <summary>
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </summary>
        private readonly AuthenticationStateProvider authStateProvider;

        /// <summary>
        /// Gets the injected <see cref="IAuthenticationRefreshService" /> that will provide token refresh features
        /// </summary>
        private readonly IAuthenticationRefreshService automaticTokenRefreshService;

        /// <summary>
        /// Gets the injected <see cref="IProvideExternalAuthenticationService" /> used to communicate with the external authentication provider
        /// </summary>
        private readonly IProvideExternalAuthenticationService openIdConnectService;

        /// <summary>
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The injected <see cref="ISessionStorageService" /> that allows interaction with the browser session storage
        /// </summary>
        private readonly ISessionStorageService sessionStorageService;

        /// <summary>
        /// Stores the last retrieved <see cref="AuthenticationSchemeResponse" /> to allow refresh token later
        /// </summary>
        private AuthenticationSchemeResponse lastSupportedAuthenticationSchemeResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        /// <param name="sessionService">
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </param>
        /// <param name="authenticationStateProvider">
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </param>
        /// <param name="sessionStorageService">The injected <see cref="ISessionStorageService" /> that allows interaction with the browser session storage</param>
        /// <param name="openIdConnectService">The injected <see cref="IProvideExternalAuthenticationService" /> used to communicate with the external authentication provider</param>
        /// <param name="automaticTokenRefreshService">The injected <see cref="IAuthenticationRefreshService" /> that will provide token refresh features</param>
        public AuthenticationService(ISessionService sessionService, AuthenticationStateProvider authenticationStateProvider, ISessionStorageService sessionStorageService,
            IProvideExternalAuthenticationService openIdConnectService, IAuthenticationRefreshService automaticTokenRefreshService)
        {
            this.authStateProvider = authenticationStateProvider;
            this.sessionService = sessionService;
            this.sessionStorageService = sessionStorageService;
            this.openIdConnectService = openIdConnectService;
            this.automaticTokenRefreshService = automaticTokenRefreshService;

            this.automaticTokenRefreshService.AuthenticationRefreshed += this.StoreAuthenticationTokensAsync;
        }

        /// <summary>
        /// Login (authenticate) with authentication information to a data source
        /// </summary>
        /// <param name="authenticationDto">
        /// The authentication information with data source, username and password
        /// </param>
        /// <returns>
        /// The <see cref="Result" /> of the request
        /// </returns>
        public async Task<Result> Login(AuthenticationDto authenticationDto)
        {
            var result = new Result();

            if (authenticationDto.SourceAddress == null)
            {
                result.Reasons.Add(new Error("The source address should not be empty").AddReasonIdentifier(HttpStatusCode.BadRequest));
                return result;
            }

            var uri = new Uri(authenticationDto.SourceAddress);
            var credentials = new Credentials(authenticationDto.UserName, authenticationDto.Password, uri, authenticationDto.FullTrust);
            result = await this.sessionService.OpenSession(credentials);

            if (result.IsSuccess)
            {
                ((CometWebAuthStateProvider)this.authStateProvider).NotifyAuthenticationStateChanged();
            }

            return result;
        }

        /// <summary>
        /// Provides authentication capability against a pre-initialized <see cref="ISession" /> and open it in case of success
        /// </summary>
        /// <param name="authenticationSchemeKind">The <see cref="AuthenticationSchemeKind" /> that has been selected</param>
        /// <param name="authenticationInformation">The <see cref="AuthenticationInformation" /> that contains required information that should be used for authentication</param>
        /// <returns>An awaitable <see cref="Task" /> that contains the <see cref="Result" /> of the operation</returns>
        public async Task<Result> LoginAsync(AuthenticationSchemeKind authenticationSchemeKind, AuthenticationInformation authenticationInformation)
        {
            var authenticationResult = await this.sessionService.AuthenticateAndOpenSession(authenticationSchemeKind, authenticationInformation);

            if (authenticationResult.IsSuccess)
            {
                ((CometWebAuthStateProvider)this.authStateProvider).NotifyAuthenticationStateChanged();

                if (authenticationSchemeKind is AuthenticationSchemeKind.LocalJwtBearer or AuthenticationSchemeKind.ExternalJwtBearer)
                {
                    await this.StoreAuthenticationTokensAsync();

                    this.automaticTokenRefreshService.Initialize(this.sessionService.Session, this.lastSupportedAuthenticationSchemeResponse);
                    _ = this.automaticTokenRefreshService.StartAsync().ConfigureAwait(false);
                }
            }

            return authenticationResult;
        }

        /// <summary>
        /// Request available <see cref="AuthenticationSchemeKind" /> that are supported by a server
        /// </summary>
        /// <param name="serverUrl">The url of the server to target</param>
        /// <param name="fullTrust">A value indicating whether the connection shall be fully trusted or not (in case of HttpClient connections this includes trusing self signed SSL certificates)</param>
        /// <returns>
        /// An awaitable <see cref="Task{TResult}" /> that contains the <see cref="Result{TResult}" /> of the operation, with the returned
        /// <see cref="AuthenticationSchemeResponse" />
        /// in case of success
        /// </returns>
        public async Task<Result<AuthenticationSchemeResponse>> RequestAvailableAuthenticationSchemeAsync(string serverUrl, bool fullTrust = false)
        {
            if (string.IsNullOrEmpty(serverUrl))
            {
                return Result.Fail([new Error("The server url should not be empty").AddReasonIdentifier(HttpStatusCode.BadRequest)]);
            }

            var uri = new Uri(serverUrl);
            var credentials = new Credentials(uri, fullTrust);

            var result = await this.sessionService.InitializeSessionAndRequestServerSupportedAuthenticationScheme(credentials);

            if (result.IsSuccess && result.Value.Schemes.Intersect([AuthenticationSchemeKind.ExternalJwtBearer, AuthenticationSchemeKind.LocalJwtBearer]).Any())
            {
                // Required to be able to restore a session
                this.lastSupportedAuthenticationSchemeResponse = result.Value;
                await this.sessionStorageService.SetItemAsync(ServerUrlKey, serverUrl);
            }

            return result;
        }

        /// <summary>
        /// Retrieves the last used server url
        /// </summary>
        /// <returns>An awaitable <see cref="Task{TResult}" /> with the retrieved server url</returns>
        public async Task<string> RetrieveLastUsedServerUrlAsync()
        {
            return await this.sessionStorageService.GetItemAsync<string>(ServerUrlKey);
        }

        /// <summary>
        /// Tries to restore the last authenticated session, if applicable
        /// </summary>
        public async Task TryRestoreLastSessionAsync()
        {
            var serverUrl = await this.RetrieveLastUsedServerUrlAsync();

            if (string.IsNullOrEmpty(serverUrl))
            {
                return;
            }

            var authenticationSchemeResponse = await this.RequestAvailableAuthenticationSchemeAsync(serverUrl);

            if (authenticationSchemeResponse.IsFailed
                || !authenticationSchemeResponse.Value.Schemes.Intersect([AuthenticationSchemeKind.ExternalJwtBearer, AuthenticationSchemeKind.LocalJwtBearer]).Any())
            {
                await this.CleanupStorageAsync();
                return;
            }

            this.lastSupportedAuthenticationSchemeResponse = authenticationSchemeResponse.Value;

            var previousToken = await this.sessionStorageService.GetItemAsync<string>(AccessTokenKey);

            if (string.IsNullOrEmpty(previousToken))
            {
                await this.CleanupStorageAsync();
                return;
            }

            var refreshToken = await this.sessionStorageService.GetItemAsync<string>(RefreshTokenKey);
            var authenticationToken = new AuthenticationToken(previousToken, refreshToken);

            var authenticationSchemeToBeUsed = authenticationSchemeResponse.Value.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer)
                ? AuthenticationSchemeKind.ExternalJwtBearer
                : AuthenticationSchemeKind.LocalJwtBearer;

            this.sessionService.Session.Credentials.ProvideUserToken(authenticationToken, authenticationSchemeToBeUsed);

            try
            {
                this.automaticTokenRefreshService.Initialize(this.sessionService.Session, this.lastSupportedAuthenticationSchemeResponse);
                await this.automaticTokenRefreshService.RefreshAuthenticationInformationAsync();
            }
            catch
            {
                await this.CleanupStorageAsync();
                return;
            }

            var result = await this.LoginAsync(authenticationSchemeToBeUsed, new AuthenticationInformation(this.sessionService.Session.Credentials.Token));

            if (result.IsFailed)
            {
                await this.CleanupStorageAsync();
            }
        }

        /// <summary>
        /// Exchange an OpenId connect to retrieve the generated JWT token
        /// </summary>
        /// <param name="code">The code provided by the issuer</param>
        /// <param name="authenticationSchemeResponse">The <see cref="AuthenticationSchemeResponse" /> retrieved from the CDP4-COMET server</param>
        /// <param name="redirectUrl">The redirect url</param>
        /// <param name="clientSecret">An optional client secret</param>
        /// <returns>An awaitable <see cref="Task" /></returns>
        public async Task ExchangeOpenIdConnectCodeAsync(string code, AuthenticationSchemeResponse authenticationSchemeResponse, string redirectUrl, string clientSecret = null)
        {
            Guard.ThrowIfNull(authenticationSchemeResponse, nameof(authenticationSchemeResponse));
            Guard.ThrowIfNullOrEmpty(redirectUrl, nameof(redirectUrl));
            Guard.ThrowIfNullOrEmpty(code, nameof(code));

            if (!authenticationSchemeResponse.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer))
            {
                throw new InvalidOperationException("Supported scheme should at least contains ExternalJwtBearer");
            }

            try
            {
                var authenticationTokens = await this.openIdConnectService.RequestAuthenticationToken(code, authenticationSchemeResponse, redirectUrl, clientSecret);
                await this.LoginAsync(AuthenticationSchemeKind.ExternalJwtBearer, new AuthenticationInformation(authenticationTokens));
            }
            catch
            {
                await this.CleanupStorageAsync();
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
                await this.sessionService.CloseSession();
            }

            ((CometWebAuthStateProvider)this.authStateProvider).NotifyAuthenticationStateChanged();
            this.automaticTokenRefreshService.Dispose();
            await this.CleanupStorageAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.automaticTokenRefreshService.AuthenticationRefreshed -= this.StoreAuthenticationTokensAsync;
            this.automaticTokenRefreshService.Dispose();
        }

        /// <summary>
        /// Stores <see cref="AuthenticationToken" /> information into the session storage
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task StoreAuthenticationTokensAsync()
        {
            await this.sessionStorageService.SetItemAsync(AccessTokenKey, this.sessionService.Session.Credentials.Token.AccessToken);
            await this.sessionStorageService.SetItemAsync(RefreshTokenKey, this.sessionService.Session.Credentials.Token.RefreshToken);
        }

        /// <summary>
        /// Cleans all values that could be present inside the Session Storage
        /// </summary>
        private async Task CleanupStorageAsync()
        {
            await this.sessionStorageService.SetItemAsync(AccessTokenKey, string.Empty);
            await this.sessionStorageService.SetItemAsync(ServerUrlKey, string.Empty);
            await this.sessionStorageService.SetItemAsync(RefreshTokenKey, string.Empty);
        }
    }
}
