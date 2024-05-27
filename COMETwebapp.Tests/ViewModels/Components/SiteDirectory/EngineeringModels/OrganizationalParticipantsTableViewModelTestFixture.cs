// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrganizationalParticipantsTableViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OrganizationalParticipantsTableViewModelTestFixture
    {
        private OrganizationalParticipantsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<OrganizationalParticipantsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private OrganizationalParticipant organizationalParticipant;
        private EngineeringModelSetup model;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<OrganizationalParticipantsTableViewModel>>();

            this.organizationalParticipant = new OrganizationalParticipant
            {
                Organization = new Organization
                {
                    Name = "org A",
                    ShortName = "orgA"
                }
            };

            this.model = new EngineeringModelSetup
            {
                Name = "model",
                ShortName = "model"
            };

            this.model.OrganizationalParticipant.Add(this.organizationalParticipant);

            var siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory",
                Organization = { new Organization() }
            };

            siteDirectory.Model.Add(this.model);

            this.permissionService.Setup(x => x.CanWrite(this.organizationalParticipant.ClassKind, this.organizationalParticipant.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new OrganizationalParticipantsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
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
            this.viewModel.InitializeViewModel(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.organizationalParticipant));
                Assert.That(this.viewModel.Organizations, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.ParticipatingOrganizations, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyOrganizationalParticipantRowProperties()
        {
            this.viewModel.InitializeViewModel(this.model);
            var participantRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(participantRow.ContainerName, Is.EqualTo(this.model.ShortName));
                Assert.That(participantRow.Name, Is.EqualTo(this.organizationalParticipant.Organization.Name));
                Assert.That(participantRow.ShortName, Is.EqualTo(this.organizationalParticipant.Organization.ShortName));
                Assert.That(participantRow.Thing, Is.EqualTo(this.organizationalParticipant));
                Assert.That(participantRow.IsAllowedToWrite, Is.EqualTo(true));
            });
        }

        [Test]
        public async Task VerifyRowOperations()
        {
            this.viewModel.InitializeViewModel(this.model);
            var organizationalParticipantRow = this.viewModel.Rows.Items.First();

            Assert.That(organizationalParticipantRow, Is.Not.Null);

            this.viewModel.OnDeleteButtonClick(organizationalParticipantRow);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.EqualTo(true));
                Assert.That(this.viewModel.CurrentThing, Is.EqualTo(organizationalParticipantRow.Thing));
            });

            this.viewModel.OnCancelPopupButtonClick();
            Assert.That(this.viewModel.IsOnDeletionMode, Is.EqualTo(false));

            await this.viewModel.OnConfirmPopupButtonClick();
            this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<EngineeringModelSetup>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel(this.model);

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var organizationalParticipantTest = new OrganizationalParticipant
            {
                Iid = Guid.NewGuid(),
                Organization = new Organization
                {
                    Name = "org B",
                    ShortName = "orgB"
                },
                Container = this.model
            };

            this.messageBus.SendObjectChangeEvent(organizationalParticipantTest, EventKind.Added);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifySetEngineeringModel()
        {
            this.viewModel.InitializeViewModel(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.ParticipatingOrganizations.ToList(), Has.Count.EqualTo(1));
            });
        }
    }
}
