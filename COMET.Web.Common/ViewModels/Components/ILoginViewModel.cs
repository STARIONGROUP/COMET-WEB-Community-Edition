// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ILoginViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.ConfigurationService;

    using FluentResults;

    /// <summary>
    /// View Model that enables the user to login against a COMET Server
    /// </summary>
    public interface ILoginViewModel
    {
        /// <summary>
        /// Gets the <see cref="IConfigurationService" />
        /// </summary>
        IConfigurationService ServerConnectionService { get; }

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
        /// Gets the <see cref="Result{TValue}" /> for a <see cref="AuthenticationSchemeResponse" /> that provides supported scheme by a
        /// </summary>
        Result<AuthenticationSchemeResponse> AuthenticationSchemeResponseResult { get; set; }

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ExecuteLogin();

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <param name="authenticationSchemeKind">The <see cref="AuthenticationSchemeKind"/> that should be used</param>
        /// <param name="authenticationInformation">The <see cref="AuthenticationInformation"/> that should be used for authentication</param>
        /// <returns>A <see cref="Task" /></returns>
        Task ExecuteLogin(AuthenticationSchemeKind authenticationSchemeKind, AuthenticationInformation authenticationInformation);

        /// <summary>
        /// Request supported <see cref="AuthenticationSchemeKind"/> by the server that we want to reach
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task RequestAvailableAuthenticationScheme();
    }
}
