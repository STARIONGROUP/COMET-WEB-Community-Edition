// --------------------------------------------------------------------------------------------------------------------
// <copyright file="E2ETestBase.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.IntegrationTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using COMETwebapp.Tests.IntegrationTests.PageModels;

    using Microsoft.Playwright;

    using NUnit.Framework;
    using NUnit.Framework.Interfaces;

    /// <summary>
    /// Base class for the Playwright end-to-end tests. Owns the Playwright/browser/page lifecycle and exposes the
    /// environment-driven configuration so the same suite runs unchanged locally and in the containerised CI pipeline.
    /// </summary>
    /// <remarks>
    /// These tests require a running COMET WEB application and a reachable COMET Web Services server. They live in the
    /// <c>IntegrationTests</c> namespace and carry the <c>EndToEnd</c> category so the unit-test CI filter
    /// (<c>FullyQualifiedName!~IntegrationTests</c>) excludes them; the dedicated end-to-end workflow runs them explicitly.
    /// </remarks>
    [TestFixture]
    [Category("EndToEnd")]
    public abstract class E2ETestBase
    {
        /// <summary>
        /// The timeout, in milliseconds, allowed for a wait that depends on a COMET server round-trip through the Blazor
        /// circuit (logging in, opening a tab): those cross the network and can be slow on a cold CI runner. Ordinary UI
        /// assertions use the shorter default set in <see cref="StartBrowserAsync" /> so a genuine failure surfaces fast.
        /// </summary>
        internal const int ServerRoundTripTimeoutMilliseconds = 30_000;

        /// <summary>
        /// The number of times <see cref="TypeIntoEditorAsync" /> will type a value before letting the assertion fail.
        /// </summary>
        private const int TypingAttempts = 3;

        /// <summary>
        /// The per-keystroke typing cadence for the DevExpress login editors, slow enough that each keystroke's
        /// on-input binding commits through the Blazor circuit in order.
        /// </summary>
        private static LocatorPressSequentiallyOptions TypingCadence => new() { Delay = 30 };

        /// <summary>
        /// Types a value into a DevExpress text editor and confirms the editor holds it, retyping if the editor swallowed
        /// the leading keystrokes.
        /// </summary>
        /// <param name="input">The native <c>input</c> inside the DevExpress editor.</param>
        /// <param name="value">The value to type.</param>
        /// <returns>A <see cref="Task" />.</returns>
        /// <remarks>
        /// A form's own <c>data-app-ready</c> marker proves the Blazor render sequence settled, but not that every
        /// DevExpress editor on the page has finished loading its client-side script. An editor that attaches *after*
        /// typing started re-syncs the DOM input from the still-empty server-side value and swallows what was typed so
        /// far, which is how "admin" arrives as "in". The window widens with every extra DevExpress component on the
        /// page and only on the very first, coldest circuit, so this retypes instead of failing on the first wipe. The
        /// last attempt asserts without catching, so a genuinely broken editor still fails with Playwright's own diff.
        /// </remarks>
        internal static async Task TypeIntoEditorAsync(ILocator input, string value)
        {
            for (var attempt = 1; attempt < TypingAttempts; attempt++)
            {
                await ClearAsync(input);
                await input.PressSequentiallyAsync(value, TypingCadence);

                try
                {
                    await Assertions.Expect(input).ToHaveValueAsync(value, new LocatorAssertionsToHaveValueOptions { Timeout = 5_000 });
                    return;
                }
                catch (PlaywrightException)
                {
                    TestContext.Out.WriteLine($"the editor swallowed the typed value on attempt {attempt}, retyping");
                }
            }

            await ClearAsync(input);
            await input.PressSequentiallyAsync(value, TypingCadence);
            await Assertions.Expect(input).ToHaveValueAsync(value);
        }

        /// <summary>
        /// Clears a text editor the way a user would, so the on-input binding sees the change.
        /// </summary>
        /// <param name="input">The native <c>input</c> inside the DevExpress editor.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private static async Task ClearAsync(ILocator input)
        {
            await input.PressAsync("Control+a");
            await input.PressAsync("Delete");
        }

        /// <summary>
        /// Gets the URL of the COMET WEB application under test (env <c>COMETWEBAPP_URL</c>, default the local Kestrel port).
        /// </summary>
        protected static string AppUrl => Environment.GetEnvironmentVariable("COMETWEBAPP_URL") ?? "http://localhost:8080";

        /// <summary>
        /// Gets the URL of the COMET Web Services server to connect to (env <c>COMET_SERVER_URL</c>).
        /// </summary>
        protected static string ServerUrl => Environment.GetEnvironmentVariable("COMET_SERVER_URL") ?? "http://localhost:5000";

        /// <summary>
        /// Gets the user name used to authenticate against the server (env <c>COMET_USERNAME</c>).
        /// </summary>
        protected static string Username => Environment.GetEnvironmentVariable("COMET_USERNAME") ?? "admin";

        /// <summary>
        /// Gets the password used to authenticate against the server (env <c>COMET_PASSWORD</c>).
        /// </summary>
        protected static string Password => Environment.GetEnvironmentVariable("COMET_PASSWORD") ?? "pass";

        /// <summary>
        /// Gets a value indicating whether the browser runs headless (env <c>COMET_E2E_HEADLESS</c>, default true).
        /// </summary>
        protected static bool Headless => !string.Equals(Environment.GetEnvironmentVariable("COMET_E2E_HEADLESS"), "false", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the Playwright <see cref="IPage" /> driving the browser for the current test.
        /// </summary>
        protected IPage Page { get; private set; }

        /// <summary>
        /// Gets the <see cref="LoginPageModel" /> page object for the current test.
        /// </summary>
        protected LoginPageModel Login { get; private set; }

        /// <summary>
        /// Gets the <see cref="HomePageModel" /> page object for the current test.
        /// </summary>
        protected HomePageModel Home { get; private set; }

        /// <summary>
        /// Gets the <see cref="TabsPageModel" /> page object for the current test.
        /// </summary>
        protected TabsPageModel Tabs { get; private set; }

        /// <summary>
        /// The Playwright runtime for the current test.
        /// </summary>
        private IPlaywright playwright;

        /// <summary>
        /// The browser launched for the current test.
        /// </summary>
        private IBrowser browser;

        /// <summary>
        /// The browser context for the current test.
        /// </summary>
        private IBrowserContext context;

        /// <summary>
        /// Launches a fresh browser, context and page and initialises the page objects. Fixtures call this either
        /// per-test (fresh unauthenticated browser) or once (shared, already-authenticated session).
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        protected async Task StartBrowserAsync()
        {
            this.playwright = await Playwright.CreateAsync();
            this.browser = await this.playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Headless });
            this.context = await this.browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
            this.Page = await this.context.NewPageAsync();

            // Record a Playwright trace (with screenshots and DOM snapshots); it is kept only when the test fails so it
            // can be opened with `playwright show-trace` to diagnose the first CI failure.
            await this.context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true });

            // Most UI actions resolve in a second or two, so keep the default waits short: a failing assertion should
            // surface quickly rather than hang for half a minute. The few waits that cross the network to the COMET
            // server (login, open-tab) opt into the longer ServerRoundTripTimeoutMilliseconds explicitly, and the first
            // page load boots the Blazor circuit so its navigation timeout is generous.
            this.Page.SetDefaultTimeout(10_000);
            this.Page.SetDefaultNavigationTimeout(ServerRoundTripTimeoutMilliseconds);
            Assertions.SetDefaultExpectTimeout(10_000);

            this.Login = new LoginPageModel(this.Page);
            this.Home = new HomePageModel(this.Page);
            this.Tabs = new TabsPageModel(this.Page);
        }

        /// <summary>
        /// Disposes the browser context and Playwright, saving the trace first when the test failed.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        protected async Task StopBrowserAsync()
        {
            if (this.context != null)
            {
                await this.SaveTraceOnFailureAsync();
                await this.context.CloseAsync();
            }

            if (this.browser != null)
            {
                await this.browser.CloseAsync();
            }

            this.playwright?.Dispose();
        }

        /// <summary>
        /// Stops tracing, writing the trace to a zip under a <c>playwright-traces</c> folder only when the test (or the
        /// fixture's one-time setup) failed.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task SaveTraceOnFailureAsync()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
            {
                await this.context.Tracing.StopAsync();
                return;
            }

            var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "playwright-traces");
            Directory.CreateDirectory(directory);

            var traceName = TestContext.CurrentContext.Test.Name.Replace('(', '_').Replace(')', '_').Replace('"', '_');
            await this.context.Tracing.StopAsync(new TracingStopOptions { Path = Path.Combine(directory, $"{traceName}.zip") });
        }
    }
}
