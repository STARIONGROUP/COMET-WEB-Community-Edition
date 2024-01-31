// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthorizedMenuEntryViewModel.cs" company="RHEA System S.A.">
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

    using ReactiveUI;

    /// <summary>
    /// View model that enables the authorization state into the top menu
    /// </summary>
    public class AuthorizedMenuEntryViewModel : DisposableObject, IAuthorizedMenuEntryViewModel
    {
        /// <summary>
        /// The <see cref="AuthenticationStateProvider" />
        /// </summary>
        private readonly AuthenticationStateProvider stateProvider;

        /// <summary>
        /// Backing field for <see cref="CurrentState" />
        /// </summary>
        private AuthenticationState currentState;

        /// <summary>
        /// Initializes a new <see cref="AuthorizedMenuEntryViewModel" />
        /// </summary>
        /// <param name="stateProvider">The <see cref="AuthenticationStateProvider" /></param>
        public AuthorizedMenuEntryViewModel(AuthenticationStateProvider stateProvider)
        {
            this.stateProvider = stateProvider;
            this.stateProvider.AuthenticationStateChanged += this.HandleAuthenticationState;
            this.HandleAuthenticationState(this.stateProvider.GetAuthenticationStateAsync());
        }

        /// <summary>
        /// The current <see cref="AuthenticationState" />
        /// </summary>
        public AuthenticationState CurrentState
        {
            get => this.currentState;
            set => this.RaiseAndSetIfChanged(ref this.currentState, value);
        }

        /// <summary>
        /// Value asserting that the user is authenticated or not
        /// </summary>
        public bool IsAuthenticated => this.CurrentState?.User.Identity is { IsAuthenticated: true };

        /// <summary>
        /// The user name
        /// </summary>
        public string UserName => this.CurrentState?.User.Identity?.Name;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.stateProvider.AuthenticationStateChanged -= this.HandleAuthenticationState;
        }

        /// <summary>
        /// Handle the change of <see cref="AuthenticationState" />
        /// </summary>
        /// <param name="state">A <see cref="Task" /></param>
        private void HandleAuthenticationState(Task<AuthenticationState> state)
        {
            this.CurrentState = state.Result;
        }
    }
}
