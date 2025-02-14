// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenIdAuthenticationDto.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2025 Starion Group S.A.
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

namespace COMET.Web.Common.Model.DTO
{
    /// <summary>
    /// The <see cref="OpenIdAuthenticationDto" /> provides data model structure for response of a successfull openid authentication result
    /// </summary>
    public class OpenIdAuthenticationDto
    {
        /// <summary>
        /// The generated access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The generated refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The expires time of the <see cref="AccessToken" />, in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// The expires time of the <see cref="RefreshToken" />, in seconds
        /// </summary>
        public int RefreshExpiresIn { get; set; }
    }
}
