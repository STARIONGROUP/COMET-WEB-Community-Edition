// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StringTableServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.WebAssembly.Services.StringTableService
{
    using COMET.Web.Common.Model;
    using COMET.Web.Common.WebAssembly.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;

    using NUnit.Framework;

    using RichardSzalay.MockHttp;

    [TestFixture]
    public class StringTableServiceTestFixture
    {
        private StringTableService service;
        private MockHttpMessageHandler mockHttpMessageHandler;
        private Mock<ILogger<StringTableService>> logger;

        [SetUp]
        public void Setup()
        {
            var option = new Mock<IOptions<GlobalOptions>>();
            option.Setup(x => x.Value).Returns(new GlobalOptions());
            this.mockHttpMessageHandler= new MockHttpMessageHandler();
            var httpClient = this.mockHttpMessageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            this.logger = new Mock<ILogger<StringTableService>>();
            this.service = new StringTableService(option.Object, httpClient, this.logger.Object);
        }

        [Test]
        public async Task VerifyInitialization()
        {
            var httpResponse = new HttpResponseMessage();

            this.mockHttpMessageHandler.When(HttpMethod.Get, "/_content/CDP4.WEB.Common/DefaultTextConfiguration.json")
                .Respond(_ => httpResponse);

            await this.service.InitializeService();
            this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Error while getting the configuration file."), Times.Once());

            httpResponse.Content = new StringContent("{}");
            await this.service.InitializeService();
            this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Error while getting the configuration file."), Times.Once());
            await this.service.InitializeService();
            this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Error while getting the configuration file."), Times.Once());
        }
    }
}
