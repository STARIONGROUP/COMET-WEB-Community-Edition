// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfigurationServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.WebAssembly.Services.ConfigurationService
{
    using System.Net;

    using Castle.Core.Logging;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.WebAssembly.Services.ConfigurationService;
    using COMET.Web.Common.Test.Helpers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using Moq;

    using NUnit.Framework;

    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConfigurationServiceTestFixture
    {
        private ConfigurationService configurationService;
        private MockHttpMessageHandler mockHttpMessageHandler;
        private Mock<IOptions<GlobalOptions>> options;
        private GlobalOptions globalOptions;
        private Mock<ILogger<ConfigurationService>> logger;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new MockHttpMessageHandler();
            var httpClient = this.mockHttpMessageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost/");
            this.options = new Mock<IOptions<GlobalOptions>>();
            this.globalOptions = new GlobalOptions();
            this.options.Setup(x => x.Value).Returns(this.globalOptions);
            this.logger = new Mock<ILogger<ConfigurationService>>();
            this.configurationService = new ConfigurationService(this.options.Object, httpClient, this.logger.Object);
        }

        [Test]
        public async Task VerifiyInitialization()
        {
            this.mockHttpMessageHandler.When(HttpMethod.Get, "/_content/CDP4.WEB.Common/server_configuration.json")
                .Throw(new Exception());

            await this.configurationService.InitializeService();
            this.logger.Verify(LogLevel.Critical, o => o!.ToString()!.Contains("Exception has been raised"), Times.Once());

            this.mockHttpMessageHandler.ResetBackendDefinitions();

            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            this.mockHttpMessageHandler.When(HttpMethod.Get, "/_content/CDP4.WEB.Common/server_configuration.json")
                .Respond(_ => httpResponse);

            await this.configurationService.InitializeService();
            this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Error fetching server configuration. Status code:"), Times.Once());

            httpResponse.StatusCode = HttpStatusCode.NotFound;

            await this.configurationService.InitializeService();
            this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Server configuration file not found at "), Times.Once());

            httpResponse.StatusCode = HttpStatusCode.OK;
            httpResponse.Content = new StringContent("{\"ServerAddress\":\"http://localhost\"}");
            await this.configurationService.InitializeService();
            Assert.That(this.configurationService.ServerConfiguration.ServerAddress, Is.EqualTo("http://localhost"));
        }
    }
}
