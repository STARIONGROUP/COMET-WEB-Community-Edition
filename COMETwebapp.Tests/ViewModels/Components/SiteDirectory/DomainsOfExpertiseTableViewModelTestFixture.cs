// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainsOfExpertiseTableViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SiteDirectory
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.SiteDirectory.DomainsOfExpertise;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DomainsOfExpertiseTableViewModelTestFixture
    {
        private DomainsOfExpertiseTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Mock<ILogger<DomainsOfExpertiseTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private DomainOfExpertise domainOfExpertise;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<DomainsOfExpertiseTableViewModel>>();

            this.domainOfExpertise = new DomainOfExpertise()
            {
                ShortName = "scale",
                Name = "scale",
            };

            var siteDirectory = new SiteDirectory()
            {
                ShortName = "siteDirectory"
            };

            siteDirectory.Domain.Add(this.domainOfExpertise);

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyDomain = new Lazy<Thing>(this.domainOfExpertise);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyDomain);

            this.permissionService.Setup(x => x.CanWrite(this.domainOfExpertise.ClassKind, this.domainOfExpertise.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new DomainsOfExpertiseTableViewModel(this.sessionService.Object, this.showHideService.Object, this.messageBus, this.loggerMock.Object);
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
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.domainOfExpertise));
            });
        }

        [Test]
        public void VerifyDomainRowProperties()
        {
            this.viewModel.InitializeViewModel();
            var domainRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(domainRow.ContainerName, Is.EqualTo("siteDirectory"));
                Assert.That(domainRow.Name, Is.EqualTo(this.domainOfExpertise.Name));
                Assert.That(domainRow.ShortName, Is.EqualTo(this.domainOfExpertise.ShortName));
                Assert.That(domainRow.Thing, Is.EqualTo(this.domainOfExpertise));
                Assert.That(domainRow.IsAllowedToWrite, Is.EqualTo(true));
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel();

            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var siteDirectory = new SiteDirectory()
            {
                ShortName = "newShortname"
            };

            var domainTest = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                Container = siteDirectory,
            };

            this.messageBus.SendObjectChangeEvent(domainTest, EventKind.Added);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.messageBus.SendObjectChangeEvent(siteDirectory, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(new PersonRole(), EventKind.Updated);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Items.First().ContainerName, Is.EqualTo(siteDirectory.ShortName));
                this.permissionService.Verify(x => x.CanWrite(domainTest.ClassKind, It.IsAny<Thing>()), Times.AtLeast(this.viewModel.Rows.Count));
            });
        }
        
        [Test]
         public async Task VerifyRowOperations()
         {
             this.viewModel.InitializeViewModel();
             var domainRow = this.viewModel.Rows.Items.First();
             domainRow.IsDeprecated = false;

             Assert.Multiple(() =>
             {
                 Assert.That(domainRow, Is.Not.Null);
                 Assert.That(this.viewModel.IsOnDeprecationMode, Is.EqualTo(false));
             });

             this.viewModel.OnDeprecateUnDeprecateButtonClick(domainRow);

             Assert.Multiple(() =>
             {
                 Assert.That(this.viewModel.IsOnDeprecationMode, Is.EqualTo(true));
                 Assert.That(this.viewModel.Thing, Is.EqualTo(domainRow.Thing));
             });
             
             this.viewModel.OnCancelPopupButtonClick();
             Assert.That(this.viewModel.IsOnDeprecationMode, Is.EqualTo(false));

             await this.viewModel.OnConfirmPopupButtonClick();
             this.sessionService.Verify(x => x.UpdateThings(It.IsAny<SiteDirectory>(), It.Is<DomainOfExpertise>(c => c.IsDeprecated == true)));

             this.viewModel.Thing.IsDeprecated = true;
             await this.viewModel.OnConfirmPopupButtonClick();
             this.sessionService.Verify(x => x.UpdateThings(It.IsAny<SiteDirectory>(), It.Is<DomainOfExpertise>(c => c.IsDeprecated == false)));
        }
    }
}
