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

    using static Microsoft.Playwright.Assertions;

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
        /// Gets the "Connect and Open a Model" notice shown while the user is not authenticated.
        /// </summary>
        public ILocator UnauthorizedNotice => this.page.Locator("#unauthorized-notice");

        /// <summary>
        /// Gets the Connect submit button of the credentials step.
        /// </summary>
        public ILocator ConnectButton => this.page.Locator("#connectbtn");

        /// <summary>
        /// Gets the list of client-side validation messages shown on the login form.
        /// </summary>
        public ILocator ValidationErrors => this.page.Locator(".validation-errors");

        /// <summary>
        /// Navigates to the application root and waits for the unauthenticated landing page to appear.
        /// </summary>
        /// <param name="appUrl">The base URL of the application under test.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task NavigateAsync(string appUrl)
        {
            await this.page.GotoAsync(appUrl);

            // The first request boots the Blazor Server circuit, which is slower than a steady-state interaction.
            await Expect(this.UnauthorizedNotice).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });
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
            // The source-address box is shown when the server is not pre-configured.
            var isSourceAddressRequested = await this.TextInput("sourceaddress").IsVisibleAsync();

            if (isSourceAddressRequested)
            {
                await this.TypeCredentialAsync("sourceaddress", serverUrl);
            }

            var nextButton = this.page.Locator("#nextBtn");

            // Multi-step configuration only: a "Next" screen takes the server address before asking for credentials. It
            // exists only when the source address was asked for - against a pre-configured server the form goes straight
            // to the credentials and leaves a permanently disabled "Next" button behind, which is still "visible", so
            // testing visibility alone sends the login into a branch that can never complete.
            if (isSourceAddressRequested && await nextButton.IsVisibleAsync())
            {
                // The button only enables once the app has asked the server which authentication schemes it supports, a
                // network round-trip that can outlast the 10s action default, so give it the same budget as the other
                // server-dependent steps.
                await Expect(nextButton).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });

                await nextButton.ClickAsync();
                await Expect(this.TextInput("username")).ToBeVisibleAsync();
            }

            await this.TypeCredentialAsync("username", username);
            await this.TypeCredentialAsync("password", password);

            // BindValueMode.OnInput debounces before pushing the value through the Blazor circuit; blur the last field so
            // its final value is flushed to the server-side DTO before the submit, otherwise Connect can post a truncated
            // password and the server answers 401 while the login notice stays up.
            await this.TextInput("password").BlurAsync();

            await this.page.Locator("#connectbtn").ClickAsync();

            // Connecting opens the ISession against the COMET server, a network round-trip that can be slow on CI.
            await Expect(this.UnauthorizedNotice).ToBeHiddenAsync(new LocatorAssertionsToBeHiddenOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });
        }

        /// <summary>
        /// Types a value into a login text box deterministically. It first waits for the login form's application-owned
        /// <c>data-app-ready</c> marker, which the form publishes once its editors have rendered and DevExpress has had a
        /// render cycle to attach their client-side scripts - because on a cold Blazor circuit the field re-renders as it
        /// wires up and wipes anything typed too early, so Connect ends up posting an empty value. It then types at a
        /// cadence the on-input binding can keep up with and confirms the field holds the full value.
        /// </summary>
        /// <param name="id">The text box component id.</param>
        /// <param name="value">The value to type.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task TypeCredentialAsync(string id, string value)
        {
            await Expect(this.page.Locator("#login-form[data-app-ready]"))
                .ToBeAttachedAsync(new LocatorAssertionsToBeAttachedOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });

            var input = this.TextInput(id);
            await input.PressSequentiallyAsync(value, TypingCadence);
            await Expect(input).ToHaveValueAsync(value);
        }

        /// <summary>
        /// The per-keystroke typing cadence used for the DevExpress on-input login fields, so each keystroke's binding
        /// commits through the Blazor circuit in order.
        /// </summary>
        private static LocatorPressSequentiallyOptions TypingCadence => new() { Delay = 30 };

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
