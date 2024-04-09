// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CommonFileStoreTableViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CommonFileStoreTableViewModelTestFixture
    {
        private CommonFileStoreTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Mock<ILogger<CommonFileStoreTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private CommonFileStore commonFileStore;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<CommonFileStoreTableViewModel>>();

            var siteDirectory = new SiteDirectory()
            {
                Domain =
                {
                    new DomainOfExpertise()
                    {
                        ShortName = "doe",
                        Name = "Domain Of Expertise"
                    }
                }
            };

            this.commonFileStore = new CommonFileStore()
            {
                Name = "CFS",
                Owner = siteDirectory.Domain.First()
            };

            var engineeringModel = new EngineeringModel();
            engineeringModel.CommonFileStore.Add(this.commonFileStore);

            this.iteration = new Iteration()
            {
                Container = engineeringModel
            };

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyCommonFileStore = new Lazy<Thing>(this.commonFileStore);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyCommonFileStore);

            this.permissionService.Setup(x => x.CanWrite(this.commonFileStore.ClassKind, this.commonFileStore.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new CommonFileStoreTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
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
            var firstRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.DomainsOfExpertise.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.IsPrivate, Is.EqualTo(false));
                Assert.That(firstRow.Thing, Is.EqualTo(this.commonFileStore));
                Assert.That(firstRow.CreatedOn, Is.EqualTo(this.commonFileStore.CreatedOn.ToString("dd/MM/yyyy HH:mm:ss")));
            });
        }

        [Test]
        public async Task VerifyCommonFileStoreCreateOrEdit()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SetCurrentIteration(this.iteration);
            this.viewModel.Thing = this.commonFileStore;

            await this.viewModel.CreateOrEditCommonFileStore(true);

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.UpdateThings(It.IsAny<EngineeringModel>(), It.IsAny<IEnumerable<Thing>>()), Times.Once);
                this.sessionService.Verify(x => x.RefreshSession(), Times.Once);
            });

            this.sessionService.Setup(x => x.UpdateThings(It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>())).Throws(new Exception());
            this.viewModel.Thing = new CommonFileStore();

            await this.viewModel.CreateOrEditCommonFileStore(false);
            this.sessionService.Verify(x => x.RefreshSession(), Times.Once);
        }
    }
}
