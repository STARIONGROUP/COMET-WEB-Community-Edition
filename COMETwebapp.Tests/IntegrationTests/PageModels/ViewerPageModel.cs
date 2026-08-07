// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerPageModel.cs" company="Starion Group S.A.">
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
    using Microsoft.Playwright;

    /// <summary>
    /// Page object for the 3D Viewer application (Babylon canvas, product tree, properties panel and the state selector).
    /// </summary>
    public class ViewerPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewerPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public ViewerPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        // The body id ("3dviewer-body") starts with a digit, which is not a valid CSS #id selector, so match by attribute.
        protected override string LandmarkSelector => "[id='3dviewer-body']";

        /// <summary>
        /// Gets the Babylon.js 3D canvas.
        /// </summary>
        public ILocator Canvas => this.Page.Locator("#babylon-canvas");

        /// <summary>
        /// Gets the product tree container.
        /// </summary>
        public ILocator ProductTree => this.Page.Locator("#product-tree-container");

        /// <summary>
        /// Gets the tree search bar.
        /// </summary>
        public ILocator SearchBar => this.Page.Locator("#product-tree-search-bar");

        /// <summary>
        /// Gets the tree "View" display-options cog button.
        /// </summary>
        public ILocator ViewMenuButton => this.Page.Locator("[id^='viewerTreeViewMenuButton']");

        /// <summary>
        /// Gets the properties/details panel.
        /// </summary>
        public ILocator PropertiesPanel => this.Page.Locator("#rightColumn");

        /// <summary>
        /// Gets the actual finite state selector container.
        /// </summary>
        public ILocator StateSelector => this.Page.Locator("#state-selector-container");
    }
}
