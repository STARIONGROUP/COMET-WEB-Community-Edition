// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationComponent.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Shared.TopMenuEntry
{
    using AntDesign;

    using COMET.Web.Common.Shared.TopMenuEntry;

    using COMETwebapp.Extensions;

    using DynamicData;

    using FluentResults;

    using Microsoft.AspNetCore.Components;

    using IAntDesignNotificationService = AntDesign.INotificationService;
    using INotificationService = COMET.Web.Common.Services.NotificationService.INotificationService;

    /// <summary>
    /// Menu entry to display the toast notifications
    /// </summary>
    public partial class NotificationComponent : MenuEntryBase
    {
        /// <summary>
        /// Gets or sets the <see cref="INotificationService" />
        /// </summary>
        [Inject]
        public INotificationService NotificationService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IAntDesignNotificationService" />
        /// </summary>
        [Inject]
        public IAntDesignNotificationService AntNotificationService { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.NotificationService.Results.Connect().WhereReasonsAre(ListChangeReason.Add, ListChangeReason.AddRange).Subscribe(_ =>
            {
                foreach (var result in this.NotificationService.Results.Items)
                {
                    this.DisplayToastNotificationFromResult(result);
                }

                this.NotificationService.Results.Clear();
            }));
        }

        /// <summary>
        /// Displays a toast notification in the screen from a given result
        /// </summary>
        /// <param name="result">The result of an operation</param>
        /// <exception cref="InvalidDataException">
        /// Throws an <see cref="InvalidDataException" /> if the
        /// <see cref="NotificationService" /> property is null
        /// </exception>
        /// <returns>A <see cref="Task" /></returns>
        private void DisplayToastNotificationFromResult(IResultBase result)
        {
            var key = $"open{DateTime.Now}";

            var notificationConfig = new NotificationConfig { Key = key };

            if (result.IsSuccess)
            {
                notificationConfig.Message = "Success!";
                notificationConfig.Description = "The operation was successful!";
                notificationConfig.NotificationType = NotificationType.Success;
                notificationConfig.Duration = 4.5;
            }
            else
            {
                notificationConfig.Message = "Operation Failed";
                notificationConfig.Description = result.GetHtmlErrorsDescription();
                notificationConfig.NotificationType = NotificationType.Error;
                notificationConfig.Duration = 12.5;
            }

            this.AntNotificationService.Open(notificationConfig);
        }
    }
}
