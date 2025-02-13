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
    using System.Text.Json;

    using Blazored.SessionStorage;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using CDP4Web.Extensions;

    using COMET.Web.Common.Model.DTO;

    using FluentResults;

    using Microsoft.AspNetCore.Components.Authorization;

    using JsonSerializer = System.Text.Json.JsonSerializer;

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
        private const string AccessTokenKey ="access_token";
        
        /// <summary>
        /// Gets the name of the key of the refresh token value that is store within the session storage
        /// </summary>
        private const string RefreshTokenKey ="refresh_token";
        
        /// <summary>
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </summary>
        private readonly AuthenticationStateProvider authStateProvider;

        /// <summary>
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The injected <see cref="ISessionStorageService"/> that allows interaction with the browser session storage
        /// </summary>
        private readonly ISessionStorageService sessionStorageService;

        /// <summary>
        /// Gets the <see cref="System.Text.Json.JsonSerializerOptions"/>
        /// </summary>
        private static readonly JsonSerializerOptions JsonSerializerOptions = new ()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        /// <param name="sessionService">
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </param>
        /// <param name="authenticationStateProvider">
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </param>
        /// <param name="sessionStorageService">The injected <see cref="ISessionStorageService"/> that allows interaction with the browser session storage</param>
        public AuthenticationService(ISessionService sessionService, AuthenticationStateProvider authenticationStateProvider, ISessionStorageService sessionStorageService)
        {
            this.authStateProvider = authenticationStateProvider;
            this.sessionService = sessionService;
            this.sessionStorageService = sessionStorageService;
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
        /// Provides authentication capability against a pre-initialized <see cref="ISession"/> and open it in case of success
        /// </summary>
        /// <param name="authenticationSchemeKind">The <see cref="AuthenticationSchemeKind"/> that has been selected</param>
        /// <param name="authenticationInformation">The <see cref="AuthenticationInformation"/> that contains required information that should be used for authentication</param>
        /// <returns>An awaitable <see cref="Task"/> that contains the <see cref="Result"/> of the operation</returns>
        public async Task<Result> Login(AuthenticationSchemeKind authenticationSchemeKind, AuthenticationInformation authenticationInformation)
        {
            var authenticationResult = await this.sessionService.AuthenticateAndOpenSession(authenticationSchemeKind, authenticationInformation);

            if (authenticationResult.IsSuccess)
            {
                ((CometWebAuthStateProvider)this.authStateProvider).NotifyAuthenticationStateChanged();

                if (authenticationSchemeKind is AuthenticationSchemeKind.LocalJwtBearer or AuthenticationSchemeKind.ExternalJwtBearer)
                {
                    await this.sessionStorageService.SetItemAsync(AccessTokenKey, authenticationInformation.Token);
                }
            }
            
            return authenticationResult;
        }
        
        /// <summary>
        /// Request available <see cref="AuthenticationSchemeKind" /> that are supported by a server
        /// </summary>
        /// <param name="serverUrl">The url of the server to target</param>
        /// <param name="fullTrust">A value indicating whether the connection shall be fully trusted or not (in case of HttpClient connections this includes trusing self signed SSL certificates)</param>
        /// <returns>An awaitable <see cref="Task{TResult}"/> that contains the <see cref="Result{TResult}"/> of the operation, with the returned <see cref="AuthenticationSchemeResponse"/>
        /// in case of success</returns>
        public async Task<Result<AuthenticationSchemeResponse>> RequestAvailableAuthenticationScheme(string serverUrl, bool fullTrust = false)
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
                await this.sessionStorageService.SetItemAsync(ServerUrlKey, serverUrl);
            }
            
            return result;
        }

        /// <summary>
        /// Retrieves the last used server url
        /// </summary>
        /// <returns>An awaitable <see cref="Task{TResult}"/> with the retrieved server url</returns>
        public async Task<string> RetrieveLastUsedServerUrl()
        {
            return await this.sessionStorageService.GetItemAsync<string>(ServerUrlKey);
        }

        /// <summary>
        /// Tries to restore the last authenticated session, if applicable
        /// </summary>
        public async Task TryRestoreLastSession()
        {
            var serverUrl = await this.RetrieveLastUsedServerUrl();

            if (string.IsNullOrEmpty(serverUrl))
            {
                return;
            }
            
            var authenticationSchemeResponse = await this.RequestAvailableAuthenticationScheme(serverUrl);

            if (authenticationSchemeResponse.IsFailed 
                || !authenticationSchemeResponse.Value.Schemes.Intersect([AuthenticationSchemeKind.ExternalJwtBearer, AuthenticationSchemeKind.LocalJwtBearer]).Any())
            {
                await this.CleanupStorage();
                return;
            }
            
            var previousToken = await this.sessionStorageService.GetItemAsync<string>(AccessTokenKey);

            if (string.IsNullOrEmpty(previousToken))
            {
                await this.CleanupStorage();
                return;
            }
            
            var authenticationSchemeToBeUsed = authenticationSchemeResponse.Value.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer) 
                ? AuthenticationSchemeKind.ExternalJwtBearer 
                : AuthenticationSchemeKind.LocalJwtBearer;
            
            var result = await this.Login(authenticationSchemeToBeUsed, new AuthenticationInformation(previousToken));

            if (result.IsFailed)
            {
                await this.CleanupStorage();
            }
        }

        /// <summary>
        /// Exchange an OpenId connect to retrieve the generated JWT token
        /// </summary>
        /// <param name="code">The code provided by the issuer</param>
        /// <param name="authenticationSchemeResponse">The <see cref="AuthenticationSchemeResponse"/> retrieved from the CDP4-COMET server</param>
        /// <param name="redirectUrl">The redirect url</param>
        /// <param name="clientSecret">An optional client secret</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task ExchangeOpenIdConnectCode(string code, AuthenticationSchemeResponse authenticationSchemeResponse, string redirectUrl, string clientSecret = null)
        {
            if (!authenticationSchemeResponse.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer))
            {
                throw new InvalidOperationException("Supported scheme should at least contains ExternalJwtBearer");
            }

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(authenticationSchemeResponse.Authority);
            
            var parameters =  new List<KeyValuePair<string, string>>
            {
                new ("code", code),
                new ("client_id", authenticationSchemeResponse.ClientId),
                new ("redirect_uri", redirectUrl.TrimEnd('/')),
                new ("grant_type", "authorization_code"),
            };

            if (!string.IsNullOrEmpty(clientSecret))
            {
                parameters.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            }

            var httpMessage = new HttpRequestMessage(HttpMethod.Post, $"{authenticationSchemeResponse.Authority.TrimEnd('/')}/protocol/openid-connect/token");
            httpMessage.Content = new FormUrlEncodedContent(parameters);
            using var httpResponse = await httpClient.SendAsync(httpMessage);

            if (httpResponse.StatusCode.IsSuccess())
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                var openIdAuthentication = JsonSerializer.Deserialize<OpenIdAuthenticationDto>(content, JsonSerializerOptions);
                var result = await this.Login(AuthenticationSchemeKind.ExternalJwtBearer, new AuthenticationInformation(openIdAuthentication.AccessToken));

                if (result.IsSuccess)
                {
                    await this.sessionStorageService.SetItemAsync(RefreshTokenKey, openIdAuthentication.RefreshToken);
                }
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
            
            await this.CleanupStorage();
        }

        /// <summary>
        /// Cleans all values that could be present inside the Session Storage
        /// </summary>
        private async Task CleanupStorage()
        {
            await this.sessionStorageService.SetItemAsync(AccessTokenKey, string.Empty);
            await this.sessionStorageService.SetItemAsync(ServerUrlKey, string.Empty);
            await this.sessionStorageService.SetItemAsync(RefreshTokenKey, string.Empty);
        }
    }
}
