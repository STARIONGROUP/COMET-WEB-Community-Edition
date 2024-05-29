// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantRolesTableViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.Tests.ViewModels.Components.SiteDirectory.Roles
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParticipantRolesTableViewModelTestFixture
    {
        private ParticipantRolesTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<IShowHideDeprecatedThingsService> showHideDeprecatedThingsService;
        private Assembler assembler;
        private Mock<ILogger<ParticipantRolesTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private ParticipantRole participantRole;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.showHideDeprecatedThingsService = new Mock<IShowHideDeprecatedThingsService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<ParticipantRolesTableViewModel>>();

            this.participantRole = new ParticipantRole()
            {
                ShortName = "model1",
                Name = "model 1",
            };

            this.siteDirectory = new SiteDirectory()
            {
                ShortName = "siteDirectory",
                SiteReferenceDataLibrary = { new SiteReferenceDataLibrary() },
                Domain = { new DomainOfExpertise() },
                Organization = { new Organization() }
            };

            this.siteDirectory.ParticipantRole.Add(this.participantRole);

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyModel = new Lazy<Thing>(this.participantRole);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyModel);

            this.permissionService.Setup(x => x.CanWrite(this.participantRole.ClassKind, this.participantRole.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(this.siteDirectory);

            this.viewModel = new ParticipantRolesTableViewModel(this.sessionService.Object, this.showHideDeprecatedThingsService.Object, this.messageBus, this.loggerMock.Object);
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
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.participantRole));
                Assert.That(this.viewModel.ParticipantAccessKinds, Is.Not.Empty);
            });
        }

        [Test]
        public void VerifyParticipantRoleRowProperties()
        {
            this.viewModel.InitializeViewModel();
            var participantRoleRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(participantRoleRow.ContainerName, Is.EqualTo("siteDirectory"));
                Assert.That(participantRoleRow.Name, Is.EqualTo(this.participantRole.Name));
                Assert.That(participantRoleRow.ShortName, Is.EqualTo(this.participantRole.ShortName));
                Assert.That(participantRoleRow.Thing, Is.EqualTo(this.participantRole));
                Assert.That(participantRoleRow.IsAllowedToWrite, Is.EqualTo(true));
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel();

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var participantRoleTest = new ParticipantRole()
            {
                Iid = Guid.NewGuid(),
                Container = this.siteDirectory,
            };

            this.messageBus.SendObjectChangeEvent(participantRoleTest, EventKind.Added);
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
                this.permissionService.Verify(x => x.CanWrite(participantRoleTest.ClassKind, It.IsAny<Thing>()), Times.AtLeast(this.viewModel.Rows.Count));
            });
        }

        [Test]
        public async Task VerifyParticipantRoleCreation()
        {
            this.viewModel.InitializeViewModel();
            await this.viewModel.CreateOrEditParticipantRole(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<SiteDirectory>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<SiteDirectory>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            await this.viewModel.CreateOrEditParticipantRole(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }
    }
}
