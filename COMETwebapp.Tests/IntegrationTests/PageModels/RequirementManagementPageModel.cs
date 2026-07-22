// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementManagementPageModel.cs" company="Starion Group S.A.">
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
    /// Page object for the Requirement Management application (search, owner/category filters, table of contents and
    /// the requirements document).
    /// </summary>
    public class RequirementManagementPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementManagementPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public RequirementManagementPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string Landmark => "#requirementmanagement-body";

        /// <summary>
        /// Gets the requirements editor body container.
        /// </summary>
        public ILocator Body => this.Page.Locator("#requirementmanagement-body");

        /// <summary>
        /// Gets the definitions search bar.
        /// </summary>
        public ILocator SearchBar => this.Page.Locator("#requirement-search");

        /// <summary>
        /// Gets the table-of-contents collapse toggle.
        /// </summary>
        public ILocator TableOfContentsToggle => this.Page.Locator("#requirement-toc-toggle");

        /// <summary>
        /// Gets the owner filter tag box.
        /// </summary>
        public ILocator OwnerFilter => this.Page.Locator("#requirement-owner-filter");

        /// <summary>
        /// Gets the category filter tag box.
        /// </summary>
        public ILocator CategoryFilter => this.Page.Locator("#requirement-category-filter");

        /// <summary>
        /// Gets the specification rows in the table of contents.
        /// </summary>
        public ILocator TableOfContentsRows => this.Page.Locator(".req-tree-row");

        /// <summary>
        /// Gets the requirements document viewer.
        /// </summary>
        public ILocator Document => this.Page.Locator(".req-document");

        /// <summary>
        /// Gets a value indicating whether the table of contents contains at least one specification.
        /// </summary>
        /// <returns><c>true</c> if a specification row is present.</returns>
        public async Task<bool> HasSpecificationsAsync()
        {
            return await this.TableOfContentsRows.CountAsync() > 0;
        }

        /// <summary>
        /// Selects the first specification in the table of contents, which opens it in the document viewer.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task OpenFirstSpecificationAsync()
        {
            await this.TableOfContentsRows.First.ClickAsync();
            await this.Document.Locator(".req-spec-title").First.WaitForAsync();
        }

        /// <summary>
        /// Gets the text currently shown in the document viewer.
        /// </summary>
        /// <returns>The document text.</returns>
        public Task<string> GetDocumentTextAsync()
        {
            return this.Document.InnerTextAsync();
        }
    }
}
