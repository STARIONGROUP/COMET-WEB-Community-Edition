// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationService.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Services.NotificationService
{
    using COMET.Web.Common.Model;

    using DynamicData;

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
        /// A sourcelist with <see cref="ResultNotification" />s
        /// </summary>
        public SourceList<ResultNotification> Results { get; } = new();

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
