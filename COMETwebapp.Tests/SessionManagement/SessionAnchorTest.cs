// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionAnchorTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using COMETwebapp.SessionManagement;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SessionAnchorTest
    {
        private Mock<ISession> session;
        private ISessionAnchor sessionAnchor;
        private Participant participant;
        private Person person;
        private DomainOfExpertise domain;
        private Iteration iteration;
        private ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>> openIteration;
        private Assembler assembler;
        private readonly Uri uri = new Uri("http://test.com");
        private ModelReferenceDataLibrary referenceDataLibrary;
        private EngineeringModelSetup engineeringSetup;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.sessionAnchor = new SessionAnchor() { Session = this.session.Object };
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
            this.openIteration = new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
               new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>()
               {
                    new KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant))
               });

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
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
            this.sessionAnchor.IsSessionOpen = true;
            this.session.Setup(x => x.Close()).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => this.sessionAnchor.Close());
            this.session.Setup(x => x.Close()).Throws<Exception>();
            Assert.DoesNotThrow(() => this.sessionAnchor.Close());
            this.sessionAnchor.IsSessionOpen = true;
            Assert.DoesNotThrow(() => this.sessionAnchor.Close());
        }

        [Test]
        public void VerifyGetIteration()
        {
            this.session.Setup(x => x.OpenIterations).Returns(default(IReadOnlyDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>));
            Assert.That(this.sessionAnchor.QueryIteration(), Is.Null);
            this.session.Setup(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), true)).Returns(Task.CompletedTask);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant)}
            });
            this.sessionAnchor.IsSessionOpen = true;
            Assert.That(this.sessionAnchor.QueryIteration(), Is.EqualTo(this.iteration));

            Assert.DoesNotThrowAsync(async () => await this.sessionAnchor.ReadIteration(this.engineeringSetup, this.iteration.IterationSetup));
        }

        [Test]
        public void VerifyCloseIteration()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant)}
            });
            this.sessionAnchor.IsSessionOpen = true;
            this.session.Setup(x => x.CloseIterationSetup(this.sessionAnchor.QueryIteration().IterationSetup)).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => this.sessionAnchor.CloseIteration());
        }

        [Test]
        public void VerifyGetParticipantModels()
        {
            Assert.That(this.sessionAnchor.GetParticipantModels(), Is.Not.Null);
        }

        [Test]
        public void VerifyGetModelDomains()
        {
            Assert.That(this.sessionAnchor.GetModelDomains(this.engineeringSetup), Is.Not.Null);
        }

        [Test]
        public void VerifyRefreshSession()
        {
            var beginRefreshReceived = false;
            var endRefreshReceived = false;
            CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.Refreshing).Subscribe(x =>
            {
                beginRefreshReceived = true;
            });
            CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.UpToDate).Subscribe(x =>
            {
                endRefreshReceived = true;
            });
            this.sessionAnchor.RefreshSession();

            Assert.That(beginRefreshReceived, Is.True);
            Assert.That(endRefreshReceived, Is.True);
        }

        [Test]
        public void VerifySwitchDomain()
        {
            Assert.DoesNotThrow(() => this.sessionAnchor.SwitchDomain(this.domain));

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant)}
            });
            this.sessionAnchor.IsSessionOpen = true;
            Assert.That(this.sessionAnchor.CurrentDomainOfExpertise, Is.Null);
            
            this.sessionAnchor.SwitchDomain(this.domain);
            Assert.That(this.sessionAnchor.CurrentDomainOfExpertise, Is.EqualTo(this.domain));
        }

        [Test]
        public void VerifyCreateThings()
        {
            this.sessionAnchor.IsSessionOpen = true;
            var thingsToCreate = new List<ElementDefinition>();
            var element = new ElementDefinition();
            element.Name = "Battery";
            element.Owner = this.sessionAnchor.CurrentDomainOfExpertise;
            thingsToCreate.Add(element.Clone(false));
            Assert.DoesNotThrow(() => this.sessionAnchor.CreateThings(thingsToCreate));
        }

        [Test]
        public void VerifyUpdateThings()
        {
            var thingsToUpdate = new List<ElementDefinition>();
            this.sessionAnchor.IsSessionOpen = true;
            var element = new ElementDefinition();
            element.Name = "Battery";
            element.Owner = this.sessionAnchor.CurrentDomainOfExpertise;

            var clone = element.Clone(false);
            clone.Name = "Satellite";
            thingsToUpdate.Add(clone);
            Assert.DoesNotThrow(() => this.sessionAnchor.UpdateThings(thingsToUpdate));
        }
    }
}
