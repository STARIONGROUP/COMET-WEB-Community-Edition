// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionMenuViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry
{
    using CDP4Dal;

    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the menu entry related to the <see cref="ISession" />
    /// </summary>
    public class SessionMenuViewModel : DisposableObject, ISessionMenuViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsOnEditPersonMode" />.
        /// </summary>
        private bool isOnEditPersonMode;

        /// <summary>
        /// Initializes a <see cref="SessionMenuViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionMenuViewModel" /></param>
        /// <param name="autoRefreshService">The <see cref="IAutoRefreshService" /></param>
        /// <param name="notificationService">The <see cref="INotificationService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="personEditViewModel">The <see cref="IPersonEditViewModel" /> driving the self-service profile dialog.</param>
        public SessionMenuViewModel(ISessionService sessionService, IAutoRefreshService autoRefreshService, INotificationService notificationService, ICDPMessageBus messageBus, IPersonEditViewModel personEditViewModel)
        {
            this.SessionService = sessionService;
            this.AutoRefreshService = autoRefreshService;
            this.NotificationService = notificationService;
            this.PersonEditViewModel = personEditViewModel;
            this.PersonEditViewModel.OnSaved = EventCallback.Factory.Create(this, () => this.IsOnEditPersonMode = false);
        }

        /// <summary>
        /// The <see cref="INotificationService" />
        /// </summary>
        public INotificationService NotificationService { get; }

        /// <summary>
        /// Gets the <see cref="IAutoRefreshService" />
        /// </summary>
        public IAutoRefreshService AutoRefreshService { get; }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }

        /// <summary>
        /// Refreshes the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task RefreshSession()
        {
            return this.SessionService.RefreshSession();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the self-service "Edit my profile" popup is open.
        /// </summary>
        public bool IsOnEditPersonMode
        {
            get => this.isOnEditPersonMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditPersonMode, value);
        }

        /// <summary>
        /// Gets the <see cref="IPersonEditViewModel" /> driving the "Edit my profile" dialog.
        /// </summary>
        public IPersonEditViewModel PersonEditViewModel { get; }

        /// <summary>
        /// Initializes <see cref="PersonEditViewModel" /> from the active session's
        /// <see cref="CDP4Common.SiteDirectoryData.Person" /> and opens the popup. No-op when no
        /// person is active (the session is unauthenticated or the SDK has not yet populated it).
        /// </summary>
        public void OpenEditPersonPopup()
        {
            var activePerson = this.SessionService?.Session?.ActivePerson;

            if (activePerson is null)
            {
                return;
            }

            this.PersonEditViewModel.Initialize(activePerson);
            this.IsOnEditPersonMode = true;
        }
    }
}
