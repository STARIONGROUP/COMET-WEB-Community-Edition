﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FolderHandlerViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel.FileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FolderHandlerViewModelTestFixture
    {
        private FolderHandlerViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISessionService> sessionService;
        private CommonFileStore commonFileStore;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();

            var person = new Person();

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

            var engineeringModelSetup = new EngineeringModelSetup()
            {
                Participant = { new Participant { Person = person } }
            };

            var folder1 = new Folder()
            {
                Iid = Guid.NewGuid(),
                Name = "folder 1"
            };

            var folder2 = new Folder()
            {
                Iid = Guid.NewGuid(),
                Name = "folder 2"
            };

            var file = new File()
            {
                Iid = Guid.NewGuid(), 
                CurrentContainingFolder = folder1
            };

            file.FileRevision.Add(new FileRevision());

            this.commonFileStore = new CommonFileStore()
            {
                Name = "CFS",
                Folder = { folder1, folder2 },
                File = { file },
                Owner = siteDirectory.Domain.First(),
                Container = new EngineeringModel { EngineeringModelSetup = engineeringModelSetup }
            };

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session.ActivePerson).Returns(person);
            this.viewModel = new FolderHandlerViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Folders, Has.Count.EqualTo(this.commonFileStore.Folder.Count + 1));
                Assert.That(this.viewModel.Folders, Contains.Item(null));
                Assert.That(this.viewModel.DomainsOfExpertise, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyMoveAndDeleteFolder()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore);
            await this.viewModel.MoveFolder(this.commonFileStore.Folder[0], this.commonFileStore.Folder[1]);
            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<FileStore>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Once);

            this.viewModel.SelectFolder(this.commonFileStore.Folder[0]);
            await this.viewModel.DeleteFolder();
            this.sessionService.Verify(x => x.DeleteThings(It.IsAny<FileStore>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Once);
        }

        [Test]
        public async Task VerifyCreateOrEditFolder()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore);
            this.viewModel.SelectFolder(this.commonFileStore.Folder[0]);
            await this.viewModel.CreateOrEditFolder(false);
            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<FileStore>(), It.Is<IReadOnlyCollection<Thing>>(c => !c.OfType<FileStore>().Any())), Times.Once);

            await this.viewModel.CreateOrEditFolder(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<FileStore>(), It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<FileStore>().Any())), Times.Once);
        }
    }
}