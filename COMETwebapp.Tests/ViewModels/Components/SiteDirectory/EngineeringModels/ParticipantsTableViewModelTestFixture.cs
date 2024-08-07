﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantsTableViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParticipantsTableViewModelTestFixture
    {
        private ParticipantsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<ParticipantsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Participant participant;
        private EngineeringModelSetup model;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<ParticipantsTableViewModel>>();

            this.participant = new Participant
            {
                Person = new Person
                {
                    GivenName = "person",
                    Surname = "A",
                    Organization = new Organization { Name = "org" }
                },
                Role = new ParticipantRole { Name = "role" },
                Domain = { new DomainOfExpertise { Name = "doe" } }
            };

            this.model = new EngineeringModelSetup
            {
                Name = "model",
                ShortName = "model",
                ActiveDomain = { new DomainOfExpertise() }
            };

            this.model.Participant.Add(this.participant);

            var siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory",
                Person = { new Person() },
                ParticipantRole = { new ParticipantRole() }
            };

            siteDirectory.Model.Add(this.model);

            this.permissionService.Setup(x => x.CanWrite(this.participant.ClassKind, this.participant.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new ParticipantsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
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
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.participant));
                Assert.That(this.viewModel.Persons, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.ParticipantRoles, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.DomainsOfExpertise, Is.Not.Null);
                Assert.That(this.viewModel.SelectedDomains, Is.Empty);
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
                Assert.That(participantRow.Name, Is.EqualTo(this.participant.Person.Name));
                Assert.That(participantRow.Thing, Is.EqualTo(this.participant));
                Assert.That(participantRow.IsAllowedToWrite, Is.EqualTo(true));
                Assert.That(participantRow.Organization, Is.EqualTo(this.participant.Person.Organization.Name));
                Assert.That(participantRow.Role, Is.EqualTo(this.participant.Role.Name));
                Assert.That(participantRow.AssignedDomains, Is.EqualTo(string.Join(ParticipantRowViewModel.Separator, this.participant.Domain.Select(x => x.Name))));
            });
        }

        [Test]
        public async Task VerifyParticipantsActions()
        {
            this.viewModel.InitializeViewModel(this.model);
            this.viewModel.CurrentThing = this.participant.Clone(true);

            this.viewModel.SelectedDomains = [this.participant.Domain.First(), this.participant.Domain.First().Clone(true)];
            await this.viewModel.CreateOrEditParticipant(false);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<EngineeringModelSetup>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            await this.viewModel.CreateOrEditParticipant(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<EngineeringModelSetup>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Exactly(2));

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            await this.viewModel.CreateOrEditParticipant(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public async Task VerifyRowOperations()
        {
            this.viewModel.InitializeViewModel(this.model);
            var participantRow = this.viewModel.Rows.Items.First();

            Assert.That(participantRow, Is.Not.Null);

            this.viewModel.OnDeleteButtonClick(participantRow);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeletionMode, Is.EqualTo(true));
                Assert.That(this.viewModel.CurrentThing, Is.EqualTo(participantRow.Thing));
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

            var participantTest = new Participant
            {
                Iid = Guid.NewGuid(),
                Person = new Person
                {
                    GivenName = "person",
                    Surname = "B",
                    Organization = new Organization { Name = "org2" }
                },
                Role = new ParticipantRole { Name = "role2" },
                Container = this.model,
                Domain = { new DomainOfExpertise { Name = "doe2" } }
            };

            this.messageBus.SendObjectChangeEvent(participantTest, EventKind.Added);
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
            this.viewModel.CurrentThing = this.participant;

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentThing.Iid, Is.EqualTo(this.participant.Iid));
                Assert.That(this.viewModel.SelectedDomains, Is.EqualTo(this.participant.Domain));
                Assert.That(this.viewModel.DomainsOfExpertise, Has.Count.EqualTo(1));
            });
        }
    }
}
