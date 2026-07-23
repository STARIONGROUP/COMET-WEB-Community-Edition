// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.IntegrationTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// End-to-end tests covering the unauthenticated landing page, login and logout. Each test uses its own fresh
    /// browser (these tests need an unauthenticated starting state).
    /// </summary>
    [TestFixture]
    public class LoginTestFixture : E2ETestBase
    {
        [SetUp]
        public Task SetUp()
        {
            return this.StartBrowserAsync();
        }

        [TearDown]
        public Task TearDown()
        {
            return this.StopBrowserAsync();
        }

        [Test]
        public async Task VerifyUnauthenticatedLandingPage()
        {
            await this.Login.NavigateAsync(AppUrl);

            await Expect(this.Page).ToHaveTitleAsync("CDP4-COMET Community Edition");
            await Expect(this.Login.UnauthorizedNotice).ToHaveTextAsync("Connect and Open a Model.");
        }

        [Test]
        public async Task VerifyUserCanLogin()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Login.LoginAsync(ServerUrl, Username, Password);

            await Expect(this.Home.SessionSidebar).ToBeVisibleAsync();
        }

        [Test]
        public async Task VerifyUserCanRefreshSession()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Login.LoginAsync(ServerUrl, Username, Password);

            await this.Home.RefreshSessionAsync();

            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        [Test]
        public async Task VerifyUserCanLogout()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Login.LoginAsync(ServerUrl, Username, Password);

            await this.Home.LogoutAsync();

            await Expect(this.Login.UnauthorizedNotice).ToHaveTextAsync("Connect and Open a Model.");
        }
    }
}
