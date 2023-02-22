// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionAnchorTest.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.SessionManagement
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
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
        private Person person1;
        private Organization organization;
        private PersonRole personRole;
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
            this.person1 = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.organization = new Organization(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.personRole = new PersonRole(Guid.NewGuid(), this.assembler.Cache, this.uri);

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
               new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>()
               {
                    new KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant))
               });

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Model = { this.engineeringSetup }
            };
            this.siteDirectory.Person.Add(this.person);
            this.siteDirectory.Person.Add(this.person1);
            this.siteDirectory.Organization.Add(this.organization);
            this.siteDirectory.PersonRole.Add(this.personRole);
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
        public void VerifyCloseIteration()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant)}
            });
            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            this.session.Setup(x => x.CloseIterationSetup(this.sessionAnchor.OpenIteration.IterationSetup)).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => this.sessionAnchor.CloseIteration());
        }

        [Test]
        public void VerifyGetParticipantModels()
        {
            Assert.That(this.sessionAnchor.GetParticipantModels().ToList(), Has.Count.EqualTo(1));
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
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
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

        [Test]
        public void VerifyGetParticipant()
        {
            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            Assert.That(this.sessionAnchor.GetParticipant(), Is.EqualTo(this.participant));
        }

        [Test]
        public void VerifyCreateThingsSiteDirectory()
        {
            Assert.ThrowsAsync<ArgumentException>(() => this.sessionAnchor.CreateThingsSiteDirectory(null));
            this.sessionAnchor.IsSessionOpen = true;
            var thingsToCreate = new List<Person>();
            var person = new Person();
            person.ShortName = "USR1";
            person.GivenName = "USR1";
            thingsToCreate.Add(person.Clone(false));
            Assert.DoesNotThrow(() => this.sessionAnchor.CreateThingsSiteDirectory(thingsToCreate));
        }

        [Test]
        public void VerifyUpdateThingsSiteDirectory()
        {
            Assert.ThrowsAsync<ArgumentException>(() => this.sessionAnchor.UpdateThingsSiteDirectory(null));
            var thingsToUpdate = new List<Person>();
            this.sessionAnchor.IsSessionOpen = true;
            var person = new Person();
            person.ShortName = "USR";
            person.IsDeprecated = false;

            var clone = person.Clone(false);
            clone.IsDeprecated = true;
            thingsToUpdate.Add(clone);
            Assert.DoesNotThrow(() => this.sessionAnchor.UpdateThingsSiteDirectory(thingsToUpdate));
        }

        [Test]
        public void VerifyGetPersons()
        {
            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            Assert.That(this.sessionAnchor.GetPersons().ToList(), Has.Count.EqualTo(2));
        }

        [Test]
        public void VerifyGetAvailableOrganizations()
        {
            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            Assert.That(this.sessionAnchor.GetAvailableOrganizations().ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyGetAvailablePersonRoles()
        {
            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            Assert.That(this.sessionAnchor.GetAvailablePersonRoles().ToList(), Has.Count.EqualTo(1));
        }
        
        [Test]
        public void VerifyGetAvailableDomains()
        {
            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            Assert.That(this.sessionAnchor.GetAvailableDomains().ToList(), Has.Count.EqualTo(1));
        }
    }
}
