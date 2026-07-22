// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPageTestBase.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using COMETwebapp.Tests.IntegrationTests.PageModels;

    using Microsoft.Playwright;

    using NUnit.Framework;

    /// <summary>
    /// Base fixture for the per-application end-to-end tests. It logs in <b>once</b> and opens the application under
    /// test as a tab in <see cref="OpenApplicationOnceAsync" />, then leaves it open for the tests. A derived fixture
    /// declares the application name and how to build its page object; add feature-specific tests to it — the page is
    /// already open and its page object is exposed as <see cref="PageModel" /> (plus the shared <c>this.Tabs</c> etc.).
    /// </summary>
    /// <typeparam name="TPageModel">The page object type for the application under test.</typeparam>
    public abstract class ApplicationPageTestBase<TPageModel> : E2ETestBase where TPageModel : class, IApplicationPageModel
    {
        /// <summary>
        /// Gets the page object for the application under test.
        /// </summary>
        protected TPageModel PageModel { get; private set; }

        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected abstract string ApplicationName { get; }

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected abstract TPageModel CreatePageModel(IPage page);

        /// <summary>
        /// Logs in and opens the application under test as a tab, once for the whole fixture, and waits for it to load.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [OneTimeSetUp]
        public async Task OpenApplicationOnceAsync()
        {
            await this.StartBrowserAsync();
            await this.Login.NavigateAsync(AppUrl);
            await this.Login.LoginAsync(ServerUrl, Username, Password);
            await this.Tabs.OpenApplicationTabAsync(this.ApplicationName);

            this.PageModel = this.CreatePageModel(this.Page);
            await this.PageModel.WaitForLoadedAsync();
        }

        /// <summary>
        /// Disposes the shared browser after all the fixture's tests have run.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [OneTimeTearDown]
        public Task CloseBrowserAsync()
        {
            return this.StopBrowserAsync();
        }

        [Test]
        public async Task VerifyPageIsDisplayed()
        {
            var loaded = await this.PageModel.IsLoadedAsync();
            var hasError = await this.Tabs.HasBlazorErrorAsync();

            Assert.Multiple(() =>
            {
                Assert.That(loaded, Is.True, $"The '{this.ApplicationName}' page did not load.");
                Assert.That(hasError, Is.False, $"The '{this.ApplicationName}' page raised a Blazor error.");
            });
        }
    }
}
