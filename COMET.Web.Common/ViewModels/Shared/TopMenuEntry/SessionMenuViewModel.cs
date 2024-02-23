// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionMenuViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Dal;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

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
        /// <param name="notificationService">The <see cref="INotificationService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public SessionMenuViewModel(ISessionService sessionService, IAutoRefreshService autoRefreshService, INotificationService notificationService, ICDPMessageBus messageBus)
        {
            this.SessionService = sessionService;
            this.AutoRefreshService = autoRefreshService;
            this.NotificationService = notificationService;

            this.Disposables.Add(messageBus.Listen<SessionStateKind>()
                .Subscribe(this.HandleSessionStateKind));
        }

        /// <summary>
        /// The <see cref="INotificationService" />
        /// </summary>
        public INotificationService NotificationService { get; }

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
        /// Gets the <see cref="IAutoRefreshService" />
        /// </summary>
        public IAutoRefreshService AutoRefreshService { get; }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }

        /// <summary>
        /// Handles the change of <see cref="SessionStateKind" />
        /// </summary>
        /// <param name="sessionState">The new <see cref="SessionStateKind" /></param>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="SessionStateKind" /> is unknowned</exception>
        private void HandleSessionStateKind(SessionStateKind sessionState)
        {
            switch (sessionState)
            {
                case SessionStateKind.Refreshing:
                    this.IsRefreshing = true;
                    return;
                case SessionStateKind.RefreshEnded:
                    this.IsRefreshing = false;
                    return;
                case SessionStateKind.IterationClosed:
                case SessionStateKind.IterationOpened:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sessionState), $"Unknowned SessionStateKind {sessionState}");
            }
        }
    }
}
