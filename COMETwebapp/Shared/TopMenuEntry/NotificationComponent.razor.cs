// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationComponent.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Shared.TopMenuEntry;

    using COMETwebapp.Extensions;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

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
        /// Gets or sets the <see cref="IToastNotificationService" />
        /// </summary>
        [Inject]
        public IToastNotificationService ToastNotificationService { get; set; }

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
        /// <param name="resultNotification">The result notification of an operation</param>
        private void DisplayToastNotificationFromResult(ResultNotification resultNotification)
        {
            if (resultNotification?.NotificationDescription is null)
            {
                return;
            }

            if (resultNotification.Result.IsSuccess)
            {
                var toastOptions = new ToastOptions
                {
                    Title = "Success!",
                    Text = resultNotification.NotificationDescription.OnSuccess,
                    RenderStyle = ToastRenderStyle.Success,
                    DisplayTime = TimeSpan.FromSeconds(4.5)
                };

                this.ToastNotificationService.ShowToast(toastOptions);
            }
            else
            {
                var toastOptions = new ToastOptions
                {
                    Title = "Operation Failed!",
                    RenderStyle = ToastRenderStyle.Danger,
                    DisplayTime = TimeSpan.FromSeconds(12.5)
                };

                var htmlText = $"<div>{resultNotification.NotificationDescription.OnError}<br>{resultNotification.Result.GetHtmlErrorsDescription()}</div>";
                this.ToastNotificationService.ShowToast(toastOptions, builder => builder.AddMarkupContent(0, htmlText));
            }
        }
    }
}
