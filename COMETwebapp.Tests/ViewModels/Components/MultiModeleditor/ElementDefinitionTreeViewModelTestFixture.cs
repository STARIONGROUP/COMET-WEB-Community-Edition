// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.ViewModels.Components.MultiModeleditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.MultiModelEditor;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionTreeViewModelTestFixture
    {
        private ElementDefinitionTreeViewModel viewModel;
        private DomainOfExpertise domain;
        private Mock<ISessionService> sessionService;
        private Iteration iteration;
        private CDPMessageBus messageBus;
        private ElementDefinition topElement;
        private SourceList<Iteration> iterations;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.domain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                ShortName = "SYS"
            };

            session.Setup(x => x.ActivePerson).Returns(new Person
            {
                DefaultDomain = this.domain
            });

            var siteDirectory = new SiteDirectory { Domain = { this.domain } };
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.topElement = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                Owner = this.domain
            };

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                Owner = this.domain
            };

            var usage1 = new ElementUsage
            {
                Name = "Box1",
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDefinition,
                Owner = this.domain
            };

            this.topElement.ContainedElement.Add(usage1);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.topElement, elementDefinition },
                TopElement = this.topElement,
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = new EngineeringModelSetup()
                },
                IterationSetup = new IterationSetup
                {
                    IterationNumber = 1,
                    Container = new EngineeringModelSetup { Name = "ModelName", ShortName = "ModelShortName" }
                }
            };

            this.iterations = new SourceList<Iteration>();
            this.iterations.Add(this.iteration);

            this.sessionService.Setup(x => x.OpenIterations).Returns(this.iterations);

            this.viewModel = new ElementDefinitionTreeViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySelectIteration()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Description, Is.EqualTo("Please select a model"));
                Assert.That(this.viewModel.Iteration, Is.Null);
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedIterationData, Is.Null);
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(0));
            });

            this.viewModel.Iteration = this.iteration;
            var expectedIterationData = new IterationData(this.iteration.IterationSetup, true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Description, Is.EqualTo(expectedIterationData.IterationName));
                Assert.That(this.viewModel.Iteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedIterationData, Is.EqualTo(expectedIterationData));
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2));
            });
        }

        [Test]
        public void VerifyUpdatedElement()
        {
            this.viewModel.Iteration = this.iteration;
            var expectedIterationData = new IterationData(this.iteration.IterationSetup, true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Description, Is.EqualTo(expectedIterationData.IterationName));
                Assert.That(this.viewModel.Iteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedIterationData, Is.EqualTo(expectedIterationData));
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2));
            });

            Assert.That(this.viewModel.GetUpdatedThings(), Is.Empty);

            this.topElement.Name = "Changed";
            this.messageBus.SendObjectChangeEvent(this.topElement, EventKind.Updated);

            Assert.That(this.viewModel.GetUpdatedThings(), Has.Count.EqualTo(1));

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows.Single(x => x.IsTopElement).ElementName, Is.EqualTo(this.topElement.Name));
        }

        [Test]
        public void VerifyRemoveAndAddElement()
        {
            this.viewModel.Iteration = this.iteration;
            var expectedIterationData = new IterationData(this.iteration.IterationSetup, true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Description, Is.EqualTo(expectedIterationData.IterationName));
                Assert.That(this.viewModel.Iteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedIterationData, Is.EqualTo(expectedIterationData));
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2));
            });

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            Assert.That(this.viewModel.GetDeletedThings(), Is.Empty);
            Assert.That(this.viewModel.GetAddedThings(), Is.Empty);
            Assert.That(this.viewModel.GetUpdatedThings(), Is.Empty);

            this.messageBus.SendObjectChangeEvent(this.topElement, EventKind.Removed);

            Assert.That(this.viewModel.GetDeletedThings(), Has.Count.EqualTo(1));
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(2));

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.messageBus.SendObjectChangeEvent(this.topElement, EventKind.Added);

            Assert.That(this.viewModel.GetAddedThings(), Has.Count.EqualTo(1));
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(2));

            Assert.That(this.viewModel.Rows.Single(x => x.IsTopElement).ElementName, Is.EqualTo(this.topElement.Name));
        }

        [Test]
        public void VerifyIterationRefresh()
        {
            Assert.That(this.viewModel.Iterations, Has.Count.EqualTo(2));

            this.iterations.Remove(this.iteration);
            Assert.That(this.viewModel.Iterations, Has.Count.EqualTo(1));

            this.iterations.Add(this.iteration);

            Assert.That(this.viewModel.Iterations, Has.Count.EqualTo(2));
        }
    }
}
