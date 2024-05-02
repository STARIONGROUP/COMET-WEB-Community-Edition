// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfigurationServiceTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Server.Services.ConfigurationService
{
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Server.Services.ConfigurationService;

    using Microsoft.Extensions.Configuration;

    using Moq;

    using NUnit.Framework;
    
    [TestFixture]
    public class ConfigurationServiceTestFixture
    {
        [Test]
        public async Task VerifyInitializeServiceWithEmptyConfiguration()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(ConfigurationService.ServerConfigurationSection)).Returns(new Mock<IConfigurationSection>().Object);
            var service = new ConfigurationService(configuration.Object);
            await service.InitializeService();

            Assert.Multiple(() =>
            {
                configuration.Verify(x => x.GetSection(ConfigurationService.ServerConfigurationSection), Times.Once);
                Assert.That(service.ServerConfiguration, Is.Null);
            });
            
            await service.InitializeService();

            Assert.Multiple(() =>
            {
                configuration.Verify(x => x.GetSection(ConfigurationService.ServerConfigurationSection), Times.Once);
            });
        }

        [Test]
        public async Task VerifyInitializeServiceWithConfiguration()
        {
            var serverConfiguration = new ServerConfiguration
            {
                ServerAddress = "https://a.b.c",
                BookInputConfiguration = new BookInputConfiguration()
                {
                    ShowName = true,
                    ShowShortName = true
                }
            };
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("Data/server_configuration_tests.json")
                .Build();
            
            var service = new ConfigurationService(config);
            await service.InitializeService();
            
            Assert.Multiple(() =>
            {
                Assert.That(service.ServerConfiguration.ServerAddress, Is.EqualTo(serverConfiguration.ServerAddress));
                Assert.That(service.ServerConfiguration.BookInputConfiguration, Is.Not.Null);
                Assert.That(service.ServerConfiguration.BookInputConfiguration.ShowName, Is.EqualTo(serverConfiguration.BookInputConfiguration.ShowName));
                Assert.That(service.ServerConfiguration.BookInputConfiguration.ShowShortName, Is.EqualTo(serverConfiguration.BookInputConfiguration.ShowShortName));
            });
        }
    }
}
