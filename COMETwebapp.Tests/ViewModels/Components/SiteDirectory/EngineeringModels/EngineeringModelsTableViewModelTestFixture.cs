// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelsTableViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;

    using DynamicData;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EngineeringModelsTableViewModelTestFixture
    {
        private EngineeringModelsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<IOrganizationalParticipantsTableViewModel> organizationalParticipantsTableViewModel;
        private Mock<IParticipantsTableViewModel> participantsTableViewModel;
        private Mock<ILogger<EngineeringModelsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private EngineeringModelSetup engineeringModel;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<EngineeringModelsTableViewModel>>();

            this.engineeringModel = new EngineeringModelSetup
            {
                Iid = Guid.NewGuid(),
                ShortName = "model1",
                Name = "model 1",
                OrganizationalParticipant =
                {
                    new OrganizationalParticipant { Organization = new Organization() }
                }
            };

            this.siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory",
                SiteReferenceDataLibrary = { new SiteReferenceDataLibrary() },
                Domain = { new DomainOfExpertise() },
                Organization = { new Organization() }
            };

            this.siteDirectory.Model.Add(this.engineeringModel);

            var iteration = new Iteration
            {
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = this.engineeringModel
                }
            };

            var openIterations = new SourceList<Iteration>();
            openIterations.Add(iteration);

            this.permissionService.Setup(x => x.CanWrite(this.engineeringModel.ClassKind, this.engineeringModel.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.OpenIterations).Returns(openIterations);

            this.organizationalParticipantsTableViewModel = new Mock<IOrganizationalParticipantsTableViewModel>();
            this.participantsTableViewModel = new Mock<IParticipantsTableViewModel>();

            this.viewModel = new EngineeringModelsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object, this.organizationalParticipantsTableViewModel.Object, this.participantsTableViewModel.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.engineeringModel));
                Assert.That(this.viewModel.DomainsOfExpertise, Is.EqualTo(this.siteDirectory.Domain));
                Assert.That(this.viewModel.EngineeringModels, Is.EqualTo(this.siteDirectory.Model));
                Assert.That(this.viewModel.Organizations, Is.EqualTo(this.siteDirectory.Organization));
                Assert.That(this.viewModel.SiteRdls, Is.EqualTo(this.siteDirectory.SiteReferenceDataLibrary));
                Assert.That(this.viewModel.ModelKinds, Is.Not.Empty);
                Assert.That(this.viewModel.StudyPhases, Is.Not.Empty);
            });
        }

        [Test]
        public async Task VerifyModelCreation()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.CurrentThing = this.engineeringModel.Clone(true);
            this.viewModel.SelectedSourceModel = new EngineeringModelSetup();

            await this.viewModel.CreateOrEditEngineeringModel(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<SiteDirectory>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedSiteRdl, Is.Null);
                Assert.That(this.viewModel.SelectedSourceModel, Is.Null);
                Assert.That(this.viewModel.CurrentThing.Original, Is.Not.Null);
            });

            this.viewModel.SelectedSourceModel = null;
            this.viewModel.SelectedSiteRdl = new SiteReferenceDataLibrary();
            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<SiteDirectory>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            await this.viewModel.CreateOrEditEngineeringModel(true);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public void VerifyModelRowProperties()
        {
            this.viewModel.InitializeViewModel();
            var modelRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(modelRow.ContainerName, Is.EqualTo("siteDirectory"));
                Assert.That(modelRow.Name, Is.EqualTo(this.engineeringModel.Name));
                Assert.That(modelRow.ShortName, Is.EqualTo(this.engineeringModel.ShortName));
                Assert.That(modelRow.Thing, Is.EqualTo(this.engineeringModel));
                Assert.That(modelRow.IsAllowedToWrite, Is.EqualTo(true));
            });
        }

        [Test]
        public async Task VerifyRowOperations()
        {
            this.viewModel.InitializeViewModel();
            var modelRow = this.viewModel.Rows.Items.First();

            Assert.That(modelRow, Is.Not.Null);

            this.viewModel.OnDeleteButtonClick(modelRow);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.EqualTo(true));
                Assert.That(this.viewModel.CurrentThing, Is.EqualTo(modelRow.Thing));
            });

            this.viewModel.OnCancelPopupButtonClick();
            Assert.That(this.viewModel.IsOnDeletionMode, Is.EqualTo(false));

            await this.viewModel.OnConfirmPopupButtonClick();
            this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<SiteDirectory>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            this.sessionService.Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            await this.viewModel.DeleteThing();
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel();

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var engineeringModelTest = new EngineeringModelSetup
            {
                Iid = Guid.NewGuid(),
                Container = this.siteDirectory
            };

            this.messageBus.SendObjectChangeEvent(engineeringModelTest, EventKind.Added);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.messageBus.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(new PersonRole(), EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Items.First().ContainerName, Is.EqualTo(this.siteDirectory.ShortName));
                this.permissionService.Verify(x => x.CanWrite(engineeringModelTest.ClassKind, It.IsAny<Thing>()), Times.AtLeast(this.viewModel.Rows.Count));
            });
        }
    }
}
