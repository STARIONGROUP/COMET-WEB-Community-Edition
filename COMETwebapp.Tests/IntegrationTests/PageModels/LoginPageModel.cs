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
            var unauthorizedNotice = this.page.Locator("#unauthorized-notice");

            // The DevExpress text boxes bind on input and round-trip to the server via the Blazor circuit; a value can
            // occasionally not commit before the submit, so the login silently does not take. Re-fill and re-submit
            // until the landing page is gone, then let a final attempt surface any real failure.
            for (var attempt = 0; attempt < 4; attempt++)
            {
                await this.FillLoginFormAsync(serverUrl, username, password);
                await this.page.Locator("#connectbtn").ClickAsync();

                try
                {
                    await unauthorizedNotice.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached, Timeout = 20_000 });
                    return;
                }
                catch (TimeoutException)
                {
                    // The login did not take; try again.
                }
            }

            await this.FillLoginFormAsync(serverUrl, username, password);
            await this.page.Locator("#connectbtn").ClickAsync();
            await unauthorizedNotice.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        }

        /// <summary>
        /// Fills the login form: the source address (when shown), advancing past the multi-step "Next" screen if the
        /// server is configured for it, then the user name and password.
        /// </summary>
        /// <param name="serverUrl">The COMET Web Services URL to connect to.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task FillLoginFormAsync(string serverUrl, string username, string password)
        {
            var sourceAddress = this.TextInput("sourceaddress");

            if (await sourceAddress.IsVisibleAsync())
            {
                await sourceAddress.FillAsync(serverUrl);

                // Give the on-input value time to round-trip before it is needed by Next or Connect.
                await this.page.WaitForTimeoutAsync(600);
            }

            var nextButton = this.page.Locator("#nextBtn");
            var usernameInput = this.TextInput("username");

            if (await nextButton.IsVisibleAsync())
            {
                // Multi-step: clicking Next before the source address commits leaves it empty. Click and retry until
                // the credentials step appears.
                for (var attempt = 0; attempt < 5; attempt++)
                {
                    await nextButton.ClickAsync();

                    try
                    {
                        await usernameInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 2500 });
                        break;
                    }
                    catch (TimeoutException)
                    {
                        await sourceAddress.FillAsync(serverUrl);
                        await this.page.WaitForTimeoutAsync(600);
                    }
                }
            }

            await usernameInput.FillAsync(username);
            await this.TextInput("password").FillAsync(password);
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
