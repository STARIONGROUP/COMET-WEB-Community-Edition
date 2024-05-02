// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NamingConventionServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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


namespace COMET.Web.Common.Tests.Services.NamingConventionService
{
    using COMET.Web.Common.Server.Services.NamingConventionService;
    using COMET.Web.Common.Services.NamingConventionService;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NamingConventionServiceTestFixture
    {
        private NamingConventionService<NamingConventionKindTestEnum> service;
        private Mock<ILogger<INamingConventionService<NamingConventionKindTestEnum>>> logger;

        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<INamingConventionService<NamingConventionKindTestEnum>>>();
            this.service = new NamingConventionService<NamingConventionKindTestEnum>(this.logger.Object);
        }

        [Test]
        public async Task VerifyInitializationAndGetNamingConvention()
        {
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
}
