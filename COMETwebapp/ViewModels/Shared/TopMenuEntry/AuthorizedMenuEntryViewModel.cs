// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthorizedMenuEntryViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Shared.TopMenuEntry
{
    using COMETwebapp.Utilities.DisposableObject;

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
