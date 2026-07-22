// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BookEditorPageModel.cs" company="Starion Group S.A.">
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
    /// Page object for the Book Editor application (Books and Sections columns).
    /// </summary>
    public class BookEditorPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BookEditorPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public BookEditorPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string Landmark => "#bookeditor-body";

        /// <summary>
        /// Gets the book editor body container.
        /// </summary>
        public ILocator Body => this.Page.Locator("#bookeditor-body");

        /// <summary>
        /// Gets the "Books" column.
        /// </summary>
        public ILocator BooksColumn => this.Page.Locator("#books-column");

        /// <summary>
        /// Gets the "Sections" column.
        /// </summary>
        public ILocator SectionsColumn => this.Page.Locator("#sections-column");

        /// <summary>
        /// Gets the "add book" button in the Books column header.
        /// </summary>
        public ILocator AddBookButton => this.Page.Locator("#books-column .add-item-button");

        /// <summary>
        /// Gets the create/edit popup.
        /// </summary>
        public ILocator EditorPopup => this.Page.Locator(".dxbl-popup");

        /// <summary>
        /// Opens the "add book" editor dialog.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task OpenAddBookDialogAsync()
        {
            await this.AddBookButton.ClickAsync();
            await this.EditorPopup.WaitForAsync();
        }

        /// <summary>
        /// Cancels the currently open editor dialog.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task CancelDialogAsync()
        {
            await this.EditorPopup.GetByText("Cancel").First.ClickAsync();
            await this.EditorPopup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        }
    }
}
