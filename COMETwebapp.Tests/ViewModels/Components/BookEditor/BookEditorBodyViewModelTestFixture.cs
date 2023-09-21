// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorBodyViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.BookEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.BookEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BookEditorBodyViewModelTestFixture
    {
        private IBookEditorBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.sessionService = new Mock<ISessionService>();

            this.viewModel = new BookEditorBodyViewModel(this.sessionService.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedBook, Is.Null);
                Assert.That(this.viewModel.SelectedSection, Is.Null);
                Assert.That(this.viewModel.SelectedPage, Is.Null);
                Assert.That(this.viewModel.SelectedNote, Is.Null);
                Assert.That(this.viewModel.AvailableBooks, Is.Not.Null);
                Assert.That(this.viewModel.AvailableBooks, Is.Empty);
                Assert.That(this.viewModel.AvailableCategories, Is.Not.Null);
                Assert.That(this.viewModel.AvailableCategories, Is.Empty);
                Assert.That(this.viewModel.ActiveDomains, Is.Not.Null);
                Assert.That(this.viewModel.ActiveDomains, Is.Empty);
                Assert.That(this.viewModel.ThingToCreate, Is.Null);
                Assert.That(this.viewModel.ThingToDelete, Is.Null);
                Assert.That(this.viewModel.ThingToEdit, Is.Null);
                Assert.That(this.viewModel.EditorPopupViewModel, Is.Not.Null);
                Assert.That(this.viewModel.ConfirmCancelPopupViewModel, Is.Not.Null);
            });
        }

        private void FillSelectedItems()
        {
            var book = new Book();
            var section = new Section();
            var page = new Page();
            var note = new TextualNote();

            this.viewModel.SelectedBook = book;
            this.viewModel.SelectedSection = section;
            this.viewModel.SelectedPage = page;
            this.viewModel.SelectedNote = note;
        }

        [Test]
        public void VerifySelectedItemsAreReset()
        {
            this.FillSelectedItems();
            this.viewModel.SelectedBook = new Book();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedSection, Is.Null);
                Assert.That(this.viewModel.SelectedPage, Is.Null);
                Assert.That(this.viewModel.SelectedNote, Is.Null);
            });

            this.FillSelectedItems();
            this.viewModel.SelectedSection = new Section();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedBook, Is.Not.Null);
                Assert.That(this.viewModel.SelectedPage, Is.Null);
                Assert.That(this.viewModel.SelectedNote, Is.Null);
            });

            this.FillSelectedItems();
            this.viewModel.SelectedPage = new Page();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedBook, Is.Not.Null);
                Assert.That(this.viewModel.SelectedSection, Is.Not.Null);
                Assert.That(this.viewModel.SelectedNote, Is.Null);
            });
        }

        [Test]
        public void VerifySetThingToCreate()
        {
            //Try to set a thing that is not a book, section, page or note
            Assert.That(() => this.viewModel.SetThingToCreate(new Iteration()), Throws.ArgumentException);

            var book = new Book();
            this.viewModel.SetThingToCreate(book);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToCreate, Is.Not.Null);
                Assert.That(this.viewModel.ThingToCreate, Is.EqualTo(book));
                Assert.That(this.viewModel.EditorPopupViewModel.HeaderText, Is.EqualTo("Create a new Book"));
                Assert.That(this.viewModel.EditorPopupViewModel.OnConfirmClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.OnCancelClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible = true);
            });
        }

        [Test]
        public void VerifyOnCreateThing()
        {
            //Try to create a thing when the thing to create has not been set
            Assert.That(() => this.viewModel.OnCreateThing(), Throws.InvalidOperationException);

            var section = new Section();
            this.viewModel.SelectedBook = new Book();
            this.viewModel.SetThingToCreate(section);

            Assert.That(() => this.viewModel.OnCreateThing(), Throws.Nothing);

            this.sessionService.Verify(x => x.CreateThing(It.IsAny<Thing>(), It.IsAny<Thing>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToCreate, Is.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.False);
            });
        }

        [Test]
        public void VerifySetThingToEdit()
        {
            //Try to set a thing that is not a book, section, page or note
            Assert.That(() => this.viewModel.SetThingToEdit(new Iteration()), Throws.ArgumentException);

            var book = new Book();
            this.viewModel.SetThingToEdit(book);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToEdit, Is.Not.Null);
                Assert.That(this.viewModel.ThingToEdit, Is.EqualTo(book));
                Assert.That(this.viewModel.EditorPopupViewModel.HeaderText, Is.EqualTo("Edit the Book"));
                Assert.That(this.viewModel.EditorPopupViewModel.OnConfirmClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.OnCancelClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible = true);
            });
        }

        [Test]
        public void VerifyOnEditThing()
        {
            //Try to edit a thing when the thing to edit has not been set
            Assert.That(() => this.viewModel.OnEditThing(), Throws.InvalidOperationException);

            var section = new Section();
            section.Container = new Book();
            this.viewModel.SetThingToEdit(section);

            Assert.That(() => this.viewModel.OnEditThing(), Throws.Nothing);

            this.sessionService.Verify(x => x.UpdateThing(It.IsAny<Thing>(), It.IsAny<Thing>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToEdit, Is.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.False);
            });
        }

        [Test]
        public void VerifySetThingToDelete()
        {
            //Try to set a thing that is not a book, section, page or note
            Assert.That(() => this.viewModel.SetThingToDelete(new Iteration()), Throws.ArgumentException);

            var book = new Book();
            this.viewModel.SetThingToDelete(book);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToDelete, Is.Not.Null);
                Assert.That(this.viewModel.ThingToDelete, Is.EqualTo(book));
                Assert.That(this.viewModel.ConfirmCancelPopupViewModel.IsVisible, Is.True);
            });
        }

        [Test]
        public void VerifyOnDeleteThing()
        {
            //Try to delete a thing when the thing to delete has not been set
            Assert.That(() => this.viewModel.OnDeleteThing(), Throws.InvalidOperationException);

            var section = new Section();
            section.Container = new Book();
            this.viewModel.SetThingToDelete(section);

            Assert.That(() => this.viewModel.OnDeleteThing(), Throws.Nothing);

            this.sessionService.Verify(x => x.DeleteThing(It.IsAny<Thing>(), It.IsAny<Thing>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToDelete, Is.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.False);
            });
        }
    }
}
