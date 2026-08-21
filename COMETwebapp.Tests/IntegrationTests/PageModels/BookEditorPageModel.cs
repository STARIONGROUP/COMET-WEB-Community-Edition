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
        protected override string LandmarkSelector => "#bookeditor-body";

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
        /// Gets the book nodes of the Books column. A repeated element, so matched by its app-owned class.
        /// </summary>
        public ILocator BookNodes => this.Page.Locator("#books-column .node-button");

        /// <summary>
        /// Gets the "delete" button shown on the selected book.
        /// </summary>
        public ILocator DeleteBookButton => this.Page.Locator("#books-column .delete-button");

        /// <summary>
        /// Gets the create/edit popup, targeted by the application-owned <c>book-editor-popup</c> class set on it.
        /// </summary>
        public ILocator EditorPopup => this.Page.Locator(".book-editor-popup");

        /// <summary>
        /// Gets the "Name" text box of the editor popup.
        /// </summary>
        public ILocator EditorNameInput => this.EditorPopup.Locator("#editor-row-name input");

        /// <summary>
        /// Gets the "ShortName" text box of the editor popup.
        /// </summary>
        public ILocator EditorShortNameInput => this.EditorPopup.Locator("#editor-row-shortname input");

        /// <summary>
        /// Gets the "Owner" combo box of the editor popup.
        /// </summary>
        public ILocator EditorOwnerComboBox => this.EditorPopup.Locator("#editor-row-owner input");

        /// <summary>
        /// Gets the "OK" button of the editor popup.
        /// </summary>
        public ILocator EditorOkButton => this.EditorPopup.Locator(".ok-button");

        /// <summary>
        /// Gets the error messages the editor popup reports, both the client-side validation ones and the ones the
        /// COMET server answered a refused write with.
        /// </summary>
        public ILocator EditorErrors => this.EditorPopup.Locator("li.text-danger");

        /// <summary>
        /// Gets the "Confirm" button of the deletion confirmation popup. The popup's content is teleported out of its
        /// own element, so its buttons are matched from the page by their application-owned class.
        /// </summary>
        public ILocator ConfirmDeletionButton => this.Page.Locator(".confirm-button");

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

        /// <summary>
        /// Fills the open editor popup with a name, a short name and the first available owner, then confirms it.
        /// </summary>
        /// <param name="name">The name to give the item.</param>
        /// <param name="shortName">The short name to give the item.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task SubmitEditorAsync(string name, string shortName)
        {
            await this.EditorNameInput.FillAsync(name);
            await this.EditorShortNameInput.FillAsync(shortName);

            // The combo's drop-down list is rendered outside the popup, so its items are matched from the page.
            await this.EditorOwnerComboBox.ClickAsync();
            await this.Page.Locator(".dxbl-listbox-item").First.ClickAsync();

            await this.EditorOkButton.ClickAsync();
        }

        /// <summary>
        /// Waits until the confirmed editor has settled on one of its two outcomes - the named book showing up in the
        /// Books column, or the popup reporting why the COMET server refused the write - so that a caller does not race
        /// the server round-trip the confirmation triggers.
        /// </summary>
        /// <param name="name">The name given to the book being created.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public Task WaitForCreatedBookOrErrorAsync(string name)
        {
            return this.Page.Locator($"#books-column .node-button:has-text(\"{name}\"), .book-editor-popup li.text-danger")
                .First.WaitForAsync();
        }

        /// <summary>
        /// Selects the book carrying the supplied name.
        /// </summary>
        /// <param name="name">The name of the book to select.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public Task SelectBookAsync(string name)
        {
            return this.BookNodes.Filter(new LocatorFilterOptions { HasTextString = name }).First.ClickAsync();
        }

        /// <summary>
        /// Deletes the currently selected book and confirms the removal.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task DeleteSelectedBookAsync()
        {
            await this.DeleteBookButton.First.ClickAsync();
            await this.ConfirmDeletionButton.ClickAsync();
        }
    }
}
