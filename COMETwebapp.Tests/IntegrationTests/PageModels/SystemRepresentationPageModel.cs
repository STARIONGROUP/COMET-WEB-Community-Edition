// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemRepresentationPageModel.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    /// <summary>
    /// Page object for the System Representation application: the product tree (expand/collapse/select), the element
    /// details panel and the surrounding controls.
    /// </summary>
    public class SystemRepresentationPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemRepresentationPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public SystemRepresentationPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string LandmarkSelector => "#product-tree-container";

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
        public ILocator ViewMenuButton => this.ProductTree.Locator("[id^='systemTreeViewMenuButton']");

        /// <summary>
        /// Gets the option filter selector.
        /// </summary>
        public ILocator OptionSelector => this.Page.Locator("#option-filter");

        /// <summary>
        /// Gets the element details panel (populated when a node is selected).
        /// </summary>
        public ILocator DetailsPanel => this.Page.Locator("#rightColumn");

        /// <summary>
        /// Gets all currently rendered tree node rows.
        /// </summary>
        public ILocator TreeNodes => this.Page.Locator(".treeNode");

        /// <summary>
        /// Expands the first collapsed node in the tree.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task ExpandFirstCollapsedNodeAsync()
        {
            return this.Page.Locator("img.expandIcon[src*=Collapsed]").First.ClickAsync();
        }

        /// <summary>
        /// Collapses the first expanded node in the tree.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task CollapseFirstExpandedNodeAsync()
        {
            return this.Page.Locator("img.expandIcon[src*=Expanded]").First.ClickAsync();
        }

        /// <summary>
        /// Selects the root node, which opens its element details in the right-hand panel.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task SelectRootNodeAsync()
        {
            return this.TreeNodes.First.ClickAsync();
        }
    }
}
