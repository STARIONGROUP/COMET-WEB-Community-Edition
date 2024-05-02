// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileRevisionHandlerViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel.FileStore
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileRevisionHandlerViewModelTestFixture
    {
        private FileRevisionHandlerViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISessionService> sessionService;
        private Mock<ILogger<FileRevisionHandlerViewModel>> logger;
        private Mock<IJsUtilitiesService> jsUtilitiesService;
        private CommonFileStore commonFileStore;
        private File file;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.logger = new Mock<ILogger<FileRevisionHandlerViewModel>>();
            this.jsUtilitiesService = new Mock<IJsUtilitiesService>();

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    [Constants.MaxUploadFileSizeInMbConfigurationKey] = "500"
                })
                .Build();

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
                },
            };

            siteDirectory.SiteReferenceDataLibrary.Add(new SiteReferenceDataLibrary()
            {
                FileType =
                {
                    new FileType()
                    {
                        Extension = "txt",
                        Name = "text/txt",
                        ShortName = "text/txt"
                    }
                }
            });

            var engineeringModelSetup = new EngineeringModelSetup()
            {
                Participant = { new Participant { Person = person } }
            };

            var folder1 = new Folder()
            {
                Iid = Guid.NewGuid(),
                Name = "folder 1"
            };

            this.file = new File()
            {
                Iid = Guid.NewGuid(), 
                CurrentContainingFolder = folder1
            };

            var fileRevision = new FileRevision() { Name = "file rev 1", };
            fileRevision.FileType.AddRange(siteDirectory.SiteReferenceDataLibrary.First().FileType);
            this.file.FileRevision.Add(fileRevision);

            this.commonFileStore = new CommonFileStore()
            {
                Name = "CFS",
                Folder = { folder1 },
                File = { this.file },
                Owner = siteDirectory.Domain.First(),
                Container = new EngineeringModel { EngineeringModelSetup = engineeringModelSetup }
            };

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session.ActivePerson).Returns(person);
            this.viewModel = new FileRevisionHandlerViewModel(this.sessionService.Object, this.messageBus, this.logger.Object, this.jsUtilitiesService.Object, configuration);
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
            this.viewModel.InitializeViewModel(this.file, this.commonFileStore);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentFile, Is.EqualTo(this.file));
                Assert.That(this.viewModel.FileRevision, Is.Not.Null);
                Assert.That(this.viewModel.ErrorMessage, Is.Empty);
            });
        }

        [Test]
        public async Task VerifyUploadFile()
        {
            this.viewModel.InitializeViewModel(this.file, this.commonFileStore);

            Assert.That(this.viewModel.ErrorMessage, Is.Empty);

            var fileMock = new Mock<IBrowserFile>();
            fileMock.Setup(x => x.Size).Returns(1000 * 1024 * 1024);
            await this.viewModel.UploadFile(fileMock.Object);
            Assert.That(this.viewModel.ErrorMessage, Is.Not.Empty);

            fileMock.Setup(x => x.Size).Returns(1);
            fileMock.Setup(x => x.Name).Returns("file.txt");
            fileMock.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(new MemoryStream());
            await this.viewModel.UploadFile(fileMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ErrorMessage, Is.Empty);
                Assert.That(this.viewModel.FileRevision.Name, Is.EqualTo("file"));
                Assert.That(this.viewModel.FileRevision.FileType.First().Extension, Is.EqualTo("txt"));
            });
        }

        [Test]
        public async Task VerifyDownloadFile()
        {
            this.viewModel.InitializeViewModel(this.file, this.commonFileStore);
            await this.viewModel.DownloadFileRevision(this.file.CurrentFileRevision);
            this.jsUtilitiesService.Verify(x => x.DownloadFileFromStreamAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);

            this.sessionService.Setup(x => x.Session.ReadFile(It.IsAny<FileRevision>())).Throws(new Exception());
            await this.viewModel.DownloadFileRevision(this.file.CurrentFileRevision);
            this.jsUtilitiesService.Verify(x => x.DownloadFileFromStreamAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
        }
    }
}
