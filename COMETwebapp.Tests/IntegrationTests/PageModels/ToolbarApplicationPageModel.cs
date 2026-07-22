// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolbarApplicationPageModel.cs" company="Starion Group S.A.">
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
    /// Base class for the applications whose body is a DevExpress toolbar of sub-views (Engineering Model, Reference
    /// Data, Server Administration). Exposes the toolbar, the available sections and a helper to switch section.
    /// </summary>
    public abstract class ToolbarApplicationPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarApplicationPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        protected ToolbarApplicationPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets the names of the sub-view sections shown on the toolbar.
        /// </summary>
        public abstract IReadOnlyList<string> Sections { get; }

        /// <summary>
        /// Gets the section toolbar.
        /// </summary>
        public ILocator Toolbar => this.Page.Locator(this.Landmark);

        /// <summary>
        /// Clicks the toolbar item that switches to the given section.
        /// </summary>
        /// <param name="sectionName">The section's toolbar text (see the fixture's section list).</param>
        /// <returns>A <see cref="Task" />.</returns>
        public Task SelectSectionAsync(string sectionName)
        {
            return this.Toolbar.GetByText(sectionName, new LocatorGetByTextOptions { Exact = true }).First.ClickAsync();
        }
    }
}
