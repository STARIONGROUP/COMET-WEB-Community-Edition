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
    using System.Collections.Concurrent;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SessionServiceTestFixture
    {
        private Mock<ISession> session;
        private SessionService sessionService;
        private Participant participant;
        private Person person;
        private DomainOfExpertise domain;
        private Iteration iteration;
        private ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>> openIteration;
        private Assembler assembler;
        private readonly Uri uri = new("http://test.com");
        private ModelReferenceDataLibrary referenceDataLibrary;
        private EngineeringModelSetup engineeringSetup;
        private SiteDirectory siteDirectory;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            var logger = new Mock<ILogger<SessionService>>();
            this.messageBus = new CDPMessageBus();

            this.session = new Mock<ISession>();

            this.sessionService = new SessionService(logger.Object, this.messageBus)
            {
                Session = this.session.Object
            };

            this.assembler = new Assembler(this.uri, this.messageBus);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Person = this.person
            };

            this.referenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "ARDL"
            };

            this.engineeringSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                RequiredRdl =
                {
                    this.referenceDataLibrary
                },
                Participant = { this.participant }
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        RequiredRdl =
                        {
                            new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                FileType =
                                {
                                    new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "tar" },
                                    new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "gz" },
                                    new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "zip" }
                                }
                            }
                        },
                        Participant = { this.participant }
                    }
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    Container = this.engineeringSetup
                },
                DomainFileStore =
                {
                    new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain }
                }
            };

            this.engineeringSetup.IterationSetup.Add(this.iteration.IterationSetup);

            this.openIteration = new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
                new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>
                {
                    new(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant))
                });

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Model = { this.engineeringSetup }
            };

            this.siteDirectory.Person.Add(this.person);
            this.siteDirectory.Organization.Add(new Organization());
            this.siteDirectory.PersonRole.Add(new PersonRole());
            this.siteDirectory.Domain.Add(this.domain);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(this.openIteration);
            this.session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", this.uri));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyClose()
        {
            this.sessionService.IsSessionOpen = true;
            this.session.Setup(x => x.Close()).Returns(Task.CompletedTask);
            await this.sessionService.Close();

            Assert.Multiple(() =>
            {
                Assert.That(this.sessionService.IsSessionOpen, Is.False);
                Assert.That(() => this.sessionService.Close(), Throws.Nothing);
            });

            this.session.Setup(x => x.Close()).ThrowsAsync(new Exception());
            Assert.That(() => this.sessionService.Close(), Throws.Nothing);

            this.sessionService.IsSessionOpen = true;
            Assert.That(() => this.sessionService.Close(), Throws.Exception);
        }

        [Test]
        public void VerifyCloseIteration()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) }
            });

            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);
            this.session.Setup(x => x.CloseIterationSetup(It.IsAny<IterationSetup>())).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => this.sessionService.CloseIterations());
        }

        [Test]
        public void VerifyCreateThings()
        {
            this.sessionService.IsSessionOpen = true;
            var thingsToCreate = new List<ElementDefinition>();

            var element = new ElementDefinition
            {
                Name = "Battery",
                Owner = this.sessionService.GetDomainOfExpertise(this.iteration)
            };

            thingsToCreate.Add(element.Clone(false));
            Assert.DoesNotThrow(() => this.sessionService.CreateThings(this.iteration, thingsToCreate));
        }

        [Test]
        public void VerifyDeleteThings()
        {
            this.sessionService.IsSessionOpen = true;

            var element = new ElementDefinition
            {
                Name = "Battery",
                Owner = this.sessionService.GetDomainOfExpertise(this.iteration)
            };

            Assert.DoesNotThrow(() => this.sessionService.DeleteThing(this.iteration, element.Clone(false)));
        }

        [Test]
        public void VerifyGetAvailableDomains()
        {
            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);
            Assert.That(this.sessionService.Session.RetrieveSiteDirectory().Domain.ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyGetAvailableOrganizations()
        {
            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);
            Assert.That(this.sessionService.Session.RetrieveSiteDirectory().Organization.ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyGetAvailablePersonRoles()
        {
            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);
            Assert.That(this.sessionService.Session.RetrieveSiteDirectory().PersonRole.ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyGetModelDomains()
        {
            Assert.That(this.sessionService.GetModelDomains(this.engineeringSetup), Is.Not.Null);
        }

        [Test]
        public void VerifyGetParticipant()
        {
            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);
            Assert.That(this.sessionService.GetParticipant(this.iteration), Is.EqualTo(this.participant));
        }

        [Test]
        public void VerifyGetParticipantModels()
        {
            Assert.That(this.sessionService.GetParticipantModels(), Is.Not.Null);
        }

        [Test]
        public void VerifyGetPersons()
        {
            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);
            Assert.That(this.sessionService.Session.RetrieveSiteDirectory().Person.ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyRefreshEndedCalled()
        {
            var onSessionRefreshedCalled = false;

            void SetRefreshEndedCalled()
            {
                onSessionRefreshedCalled = true;
            }

            this.messageBus.Listen<SessionStateKind>().Where(x => x == SessionStateKind.RefreshEnded)
                .Subscribe(_ => { SetRefreshEndedCalled(); });

            this.sessionService.RefreshSession();

            Assert.That(onSessionRefreshedCalled, Is.True);
        }

        [Test]
        public void VerifyRefreshSession()
        {
            var beginRefreshReceived = false;

            this.messageBus.Listen<SessionStateKind>().Where(x => x == SessionStateKind.Refreshing)
                .Subscribe(_ => { beginRefreshReceived = true; });

            this.sessionService.RefreshSession();

            Assert.That(beginRefreshReceived, Is.True);
        }

        [Test]
        public void VerifySwitchDomain()
        {
            Assert.DoesNotThrow(() => this.sessionService.SwitchDomain(this.iteration, this.domain));

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) }
            });

            this.sessionService.IsSessionOpen = true;
            this.sessionService.ReadIteration(this.iteration.IterationSetup, this.domain);

            this.sessionService.SwitchDomain(this.iteration, this.domain);
            Assert.That(this.sessionService.GetDomainOfExpertise(this.iteration), Is.EqualTo(this.domain));
        }

        [Test]
        public async Task VerifyUpdateThings()
        {
            var thingsToUpdate = new List<ElementDefinition>();
            this.sessionService.IsSessionOpen = true;

            var element = new ElementDefinition
            {
                Name = "Battery",
                Owner = this.sessionService.GetDomainOfExpertise(this.iteration)
            };

            this.iteration.Element.Add(element);

            var clone = element.Clone(false);
            clone.Name = "Satellite";
            thingsToUpdate.Add(clone);
            Assert.DoesNotThrowAsync(async() => await this.sessionService.UpdateThings(this.iteration, thingsToUpdate));

            var filesToUpload = new List<string> { this.uri.LocalPath };

            Assert.Multiple(() =>
            {
                Assert.DoesNotThrowAsync(async () => await this.sessionService.UpdateThings(this.iteration, thingsToUpdate, filesToUpload));
                this.session.Verify(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>()), Times.Once);
            });

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>())).Throws(new DalWriteException());
            Assert.DoesNotThrowAsync(async () => await this.sessionService.UpdateThings(this.iteration, thingsToUpdate, filesToUpload));
        }

        [Test]
        public void VerifyReadEngineeringModels()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.sessionService.ReadEngineeringModels(Enumerable.Empty<Guid>()).Result.IsSuccess, Is.True);
                Assert.That(() => this.sessionService.ReadEngineeringModels(Enumerable.Empty<EngineeringModelSetup>()).Result.IsSuccess, Is.True);
                Assert.That(() => this.sessionService.ReadEngineeringModels(new EngineeringModelSetup[]{new (){EngineeringModelIid = Guid.NewGuid()}}).Result.IsSuccess, Is.True);
                this.session.Verify(x => x.Read(It.Is<IEnumerable<Guid>>(g => !g.Any())), Times.Exactly(2));
                this.session.Verify(x => x.Read(It.Is<IEnumerable<Guid>>(g => g.Any())), Times.Once);
            });

            this.session.Setup(x => x.Read(It.IsAny<IEnumerable<Guid>>())).ThrowsAsync(new InvalidDataException());

            Assert.Multiple(() =>
            {
                Assert.That(() => this.sessionService.ReadEngineeringModels(Enumerable.Empty<Guid>()).Result.IsSuccess, Is.False);
                Assert.That(() => this.sessionService.ReadEngineeringModels(Enumerable.Empty<EngineeringModelSetup>()).Result.IsSuccess, Is.False);
            });
        }
    }
}
