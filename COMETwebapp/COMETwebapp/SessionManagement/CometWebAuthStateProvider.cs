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
    using CDP4Common.SiteDirectoryData;
    using Microsoft.AspNetCore.Components.Authorization;
    using System.Security.Claims;
    public class CometWebAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionAnchor sessionAnchor;

        public CometWebAuthStateProvider(ISessionAnchor sessionAnchor)
        {
            this.sessionAnchor = sessionAnchor;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            ClaimsIdentity identity;
            if (this.sessionAnchor == null || this.sessionAnchor.Session == null)
            {
                identity = new ClaimsIdentity();
            } else
            {
                var person = this.sessionAnchor.Session.ActivePerson;
                identity = this.CreateClaimsIdentity(person);
            }
            

            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        private ClaimsIdentity CreateClaimsIdentity(Person person)
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
        public void NotifyAuthenticationStateChanged()
        {
            this.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
