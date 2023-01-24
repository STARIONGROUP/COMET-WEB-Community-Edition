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

namespace COMETwebapp.Tests.Services.SessionManagement
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
            session = new Mock<ISession>();
            sessionAnchor = new SessionAnchor() { Session = session.Object };
            assembler = new Assembler(uri);
            domain = new DomainOfExpertise(Guid.NewGuid(), assembler.Cache, uri);

            person = new Person(Guid.NewGuid(), assembler.Cache, uri);

            participant = new Participant(Guid.NewGuid(), assembler.Cache, uri)
            {
                Person = person
            };

            referenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, uri)
            {
                ShortName = "ARDL"
            };

            engineeringSetup = new EngineeringModelSetup(Guid.NewGuid(), assembler.Cache, uri)
            {
                RequiredRdl =
                {
                    referenceDataLibrary
                },
                Participant = { participant }
            };

            iteration = new Iteration(Guid.NewGuid(), assembler.Cache, uri)
            {
                Container = new EngineeringModel(Guid.NewGuid(), assembler.Cache, uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), assembler.Cache, uri)
                    {
                        RequiredRdl =
                        {
                            new ModelReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, uri)
                            {
                                FileType =
                                {
                                    new FileType(Guid.NewGuid(), assembler.Cache, uri) { Extension = "tar" },
                                    new FileType(Guid.NewGuid(), assembler.Cache, uri) { Extension = "gz" },
                                    new FileType(Guid.NewGuid(), assembler.Cache, uri) { Extension = "zip" }
                                }
                            }
                        },
                        Participant = { participant }
                    }
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), assembler.Cache, uri)
                {
                    Container = engineeringSetup
                },
                DomainFileStore =
                {
                    new DomainFileStore(Guid.NewGuid(), assembler.Cache, uri) { Owner = domain }
                }
            };
            engineeringSetup.IterationSetup.Add(iteration.IterationSetup);
            openIteration = new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
               new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>()
               {
                    new KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>(iteration, new Tuple<DomainOfExpertise, Participant>(domain, participant))
               });

            siteDirectory = new SiteDirectory(Guid.NewGuid(), assembler.Cache, uri)
            {
                Model = { engineeringSetup }
            };
            siteDirectory.Person.Add(person);
            siteDirectory.Domain.Add(domain);

            session.Setup(x => x.Assembler).Returns(assembler);
            session.Setup(x => x.OpenIterations).Returns(openIteration);
            session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", uri));
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            session.Setup(x => x.ActivePerson).Returns(person);
        }

        [Test]
        public void VerifyClose()
        {
            sessionAnchor.IsSessionOpen = true;
            session.Setup(x => x.Close()).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => sessionAnchor.Close());
            session.Setup(x => x.Close()).Throws<Exception>();
            Assert.DoesNotThrow(() => sessionAnchor.Close());
            sessionAnchor.IsSessionOpen = true;
            Assert.DoesNotThrow(() => sessionAnchor.Close());
        }

        [Test]
        public void VerifyCloseIteration()
        {
            session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
            {
                { iteration, new Tuple<DomainOfExpertise, Participant>(domain, participant)}
            });
            sessionAnchor.IsSessionOpen = true;
            sessionAnchor.ReadIteration(iteration.IterationSetup);
            session.Setup(x => x.CloseIterationSetup(sessionAnchor.OpenIteration.IterationSetup)).Returns(Task.CompletedTask);
            Assert.DoesNotThrow(() => sessionAnchor.CloseIteration());
        }

        [Test]
        public void VerifyGetParticipantModels()
        {
            Assert.That(sessionAnchor.GetParticipantModels(), Is.Not.Null);
        }

        [Test]
        public void VerifyGetModelDomains()
        {
            Assert.That(sessionAnchor.GetModelDomains(engineeringSetup), Is.Not.Null);
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
            sessionAnchor.RefreshSession();

            Assert.That(beginRefreshReceived, Is.True);
            Assert.That(endRefreshReceived, Is.True);
        }

        [Test]
        public void VerifySwitchDomain()
        {
            Assert.DoesNotThrow(() => sessionAnchor.SwitchDomain(domain));

            session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
            {
                { iteration, new Tuple<DomainOfExpertise, Participant>(domain, participant)}
            });
            sessionAnchor.IsSessionOpen = true;
            sessionAnchor.ReadIteration(iteration.IterationSetup);
            Assert.That(sessionAnchor.CurrentDomainOfExpertise, Is.Null);

            sessionAnchor.SwitchDomain(domain);
            Assert.That(sessionAnchor.CurrentDomainOfExpertise, Is.EqualTo(domain));
        }

        [Test]
        public void VerifyCreateThings()
        {
            sessionAnchor.IsSessionOpen = true;
            var thingsToCreate = new List<ElementDefinition>();
            var element = new ElementDefinition();
            element.Name = "Battery";
            element.Owner = sessionAnchor.CurrentDomainOfExpertise;
            thingsToCreate.Add(element.Clone(false));
            Assert.DoesNotThrow(() => sessionAnchor.CreateThings(thingsToCreate));
        }

        [Test]
        public void VerifyUpdateThings()
        {
            var thingsToUpdate = new List<ElementDefinition>();
            sessionAnchor.IsSessionOpen = true;
            var element = new ElementDefinition();
            element.Name = "Battery";
            element.Owner = sessionAnchor.CurrentDomainOfExpertise;

            var clone = element.Clone(false);
            clone.Name = "Satellite";
            thingsToUpdate.Add(clone);
            Assert.DoesNotThrow(() => sessionAnchor.UpdateThings(thingsToUpdate));
        }

        [Test]
        public void VerifyGetParticipant()
        {
            sessionAnchor.IsSessionOpen = true;
            sessionAnchor.ReadIteration(iteration.IterationSetup);
            Assert.That(sessionAnchor.GetParticipant(), Is.EqualTo(participant));
        }
    }
}
