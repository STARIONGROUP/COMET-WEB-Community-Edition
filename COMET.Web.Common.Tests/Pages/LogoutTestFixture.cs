// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LogoutTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Pages
{
    using Bunit;

    using COMET.Web.Common.Pages;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class LogoutTestFixture
    {
        private Mock<IAuthenticationService> authenticationService;
        private BunitContext context;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.authenticationService.Object);
        }

        [Test]
        public void VerifyLogout()
        {
            var navigation = this.context.Services.GetService<NavigationManager>();
            navigation.NavigateTo("/Logout");
            this.context.Render<Logout>();

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.Logout(), Times.Once);
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
                Assert.That(navigation.Uri, Does.Not.Contain("Logout"));
            });

            const string externalLogoutUrl = "http://localhost:8080/realms/test/protocol/openid-connect/logout?client_id=test&post_logout_redirect_uri=http%3A%2F%2Flocalhost%2FLogout%3Fconfirmed%3Dtrue";
            this.authenticationService.Setup(x => x.BuildExternalProviderLogoutUrl("http://localhost/Logout?confirmed=true")).Returns(externalLogoutUrl);

            navigation.NavigateTo("/Logout");
            this.context.Render<Logout>();

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.Logout(), Times.Once);
                Assert.That(navigation.Uri, Is.EqualTo(externalLogoutUrl));
            });

            navigation.NavigateTo("http://localhost/Logout?confirmed=true");
            this.context.Render<Logout>();

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.Logout(), Times.Exactly(2));
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });
        }
    }
}
