// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometWebAuthStateProvider.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Services.SessionManagement
{
    using System.Security.Claims;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// Provides information about the authentication state of the current user.
    /// </summary>
    public class CometWebAuthStateProvider : AuthenticationStateProvider
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to get access to the <see cref="ISession" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CometWebAuthStateProvider" />
        /// </summary>
        /// <param name="sessionService">
        /// The (injected) <see cref="ISessionService" /> used to get access to the <see cref="ISession" />
        /// </param>
        public CometWebAuthStateProvider(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Asynchronously gets an <see cref="AuthenticationState" /> that describes the current user.
        /// </summary>
        /// <returns>
        /// A <see cref="Task" /> that, when resolved, gives an <see cref="AuthenticationState" /> instance that describes the current user.
        /// </returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity;

            if (!this.sessionService.IsSessionOpen)
            {
                identity = new ClaimsIdentity();
            }
            else
            {
                var person = this.sessionService.Session.ActivePerson;
                identity = CreateClaimsIdentity(person);

                if (!string.IsNullOrEmpty(person.Role.ShortName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, person.Role.ShortName));
                }
            }

            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        /// <summary>
        /// Force the <see cref="NotifyAuthenticationStateChanged" /> event to be raised
        /// </summary>
        public void NotifyAuthenticationStateChanged()
        {
            this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity" /> based on a <see cref="Person" />
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person" /> on the basis of which the <see cref="ClaimsIdentity" /> is created
        /// </param>
        /// <returns>
        /// an instance of <see cref="ClaimsIdentity" />
        /// </returns>
        /// <remarks>
        /// When the <paramref name="person" /> is null an anonymous <see cref="ClaimsIdentity" /> is returned
        /// </remarks>
        private static ClaimsIdentity CreateClaimsIdentity(Person person)
        {
            ClaimsIdentity identity;

            if (person.Name == null)
            {
                identity = new ClaimsIdentity();
            }
            else
            {
                var claim = new List<Claim>
                {
                    new(ClaimTypes.Name, person.Name)
                };

                identity = new ClaimsIdentity(claim, "10-25 Authenticated");
            }

            return identity;
        }
    }
}
