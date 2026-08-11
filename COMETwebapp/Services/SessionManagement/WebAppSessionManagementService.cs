// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="WebAppSessionManagementService.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Services.SessionManagement
{
    using Blazored.SessionStorage;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// Service responsible for managing COMETwebapp-specific session concerns, such as cleaning up stored web app state on
    /// logout
    /// </summary>
    public class WebAppSessionManagementService : IWebAppSessionManagementService
    {
        /// <summary>
        /// Gets the injected <see cref="AuthenticationStateProvider" />
        /// </summary>
        private readonly AuthenticationStateProvider authenticationStateProvider;

        /// <summary>
        /// Gets the injected <see cref="ISessionStorageService" />
        /// </summary>
        private readonly ISessionStorageService sessionStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAppSessionManagementService" /> class.
        /// </summary>
        /// <param name="authenticationStateProvider">The injected <see cref="AuthenticationStateProvider" /></param>
        /// <param name="sessionStorageService">The injected <see cref="ISessionStorageService" /></param>
        public WebAppSessionManagementService(AuthenticationStateProvider authenticationStateProvider, ISessionStorageService sessionStorageService)
        {
            this.authenticationStateProvider = authenticationStateProvider;
            this.sessionStorageService = sessionStorageService;
            this.authenticationStateProvider.AuthenticationStateChanged += this.OnAuthenticationStateChanged;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            this.authenticationStateProvider.AuthenticationStateChanged -= this.OnAuthenticationStateChanged;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Handles the authentication state changed event
        /// </summary>
        /// <param name="task">The <see cref="Task{AuthenticationState}" /></param>
        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            Task.Run(async () =>
            {
                var authenticationState = await task;

                // If the user logs out, clear the saved tabs from session storage to prevent them from being restored on the next login
                if (authenticationState?.User.Identity is not { IsAuthenticated: true })
                {
                    await this.sessionStorageService.RemoveItemAsync(WebAppConstantValues.SavedTabsKey);
                }
            });
        }
    }
}
