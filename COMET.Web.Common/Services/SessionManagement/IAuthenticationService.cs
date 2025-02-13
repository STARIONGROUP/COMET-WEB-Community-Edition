// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IAuthenticationService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Model.DTO;

    using FluentResults;

    /// <summary>
    /// The purpose of the <see cref="IAuthenticationService" /> is to authenticate against
    /// a E-TM-10-25 Annex C.2 data source
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Login (authenticate) with authentication information to a data source
        /// </summary>
        /// <param name="authenticationDto">
        /// The authentication information with data source, username and password
        /// </param>
        /// <returns>
        /// The <see cref="Result"/> of the request
        /// </returns>
        Task<Result> Login(AuthenticationDto authenticationDto);

        /// <summary>
        /// Provides authentication capability against a pre-initialized <see cref="ISession"/> and open it in case of success
        /// </summary>
        /// <param name="authenticationSchemeKind">The <see cref="AuthenticationSchemeKind"/> that has been selected</param>
        /// <param name="authenticationInformation">The <see cref="AuthenticationInformation"/> that contains required information that should be used for authentication</param>
        /// <returns>An awaitable <see cref="Task"/> that contains the <see cref="Result"/> of the operation</returns>
        Task<Result> Login(AuthenticationSchemeKind authenticationSchemeKind, AuthenticationInformation authenticationInformation);
        
        /// <summary>
        /// Logout from the data source
        /// </summary>
        /// <returns>
        /// a <see cref="Task" />
        /// </returns>
        Task Logout();

        /// <summary>
        /// Request available <see cref="AuthenticationSchemeKind" /> that are supported by a server
        /// </summary>
        /// <param name="serverUrl">The url of the server to target</param>
        /// <param name="fullTrust">A value indicating whether the connection shall be fully trusted or not (in case of HttpClient connections this includes trusing self signed SSL certificates)</param>
        /// <returns>An awaitable <see cref="Task{TResult}"/> that contains the <see cref="Result{TResult}"/> of the operation, with the returned <see cref="AuthenticationSchemeResponse"/>
        /// in case of success</returns>
        Task<Result<AuthenticationSchemeResponse>> RequestAvailableAuthenticationScheme(string serverUrl, bool fullTrust = false);

        /// <summary>
        /// Retrieves the last used server url
        /// </summary>
        /// <returns>An awaitable <see cref="Task{TResult}"/> with the retrieved server url</returns>
        Task<string> RetrieveLastUsedServerUrl();

        /// <summary>
        /// Exchange an OpenId connect to retrieve the generated JWT token
        /// </summary>
        /// <param name="code">The code provided by the issuer</param>
        /// <param name="authenticationSchemeResponse">The <see cref="AuthenticationSchemeResponse"/> retrieved from the CDP4-COMET server</param>
        /// <param name="redirectUrl">The redirect url</param>
        /// <param name="clientSecret">An optional client secret</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task ExchangeOpenIdConnectCode(string code, AuthenticationSchemeResponse authenticationSchemeResponse, string redirectUrl, string clientSecret = null);

        /// <summary>
        /// Tries to restore the last authenticated session, if applicable
        /// </summary>
        Task TryRestoreLastSession();
    }
}
