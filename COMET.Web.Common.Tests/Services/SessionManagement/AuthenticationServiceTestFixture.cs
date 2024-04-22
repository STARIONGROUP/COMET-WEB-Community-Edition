// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.Services.SessionManagement
{
    using CDP4Dal;
    using CDP4Dal.DAL;

    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.SessionManagement;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AuthenticationServiceTestFixture
    {
        private Mock<ISession> session;
        private Mock<ISessionService> sessionService;
        private CometWebAuthStateProvider cometWebAuthStateProvider;
        private AuthenticationService authenticationService;
        private AuthenticationDto authenticationDto;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.sessionService = new Mock<ISessionService>();

            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(false);

            this.cometWebAuthStateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
            this.authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider);

            this.authenticationDto = new AuthenticationDto
            {
                SourceAddress = "https://www.rheagroup.com",
                UserName = "John Doe",
                Password = "secret"
            };
        }

        [Test]
        public async Task VerifyLogout()
        {
            await this.authenticationService.Logout();
            this.sessionService.Verify(x => x.CloseSession(), Times.Once);
        }

        [Test]
        public async Task VerifyValidLogin()
        {
            this.sessionService.Setup(x => x.OpenSession(It.IsAny<Credentials>())).ReturnsAsync(Result.Ok);
            var loginResult = await this.authenticationService.Login(this.authenticationDto);

            Assert.That(loginResult.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public async Task VerifyInvalidLogin()
        {
            this.sessionService.Setup(x => x.OpenSession(It.IsAny<Credentials>())).ReturnsAsync(Result.Fail(["error"]));

            var loginResult = await this.authenticationService.Login(new AuthenticationDto());
            Assert.That(loginResult.IsSuccess, Is.EqualTo(false));

            loginResult = await this.authenticationService.Login(this.authenticationDto);
            Assert.That(loginResult.IsSuccess, Is.EqualTo(false));
        }
    }
}
