// --------------------------------------------------------------------------------------------------------------------
// <copyright file="COMETAuthStateProvider.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.SessionManagement
{
    using System.Security.Claims;

    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components.Authorization;
    
    /// <summary>
    /// Provides information about the authentication state of the current user.
    /// </summary>
    public class CometWebAuthStateProvider : AuthenticationStateProvider
    {
        /// <summary>
        /// The <see cref="ISessionAnchor"/> used to get access to the <see cref="ISession"/>
        /// </summary>
        private readonly ISessionAnchor sessionAnchor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CometWebAuthStateProvider"/>
        /// </summary>
        /// <param name="sessionAnchor">
        /// The (injected) <see cref="ISessionAnchor"/> used to get access to the <see cref="ISession"/>
        /// </param>
        public CometWebAuthStateProvider(ISessionAnchor sessionAnchor)
        {
            this.sessionAnchor = sessionAnchor;
        }

        /// <summary>
        /// Asynchronously gets an <see cref="AuthenticationState"/> that describes the current user.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that, when resolved, gives an <see cref="AuthenticationState"/> instance that describes the current user.
        /// </returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity;
            if (!this.sessionAnchor.IsSessionOpen)
            {
                identity = new ClaimsIdentity();
            }
            else
            {
                var person = this.sessionAnchor.Session.ActivePerson;
                identity = CreateClaimsIdentity(person);
            }

            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity"/> based on a <see cref="Person"/>
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person"/> on the basis of which the <see cref="ClaimsIdentity"/> is created
        /// </param>
        /// <returns>
        /// an instance of <see cref="ClaimsIdentity"/>
        /// </returns>
        /// <remarks>
        /// When the <paramref name="person"/> is null an anonymous <see cref="ClaimsIdentity"/> is returned
        /// </remarks>
        private static ClaimsIdentity CreateClaimsIdentity(Person person)
        {
            ClaimsIdentity identity;

            if (person.Name == null)
            {
                identity = new ClaimsIdentity();
            } else
            {
                var claim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, person.Name)
                };

                identity = new ClaimsIdentity(claim, "10-25 Authenticated");
            }

            return identity;
        }

        /// <summary>
        /// Force the <see cref="NotifyAuthenticationStateChanged"/> event to be raised
        /// </summary>
        public void NotifyAuthenticationStateChanged()
        {
            this.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
