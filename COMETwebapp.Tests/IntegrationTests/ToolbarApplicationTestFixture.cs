// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolbarApplicationTestFixture.cs" company="Starion Group S.A.">
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

    using NUnit.Framework;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// Base fixture for the toolbar-based applications (Engineering Model, Reference Data, Server Administration). Adds
    /// a test that opens every toolbar section, on top of the inherited page-loaded smoke test.
    /// </summary>
    /// <typeparam name="TPageModel">The toolbar page object type.</typeparam>
    public abstract class ToolbarApplicationTestFixture<TPageModel> : ApplicationPageTestBase<TPageModel>
        where TPageModel : ToolbarApplicationPageModel
    {
        [Test]
        public async Task VerifyCanNavigateAllSections()
        {
            foreach (var section in this.PageModel.Sections)
            {
                await this.PageModel.SelectSectionAsync(section);

                await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
            }
        }
    }
}
