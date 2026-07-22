// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginPageModel.cs" company="Starion Group S.A.">
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
    /// Page object for the COMET WEB landing/login page. Encapsulates the selectors and steps so a selector change is a
    /// one-line fix here rather than a change scattered across every fixture.
    /// </summary>
    public class LoginPageModel
    {
        /// <summary>
        /// The <see cref="IPage" /> this page object drives.
        /// </summary>
        private readonly IPage page;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public LoginPageModel(IPage page)
        {
            this.page = page;
        }

        /// <summary>
        /// Navigates to the application root and waits for the unauthenticated landing page to appear.
        /// </summary>
        /// <param name="appUrl">The base URL of the application under test.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task NavigateAsync(string appUrl)
        {
            await this.page.GotoAsync(appUrl);
            await this.page.Locator("#unauthorized-notice").WaitForAsync();
        }

        /// <summary>
        /// Gets the text of the unauthorized notice shown before login.
        /// </summary>
        /// <returns>The notice text.</returns>
        public Task<string> GetUnauthorizedNoticeAsync()
        {
            return this.page.Locator("#unauthorized-notice").InnerTextAsync();
        }

        /// <summary>
        /// Performs the full login flow, transparently handling both the single-step and the multi-step
        /// (source address then credentials) server authentication configurations, and waits until the session is
        /// authenticated.
        /// </summary>
        /// <param name="serverUrl">The COMET Web Services URL to connect to.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task LoginAsync(string serverUrl, string username, string password)
        {
            var sourceAddress = this.TextInput("sourceaddress");
            await sourceAddress.FillAsync(serverUrl);

            var nextButton = this.page.Locator("#nextBtn");
            var usernameInput = this.TextInput("username");

            if (await nextButton.IsVisibleAsync())
            {
                // The source-address text box binds on input and round-trips to the server via the Blazor circuit;
                // clicking Next before that commits leaves the address empty. Settle, click, and retry until the
                // credentials step appears.
                for (var attempt = 0; attempt < 5; attempt++)
                {
                    await this.page.WaitForTimeoutAsync(600);
                    await nextButton.ClickAsync();

                    try
                    {
                        await usernameInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 2500 });
                        break;
                    }
                    catch (TimeoutException)
                    {
                        await sourceAddress.FillAsync(serverUrl);
                    }
                }
            }

            await usernameInput.FillAsync(username);
            await this.TextInput("password").FillAsync(password);

            // The Connect button is only enabled once the credentials validate, so the click auto-waits for it.
            await this.page.Locator("#connectbtn").ClickAsync();

            await this.page.Locator("#unauthorized-notice").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        }

        /// <summary>
        /// Resolves the inner <c>input</c> of a DevExpress text box regardless of whether the component
        /// <c>Id</c> is rendered on the wrapper or on the input itself.
        /// </summary>
        /// <param name="id">The component id.</param>
        /// <returns>A locator resolving to the single text input.</returns>
        private ILocator TextInput(string id)
        {
            return this.page.Locator($"#{id} input, input#{id}");
        }
    }
}
