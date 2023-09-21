// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorBodyTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.BookEditor
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.BookEditor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class BookEditorBodyTestFixture
    {
        private TestContext context;
        private IRenderedComponent<BookEditorBody> component;
        private Mock<IBookEditorBodyViewModel> viewModel;
        private Mock<ISessionService> sessionService;
        private Book selectedBook;
        private Section selectedSection;
        private Page selectedPage;
        private Note selectedNote;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();

            this.selectedBook = new Book();
            this.selectedSection = new Section();
            this.selectedPage = new Page();
            this.selectedNote = new TextualNote();

            this.selectedBook.Section.Add(this.selectedSection);
            this.selectedSection.Page.Add(this.selectedPage);
            this.selectedPage.Note.Add(this.selectedNote);

            var availableBooks = new SourceList<Book>();
            availableBooks.Add(this.selectedBook);

            this.viewModel = new Mock<IBookEditorBodyViewModel>();
            this.viewModel.Setup(x => x.CurrentIteration).Returns(new Iteration());
            this.viewModel.Setup(x => x.CurrentDomain).Returns(new DomainOfExpertise());
            this.viewModel.Setup(x => x.AvailableBooks).Returns(availableBooks);
            this.viewModel.Setup(x => x.SelectedBook).Returns(this.selectedBook);
            this.viewModel.Setup(x => x.SelectedSection).Returns(this.selectedSection);
            this.viewModel.Setup(x => x.SelectedPage).Returns(this.selectedPage);
            this.viewModel.Setup(x => x.SelectedNote).Returns(this.selectedNote);

            var editorPopupViewModel = new Mock<IEditorPopupViewModel>();
            editorPopupViewModel.Setup(x => x.ValidationErrors).Returns(new SourceList<string>());
            editorPopupViewModel.Setup(x => x.Item).Returns(new Book());
            editorPopupViewModel.Setup(x => x.ActiveDomains).Returns(new List<DomainOfExpertise>());
            editorPopupViewModel.Setup(x => x.AvailableCategories).Returns(new List<Category>());

            this.viewModel.Setup(x => x.EditorPopupViewModel).Returns(editorPopupViewModel.Object);

            var confirmCancelPopupViewModel = new Mock<IConfirmCancelPopupViewModel>();

            this.viewModel.Setup(x => x.ConfirmCancelPopupViewModel).Returns(confirmCancelPopupViewModel.Object);

            var domDataService = new Mock<IDomDataService>();

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(domDataService.Object);

            this.component = this.context.RenderComponent<BookEditorBody>();
        }
        
        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            var bookEditorColumn = this.component.FindComponent<BookEditorColumn<Book>>();
            var sectionEditorColumn = this.component.FindComponent<BookEditorColumn<Section>>();
            var pageEditorColumn = this.component.FindComponent<BookEditorColumn<Page>>();
            var noteEditorColumn = this.component.FindComponent<BookEditorColumn<Note>>();

            Assert.Multiple(() =>
            {
                Assert.That(bookEditorColumn.Instance, Is.Not.Null);
                Assert.That(bookEditorColumn.Instance.HeaderHexColor, Is.EqualTo("#eba434"));
                Assert.That(bookEditorColumn.Instance.HeaderTitle, Is.EqualTo("Books"));
                Assert.That(bookEditorColumn.Instance.Items, Is.Not.Null.Or.Empty);
                Assert.That(bookEditorColumn.Instance.DrawLeftLines, Is.False);
                Assert.That(bookEditorColumn.Instance.CollapseButtonIconClass, Is.EqualTo("no-display"));
                Assert.That(bookEditorColumn.Instance.CssClass, Is.EqualTo("book-node"));
                Assert.That(bookEditorColumn.Instance.OnCreateNewItemClick.HasDelegate, Is.True);
                Assert.That(bookEditorColumn.Instance.OnEditClicked.HasDelegate, Is.True);
                Assert.That(bookEditorColumn.Instance.OnDeleteClicked.HasDelegate, Is.True);
                
                Assert.That(sectionEditorColumn.Instance, Is.Not.Null);
                Assert.That(sectionEditorColumn.Instance.HeaderHexColor, Is.EqualTo("#56bd08"));
                Assert.That(sectionEditorColumn.Instance.HeaderTitle, Is.EqualTo("Sections"));
                Assert.That(sectionEditorColumn.Instance.Items, Is.Not.Null.Or.Empty);
                Assert.That(sectionEditorColumn.Instance.DrawLeftLines, Is.True);
                Assert.That(sectionEditorColumn.Instance.CssClass, Is.EqualTo("section-node"));
                Assert.That(sectionEditorColumn.Instance.OnCreateNewItemClick.HasDelegate, Is.True);
                Assert.That(sectionEditorColumn.Instance.OnEditClicked.HasDelegate, Is.True);
                Assert.That(sectionEditorColumn.Instance.OnDeleteClicked.HasDelegate, Is.True);

                Assert.That(pageEditorColumn.Instance, Is.Not.Null);
                Assert.That(pageEditorColumn.Instance.HeaderHexColor, Is.EqualTo("#51dded"));
                Assert.That(pageEditorColumn.Instance.HeaderTitle, Is.EqualTo("Pages"));
                Assert.That(pageEditorColumn.Instance.Items, Is.Not.Null.Or.Empty);
                Assert.That(pageEditorColumn.Instance.DrawLeftLines, Is.True);
                Assert.That(pageEditorColumn.Instance.CssClass, Is.EqualTo("page-node"));
                Assert.That(pageEditorColumn.Instance.OnCreateNewItemClick.HasDelegate, Is.True);
                Assert.That(pageEditorColumn.Instance.OnEditClicked.HasDelegate, Is.True);
                Assert.That(pageEditorColumn.Instance.OnDeleteClicked.HasDelegate, Is.True);

                Assert.That(noteEditorColumn.Instance, Is.Not.Null);
                Assert.That(noteEditorColumn.Instance, Is.Not.Null);
                Assert.That(noteEditorColumn.Instance.HeaderHexColor, Is.EqualTo("#eb6075"));
                Assert.That(noteEditorColumn.Instance.HeaderTitle, Is.EqualTo("Notes"));
                Assert.That(noteEditorColumn.Instance.Items, Is.Not.Null.Or.Empty);
                Assert.That(noteEditorColumn.Instance.DrawLeftLines, Is.True);
                Assert.That(noteEditorColumn.Instance.CssClass, Is.EqualTo("note-node"));
                Assert.That(noteEditorColumn.Instance.OnCreateNewItemClick.HasDelegate, Is.True);
                Assert.That(noteEditorColumn.Instance.OnEditClicked.HasDelegate, Is.True);
                Assert.That(noteEditorColumn.Instance.OnDeleteClicked.HasDelegate, Is.True);
            });
        }

        [Test]
        public async Task VerifyCallbackAreCalled()
        {
            await this.component.InvokeAsync(() => this.component.FindAll(".add-item-button")[0].Click());
            await this.component.InvokeAsync(() => this.component.FindAll(".add-item-button")[1].Click());
            await this.component.InvokeAsync(() => this.component.FindAll(".add-item-button")[2].Click());
            await this.component.InvokeAsync(() => this.component.FindAll(".add-item-button")[3].Click());

            this.viewModel.Verify(x => x.SetThingToCreate(It.IsAny<Thing>()), Times.Exactly(4));
        }
    }
}

