﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FolderFileStructureViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel.FileStore
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FolderFileStructureViewModelTestFixture
    {
        private FolderFileStructureViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISessionService> sessionService;
        private Mock<IFileHandlerViewModel> fileHandlerViewModel;
        private Mock<IFolderHandlerViewModel> folderHandlerViewModel;
        private CommonFileStore commonFileStore;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.fileHandlerViewModel = new Mock<IFileHandlerViewModel>();
            this.folderHandlerViewModel = new Mock<IFolderHandlerViewModel>();
            this.iteration = new Iteration();

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

            var folder1 = new Folder
            {
                Iid = Guid.NewGuid(),
                Name = "folder 1"
            };

            var file = new File
            {
                Iid = Guid.NewGuid(),
                CurrentContainingFolder = folder1
            };

            file.FileRevision.Add(new FileRevision());

            this.commonFileStore = new CommonFileStore
            {
                Name = "CFS",
                Owner = siteDirectory.Domain.First()
            };

            this.commonFileStore.Folder.Add(folder1);
            this.commonFileStore.File.Add(file);

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.viewModel = new FolderFileStructureViewModel(this.sessionService.Object, this.messageBus, this.fileHandlerViewModel.Object, this.folderHandlerViewModel.Object);
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
            this.viewModel.InitializeViewModel(this.commonFileStore, this.iteration);

            /*
             * Here the folder-file structure is:
             * - Root
             *      - Folder 1
             *          - Folder 2
             *          - File
             */
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Structure, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.Structure.First().Content, Has.Count.EqualTo(2));
                this.fileHandlerViewModel.Verify(x => x.InitializeViewModel(this.commonFileStore, this.iteration));
                this.folderHandlerViewModel.Verify(x => x.InitializeViewModel(this.commonFileStore, this.iteration));
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore, this.iteration);
            var rootNodeContent = this.viewModel.Structure.First().Content;
            Assert.That(rootNodeContent, Has.Count.EqualTo(2));

            this.messageBus.SendObjectChangeEvent(this.commonFileStore, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.Multiple(() =>
            {
                this.fileHandlerViewModel.VerifySet(x => x.Folders = It.IsAny<IEnumerable<Folder>>(), Times.Exactly(2));
                this.folderHandlerViewModel.VerifySet(x => x.Folders = It.IsAny<IEnumerable<Folder>>(), Times.Exactly(2));
            });

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(rootNodeContent, Has.Count.EqualTo(2));

            var newFile = new File
            {
                Container = this.commonFileStore
            };

            this.messageBus.SendObjectChangeEvent(newFile, EventKind.Added);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.Multiple(() =>
            {
                Assert.That(rootNodeContent.Select(x => x.Thing), Contains.Item(newFile));
                Assert.That(rootNodeContent, Has.Count.EqualTo(3));
            });

            var existingFile = this.commonFileStore.File.First();
            existingFile.LockedBy = new Person { ShortName = "locker" };
            this.messageBus.SendObjectChangeEvent(newFile, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.Multiple(() =>
            {
                Assert.That(rootNodeContent.Select(x => x.Thing).OfType<File>().Any(x => x.LockedBy.ShortName == "locker"), Is.EqualTo(true));
                Assert.That(rootNodeContent, Has.Count.EqualTo(3));
            });

            this.messageBus.SendObjectChangeEvent(newFile, EventKind.Removed);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.Multiple(() =>
            {
                Assert.That(rootNodeContent.Select(x => x.Thing), Does.Not.Contain(newFile));
                Assert.That(rootNodeContent, Has.Count.EqualTo(2));
            });
        }
    }
}
