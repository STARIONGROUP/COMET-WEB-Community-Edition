// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelPageModel.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    /// <summary>
    /// Page object for the Engineering Model application. Use <see cref="ToolbarApplicationPageModel.SelectSectionAsync" />
    /// with one of the <see cref="Sections" /> to switch sub-view.
    /// </summary>
    public class EngineeringModelPageModel : ToolbarApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public EngineeringModelPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets the names of the sub-view sections shown on the toolbar.
        /// </summary>
        public override IReadOnlyList<string> Sections => ["Options", "Publications", "Common File Store", "Domain File Store"];

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string LandmarkSelector => "#engineering-model-toolbar";

        /// <summary>
        /// Gets the view/edit details panel shown next to the table of the current section. It is the shared
        /// <c>DataItemDetailsComponent</c>, so what holds here holds on every table-plus-details application page.
        /// It is matched by its application-owned class rather than an id, because a split view renders one panel per
        /// open tab.
        /// </summary>
        public ILocator DetailsPanel => this.Page.Locator(".data-item-details-section").First;

        /// <summary>
        /// Gets the table of the current section, matched by the application-owned class every one of these grids
        /// carries.
        /// </summary>
        public ILocator SectionTable => this.Page.Locator(".selected-data-item-table").First;

        /// <summary>
        /// Gets the section tabs rendered above the table by the toolbar. The toolbar keeps a hidden copy of every
        /// item in a <c>dxbl-virtual-toolbar</c> it measures against, so the tabs must be taken from the real one or
        /// each of these locators resolves to two elements; the trailing "More items..." button carries no
        /// <c>dxbl-toolbar-btn</c> and is left out.
        /// </summary>
        public ILocator SectionTabs => this.Toolbar.Locator(".dxbl-btn-toolbar:not(.dxbl-virtual-toolbar) button.dxbl-toolbar-btn");

        /// <summary>
        /// Gets the tab of the section currently shown. DevExpress marks it with the primary render style, which its
        /// Plain toolbar mode emits as <c>dxbl-btn-text-primary</c>.
        /// </summary>
        public ILocator CurrentSectionTab => this.Toolbar.Locator(".dxbl-btn-toolbar:not(.dxbl-virtual-toolbar) button.dxbl-toolbar-btn.dxbl-btn-text-primary");

        /// <summary>
        /// Gets the page introduction box shown above the section tabs.
        /// </summary>
        public ILocator IntroductionBox => this.Page.Locator(".page-intro-box").First;

        /// <summary>
        /// Gets the information glyph in the introduction box header.
        /// </summary>
        public ILocator IntroductionIcon => this.IntroductionBox.Locator(".comet-icon").First;

        /// <summary>
        /// Switches to a section and waits until its content has actually been swapped in. Measuring straight after
        /// <see cref="ToolbarApplicationPageModel.SelectSectionAsync" /> can still resolve the outgoing section's
        /// elements, and a detached element reports every computed style as an empty string.
        /// </summary>
        /// <param name="sectionName">The section's toolbar text (see <see cref="Sections" />).</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task OpenSectionAsync(string sectionName)
        {
            await this.SelectSectionAsync(sectionName);
            await Assertions.Expect(this.CurrentSectionTab).ToHaveTextAsync(sectionName);
            await this.SectionTable.WaitForAsync();
        }
    }
}
