// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingCreatorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
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

namespace COMET.Web.Common.Tests.Utilities
{
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Utilities;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ThingCreator" /> class
    /// </summary>
    [TestFixture]
    public class ThingCreatorTestFixture
    {
        private Mock<ISession> session;
        private Mock<ISession> sessionThatThrowsException;
        private ThingCreator thingCreator;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.sessionThatThrowsException = new Mock<ISession>();
            this.sessionThatThrowsException.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws<Exception>();

            this.thingCreator = new ThingCreator();
        }

        [Test]
        public void VerifyThatArgumentNullExceptionsAreThrowOnCreateElementUsage()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), this.cache, null);

            iteration.Element.Add(elementDefinitionA);
            iteration.Element.Add(elementDefinitionB);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsageAsync(null, elementDefinitionB, domainOfExpertise, this.session.Object));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsageAsync(elementDefinitionA, null, domainOfExpertise, this.session.Object));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsageAsync(elementDefinitionA, elementDefinitionB, null, this.session.Object));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsageAsync(elementDefinitionA, elementDefinitionB, domainOfExpertise, null));
        }

        [Test]
        public async Task VerifyThatCreateElementUsageExecutesWrite()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), this.cache, null);

            iteration.Element.Add(elementDefinitionA);
            iteration.Element.Add(elementDefinitionB);

            await this.thingCreator.CreateElementUsageAsync(elementDefinitionA, elementDefinitionB, domainOfExpertise, this.session.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatExceptionIsThrownWhenCreateElementUsageFails()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), this.cache, null);

            iteration.Element.Add(elementDefinitionA);
            iteration.Element.Add(elementDefinitionB);

            Assert.ThrowsAsync<Exception>(async () => await this.thingCreator.CreateElementUsageAsync(elementDefinitionA, elementDefinitionB, domainOfExpertise, this.sessionThatThrowsException.Object));
        }
    }
}
