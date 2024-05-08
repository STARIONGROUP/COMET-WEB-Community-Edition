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

    using CDP4Dal;
    using CDP4Dal.DAL;

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
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </summary>
        private readonly AuthenticationStateProvider authStateProvider;

        /// <summary>
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        /// <param name="sessionService">
        /// The (injected) <see cref="ISessionService" /> that provides access to the <see cref="ISession" />
        /// </param>
        /// <param name="authenticationStateProvider">
        /// The (injected) <see cref="AuthenticationStateProvider" />
        /// </param>
        public AuthenticationService(ISessionService sessionService, AuthenticationStateProvider authenticationStateProvider)
        {
            this.authStateProvider = authenticationStateProvider;
            this.sessionService = sessionService;
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
        }
    }
}
