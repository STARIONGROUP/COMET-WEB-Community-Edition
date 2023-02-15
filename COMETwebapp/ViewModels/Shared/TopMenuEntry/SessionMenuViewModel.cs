// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionMenuViewModel.cs" company="RHEA System S.A.">
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
    using System.Reactive.Linq;

    using CDP4Dal;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities.DisposableObject;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the menu entry related to the <see cref="ISession" />
    /// </summary>
    public class SessionMenuViewModel : DisposableObject, ISessionMenuViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsRefreshing" />
        /// </summary>
        private bool isRefreshing;

        /// <summary>
        /// Initializes a <see cref="SessionMenuViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionMenuViewModel" /></param>
        /// <param name="autoRefreshService">The <see cref="IAutoRefreshService" /></param>
        /// <param name="authenticationService">The <see cref="IAuthenticationService" /></param>
        /// <param name="subscriptionService">The <see cref="ISubscriptionService" /></param>
        public SessionMenuViewModel(ISessionService sessionService, IAutoRefreshService autoRefreshService, IAuthenticationService authenticationService
            , ISubscriptionService subscriptionService)
        {
            this.SessionService = sessionService;
            this.AutoRefreshService = autoRefreshService;
            this.AuthenticationService = authenticationService;
            this.SubscriptionService = subscriptionService;

            this.Disposables.Add(CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.Refreshing)
                .Subscribe(_ => { this.IsRefreshing = true; }));

            this.Disposables.Add(CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.UpToDate)
                .Subscribe(_ => { this.IsRefreshing = false; }));
        }

        /// <summary>
        /// The <see cref="ISubscriptionService" />
        /// </summary>
        public ISubscriptionService SubscriptionService { get; }

        /// <summary>
        /// Value indiciating that the <see cref="ISession" /> is currently refreshing
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.RaiseAndSetIfChanged(ref this.isRefreshing, value);
        }

        /// <summary>
        /// Refreshes the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task RefreshSession()
        {
            return this.SessionService.RefreshSession();
        }

        /// <summary>
        /// Gets the <see cref="AuthenticationService" />
        /// </summary>
        public IAuthenticationService AuthenticationService { get; }

        /// <summary>
        /// Gets the <see cref="IAutoRefreshService" />
        /// </summary>
        public IAutoRefreshService AutoRefreshService { get; }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }
    }
}
