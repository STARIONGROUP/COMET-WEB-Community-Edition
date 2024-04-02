// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ActiveDomainsTableViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

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
        private Assembler assembler;
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

            this.domain = new DomainOfExpertise()
            {
                Name = "domain A",
                ShortName = "domainA"
            };

            this.model = new EngineeringModelSetup()
            {
                Name = "model",
                ShortName = "model",
                ActiveDomain = { this.domain }
            };

            var siteDirectory = new SiteDirectory()
            {
                ShortName = "siteDirectory",
                Domain = { new DomainOfExpertise() }
            };

            siteDirectory.Domain.Add(this.domain);
            siteDirectory.Model.Add(this.model);

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyActiveDomain = new Lazy<Thing>(this.domain);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyActiveDomain);

            this.permissionService.Setup(x => x.CanWrite(this.domain.ClassKind, this.domain.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
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
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.domain));
                Assert.That(this.viewModel.DomainsOfExpertise, Has.Count.EqualTo(2));
                Assert.That(this.viewModel.SelectedDomainsOfExpertise, Is.Null);
            });
        }

        [Test]
        public void VerifyActiveDomainRowProperties()
        {
            this.viewModel.InitializeViewModel();
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
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel();

            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var domainTest = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                Name = "domain A",
                ShortName = "domainA",
                Container = new SiteDirectory(){ Name = "newSite" },
            };

            this.messageBus.SendObjectChangeEvent(domainTest, EventKind.Added);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifySetEngineeringModel()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SetEngineeringModel(this.model);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.SelectedDomainsOfExpertise.ToList(), Has.Count.EqualTo(1));
            });
        }
    }
}
