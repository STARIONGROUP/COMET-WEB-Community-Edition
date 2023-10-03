// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServiceProviderExtensionsTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Extensions
{
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.StringTableService;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ServiceProviderExtensionsTestFixture
    {
        [Test]
        public void VerifyServicesInitialization()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IStringTableService))).Returns(null);
            serviceProvider.Setup(x => x.GetService(typeof(IConfigurationService))).Returns(null);

            Assert.That( () => serviceProvider.Object.InitializeServices(), Throws.Exception);

            serviceProvider.Setup(x => x.GetService(typeof(IStringTableService))).Returns(new Mock<IStringTableService>().Object);
            serviceProvider.Setup(x => x.GetService(typeof(IConfigurationService))).Returns(new Mock<IConfigurationService>().Object);

            Assert.That( () => serviceProvider.Object.InitializeServices(), Throws.Nothing);
        }
    }
}
