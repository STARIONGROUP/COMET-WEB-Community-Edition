// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionServiceTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Services.SessionManagement
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Services.SessionManagement;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SessionServiceTest
    {
        private Mock<ISession> session;
        private ISessionService sessionService;
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

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.sessionService = new SessionService { Session = this.session.Object };
            this.assembler = new Assembler(this.uri);
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
            this.siteDirectory.Domain.Add(this.domain);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(this.openIteration);
            this.session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", this.uri));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }

        [Test]
        public void VerifyClose()
        {
            this.sessionService.IsSessionOpen = true;
            this.session.Setup(x => x.Close()).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => this.sessionService.Close());
            this.session.Setup(x => x.Close()).Throws<Exception>();
            Assert.DoesNotThrow(() => this.sessionService.Close());
            this.sessionService.IsSessionOpen = true;
            Assert.DoesNotThrow(() => this.sessionService.Close());
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
            var element = new ElementDefinition();
            element.Name = "Battery";
            element.Owner = this.sessionService.GetDomainOfExpertise(this.iteration);
            thingsToCreate.Add(element.Clone(false));
            Assert.DoesNotThrow(() => this.sessionService.CreateThings(this.iteration, thingsToCreate));
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
        public void VerifyRefreshSession()
        {
            var beginRefreshReceived = false;
            var endRefreshReceived = false;
            CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.Refreshing).Subscribe(x => { beginRefreshReceived = true; });
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
        public void VerifyUpdateThings()
        {
            var thingsToUpdate = new List<ElementDefinition>();
            this.sessionService.IsSessionOpen = true;
            var element = new ElementDefinition();
            element.Name = "Battery";
            element.Owner = this.sessionService.GetDomainOfExpertise(this.iteration);

            var clone = element.Clone(false);
            clone.Name = "Satellite";
            thingsToUpdate.Add(clone);
            Assert.DoesNotThrow(() => this.sessionService.UpdateThings(this.iteration, thingsToUpdate));
        }
    }
}
