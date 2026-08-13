// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PageIntroBox.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Shared.PageIntroBox
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component representing the dismissible page introduction box
    /// </summary>
    public partial class PageIntroBox
    {
        /// <summary>
        /// The URL of the application whose preference was last loaded
        /// </summary>
        private string lastLoadedApplicationUrl = string.Empty;

        /// <summary>
        /// The resolved list of points to display
        /// </summary>
        private List<string> points = [];

        /// <summary>
        /// The resolved summary to display
        /// </summary>
        private string summary = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the page introduction box is visible.
        /// Supports two-way binding (<c>@bind-IsPageIntroductionVisible</c>).
        /// </summary>
        [Parameter]
        public bool IsPageIntroductionVisible { get; set; }

        /// <summary>
        /// Gets or sets the callback for two-way binding of <see cref="IsPageIntroductionVisible" />.
        /// </summary>
        [Parameter]
        public EventCallback<bool> IsPageIntroductionVisibleChanged { get; set; }

        /// <summary>
        /// Gets or sets the application for which the introduction box is displayed
        /// </summary>
        [Parameter]
        public Application Application { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IStringTableService" />
        /// </summary>
        [Inject]
        public IStringTableService StringTableService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService" />
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Method invoked when the component receives parameters
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (this.Application == null)
            {
                return;
            }

            // If the application URL has changed, reload the user preference and update the visibility of the page introduction box
            if (this.lastLoadedApplicationUrl != this.Application.Url)
            {
                this.lastLoadedApplicationUrl = this.Application.Url;

                this.IsPageIntroductionVisible = this.ShouldShowPageIntroduction(this.Application);
                await this.IsPageIntroductionVisibleChanged.InvokeAsync(this.IsPageIntroductionVisible);
            }

            this.summary = this.StringTableService.GetText($"{this.Application.Url}.PageIntro.Summary") ?? this.Application.PageIntroSummary;
            var pointsText = this.StringTableService.GetText($"{this.Application.Url}.PageIntro.Points");

            this.points = !string.IsNullOrEmpty(pointsText)
                ? [.. pointsText.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]
                : [.. this.Application.PageIntroPoints];
        }

        /// <summary>
        /// Dismisses the page introduction box
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task DismissAsync()
        {
            this.IsPageIntroductionVisible = false;
            await this.IsPageIntroductionVisibleChanged.InvokeAsync(false);

            // If the info button was clicked, but the dismissal user preference is already set to true, we don't need to update the preference again.
            if (!this.ShouldShowPageIntroduction(this.Application))
            {
                return;
            }

            var preferenceKey = this.Application.GetPageIntroUserPreferenceKey();
            var siteDirectory = this.SessionService.Session.RetrieveSiteDirectory().Clone(false);
            var clonedPerson = this.SessionService.Session.ActivePerson.Clone(false);

            var existingPref = this.SessionService.Session.ActivePerson.UserPreference.FirstOrDefault(x => x.ShortName == preferenceKey);
            UserPreference userPreference;

            // If the preference already exists, clone it and update the value to "true". Otherwise, create a new preference with the given key and value "true".
            if (existingPref != null)
            {
                userPreference = existingPref.Clone(false);
                userPreference.Value = "true";
            }
            else
            {
                userPreference = new UserPreference
                {
                    ShortName = preferenceKey,
                    Value = "true"
                };

                clonedPerson.UserPreference.Add(userPreference);
            }

            await this.SessionService.CreateOrUpdateThings(siteDirectory, [clonedPerson, userPreference]);
            await this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Determines whether the page introduction box should be visible based on user preferences.
        /// </summary>
        /// <param name="application">The <see cref="Application" /> to check the introduction preference for.</param>
        /// <returns>True if the introduction box should be visible; false otherwise.</returns>
        public bool ShouldShowPageIntroduction(Application application)
        {
            if (application == null)
            {
                return false;
            }

            var preferenceKey = application.GetPageIntroUserPreferenceKey();
            var preference = this.SessionService.Session.ActivePerson.UserPreference.FirstOrDefault(x => x.ShortName == preferenceKey);
            return preference is not { Value: "true" };
        }
    }
}
