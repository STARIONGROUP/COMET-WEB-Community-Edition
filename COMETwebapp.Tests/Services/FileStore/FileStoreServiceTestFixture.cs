// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileStoreServiceTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Services.FileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.FileStore;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileStoreServiceTestFixture
    {
        private FileStoreService service;
        private Mock<ISessionService> sessionService;
        private Iteration iteration;
        private EngineeringModel engineeringModel;
        private DomainOfExpertise domain;

        [SetUp]
        public void Setup()
        {
            this.domain = new DomainOfExpertise { Name = "System", ShortName = "SYS" };
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>())).ReturnsAsync(Result.Ok());

            this.iteration = new Iteration();
            this.engineeringModel = new EngineeringModel { EngineeringModelSetup = new EngineeringModelSetup() };
            this.engineeringModel.Iteration.Add(this.iteration);

            this.service = new FileStoreService(this.sessionService.Object);
        }

        [Test]
        public void VerifyStoreQueriesWhenStoresAreMissing()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.service.StoreExists(this.iteration, FileStoreType.Common), Is.False);
                Assert.That(this.service.StoreExists(this.iteration, FileStoreType.Domain), Is.False);
                Assert.That(this.service.GetFolders(this.iteration, FileStoreType.Common), Is.Empty);
                Assert.That(this.service.GetFolders(this.iteration, FileStoreType.Domain), Is.Empty);
            });
        }

        [Test]
        public void VerifyStoreQueriesWhenStoresExist()
        {
            var commonStore = new CommonFileStore { Name = "CFS", Owner = this.domain };
            this.engineeringModel.CommonFileStore.Add(commonStore);

            var domainStore = new DomainFileStore { Name = "DFS", Owner = this.domain };
            var folder = new Folder { Name = "configs", Owner = this.domain };
            domainStore.Folder.Add(folder);
            this.iteration.DomainFileStore.Add(domainStore);

            Assert.Multiple(() =>
            {
                Assert.That(this.service.StoreExists(this.iteration, FileStoreType.Common), Is.True);
                Assert.That(this.service.StoreExists(this.iteration, FileStoreType.Domain), Is.True);
                Assert.That(this.service.GetFolders(this.iteration, FileStoreType.Domain), Does.Contain(folder));
            });
        }

        [Test]
        public async Task VerifyCreateStoreAsync()
        {
            Thing writtenContainer = null;
            IReadOnlyCollection<Thing> writtenThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .Callback<Thing, IReadOnlyCollection<Thing>>((container, things) =>
                {
                    writtenContainer = container;
                    writtenThings = things;
                })
                .ReturnsAsync(Result.Ok());

            var commonResult = await this.service.CreateStoreAsync(this.iteration, FileStoreType.Common);

            Assert.Multiple(() =>
            {
                Assert.That(commonResult.IsSuccess, Is.True);
                Assert.That(writtenContainer, Is.InstanceOf<EngineeringModel>());
                Assert.That(((EngineeringModel)writtenContainer).CommonFileStore, Has.Count.EqualTo(1));
                Assert.That(writtenThings, Has.Some.InstanceOf<CommonFileStore>());
            });

            var domainResult = await this.service.CreateStoreAsync(this.iteration, FileStoreType.Domain);

            Assert.Multiple(() =>
            {
                Assert.That(domainResult.IsSuccess, Is.True);
                Assert.That(writtenContainer, Is.InstanceOf<Iteration>());
                Assert.That(((Iteration)writtenContainer).DomainFileStore, Has.Count.EqualTo(1));
                Assert.That(((Iteration)writtenContainer).DomainFileStore.Single().Owner, Is.EqualTo(this.domain));
            });

            // Existing store short-circuits without a write
            this.engineeringModel.CommonFileStore.Add(new CommonFileStore { Name = "CFS", Owner = this.domain });
            this.sessionService.Invocations.Clear();

            var skipResult = await this.service.CreateStoreAsync(this.iteration, FileStoreType.Common);

            Assert.That(skipResult.IsSuccess, Is.True);
            this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()), Times.Never);
        }

        [Test]
        public async Task VerifySaveFileAsync()
        {
            var store = new DomainFileStore { Name = "DFS", Owner = this.domain };
            this.iteration.DomainFileStore.Add(store);

            var person = new Person { GivenName = "a" };
            this.engineeringModel.EngineeringModelSetup.Participant.Add(new Participant { Person = person });

            var session = new Mock<ISession>();
            session.Setup(x => x.ActivePerson).Returns(person);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            FileRevision writtenRevision = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<IReadOnlyCollection<string>>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, IReadOnlyCollection<string>>((_, things, _) => writtenRevision = things.OfType<FileRevision>().FirstOrDefault())
                .ReturnsAsync(Result.Ok());

            var fileType = new FileType { Extension = "json" };

            // Happy path
            var result = await this.service.SaveFileAsync(this.iteration, FileStoreType.Domain, "config.json", fileType, [1, 2, 3], store.Folder.FirstOrDefault());

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(writtenRevision, Is.Not.Null);
                Assert.That(writtenRevision.Name, Is.EqualTo("config.json"));
                Assert.That(writtenRevision.ContentHash, Is.Not.Null.And.Not.Empty);
                Assert.That(writtenRevision.FileType, Does.Contain(fileType));
            });

            // A traversing name is reduced to its file component, never escaping the temp directory
            var traversal = await this.service.SaveFileAsync(this.iteration, FileStoreType.Domain, "../../evil.json", fileType, [1], null);

            Assert.Multiple(() =>
            {
                Assert.That(traversal.IsSuccess, Is.True);
                Assert.That(writtenRevision.Name, Is.EqualTo("evil.json"));
            });

            // A name that resolves to no file component is rejected
            var rejected = await this.service.SaveFileAsync(this.iteration, FileStoreType.Domain, "..", fileType, [1], null);
            Assert.That(rejected.IsFailed, Is.True);
        }
    }
}
