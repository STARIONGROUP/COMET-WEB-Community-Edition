﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensionsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Extensions
{
    using System.Reflection;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ServiceCollectionExtensionsTestFixture
    {
        [Test]
        public void VerifyServerRegistration()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();
            serviceCollection.AddSingleton(configuration.Object);
            serviceCollection.AddScoped(_ => new HttpClient());
            serviceCollection.AddLogging();
            serviceCollection.RegisterCdp4CometCommonServices(globalOptions: _ => { });
            var serviceProvider = serviceCollection.BuildServiceProvider();

            foreach (var service in serviceCollection.Where(x => x.ServiceType.Assembly == Assembly.GetAssembly(typeof(ISessionService))))
            {
                Assert.That(() => serviceProvider.GetService(service.ServiceType), Throws.Nothing);
            }
        }

        [Test]
        public void VerifyWebAssemblyRegistration()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(_ => new HttpClient());
            serviceCollection.AddLogging();
            serviceCollection.RegisterCdp4CometCommonServices(false,globalOptions: _ => { });
            var serviceProvider = serviceCollection.BuildServiceProvider();

            foreach (var service in serviceCollection.Where(x => x.ServiceType.Assembly == Assembly.GetAssembly(typeof(ISessionService))))
            {
                Assert.That(() => serviceProvider.GetService(service.ServiceType), Throws.Nothing);
            }
        }
    }
}
