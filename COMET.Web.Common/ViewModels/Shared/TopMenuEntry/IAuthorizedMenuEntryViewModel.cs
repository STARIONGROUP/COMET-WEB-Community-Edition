// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IAuthorizedMenuEntryViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry
{
    using COMET.Web.Common.Utilities.DisposableObject;

    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// View model that enables the authorization state into the top menu
    /// </summary>>
    public interface IAuthorizedMenuEntryViewModel : IDisposableObject
    {
        /// <summary>
        /// Value asserting that the user is authenticated or not
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// The user name
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// The current <see cref="AuthenticationState" />
        /// </summary>
        AuthenticationState CurrentState { get; set; }
    }
}
