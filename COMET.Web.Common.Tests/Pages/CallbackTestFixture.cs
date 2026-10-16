// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CallbackTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Pages
{
    using Bunit;

    using System.Collections.Specialized;

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Pages;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using FluentResults;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    
    [TestFixture]
    public class CallbackTestFixture
    {
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IConfigurationService> configurationService;
        private BunitContext context;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.configurationService = new Mock<IConfigurationService>();
            var serverConfig = new ServerConfiguration();
            this.configurationService.Setup(x => x.ServerConfiguration).Returns(serverConfig);
        
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.configurationService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.Dispose();
        }
        
        [Test]
        public void VerifyCallbackWithoutParams()
        {
            var navigation = this.context.Services.GetService<NavigationManager>();
            navigation.NavigateTo("/callback");
            _ = this.context.Render<Callback>();

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrlAsync(), Times.Never);
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });
        }

        [Test]
        public void VerifyCallbackWithParams()
        {
            var uri = "/callback";

            var queryParameters = new NameValueCollection
            {
                ["code"] = "abc",
                ["session_state"] = "state",
                ["iss"] = "http://localhost:8080/realms/ARealm"
            };

            uri += $"?{string.Join("&", queryParameters.AllKeys.Select(key => $"{key}={queryParameters[key]!}"))}";
            
            _ = this.context.Render<Callback>();
            var navigation = this.context.Services.GetService<NavigationManager>();
            navigation.NavigateTo(uri);
            
            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrlAsync(), Times.Once);
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            const string cometUrl = "http://localhost:5000";
            this.authenticationService.Setup(x => x.RetrieveLastUsedServerUrlAsync()).ReturnsAsync(cometUrl);
            
            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false))
                .ReturnsAsync(Result.Fail<AuthenticationSchemeResponse>("failed"));
            
            navigation.NavigateTo(uri);
            
            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrlAsync(), Times.Exactly(2));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false), Times.Once);
                
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCodeAsync(It.IsAny<string>(), 
                    It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            var authenticationResponse = new AuthenticationSchemeResponse()
            {
                Schemes = [AuthenticationSchemeKind.Basic]
            };

            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false))
                .ReturnsAsync(Result.Ok(authenticationResponse));

            navigation.NavigateTo(uri);

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrlAsync(), Times.Exactly(3));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false), Times.Exactly(2));
                
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCodeAsync(It.IsAny<string>(), 
                    It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            authenticationResponse.Schemes = [AuthenticationSchemeKind.ExternalJwtBearer];
            authenticationResponse.Authority = queryParameters["iss"];
            authenticationResponse.ClientId = "auth";

            this.authenticationService.Setup(x => x.ExchangeOpenIdConnectCodeAsync(It.IsAny<string>(), It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Ok());
            
            navigation.NavigateTo(uri);

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrlAsync(), Times.Exactly(4));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false), Times.Exactly(3));
                
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCodeAsync(queryParameters["code"], 
                    authenticationResponse, It.IsAny<string>(), null), Times.Once);
                
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            this.authenticationService.Setup(x => x.RetrieveLastUsedServerUrlAsync()).ReturnsAsync((string)null);
            this.configurationService.Object.ServerConfiguration.ServerAddress = cometUrl;

            navigation.NavigateTo(uri);

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrlAsync(), Times.Exactly(5));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false), Times.Exactly(4));
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCodeAsync(queryParameters["code"], authenticationResponse, It.IsAny<string>(), null), Times.Exactly(2));
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });
        }

        /// <summary>
        /// Verifies that a failed exchange of the OpenID Connect code is shown to the user instead of silently sending
        /// them back to the login page, which gave no indication that anything went wrong.
        /// </summary>
        [Test]
        public void VerifyCallbackShowsTheLoginFailure()
        {
            const string cometUrl = "http://localhost:5000";

            var authenticationResponse = new AuthenticationSchemeResponse
            {
                Schemes = [AuthenticationSchemeKind.ExternalJwtBearer],
                Authority = "http://localhost:8080/realms/ARealm",
                ClientId = "auth"
            };

            this.authenticationService.Setup(x => x.RetrieveLastUsedServerUrlAsync()).ReturnsAsync(cometUrl);

            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationSchemeAsync(cometUrl, false))
                .ReturnsAsync(Result.Ok(authenticationResponse));

            // The code of an OpenID Connect flow is single use, so a failed exchange must be attempted exactly once. Any
            // further attempt hangs here instead of failing again, so a page that retries (and re-renders itself into an
            // endless loop) is reported by the call count rather than by overflowing the stack of the test host.
            var exchangeCount = 0;

            this.authenticationService.Setup(x => x.ExchangeOpenIdConnectCodeAsync(It.IsAny<string>(), It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => ++exchangeCount == 1 ? Task.FromResult(Result.Fail("boom")) : new TaskCompletionSource<Result>().Task);

            var navigation = this.context.Services.GetService<NavigationManager>();
            navigation.NavigateTo("/callback?code=abc&session_state=state&iss=http://localhost:8080/realms/ARealm");

            var callback = this.context.Render<Callback>();

            callback.WaitForAssertion(() => Assert.That(callback.Instance.LoginFailure, Is.Not.Null));

            Assert.Multiple(() =>
            {
                Assert.That(exchangeCount, Is.EqualTo(1), "a failed login must not be retried by re-rendering the page");
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCodeAsync("abc", authenticationResponse, It.IsAny<string>(), null), Times.AtLeastOnce);
                Assert.That(callback.Instance.LoginFailure.IsFailed, Is.True);
                Assert.That(navigation.Uri, Does.Contain("/callback"));
                Assert.That(callback.FindAll("#callback-login-failure"), Has.Count.EqualTo(1));
                Assert.That(callback.Find("#callback-login-errors").TextContent, Does.Contain("boom"));
            });
        }
    }
}
