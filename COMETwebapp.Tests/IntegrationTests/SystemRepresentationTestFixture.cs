// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemRepresentationTestFixture.cs" company="Starion Group S.A.">
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
    /// End-to-end tests for the System Representation application, including navigating the product tree. Add further
    /// System-Representation-specific tests here (use <c>this.PageModel</c>).
    /// </summary>
    [TestFixture]
    public class SystemRepresentationTestFixture : ApplicationPageTestBase<SystemRepresentationPageModel>
    {
        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "System Representation";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override SystemRepresentationPageModel CreatePageModel(IPage page) => new(page);

        [Test]
        public async Task VerifyCanNavigateProductTree()
        {
            await Expect(this.PageModel.TreeNodes.First).ToBeVisibleAsync();
            var initialNodeCount = await this.PageModel.TreeNodes.CountAsync();

            await this.PageModel.ExpandFirstCollapsedNodeAsync();
            await Expect(this.PageModel.TreeNodes).Not.ToHaveCountAsync(initialNodeCount);

            await this.PageModel.SelectRootNodeAsync();
            await Expect(this.PageModel.DetailsPanel.Locator("[id^='element-details-new-button']")).ToBeVisibleAsync();

            await this.PageModel.CollapseFirstExpandedNodeAsync();
            await Expect(this.PageModel.TreeNodes).ToHaveCountAsync(initialNodeCount);

            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Verifies that the product tree scrolls independently (issue #931): once the tree holds more nodes than fit
        /// on one screen, neither the shell nor the tab content area scrolls as a whole, so the tree scrolls inside its
        /// own section and the selected element's cards stay in view instead of being scrolled away.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyTreeScrollsIndependently()
        {
            await this.PageModel.SelectRootNodeAsync();
            await this.PageModel.ExpandNodesAsync(12);

            var treeContentHeight = await ApplicationPageModel.GetContentHeightAsync(this.PageModel.TreeNodesSection);
            Assume.That(treeContentHeight, Is.GreaterThan(this.Page.ViewportSize.Height), "the seed model must hold more tree nodes than fit on one screen");

            var shellOverflow = await ApplicationPageModel.GetVerticalOverflowAsync(this.PageModel.ShellContentArea);
            var tabContentOverflow = await ApplicationPageModel.GetVerticalOverflowAsync(this.PageModel.TabContentArea);
            var treeSectionOverflow = await ApplicationPageModel.GetVerticalOverflowAsync(this.PageModel.TreeNodesSection);

            Assert.Multiple(() =>
            {
                Assert.That(shellOverflow, Is.Zero, "the page must not scroll as a whole");
                Assert.That(tabContentOverflow, Is.Zero, "the tab content area must not scroll as a whole");
                Assert.That(treeSectionOverflow, Is.GreaterThan(0), "the tree node section must be the element that scrolls");
            });

            await Expect(this.PageModel.DetailsPanel).ToBeInViewportAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }
    }
}
