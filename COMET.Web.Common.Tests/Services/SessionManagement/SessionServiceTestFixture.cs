// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMET.Web.Common.Tests.Services.SessionManagement
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using DynamicData;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SessionServiceTestFixture
    {
        private SessionService sessionService;
        private readonly Uri uri = new("http://test.com/");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            var logger = new Mock<ILogger<SessionService>>();
            this.messageBus = new CDPMessageBus();
            this.sessionService = new SessionService(logger.Object, this.messageBus);

            var engineeringModel = new EngineeringModel();
            this.sessionService.OpenIterations.Add(new Iteration(){ Container = engineeringModel});
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyQueryOpenEngineeringModels()
        {
            var openEngineeringModels = this.sessionService.OpenEngineeringModels;
            Assert.That(openEngineeringModels, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyCreateOrUpdateThings()
        {
            var siteDirectory = new SiteDirectory();
            var domain = new DomainOfExpertise();
            siteDirectory.Domain.Add(domain);

            Assert.That(async () => await this.sessionService.CreateOrUpdateThings(siteDirectory, [domain], ["file A"]), Throws.InvalidOperationException);
        }

        [Test]
        public async Task VerifyReadEngineeringModel()
        {
            var readingResult = await this.sessionService.ReadEngineeringModels([new EngineeringModelSetup()]);
            Assert.That(readingResult.IsSuccess, Is.EqualTo(false));
        }
    }
}
