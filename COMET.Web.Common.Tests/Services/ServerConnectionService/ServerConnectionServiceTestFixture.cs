// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServerConnectionServiceTestFixture.cs" company="RHEA System S.A.">
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
namespace COMET.Web.Common.Tests.Services.ServerConnectionService
{
	using System.Net;

	using COMET.Web.Common.Model;
	using COMET.Web.Common.Services.ServerConnectionService;

	using Microsoft.Extensions.Options;

	using Moq;
	using Moq.Protected;

	using NUnit.Framework;

	[TestFixture]
	public class ServerConnectionServiceTestFixture
	{
		private IServerConnectionService serverConnectionService;

		[Test]
		public async Task VerifyService()
		{
			var globalOptions = new GlobalOptions
			{
				ServerConfigurationFile = null,
			};

			var options = new Mock<IOptions<GlobalOptions>>();
			options.Setup(x => x.Value).Returns(globalOptions);

			var handlerMock = new Mock<HttpMessageHandler>();

			var json = """ 
                        {
                            "ServerAddress": "http://localhost:5000/"
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

			this.serverConnectionService = new ServerConnectionService(options.Object, httpClient);

			Assert.That(this.serverConnectionService, Is.Not.Null);

			await this.serverConnectionService.InitializeService();

			Assert.Multiple(() =>
			{
				Assert.That(this.serverConnectionService.ServerAddress, Is.Not.Null);
				Assert.That(this.serverConnectionService.ServerAddress, Is.EqualTo("http://localhost:5000/"));	
			});
		}
	}
}
