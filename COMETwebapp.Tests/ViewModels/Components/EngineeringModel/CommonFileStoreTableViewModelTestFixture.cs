﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CommonFileStoreTableViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CommonFileStoreTableViewModelTestFixture
    {
        private CommonFileStoreTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IFolderFileStructureViewModel> folderFileStructureViewModel;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<CommonFileStoreTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private CommonFileStore commonFileStore;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.folderFileStructureViewModel = new Mock<IFolderFileStructureViewModel>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<CommonFileStoreTableViewModel>>();

            var siteDirectory = new SiteDirectory
            {
                Domain =
                {
                    new DomainOfExpertise
                    {
                        ShortName = "doe",
                        Name = "Domain Of Expertise"
                    }
                }
            };

            this.commonFileStore = new CommonFileStore
            {
                Name = "CFS",
                Owner = siteDirectory.Domain.First()
            };

            this.iteration = new Iteration();
            var engineeringModel = new EngineeringModel { EngineeringModelSetup = new EngineeringModelSetup() };
            engineeringModel.CommonFileStore.Add(this.commonFileStore);
            engineeringModel.Iteration.Add(this.iteration);

            this.permissionService.Setup(x => x.CanWrite(this.commonFileStore.ClassKind, this.commonFileStore.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new CommonFileStoreTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object, this.folderFileStructureViewModel.Object);
            this.viewModel.SetCurrentIteration(this.iteration);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public async Task VerifyCommonFileStoreCreateOrEdit()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.CurrentThing = this.commonFileStore;

            await this.viewModel.CreateOrEditCommonFileStore(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<EngineeringModel>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception());
            this.viewModel.CurrentThing = new CommonFileStore();
            await this.viewModel.CreateOrEditCommonFileStore(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel();
            var firstRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.IsPrivate, Is.EqualTo(false));
                Assert.That(firstRow.Thing, Is.EqualTo(this.commonFileStore));
                Assert.That(firstRow.CreatedOn, Is.EqualTo(this.commonFileStore.CreatedOn));
            });
        }
    }
}
