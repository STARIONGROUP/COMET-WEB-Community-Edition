// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ILoginViewModel.cs" company="Starion Group S.A.">
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

    using FluentResults;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// View Model that enables the user to login against a COMET Server
    /// </summary>
    public interface ILoginViewModel
    {
        /// <summary>
        /// Gets the <see cref="IConfiguration" />
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Gets or sets the loading state
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Gets or sets the <see cref="Result" /> of an authentication
        /// </summary>
        Result AuthenticationResult { get; }

        /// <summary>
        /// The <see cref="AuthenticationDto" /> used for perfoming a login
        /// </summary>
        AuthenticationDto AuthenticationDto { get; }

        /// <summary>
        /// Gets the <see cref="ServerConfiguration"/> value from application settings
        /// </summary>
        ServerConfiguration ServerConfiguration { get; }

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ExecuteLogin();
    }
}
