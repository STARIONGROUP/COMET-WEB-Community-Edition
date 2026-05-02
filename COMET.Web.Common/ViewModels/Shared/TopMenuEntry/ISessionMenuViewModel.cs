// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISessionMenuViewModel.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// View model that handles the menu entry related to the <see cref="ISession" />
    /// </summary>
    public interface ISessionMenuViewModel : IDisposableObject
    {
        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        ISessionService SessionService { get; }

        /// <summary>
        /// Gets the <see cref="IAutoRefreshService" />
        /// </summary>
        IAutoRefreshService AutoRefreshService { get; }

        /// <summary>
        /// The <see cref="INotificationService" />
        /// </summary>
        INotificationService NotificationService { get; }

        /// <summary>
        /// Refreshes the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task RefreshSession();

        /// <summary>
        /// Gets or sets a value indicating whether the self-service "Edit my profile" popup is open.
        /// Bound to the <see cref="DevExpress.Blazor.DxPopup" /> visibility from
        /// <see cref="Shared.TopMenuEntry.SessionMenu" />.
        /// </summary>
        bool IsOnEditPersonMode { get; set; }

        /// <summary>
        /// Gets the <see cref="IPersonEditViewModel" /> driving the self-service "Edit my profile"
        /// dialog. Initialized from the active session's <see cref="CDP4Common.SiteDirectoryData.Person" />
        /// every time <see cref="OpenEditPersonPopup" /> is invoked.
        /// </summary>
        IPersonEditViewModel PersonEditViewModel { get; }

        /// <summary>
        /// Initializes <see cref="PersonEditViewModel" /> from
        /// <c>SessionService.Session.ActivePerson</c> and opens the popup. No-op when there is no
        /// active person (e.g. the session is not yet authenticated).
        /// </summary>
        void OpenEditPersonPopup();
    }
}
