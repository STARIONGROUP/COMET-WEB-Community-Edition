// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISessionMenuViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Dal;

    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

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
        /// Value indiciating that the <see cref="ISession" /> is currently refreshing
        /// </summary>
        bool IsRefreshing { get; set; }

        /// <summary>
        /// The <see cref="INotificationService" />
        /// </summary>
        INotificationService NotificationService { get; }

        /// <summary>
        /// Refreshes the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task RefreshSession();
    }
}
