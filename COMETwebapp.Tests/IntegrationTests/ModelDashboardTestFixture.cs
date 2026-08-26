// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelDashboardTestFixture.cs" company="Starion Group S.A.">
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
    /// End-to-end tests for the Model Dashboard application. Add Model-Dashboard-specific tests here (the page is
    /// already open; use <c>this.PageModel</c>).
    /// </summary>
    [TestFixture]
    public class ModelDashboardTestFixture : ApplicationPageTestBase<ModelDashboardPageModel>
    {
        /// <summary>
        /// The width, in pixels, of the narrow viewport that forces the open tabs to overflow the tab strip, so that its
        /// horizontal scrollbar shows.
        /// </summary>
        private const int NarrowViewportWidth = 500;

        /// <summary>
        /// The width, in pixels, of the viewport the shared page is restored to, matching the Playwright default.
        /// </summary>
        private const int DefaultViewportWidth = 1280;

        /// <summary>
        /// The height, in pixels, of the viewport the shared page is restored to, matching the Playwright default.
        /// </summary>
        private const int DefaultViewportHeight = 720;

        /// <summary>
        /// The largest growth, in pixels, the tab row may show when its horizontal scrollbar appears. The bar is drawn
        /// into the padding the row already has, so what is left is only the few pixels by which it overhangs it.
        /// </summary>
        private const int MaximumRowGrowthWithScrollbar = 4;

        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "Model Dashboard";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override ModelDashboardPageModel CreatePageModel(IPage page) => new(page);

        [Test]
        public async Task VerifyDashboardChartsRender()
        {
            await Expect(this.PageModel.Charts.First).ToBeVisibleAsync();
        }

        /// <summary>
        /// Verifies that after the issue #892 rework the parameter-value charts (the donut plus the two
        /// count-based bar charts) all render, i.e. converting the misleading full-stacked (percentage) bars
        /// to count-based stacked bars did not break the dashboard.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        [Test]
        public async Task VerifyParameterValueChartsRender()
        {
            await Expect(this.PageModel.Charts.First).ToBeVisibleAsync();
            var chartCount = await this.PageModel.Charts.CountAsync();
            Assume.That(chartCount, Is.GreaterThan(0), "the seeded model must expose parameter values for the dashboard charts to render");
            Assert.That(chartCount, Is.GreaterThanOrEqualTo(3), "expected the donut and both count-based bar charts to render");
        }

        /// <summary>
        /// Verifies how the tab strip's horizontal scrollbar sits once the tabs overflow: it must keep clear of the bottom
        /// edge of the tabs, where its track used to read as a grey line struck through them, and it must be drawn inside
        /// the padding the tab row already has rather than making the row taller. The tab strip belongs to the tabbed
        /// shell rather than to the dashboard, so this covers it through a single application instead of repeating it for
        /// every one. The viewport is narrowed to force the overflow and restored afterwards, leaving the rest of the
        /// fixture on the standard size.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyTabStripScrollbarKeepsClearOfTheTabsWithoutGrowingTheRow()
        {
            var rowHeightWithoutScrollbar = await this.Tabs.GetTabRowHeightAsync();

            await this.Page.SetViewportSizeAsync(NarrowViewportWidth, DefaultViewportHeight);

            try
            {
                var (overflow, spaceBelowTabs) = await this.Tabs.GetTabStripScrollbarMetricsAsync();
                var rowHeightWithScrollbar = await this.Tabs.GetTabRowHeightAsync();

                Assume.That(overflow, Is.GreaterThan(0), "the tabs must overflow the strip for its horizontal scrollbar to show");

                Assert.Multiple(() =>
                {
                    Assert.That(spaceBelowTabs, Is.GreaterThanOrEqualTo(3), "the horizontal scrollbar must not be drawn against the bottom edge of the tabs");
                    Assert.That(rowHeightWithScrollbar - rowHeightWithoutScrollbar, Is.LessThanOrEqualTo(MaximumRowGrowthWithScrollbar), "showing the horizontal scrollbar must not add its height to the tab row, it belongs in the padding the row already has");
                });
            }
            finally
            {
                await this.Page.SetViewportSizeAsync(DefaultViewportWidth, DefaultViewportHeight);
            }
        }
    }
}
