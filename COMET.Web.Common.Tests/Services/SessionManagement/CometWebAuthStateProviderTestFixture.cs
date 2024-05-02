// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometWebAuthStateProviderTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Services.SessionManagement
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CometWebAuthStateProviderTestFixture
	{
        private CometWebAuthStateProvider cometWebAuthStateProvider;
        private Mock<ISessionService> sessionService;

		[SetUp]
        public void SetUp()
        {
            var activePerson = new Person
            {
                GivenName = "John",
                Surname = "Doe",
                Role = new PersonRole(){ ShortName = "personRole" }
            };

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session.ActivePerson).Returns(activePerson);

            this.cometWebAuthStateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
        }

        [Test]
        public async Task Verify_that_GetAuthenticationStateAsync_returns_an_AuthenticationState()
        {
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);

            var authenticationState = await this.cometWebAuthStateProvider.GetAuthenticationStateAsync();

            Assert.That(authenticationState, Is.Not.Null);

            Assert.That(authenticationState.User.Identity?.Name, Is.EqualTo("John Doe"));
        }
    }
}
