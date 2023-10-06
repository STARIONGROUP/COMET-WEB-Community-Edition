// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfigurationServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.Server.Services.ConfigurationService
{
    using System.Text;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Server.Services.ConfigurationService;

    using Microsoft.Extensions.Configuration;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using JsonSerializer = System.Text.Json.JsonSerializer;

    [TestFixture]
    public class ConfigurationServiceTestFixture
    {
        [Test]
        public async Task VerifyInitializeServiceWithEmptyConfiguration()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(ConfigurationService.AddressSection)).Returns(new Mock<IConfigurationSection>().Object);
            configuration.Setup(x => x.GetSection(ConfigurationService.BookInputConfigurationSection)).Returns(new Mock<IConfigurationSection>().Object);
            var service = new ConfigurationService(configuration.Object);
            await service.InitializeService();

            Assert.Multiple(() =>
            {
                configuration.Verify(x => x.GetSection(ConfigurationService.AddressSection), Times.Once);
                configuration.Verify(x => x.GetSection(ConfigurationService.BookInputConfigurationSection), Times.Once);
                Assert.That(service.ServerAddress, Is.Null);
                Assert.That(service.BookInputConfiguration, Is.Null);
            });
            
            await service.InitializeService();
            configuration.Verify(x => x.GetSection(ConfigurationService.AddressSection), Times.Once);
            configuration.Verify(x => x.GetSection(ConfigurationService.BookInputConfigurationSection), Times.Once);
        }

        [Test]
        public async Task VerifyInitializeServiceWithConfiguration()
        {
            var serverAddressMockConfigurationSection = new Mock<IConfigurationSection>();
            serverAddressMockConfigurationSection.Setup(x => x.Value).Returns("https://a.b.c");
            
            var bookInputMockConfigurationSection = new Mock<IConfigurationSection>();
            var bookInputConfiguration = new BookInputConfiguration { ShowName = true, ShowShortName = true };
            var defaultBookInputConfigurationJson = JsonSerializer.Serialize(bookInputConfiguration);
            bookInputMockConfigurationSection.Setup(x => x.Value).Returns(defaultBookInputConfigurationJson);
            
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(ConfigurationService.AddressSection)).Returns(serverAddressMockConfigurationSection.Object);
            configuration.Setup(x => x.GetSection(ConfigurationService.BookInputConfigurationSection)).Returns(bookInputMockConfigurationSection.Object);
            var service = new ConfigurationService(configuration.Object);
            await service.InitializeService();
            
            Assert.Multiple(() =>
            {
                Assert.That(service.ServerAddress, Is.EqualTo(serverAddressMockConfigurationSection.Object.Value));
                Assert.IsNotNull(service.BookInputConfiguration);
                Assert.That(service.BookInputConfiguration.ShowName, Is.EqualTo(bookInputConfiguration.ShowName));
                Assert.That(service.BookInputConfiguration.ShowShortName, Is.EqualTo(bookInputConfiguration.ShowShortName));
            });
        }
    }
}
