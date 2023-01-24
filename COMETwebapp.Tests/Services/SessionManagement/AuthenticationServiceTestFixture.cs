// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Services.SessionManagement
{
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Exceptions;
    using COMETwebapp.Enumerations;
    using COMETwebapp.Model.DTO;
    using COMETwebapp.SessionManagement;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AuthenticationServiceTestFixture
    {
        private Mock<ISession> session;

        private Mock<ISessionAnchor> sessionAnchor;

        private CometWebAuthStateProvider cometWebAuthStateProvider;

        private AuthenticationDto authenticationDto;

        private Person person;

        [SetUp]
        public void SetUp()
        {
            session = new Mock<ISession>();
            sessionAnchor = new Mock<ISessionAnchor>();

            sessionAnchor.Setup(x => x.Session).Returns(session.Object);
            sessionAnchor.SetupProperty(x => x.IsSessionOpen);

            cometWebAuthStateProvider = new CometWebAuthStateProvider(sessionAnchor.Object);

            authenticationDto = new AuthenticationDto
            {
                SourceAddress = "https://www.rheagroup.com",
                UserName = "John Doe",
                Password = "secret"
            };

            person = new Person
            {
                GivenName = "John",
                Surname = "Doe"
            };
        }

        [Test]
        public async Task Verify_that_when_the_source_address_is_null_authentication_fails()
        {
            authenticationDto.SourceAddress = null;

            var authenticationService = new AuthenticationService(sessionAnchor.Object, cometWebAuthStateProvider);

            var loginResult = await authenticationService.Login(authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_an_authorized_user_can_login()
        {
            session.Setup(x => x.ActivePerson).Returns(person);

            var siteDirectory = new SiteDirectory();

            sessionAnchor.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var authenticationService = new AuthenticationService(sessionAnchor.Object, cometWebAuthStateProvider);

            var loginResult = await authenticationService.Login(authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Success));
        }

        [Test]
        public async Task Verify_that_when_the_server_returns_an_error_the_login_fails()
        {
            session.Setup(x => x.Open(It.IsAny<bool>())).Throws(new DalReadException());

            var authenticationService = new AuthenticationService(sessionAnchor.Object, cometWebAuthStateProvider);

            var loginResult = await authenticationService.Login(authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_when_the_server_cannot_be_reached_the_login_fails()
        {
            session.Setup(x => x.Open(It.IsAny<bool>())).Throws(new DalReadException());

            var authenticationService = new AuthenticationService(sessionAnchor.Object, cometWebAuthStateProvider);
            var authenticationDto = new AuthenticationDto
            {
                SourceAddress = "https://www.rheagroup",
                UserName = "John Doe",
                Password = "secret"
            };
            var loginResult = await authenticationService.Login(authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_a_nonauthorized_user_cannot_login()
        {
            session.Setup(x => x.ActivePerson).Returns(person);

            SiteDirectory siteDirectory = null;

            sessionAnchor.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var authenticationService = new AuthenticationService(sessionAnchor.Object, cometWebAuthStateProvider);

            var loginResult = await authenticationService.Login(authenticationDto);

            Assert.That(loginResult, Is.EqualTo(AuthenticationStateKind.Fail));
        }

        [Test]
        public async Task Verify_that_a_logged_in_user_can_logout()
        {
            var authenticationService = new AuthenticationService(sessionAnchor.Object, cometWebAuthStateProvider);

            await authenticationService.Logout();

            sessionAnchor.Verify(x => x.Close(), Times.Once);
        }
    }
}
