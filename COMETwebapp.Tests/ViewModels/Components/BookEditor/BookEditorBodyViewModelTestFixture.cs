// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorBodyViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.BookEditor;

    using Moq;

    using NUnit.Framework;

    using System.Collections.Generic;

    using CDP4Web.Enumerations;

    using DynamicData;

    using FluentResults;

    [TestFixture]
    public class BookEditorBodyViewModelTestFixture
    {
        /// <summary>
        /// The message a COMET server answers a refused write with, as reported in issue #938
        /// </summary>
        private const string ServerError = "The person admin does not have an appropriate create permission for Book.";

        private BookEditorBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .ReturnsAsync(Result.Ok());

            this.sessionService.Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.messageBus = new CDPMessageBus();
            this.viewModel = new BookEditorBodyViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
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
                Assert.That(this.viewModel.ThingToCreate, Is.TypeOf(typeof(Book)));
                Assert.That(this.viewModel.ThingToCreate.Original, Is.EqualTo(book));
                Assert.That(this.viewModel.EditorPopupViewModel.HeaderText, Is.EqualTo("Create a new Book"));
                Assert.That(this.viewModel.EditorPopupViewModel.OnConfirmClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.OnCancelClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.True);
            });
        }

        [Test]
        public void VerifyOnCreateThing()
        {
            //Try to create a thing when the thing to create has not been set
            Assert.That(() => this.viewModel.OnCreateThing(), Throws.InvalidOperationException);

            this.viewModel.CurrentThing = new EngineeringModel()
            {
                EngineeringModelSetup = new EngineeringModelSetup()
            };

            var book = new Book();
            this.viewModel.SetThingToCreate(book);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToCreate, Is.InstanceOf(typeof(Book)));
                Assert.That(this.viewModel.OnCreateThing, Throws.Nothing);
                this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Exactly(1));
            });

            var section = new Section();
            this.viewModel.SelectedBook = book;
            this.viewModel.SetThingToCreate(section);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToCreate, Is.InstanceOf(typeof(Section)));
                Assert.That(this.viewModel.OnCreateThing, Throws.Nothing);
                this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Exactly(2));
            });

            var page = new Page();
            this.viewModel.SelectedSection = section;
            this.viewModel.SetThingToCreate(page);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToCreate, Is.InstanceOf(typeof(Page)));
                Assert.That(this.viewModel.OnCreateThing, Throws.Nothing);
                this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Exactly(3));
            });

            var note = new TextualNote();
            this.viewModel.SelectedPage = page;
            this.viewModel.SetThingToCreate(note);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToCreate, Is.InstanceOf(typeof(Note)));
                Assert.That(this.viewModel.OnCreateThing, Throws.Nothing);
                this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Exactly(4));
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
                Assert.That(this.viewModel.ThingToEdit, Is.TypeOf(typeof(Book)));
                Assert.That(this.viewModel.ThingToEdit.Original, Is.EqualTo(book));
                Assert.That(this.viewModel.EditorPopupViewModel.HeaderText, Is.EqualTo("Edit the Book"));
                Assert.That(this.viewModel.EditorPopupViewModel.OnConfirmClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.OnCancelClick, Is.Not.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.True);
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

            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Once);

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

            this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ThingToDelete, Is.Null);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.False);
            });
        }

        [Test]
        public async Task VerifyOnCreateThingReportsAFailedWrite()
        {
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .ReturnsAsync(Result.Fail(ServerError));

            this.viewModel.CurrentThing = new EngineeringModel
            {
                EngineeringModelSetup = new EngineeringModelSetup()
            };

            this.viewModel.SetThingToCreate(new Book());
            await this.viewModel.OnCreateThing();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.EditorPopupViewModel.ValidationErrors.Items, Has.Member(ServerError));
                Assert.That(this.viewModel.ThingToCreate, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyOnEditThingReportsAFailedWrite()
        {
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .ReturnsAsync(Result.Fail(ServerError));

            var section = new Section
            {
                Container = new Book()
            };

            this.viewModel.SetThingToEdit(section);
            await this.viewModel.OnEditThing();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.EditorPopupViewModel.ValidationErrors.Items, Has.Member(ServerError));
                Assert.That(this.viewModel.ThingToEdit, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifySuccessfulWriteClearsThePreviousErrors()
        {
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .ReturnsAsync(Result.Fail("failed"));

            this.viewModel.CurrentThing = new EngineeringModel
            {
                EngineeringModelSetup = new EngineeringModelSetup()
            };

            this.viewModel.SetThingToCreate(new Book());
            await this.viewModel.OnCreateThing();

            Assert.That(this.viewModel.EditorPopupViewModel.ValidationErrors.Items, Is.Not.Empty);

            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .ReturnsAsync(Result.Ok());

            await this.viewModel.OnCreateThing();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.EditorPopupViewModel.ValidationErrors.Items, Is.Empty);
                Assert.That(this.viewModel.EditorPopupViewModel.IsVisible, Is.False);
                Assert.That(this.viewModel.ThingToCreate, Is.Null);
            });
        }

        [Test]
        public void VerifyCreationRequiresBothAContainerAndThePermission()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanCreateBook, Is.False);
                Assert.That(this.viewModel.CanCreateSection, Is.False);
                Assert.That(this.viewModel.CanCreatePage, Is.False);
                Assert.That(this.viewModel.CanCreateNote, Is.False);
            });

            this.viewModel.CurrentThing = new EngineeringModel
            {
                EngineeringModelSetup = new EngineeringModelSetup()
            };

            this.viewModel.SelectedBook = new Book();
            this.viewModel.SelectedSection = new Section();
            this.viewModel.SelectedPage = new Page();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanCreateBook, Is.True);
                Assert.That(this.viewModel.CanCreateSection, Is.True);
                Assert.That(this.viewModel.CanCreatePage, Is.True);
                Assert.That(this.viewModel.CanCreateNote, Is.True);
            });

            // The default participant role denies the whole Book hierarchy, which is what issue #938 reported.
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanCreateBook, Is.False);
                Assert.That(this.viewModel.CanCreateSection, Is.False);
                Assert.That(this.viewModel.CanCreatePage, Is.False);
                Assert.That(this.viewModel.CanCreateNote, Is.False);
            });
        }

        [Test]
        public void VerifyOnThingChanged()
        {
            //Try to set a new engineering model value to the CurrentThing property
            Assert.That(this.viewModel.CurrentThing, Is.Null);

            var category = new Category()
            {
                Name = "category A",
                ShortName = "catA"
            };

            var rdl = new ModelReferenceDataLibrary()
            {
                DefinedCategory = { category }
            };

            var activeDomain = new DomainOfExpertise()
            {
                Name = "tester",
                ShortName = "tester"
            };

            var engineeringModelSetup = new EngineeringModelSetup()
            {
                ActiveDomain = { activeDomain },
                RequiredRdl = { rdl }
            };

            this.viewModel.CurrentThing = new EngineeringModel()
            {
                EngineeringModelSetup = engineeringModelSetup
            };

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentThing, Is.Not.Null);
                Assert.That(this.viewModel.AvailableCategories, Contains.Item(category));
                Assert.That(this.viewModel.ActiveDomains, Contains.Item(activeDomain));
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.CurrentThing = new EngineeringModel()
            {
                EngineeringModelSetup = new EngineeringModelSetup()
            };

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.CurrentThing, Is.Not.Null);

            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));
            Assert.That(this.viewModel.CurrentThing, Is.Not.Null);
        }
    }
}
