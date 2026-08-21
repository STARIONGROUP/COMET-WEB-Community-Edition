// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelEditorTestFixture.cs" company="Starion Group S.A.">
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

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// End-to-end tests for the Model Editor application. Add Model-Editor-specific tests here.
    /// </summary>
    [TestFixture]
    public class ModelEditorTestFixture : ApplicationPageTestBase<ModelEditorPageModel>
    {
        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "Model Editor";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override ModelEditorPageModel CreatePageModel(IPage page) => new(page);

        /// <summary>
        /// Verifies that the harmonized "View" display-options cog is shown in the upper-right of the toolbar.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyViewMenuButtonIsDisplayed()
        {
            await Expect(this.PageModel.ViewMenuButton).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Verifies that the Model Editor panels scroll independently (issue #934): even with the page introduction
        /// box shown, the application fits both the shell content area and the tab content area, so a vertical scroll
        /// no longer moves everything at once but scrolls the model tree panel it happens over.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyPanelsScrollIndependently()
        {
            await this.PageModel.ShowIntroductionAsync();

            var treeContentHeight = await ModelEditorPageModel.GetContentHeightAsync(this.PageModel.SourceTreeScrollArea);
            Assume.That(treeContentHeight, Is.GreaterThan(this.Page.ViewportSize.Height), "the seed model must hold more elements than fit on one screen");

            var shellOverflow = await ModelEditorPageModel.GetVerticalOverflowAsync(this.PageModel.ShellContentArea);
            var tabContentOverflow = await ModelEditorPageModel.GetVerticalOverflowAsync(this.PageModel.TabContentArea);

            Assert.Multiple(() =>
            {
                Assert.That(shellOverflow, Is.Zero, "the page must not scroll as a whole");
                Assert.That(tabContentOverflow, Is.Zero, "the tab content area must not scroll as a whole");
            });

            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        [Test]
        public async Task VerifyOpeningAnElementShowsItsDetails()
        {
            await this.PageModel.SelectFirstSourceElementAsync();

            await Expect(this.PageModel.DetailsPanel.Locator("[id^='element-details-new-button']")).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }
    }
}
