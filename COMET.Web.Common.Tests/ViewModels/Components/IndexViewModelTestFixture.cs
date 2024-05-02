// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.ViewModels.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IndexViewModelTestFixture
    {
        private IndexViewModel viewModel;
        private Mock<IVersionService> versionService;
        private Mock<ISessionService> sessionService;
        private Mock<IAuthenticationService> authenticationService;
        private const string Version = "1.1.2";

        [SetUp]
        public void Setup()
        {
            this.versionService = new Mock<IVersionService>();
            this.sessionService = new Mock<ISessionService>();
            this.authenticationService = new Mock<IAuthenticationService>();
            this.versionService.Setup(x => x.GetVersion()).Returns(Version);

            this.viewModel = new IndexViewModel(this.versionService.Object, this.sessionService.Object, this.authenticationService.Object);
        }

        [Test]
        public async Task VerifyLogout()
        {
            await this.viewModel.Logout();
            this.authenticationService.Verify(x => x.Logout(), Times.Once);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Version, Is.EqualTo(Version));
                Assert.That(this.viewModel.SessionService, Is.Not.Null);
            });
        }
    }
}
