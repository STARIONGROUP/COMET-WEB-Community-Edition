// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ILoginViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Components
{
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.ServerConnectionService;

    /// <summary>
    /// View Model that enables the user to login against a COMET Server
    /// </summary>
    public interface ILoginViewModel
    {
        /// <summary>
        /// Gets the <see cref="IServerConnectionService" />
        /// </summary>
        IServerConnectionService serverConnectionService { get; }

        /// <summary>
        /// Gets or sets the <see cref="AuthenticationStateKind" />
        /// </summary>
        AuthenticationStateKind AuthenticationState { get; set; }

        /// <summary>
        /// The <see cref="AuthenticationDto" /> used for perfoming a login
        /// </summary>
        AuthenticationDto AuthenticationDto { get; }

        /// <summary>
        /// Attempt to login to a COMET Server
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ExecuteLogin();
    }
}
