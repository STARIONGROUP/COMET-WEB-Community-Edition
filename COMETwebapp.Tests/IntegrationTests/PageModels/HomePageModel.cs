// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomePageModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.IntegrationTests.PageModels
{
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    /// <summary>
    /// Page object for the authenticated home page's session sidebar (log out).
    /// </summary>
    public class HomePageModel
    {
        /// <summary>
        /// The <see cref="IPage" /> this page object drives.
        /// </summary>
        private readonly IPage page;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public HomePageModel(IPage page)
        {
            this.page = page;
        }

        /// <summary>
        /// Gets the authenticated session sidebar entry (only present once the user is logged in).
        /// </summary>
        public ILocator SessionSidebar => this.page.Locator("#session-side-bar-item");

        /// <summary>
        /// Logs the current user out through the session sidebar and waits for the landing page to reappear.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task LogoutAsync()
        {
            await this.page.Locator("#session-side-bar-item").ClickAsync();
            await this.page.Locator("#logout-button").ClickAsync();
            await this.page.Locator("#unauthorized-notice").WaitForAsync();
        }

        /// <summary>
        /// Refreshes the session through the session sidebar and waits for the refresh to complete (the button returns
        /// to its enabled "Refresh" state).
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task RefreshSessionAsync()
        {
            await this.page.Locator("#session-side-bar-item").ClickAsync();

            var refreshButton = this.page.Locator("#refresh-button");
            await refreshButton.ClickAsync();
            await refreshButton.WaitForAsync();
        }
    }
}
