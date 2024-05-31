// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ActiveDomainsTableViewModelTestFixture.cs" company="Starion Group S.A.">
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

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ActiveDomainsTableViewModelTestFixture
    {
        private ActiveDomainsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<ActiveDomainsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private DomainOfExpertise domain;
        private EngineeringModelSetup model;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<ActiveDomainsTableViewModel>>();

            this.domain = new DomainOfExpertise
            {
                Name = "domain A",
                ShortName = "domainA"
            };

            this.model = new EngineeringModelSetup
            {
                Name = "model",
                ShortName = "model",
                ActiveDomain = { this.domain }
            };

            var siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory",
                Domain = { new DomainOfExpertise() }
            };

            siteDirectory.Domain.Add(this.domain);
            siteDirectory.Model.Add(this.model);

            this.permissionService.Setup(x => x.CanWrite(this.domain.ClassKind, this.domain.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new ActiveDomainsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyActiveDomainRowProperties()
        {
            this.viewModel.InitializeViewModel(this.model);
            var activeDomainRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(activeDomainRow.ContainerName, Is.EqualTo("siteDirectory"));
                Assert.That(activeDomainRow.Name, Is.EqualTo(this.domain.Name));
                Assert.That(activeDomainRow.ShortName, Is.EqualTo(this.domain.ShortName));
                Assert.That(activeDomainRow.Thing, Is.EqualTo(this.domain));
                Assert.That(activeDomainRow.IsAllowedToWrite, Is.EqualTo(true));
            });
        }

        [Test]
        public async Task VerifyActiveDomainsEdit()
        {
            this.viewModel.InitializeViewModel(this.model);

            await this.viewModel.EditActiveDomains();
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);

            await this.viewModel.EditActiveDomains();

            Assert.Multiple(() => { this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once); });

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            await this.viewModel.EditActiveDomains();
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.domain));
                Assert.That(this.viewModel.DomainsOfExpertise, Has.Count.EqualTo(2));
                Assert.That(this.viewModel.SelectedDomainsOfExpertise, Is.Not.Null);
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel(this.model);

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var domainTest = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "domain A",
                ShortName = "domainA",
                Container = new SiteDirectory { Name = "newSite" }
            };

            this.messageBus.SendObjectChangeEvent(domainTest, EventKind.Added);
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
                Assert.That(this.viewModel.SelectedDomainsOfExpertise.ToList(), Has.Count.EqualTo(1));
            });
        }
    }
}
