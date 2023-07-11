// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfigurationServiceTestFixture.cs" company="RHEA System S.A.">
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
namespace COMET.Web.Common.Tests.Services.ConfigurationService
{
    using System.Net;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;
    using Moq.Protected;

    using NUnit.Framework;

    [TestFixture]
	public class ConfigurationServiceTestFixture
    {
        private IConfigurationService configurationService;

        [Test]
        public async Task VerifyService()
        {
            var globalOptions = new GlobalOptions
            {
                JsonConfigurationFile = null,
            };

            var options = new Mock<IOptions<GlobalOptions>>();
            options.Setup(x => x.Value).Returns(globalOptions);

            var handlerMock = new Mock<HttpMessageHandler>();

            var json = """ 
                        {
                            "OpenEngineeringModelPlaceholder": "Select an Engineering Model",
                            "OpenIterationPlaceholder": "Select an Iteration",
                            "OpenDomainOfExpertisePlaceholder": "Select a Domain of Expertise",
                            "ModelTitleCaption" : "Model",
                            "IterationTitleCaption" : "Iteration",
                            "DomainTitleCaption" : "Domain",
                            "LandingPageTitle" : ""
                        }
                        """;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var loggerMock = new Mock<ILogger<ConfigurationService>>();
            this.configurationService = new ConfigurationService(options.Object, httpClient, loggerMock.Object);

            Assert.Multiple(() =>
            {
                Assert.That(this.configurationService, Is.Not.Null);
                Assert.Throws<InvalidOperationException>(()=>this.configurationService.GetText(""));
            });

            await this.configurationService.InitializeService();

            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => this.configurationService.GetText(""));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.OpenEngineeringModelPlaceholder), Is.EqualTo("Select an Engineering Model"));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.OpenIterationPlaceholder), Is.EqualTo("Select an Iteration"));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.OpenDomainOfExpertisePlaceholder), Is.EqualTo("Select a Domain of Expertise"));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.ModelTitleCaption), Is.EqualTo("Model"));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.IterationTitleCaption), Is.EqualTo("Iteration"));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.DomainTitleCaption), Is.EqualTo("Domain"));
                Assert.That(this.configurationService.GetText(TextConfigurationKind.LandingPageTitle), Is.EqualTo(""));
                Assert.That(this.configurationService.GetConfigurations(), Is.Not.Null);
                Assert.That(this.configurationService.GetConfigurations(), Is.Not.Empty);
            });
        }

        [Test]
        public async Task VerifyExceptionIsNotThrown()
        {
            var globalOptions = new GlobalOptions
            {
                JsonConfigurationFile = null,
            };

            var options = new Mock<IOptions<GlobalOptions>>();
            options.Setup(x => x.Value).Returns(globalOptions);

            var handlerMock = new Mock<HttpMessageHandler>();

            var Invalidjson = """ 
                        {
                            "OpenEngineeringModelPlaceholder": "Select an Engineering Model",
                            "OpenIterationPlaceholder": "Select an Iteration",
                            "OpenDomainOfExpertisePlaceholder": "Select a Domain of Expertise",
                            "ModelTitleCaption" : "Model",
                            "IterationTitleCaption" : "Iteration",
                            "DomainTitleCaption" : "Domain",
                            "LandingPageTitle" : 1
                        }
                        """;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Invalidjson)
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var loggerMock = new Mock<ILogger<ConfigurationService>>();
            this.configurationService = new ConfigurationService(options.Object, httpClient, loggerMock.Object);

            await this.configurationService.InitializeService();

            Assert.Multiple(() =>
            {
                Assert.That(this.configurationService.GetConfigurations(), Is.Not.Null);
                Assert.That(this.configurationService.GetConfigurations(), Is.Empty);
            });
        }
	}
}
