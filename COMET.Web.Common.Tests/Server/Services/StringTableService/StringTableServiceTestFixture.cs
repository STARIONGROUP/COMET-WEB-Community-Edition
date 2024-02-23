// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StringTableServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Server.Services.StringTableService
{
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Server.Services.StringTableService;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class StringTableServiceTestFixture
    {
        [Test]
        public async Task VerifyServiceInitialization()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x["StringTablePath"]).Returns("");

            var service = new StringTableService(configuration.Object, NullLogger<StringTableService>.Instance);
            await service.InitializeService();
            Assert.That(() => service.GetText(TextConfigurationKind.DomainTitleCaption), Throws.Exception);

            configuration.Setup(x => x["StringTablePath"]).Returns(Path.Combine("Resources", "configuration", "DefaultTextConfiguration.json"));

            service = new StringTableService(configuration.Object, NullLogger<StringTableService>.Instance);
            await service.InitializeService();
            Assert.That(() => service.GetText(TextConfigurationKind.DomainTitleCaption), Throws.Nothing);
        }
    }
}
