// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BookEditorTestFixture.cs" company="Starion Group S.A.">
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
    using System;
    using System.Threading.Tasks;

    using COMETwebapp.Tests.IntegrationTests.PageModels;

    using Microsoft.Playwright;

    using NUnit.Framework;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// End-to-end tests for the Book Editor application. Add Book-Editor-specific tests here.
    /// </summary>
    [TestFixture]
    public class BookEditorTestFixture : ApplicationPageTestBase<BookEditorPageModel>
    {
        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "Book Editor";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override BookEditorPageModel CreatePageModel(IPage page) => new(page);

        [Test]
        public async Task VerifyCanOpenTheAddBookDialog()
        {
            await AssumeTheParticipantMayCreateABookAsync();

            // Non-mutating: this exercises the create-book flow up to the editor without persisting a book.
            await this.PageModel.OpenAddBookDialogAsync();

            await Expect(this.PageModel.EditorPopup).ToBeVisibleAsync();

            await this.PageModel.CancelDialogAsync();
        }

        /// <summary>
        /// Covers issue #938: confirming the add-book editor used to close the popup whatever the COMET server
        /// answered, so a refused write was indistinguishable from a successful one. The new book must now either show
        /// up in the Books column, or the popup must stay open carrying the reason it did not.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyCreatingABookEitherShowsItOrReportsWhyItFailed()
        {
            await AssumeTheParticipantMayCreateABookAsync();

            // A fresh name per run, so that a run interrupted before its clean-up cannot collide with the next one.
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var name = "E2EBook" + suffix;
            var namedBook = this.PageModel.BookNodes.Filter(new LocatorFilterOptions { HasTextString = name });

            await this.PageModel.OpenAddBookDialogAsync();
            await this.PageModel.SubmitEditorAsync(name, "e2ebook" + suffix);
            await this.PageModel.WaitForCreatedBookOrErrorAsync(name);

            if (await namedBook.CountAsync() == 0)
            {
                // The server refused the write, so the popup has to still be there, carrying the reason.
                await Expect(this.PageModel.EditorPopup).ToBeVisibleAsync();
                await Expect(this.PageModel.EditorErrors.First).ToBeVisibleAsync();
                await this.PageModel.CancelDialogAsync();
                return;
            }

            await Expect(this.PageModel.EditorPopup).ToBeHiddenAsync();
            await Expect(namedBook).ToHaveCountAsync(1);

            // This is the one test in the suite that writes, so it removes what it created.
            await this.PageModel.SelectBookAsync(name);
            await this.PageModel.DeleteSelectedBookAsync();

            await Expect(namedBook).ToHaveCountAsync(0);
        }

        /// <summary>
        /// Goes inconclusive when the active participant may not create a <c>Book</c> in the model under test - the
        /// default participant role ships the whole Book hierarchy with an access right of NONE, and the application
        /// then disables the add button.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task AssumeTheParticipantMayCreateABookAsync()
        {
            Assume.That(await this.PageModel.AddBookButton.IsEnabledAsync(), "The participant may not create a Book in this model.");
        }
    }
}
