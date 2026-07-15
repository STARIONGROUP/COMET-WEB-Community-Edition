// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ElementDefinitionTreeViewModelTestFixture
    {
        private ElementDefinitionTreeViewModel viewModel;
        private DomainOfExpertise domain;
        private Mock<ISessionService> sessionService;
        private Mock<ICopySettingsViewModel> copySettingsViewModel;
        private Iteration iteration;
        private CDPMessageBus messageBus;
        private ElementDefinition topElement;
        private SourceList<Iteration> iterations;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.copySettingsViewModel = new Mock<ICopySettingsViewModel>();
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

            this.viewModel = new ElementDefinitionTreeViewModel(this.sessionService.Object, this.messageBus, this.copySettingsViewModel.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyIterationRefresh()
        {
            Assert.That(this.viewModel.Iterations, Has.Count.EqualTo(1));

            this.iterations.Remove(this.iteration);
            Assert.That(this.viewModel.Iterations, Has.Count.EqualTo(0));

            this.iterations.Add(this.iteration);

            Assert.That(this.viewModel.Iterations, Has.Count.EqualTo(1));
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
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(1));
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
        public void VerifySelectIteration()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Description, Is.EqualTo("Please select a model"));
                Assert.That(this.viewModel.Iteration, Is.Null);
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.SelectedIterationData, Is.Null);
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(0));
            });

            this.viewModel.Iteration = this.iteration;
            var expectedIterationData = new IterationData(this.iteration.IterationSetup, true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Description, Is.EqualTo(expectedIterationData.IterationName));
                Assert.That(this.viewModel.Iteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(1));
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
                Assert.That(this.viewModel.Iterations.Count, Is.EqualTo(1));
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
        public async Task VerifyExternalRemovalOfRowElementPropagatesAfterEndUpdate()
        {
            this.viewModel.Iteration = this.iteration;

            Assert.That(this.viewModel.Rows.Any(x => x.ElementBase.Iid == this.topElement.Iid), Is.True);

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            // Simulate the other panel's write: the ElementDefinition is removed from the iteration and the
            // corresponding events reach this session's message bus.
            this.iteration.Element.Remove(this.topElement);
            this.messageBus.SendObjectChangeEvent(this.topElement, EventKind.Removed);
            this.messageBus.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to the external removal");

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Any(x => x.ElementBase.Iid == this.topElement.Iid), Is.False,
                    "A Removed ObjectChangedEvent for a row's ElementDefinition, followed by EndUpdate, must remove the row.");

                Assert.That(isLoadingValues, Has.Some.EqualTo(true),
                    "IsLoading must toggle so the tree component re-renders.");
            });
        }

        /// <summary>
        /// Regression test for GH799 symptom B: renaming an <see cref="ElementUsage" /> nested under a row must be
        /// reflected in the tree. <see cref="ElementDefinitionTreeViewModel.AddRows" />,
        /// <see cref="ElementDefinitionTreeViewModel.UpdateRows" /> and <see cref="ElementDefinitionTreeViewModel.RemoveRows" />
        /// all filter their input to <c>.OfType&lt;ElementDefinition&gt;()</c>, so an <see cref="ObjectChangedEvent" />
        /// for a renamed <see cref="ElementUsage" /> is recorded but silently discarded when the rows are refreshed.
        /// </summary>
        [Test]
        public void VerifyExternalRenameOfElementUsagePropagatesAfterEndUpdate()
        {
            this.viewModel.Iteration = this.iteration;
            var usage = this.topElement.ContainedElement.Single();

            Assert.That(this.viewModel.Rows.Single(x => x.IsTopElement).Rows.Single(x => x.ElementBase.Iid == usage.Iid).ElementName,
                Is.EqualTo("Box1"));

            // Simulate another panel's write: the usage is renamed and the corresponding events reach this
            // session's message bus.
            usage.Name = "Renamed";
            this.messageBus.SendObjectChangeEvent(usage, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));

            Assert.That(() => this.viewModel.Rows.Single(x => x.IsTopElement).Rows.Single(x => x.ElementBase.Iid == usage.Iid).ElementName == "Renamed",
                Is.True.After(2000, 25),
                "An ElementUsage rename followed by EndUpdate must be reflected in its nested tree row, " +
                "but AddRows/UpdateRows/RemoveRows only handle ElementDefinition changes.");
        }
    }
}
