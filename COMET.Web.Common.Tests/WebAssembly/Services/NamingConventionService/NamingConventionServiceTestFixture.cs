// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NamingConventionServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.WebAssembly.Services.NamingConventionService;

using System.Net;

using COMET.Web.Common.Services.NamingConventionService;
using COMET.Web.Common.Test.Helpers;
using COMET.Web.Common.WebAssembly.Services.NamingConventionService;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using RichardSzalay.MockHttp;

[TestFixture]
public class NamingConventionServiceTestFixture
{
    private NamingConventionService<NamingConventionKindTestEnum> service;
    private MockHttpMessageHandler mockHttpMessageHandler;
    private Mock<ILogger<INamingConventionService<NamingConventionKindTestEnum>>> logger;

    [SetUp]
    public void Setup()
    {
        this.mockHttpMessageHandler = new MockHttpMessageHandler();
        var httpClient = this.mockHttpMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost/");
        this.logger = new Mock<ILogger<INamingConventionService<NamingConventionKindTestEnum>>>();
        this.service = new NamingConventionService<NamingConventionKindTestEnum>(this.logger.Object, httpClient);
    }

    [Test]
    public async Task VerifyService()
    {
        this.mockHttpMessageHandler.When(HttpMethod.Get, "/_content/CDP4.WEB.Common/naming_convention.json")
            .Throw(new Exception());

        await this.service.InitializeService();
        this.logger.Verify(LogLevel.Critical, o => o!.ToString()!.Contains("Exception has been raised"), Times.Once());

        this.mockHttpMessageHandler.ResetBackendDefinitions();

        var httpResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        this.mockHttpMessageHandler.When(HttpMethod.Get, "/_content/CDP4.WEB.Common/naming_convention.json")
            .Respond(_ => httpResponse);

        await this.service.InitializeService();
        this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Error fetching naming conventions. Status code:"), Times.Once());

        httpResponse.StatusCode = HttpStatusCode.NotFound;

        await this.service.InitializeService();
        this.logger.Verify(LogLevel.Error, o => o!.ToString()!.Contains("Naming conventions file not found at "), Times.Once());

        httpResponse.StatusCode = HttpStatusCode.OK;

        var json = """
                   {
                     "TestValue1": "TestValue1",
                     "TestValue2": "TestValue2"
                   }
                   """;

        httpResponse.Content = new StringContent(json);
        await this.service.InitializeService();
        
        var enumValues = Enum.GetValues<NamingConventionKindTestEnum>();

        Assert.Multiple(() =>
        {
            foreach (var namingConventionKind in enumValues)
            {
                Assert.That(this.service.GetNamingConventionValue(namingConventionKind), Is.Not.Empty);
            }
        });
    }

    /// To be used for testing purposes only
    public enum NamingConventionKindTestEnum
    {
        TestValue1,
        TestValue2
    }
}
