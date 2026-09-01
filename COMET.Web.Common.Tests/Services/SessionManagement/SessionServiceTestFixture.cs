// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Tests.Services.SessionManagement
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.NotificationService;
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
        private Mock<INotificationService> notificationService;

        [SetUp]
        public void Setup()
        {
            var logger = new Mock<ILogger<SessionService>>();
            this.messageBus = new CDPMessageBus();
            this.notificationService = new Mock<INotificationService>();
            this.notificationService.Setup(x => x.Results).Returns(new SourceList<ResultNotification>());
            this.sessionService = new SessionService(logger.Object, this.messageBus, this.notificationService.Object);

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

            Assert.Multiple(() =>
            {
                Assert.That(async () => await this.sessionService.CreateOrUpdateThings(siteDirectory, [domain], ["file A"]), Throws.InvalidOperationException);
                Assert.That(async () => await this.sessionService.CreateOrUpdateThingsWithNotification(siteDirectory, [domain], ["file A"]), Throws.InvalidOperationException);
                Assert.That(async () => await this.sessionService.CreateOrUpdateThingsWithNotification(siteDirectory, [domain]), Throws.InvalidOperationException);
            });
        }

        [Test]
        public void VerifyCreateUpdateAndDeleteThingsWithNotification()
        {
            var siteDirectory = new SiteDirectory();
            var domain = new DomainOfExpertise();
            var obsoleteDomain = new DomainOfExpertise { Container = siteDirectory };
            siteDirectory.Domain.Add(domain);

            Assert.Multiple(() =>
            {
                Assert.That(async () => await this.sessionService.CreateUpdateAndDeleteThingsWithNotification(siteDirectory, [domain], [obsoleteDomain]), Throws.InvalidOperationException);
                Assert.That(async () => await this.sessionService.CreateUpdateAndDeleteThingsWithNotification(siteDirectory, [], []), Throws.TypeOf<ArgumentOutOfRangeException>());
            });
        }

        [Test]
        public async Task VerifyReadEngineeringModel()
        {
            var readingResult = await this.sessionService.ReadEngineeringModels([new EngineeringModelSetup()]);
            Assert.That(readingResult.IsSuccess, Is.EqualTo(false));
        }

        [Test]
        public async Task VerifyWriteTransaction()
        {
            var readOnlySession = CreateSessionMock(true);
            InjectSession(this.sessionService, readOnlySession.Object);

            var readOnlyResult = await this.sessionService.WriteTransaction(new OperationContainer($"/SiteDirectory/{Guid.NewGuid()}"), []);

            var writeableSession = CreateSessionMock(false);
            InjectSession(this.sessionService, writeableSession.Object);

            var writeableResult = await this.sessionService.WriteTransaction(new OperationContainer($"/SiteDirectory/{Guid.NewGuid()}"), []);

            Assert.Multiple(() =>
            {
                Assert.That(readOnlyResult.IsFailed, Is.True);
                readOnlySession.Verify(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>()), Times.Never);
                Assert.That(writeableResult.IsSuccess, Is.True);
                writeableSession.Verify(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>()), Times.Once);
            });
        }

        [Test]
        public async Task VerifyEveryWriteEntryPointIsBlockedWhenReadOnly()
        {
            var session = CreateSessionMock(true);
            InjectSession(this.sessionService, session.Object);

            var siteDirectory = new SiteDirectory();
            var domain = new DomainOfExpertise();
            siteDirectory.Domain.Add(domain);

            // The CDP4Web base class carries its own two-argument overloads that write through its own WriteTransaction,
            // so guarding only the local three-argument overload leaves these paths able to write to a read-only source.
            var results = new[]
            {
                await this.sessionService.CreateOrUpdateThings(siteDirectory.Clone(false), [domain]),
                await this.sessionService.CreateOrUpdateThings(siteDirectory.Clone(false), [domain], []),
                await this.sessionService.CreateOrUpdateThingsWithNotification(siteDirectory.Clone(false), [domain]),
                await this.sessionService.CreateOrUpdateThingsWithNotification(siteDirectory.Clone(false), [domain], []),
                await this.sessionService.WriteTransaction(new OperationContainer($"/SiteDirectory/{Guid.NewGuid()}")),
                await this.sessionService.WriteTransaction(new OperationContainer($"/SiteDirectory/{Guid.NewGuid()}"), [])
            };

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    Assert.That(result.IsFailed, Is.True, "every write entry point should refuse a read-only data source");
                }

                session.Verify(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>()), Times.Never);
                session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
            });
        }

        [Test]
        public void VerifyIsReadOnly()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.sessionService.IsReadOnly, Is.False, "a service without an open session is not read only");

                InjectSession(this.sessionService, CreateSessionMock(true).Object);
                Assert.That(this.sessionService.IsReadOnly, Is.True);

                InjectSession(this.sessionService, CreateSessionMock(false).Object);
                Assert.That(this.sessionService.IsReadOnly, Is.False);
            });
        }

        [Test]
        public async Task VerifyOpenArchiveSession()
        {
            var missingArchive = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

            Assert.Multiple(async () =>
            {
                Assert.That(async () => await this.sessionService.OpenArchiveSession(null, "user", "pass"), Throws.ArgumentNullException);
                Assert.That(async () => await this.sessionService.OpenArchiveSession(missingArchive, null, "pass"), Throws.ArgumentNullException);

                var result = await this.sessionService.OpenArchiveSession(missingArchive, "user", "pass");
                Assert.That(result.IsFailed, Is.True);
                Assert.That(this.sessionService.IsSessionOpen, Is.False);
            });
        }

        /// <summary>
        /// Creates a <see cref="Mock{T}" /> of <see cref="ISession" /> that reports an open session backed by a
        /// <see cref="IDal" /> with the provided read-only state
        /// </summary>
        /// <param name="isDalReadOnly">A value indicating whether the mocked <see cref="IDal" /> is read only</param>
        /// <returns>The configured <see cref="Mock{T}" /></returns>
        private static Mock<ISession> CreateSessionMock(bool isDalReadOnly)
        {
            var dal = new Mock<IDal>();
            dal.Setup(x => x.IsReadOnly).Returns(isDalReadOnly);

            var session = new Mock<ISession>();
            session.Setup(x => x.Dal).Returns(dal.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(new SiteDirectory());
            session.Setup(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>())).Returns(Task.CompletedTask);
            return session;
        }

        /// <summary>
        /// Assigns the provided <see cref="ISession" /> onto the CDP4Web <c>SessionService</c> base class, whose
        /// <c>Session</c> property only exposes an <c>internal</c> setter
        /// </summary>
        /// <param name="service">The <see cref="SessionService" /> to assign the session on</param>
        /// <param name="session">The <see cref="ISession" /> to assign</param>
        private static void InjectSession(SessionService service, ISession session)
        {
            typeof(CDP4Web.Services.SessionService.SessionService)
                .GetProperty(nameof(CDP4Web.Services.SessionService.SessionService.Session))
                .SetValue(service, session);
        }
    }
}
