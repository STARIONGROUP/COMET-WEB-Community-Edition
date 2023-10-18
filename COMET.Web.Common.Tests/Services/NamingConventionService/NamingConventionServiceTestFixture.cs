// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NamingConventionServiceTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Tests.Services.NamingConventionService
{
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

            foreach (var namingConventionKind in enumValues)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(this.service.GetNamingConventionValue(namingConventionKind), Is.Not.Empty);
                });
            }
        }

        /// To be used for testing purposes only
        public enum NamingConventionKindTestEnum
        {
            TestValue1,
            TestValue2
        }
    }
}
