// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CallbackTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2025 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

    using TestContext = Bunit.TestContext;
    
    [TestFixture]
    public class CallbackTestFixture
    {
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IConfigurationService> configurationService;
        private TestContext context;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.configurationService = new Mock<IConfigurationService>();
            var serverConfig = new ServerConfiguration();
            this.configurationService.Setup(x => x.ServerConfiguration).Returns(serverConfig);
        
            this.context = new TestContext();
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
            _ = this.context.RenderComponent<Callback>();

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrl(), Times.Never);
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
            
            _ = this.context.RenderComponent<Callback>();
            var navigation = this.context.Services.GetService<NavigationManager>();
            navigation.NavigateTo(uri);
            
            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrl(), Times.Once);
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            const string cometUrl = "http://localhost:5000";
            this.authenticationService.Setup(x => x.RetrieveLastUsedServerUrl()).ReturnsAsync(cometUrl);
            
            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationScheme(cometUrl, false))
                .ReturnsAsync(Result.Fail<AuthenticationSchemeResponse>("failed"));
            
            navigation.NavigateTo(uri);
            
            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrl(), Times.Exactly(2));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationScheme(cometUrl, false), Times.Once);
                
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCode(It.IsAny<string>(), 
                    It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            var authenticationResponse = new AuthenticationSchemeResponse()
            {
                Schemes = [AuthenticationSchemeKind.Basic]
            };

            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationScheme(cometUrl, false))
                .ReturnsAsync(Result.Ok(authenticationResponse));

            navigation.NavigateTo(uri);

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrl(), Times.Exactly(3));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationScheme(cometUrl, false), Times.Exactly(2));
                
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCode(It.IsAny<string>(), 
                    It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });

            authenticationResponse.Schemes = [AuthenticationSchemeKind.ExternalJwtBearer];
            authenticationResponse.Authority = queryParameters["iss"];
            authenticationResponse.ClientId = "auth";

            this.authenticationService.Setup(x => x.ExchangeOpenIdConnectCode(cometUrl, It.IsAny<AuthenticationSchemeResponse>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            
            navigation.NavigateTo(uri);

            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.RetrieveLastUsedServerUrl(), Times.Exactly(4));
                this.authenticationService.Verify(x => x.RequestAvailableAuthenticationScheme(cometUrl, false), Times.Exactly(3));
                
                this.authenticationService.Verify(x => x.ExchangeOpenIdConnectCode(queryParameters["code"], 
                    authenticationResponse, It.IsAny<string>(), null), Times.Once);
                
                Assert.That(navigation.Uri, Is.EqualTo("http://localhost/"));
            });
        }
    }
}
