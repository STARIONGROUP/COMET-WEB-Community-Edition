// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Exceptions;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.SessionManagement;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AuthenticationServiceTestFixture
    {
        private Mock<ISession> session;
        private Mock<ISessionService> sessionService;
        private CometWebAuthStateProvider cometWebAuthStateProvider;
        private AuthenticationDto authenticationDto;
        private Person person;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.sessionService = new Mock<ISessionService>();

            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.SetupProperty(x => x.IsSessionOpen);

            this.cometWebAuthStateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
            
            this.authenticationDto = new AuthenticationDto
            {
                SourceAddress = "https://www.rheagroup.com",
                UserName = "John Doe",
                Password = "secret"
            };

            this.person = new Person
            {
                GivenName = "John",
                Surname = "Doe"
            };

            this.messageBus = new CDPMessageBus();
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task Verify_that_a_logged_in_user_can_logout()
        {
            var authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.messageBus);

            await authenticationService.Logout();

            this.sessionService.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public async Task Verify_that_a_nonauthorized_user_cannot_login()
        {
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns((SiteDirectory)null);

            var authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.messageBus);

            var loginResult = await authenticationService.Login(this.authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_an_authorized_user_can_login()
        {
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            var siteDirectory = new SiteDirectory();

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.messageBus);

            var loginResult = await authenticationService.Login(this.authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Success));
        }

        [Test]
        public async Task Verify_that_when_the_server_cannot_be_reached_the_login_fails()
        {
            this.session.Setup(x => x.Open(It.IsAny<bool>())).Throws(new DalReadException());

            var authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.messageBus);

            var authentication = new AuthenticationDto
            {
                SourceAddress = "https://www.rheagroup",
                UserName = "John Doe",
                Password = "secret"
            };

            var loginResult = await authenticationService.Login(authentication);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_when_the_server_returns_an_error_the_login_fails()
        {
            this.session.Setup(x => x.Open(It.IsAny<bool>())).Throws(new DalReadException());

            var authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.messageBus);

            var loginResult = await authenticationService.Login(this.authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_when_the_source_address_is_null_authentication_fails()
        {
            this.authenticationDto.SourceAddress = null;

            var authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.messageBus);

            var loginResult = await authenticationService.Login(this.authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }
    }
}
