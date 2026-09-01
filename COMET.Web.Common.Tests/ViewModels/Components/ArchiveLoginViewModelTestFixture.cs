// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveLoginViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.ViewModels.Components
{
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using FluentResults;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ArchiveLoginViewModelTestFixture
    {
        private ArchiveLoginViewModel viewModel;
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IArchiveFileService> archiveFileService;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.archiveFileService = new Mock<IArchiveFileService>();
            this.viewModel = new ArchiveLoginViewModel(this.authenticationService.Object, this.archiveFileService.Object);
        }

        [Test]
        public async Task VerifyExecuteLogin()
        {
            await this.viewModel.ExecuteLogin();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AuthenticationResult.IsFailed, Is.True);
                Assert.That(this.viewModel.IsLoading, Is.False);
                this.archiveFileService.Verify(x => x.PersistAsync(It.IsAny<IBrowserFile>()), Times.Never);
                this.authenticationService.Verify(x => x.LoginFromArchive(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            });

            var selectedFile = new Mock<IBrowserFile>();
            this.viewModel.SelectedFile = selectedFile.Object;
            this.archiveFileService.Setup(x => x.PersistAsync(selectedFile.Object)).ReturnsAsync(Result.Fail<string>("could not persist"));

            await this.viewModel.ExecuteLogin();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AuthenticationResult.IsFailed, Is.True);
                this.authenticationService.Verify(x => x.LoginFromArchive(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            });

            const string archivePath = "C:\\temp\\x.zip";
            this.viewModel.SelectedFile = selectedFile.Object;
            this.viewModel.AuthenticationDto.UserName = "user";
            this.viewModel.AuthenticationDto.Password = "pass";
            this.archiveFileService.Setup(x => x.PersistAsync(selectedFile.Object)).ReturnsAsync(Result.Ok(archivePath));
            this.authenticationService.Setup(x => x.LoginFromArchive(archivePath, "user", "pass")).ReturnsAsync(Result.Ok);

            await this.viewModel.ExecuteLogin();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AuthenticationResult.IsSuccess, Is.True);
                this.authenticationService.Verify(x => x.LoginFromArchive(archivePath, "user", "pass"), Times.Once);
            });

            this.viewModel.SelectedFile = selectedFile.Object;
            this.viewModel.AuthenticationDto.UserName = "user";
            this.viewModel.AuthenticationDto.Password = "pass";
            this.authenticationService.Setup(x => x.LoginFromArchive(archivePath, "user", "pass")).ReturnsAsync(Result.Fail("invalid credentials"));

            await this.viewModel.ExecuteLogin();

            Assert.That(this.viewModel.AuthenticationResult.IsFailed, Is.True);
        }
    }
}
