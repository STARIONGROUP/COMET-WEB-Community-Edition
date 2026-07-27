// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerTestFixture.cs" company="Starion Group S.A.">
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
    /// End-to-end tests for the 3D Viewer application. Add Viewer-specific tests here.
    /// </summary>
    [TestFixture]
    public class ViewerTestFixture : ApplicationPageTestBase<ViewerPageModel>
    {
        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "3D Viewer";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override ViewerPageModel CreatePageModel(IPage page) => new(page);

        /// <summary>
        /// Verifies that the 3D Viewer product tree exposes the same controls as the System Representation tree
        /// the shared search bar and the "View" display-options cog.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyProductTreeControlsAreDisplayed()
        {
            await Expect(this.PageModel.SearchBar).ToBeVisibleAsync();
            await Expect(this.PageModel.ViewMenuButton).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }
    }
}
