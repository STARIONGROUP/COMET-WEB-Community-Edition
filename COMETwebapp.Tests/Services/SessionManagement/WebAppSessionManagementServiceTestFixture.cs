// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="WebAppSessionManagementServiceTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Services.SessionManagement
{
    using Blazored.SessionStorage;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class WebAppSessionManagementServiceTestFixture
    {
        private WebAppSessionManagementService service;
        private CometWebAuthStateProvider cometWebAuthStateProvider;
        private Mock<ISessionService> sessionService;
        private Mock<ISessionStorageService> sessionStorageService;

        [SetUp]
        public void Setup()
        {
            var activePerson = new Person
            {
                GivenName = "John",
                Surname = "Doe",
                Role = new PersonRole { ShortName = "personRole" }
            };

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session.ActivePerson).Returns(activePerson);
            this.cometWebAuthStateProvider = new CometWebAuthStateProvider(this.sessionService.Object);

            this.sessionStorageService = new Mock<ISessionStorageService>();
            this.service = new WebAppSessionManagementService(this.cometWebAuthStateProvider, this.sessionStorageService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.service.Dispose();
        }

        [Test]
        public async Task VerifyDispose()
        {
            this.service.Dispose();
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(false);
            this.cometWebAuthStateProvider.NotifyAuthenticationStateChanged();

            await Task.Delay(100);
            this.sessionStorageService.Verify(x => x.RemoveItemAsync(WebAppConstantValues.SavedTabsKey, CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task VerifyOnAuthenticationStateChanged()
        {
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.cometWebAuthStateProvider.NotifyAuthenticationStateChanged();

            await Task.Delay(100);
            this.sessionStorageService.Verify(x => x.RemoveItemAsync(WebAppConstantValues.SavedTabsKey, CancellationToken.None), Times.Never);

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(false);
            this.cometWebAuthStateProvider.NotifyAuthenticationStateChanged();

            await Task.Delay(100);
            this.sessionStorageService.Verify(x => x.RemoveItemAsync(WebAppConstantValues.SavedTabsKey, CancellationToken.None), Times.Once);
        }
    }
}
