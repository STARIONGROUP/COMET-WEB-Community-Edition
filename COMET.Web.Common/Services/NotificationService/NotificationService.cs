// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Services.NotificationService
{
    using ReactiveUI;

    /// <summary>
    /// The <see cref="NotificationService" /> provides notification information
    /// </summary>
    public class NotificationService : ReactiveObject, INotificationService
    {
        /// <summary>
        /// Backing field for <see cref="NotificationCount" />
        /// </summary>
        private int notificationCount;

        /// <summary>
        /// Gets the number of notification(s)
        /// </summary>
        public int NotificationCount
        {
            get => this.notificationCount;
            private set => this.RaiseAndSetIfChanged(ref this.notificationCount, value);
        }

        /// <summary>
        /// Increases the <see cref="NotificationCount" />
        /// </summary>
        /// <param name="amount">The increase amount</param>
        public void AddNotifications(int amount)
        {
            if (amount > 0)
            {
                this.NotificationCount += amount;
            }
        }

        /// <summary>
        /// Decreases the <see cref="NotificationCount" />
        /// </summary>
        /// <param name="amount">The decrease amount</param>
        public void RemoveNotifications(int amount)
        {
            if (amount > 0)
            {
                this.NotificationCount -= amount;
            }

            if (this.NotificationCount < 0)
            {
                this.NotificationCount = 0;
            }
        }
    }
}
