// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerBodyViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Viewer
{
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DynamicData;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ViewerBodyViewModelTestFixture
    {
        private Mock<ISessionService> sessionService;
        private Mock<ISelectionMediator> selectionMediator;
        private Mock<IBabylonInterop> babylonInterop;
        private CDPMessageBus messageBus;
        private SourceList<Iteration> openIterations;

        private ViewerBodyViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.sessionService = new Mock<ISessionService>();
            this.openIterations = new SourceList<Iteration>();
            this.sessionService.SetupGet(x => x.OpenIterations).Returns(this.openIterations);
            var session = new Mock<ISession>();
            this.sessionService.SetupGet(x => x.Session).Returns(session.Object);

            this.selectionMediator = new Mock<ISelectionMediator>();
            this.babylonInterop = new Mock<IBabylonInterop>();
            this.messageBus = new CDPMessageBus();

            this.viewModel = new ViewerBodyViewModel(
                this.sessionService.Object,
                this.selectionMediator.Object,
                this.babylonInterop.Object,
                this.messageBus);
        }

        [Test]
        public void VerifyInitializeElementsAndCreateTree()
        {
            var iteration = new Iteration { Iid = Guid.NewGuid() };
            this.openIterations.Add(iteration);
            var elementDef = new ElementDefinition { Iid = Guid.NewGuid() };
            iteration.Element.Add(elementDef);
            iteration.TopElement = elementDef;

            this.viewModel.CurrentThing = iteration;

            // Setting selected options to satisfy the conditions to create tree
            this.viewModel.OptionSelector.SelectedOption = new Option { Iid = Guid.NewGuid() };

            this.viewModel.InitializeElementsAndCreateTree();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Elements, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.ProductTreeViewModel.RootViewModel, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyInitializeViewModel()
        {
            var iteration = new Iteration { Iid = Guid.NewGuid() };
            this.openIterations.Add(iteration);
            var elementDef = new ElementDefinition { Iid = Guid.NewGuid() };
            iteration.Element.Add(elementDef);
            iteration.TopElement = elementDef;

            this.viewModel.CurrentThing = iteration;

            await this.viewModel.InitializeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsLoading, Is.False);
                Assert.That(this.viewModel.Elements, Is.Not.Null);
                Assert.That(this.viewModel.Elements, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyOnSessionRefreshed()
        {
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var uri = new Uri("http://test.com");
            var domain = new DomainOfExpertise(Guid.NewGuid(), cache, uri) { Name = "domain" };

            var shapeKindParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, uri)
            {
                Manual = new ValueArray<string>(new List<string> { "box" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var shapeKindParameterType = new EnumerationParameterType(Guid.NewGuid(), cache, uri) { Name = "Shape Kind", ShortName = SceneSettings.ShapeKindShortName };
            var shapeKindParameter = new Parameter(Guid.NewGuid(), cache, uri) { ParameterType = shapeKindParameterType };
            shapeKindParameter.ValueSet.Add(shapeKindParameterValueSet);

            var iteration = new Iteration(Guid.NewGuid(), cache, uri);
            this.openIterations.Add(iteration);

            var elementDefWithShape = new ElementDefinition(Guid.NewGuid(), cache, uri) { Container = iteration, Owner = domain };
            elementDefWithShape.Parameter.Add(shapeKindParameter);
            iteration.Element.Add(elementDefWithShape);
            iteration.TopElement = elementDefWithShape;

            var childDefWithShape = new ElementDefinition(Guid.NewGuid(), cache, uri) { Owner = domain };
            childDefWithShape.Parameter.Add(shapeKindParameter);

            var updatedElementWithShape = new ElementUsage(Guid.NewGuid(), cache, uri) { ElementDefinition = childDefWithShape, Container = elementDefWithShape, Owner = domain };
            elementDefWithShape.ContainedElement.Add(updatedElementWithShape);

            var childDefWithoutShape = new ElementDefinition(Guid.NewGuid(), cache, uri) { Owner = domain };

            var updatedElementWithoutShape = new ElementUsage(Guid.NewGuid(), cache, uri) { ElementDefinition = childDefWithoutShape, Container = elementDefWithShape, Owner = domain };
            elementDefWithShape.ContainedElement.Add(updatedElementWithoutShape);

            var childDefDeleted = new ElementDefinition(Guid.NewGuid(), cache, uri) { Owner = domain };
            var deletedElementUsage = new ElementUsage { Iid = Guid.NewGuid(), ElementDefinition = childDefDeleted, Container = elementDefWithShape };
            elementDefWithShape.ContainedElement.Add(deletedElementUsage);

            this.viewModel.CurrentThing = iteration;
            await this.viewModel.InitializeViewModel();

            this.viewModel.OptionSelector.SelectedOption = new Option(Guid.NewGuid(), cache, uri);

            var childDefNew = new ElementDefinition(Guid.NewGuid(), cache, uri) { Owner = domain };
            var newElementUsage = new ElementUsage { Iid = Guid.NewGuid(), ElementDefinition = childDefNew, Container = elementDefWithShape };
            elementDefWithShape.ContainedElement.Add(newElementUsage);

            // Set visibility to false on the one with shape
            var nodeWithShape = this.viewModel.ProductTreeViewModel.RootViewModel.GetFlatListOfDescendants(true)
                .FirstOrDefault(n => n.SceneObject != null && n.SceneObject.ElementBase.Iid == updatedElementWithShape.Iid);

            if (nodeWithShape != null)
            {
                nodeWithShape.IsSceneObjectVisible = false;
            }

            this.messageBus.SendMessage(new ObjectChangedEvent(newElementUsage, EventKind.Added), typeof(ElementBase));
            this.messageBus.SendMessage(new ObjectChangedEvent(deletedElementUsage, EventKind.Removed), typeof(ElementBase));
            this.messageBus.SendMessage(new ObjectChangedEvent(updatedElementWithShape, EventKind.Updated), typeof(ElementBase));
            this.messageBus.SendMessage(new ObjectChangedEvent(updatedElementWithoutShape, EventKind.Updated), typeof(ElementBase));

            this.babylonInterop.Invocations.Clear();

            // Trigger OnSessionRefreshed
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            await Task.Delay(200);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Elements, Does.Contain(newElementUsage));
                Assert.That(this.viewModel.Elements, Does.Not.Contain(deletedElementUsage));

                // Verify Babylon calls for updatedElementWithShape
                this.babylonInterop.Verify(x => x.ClearSceneObject(It.Is<SceneObject>(so => so.ElementBase.Iid == updatedElementWithShape.Iid)), Times.AtLeastOnce);
                this.babylonInterop.Verify(x => x.AddSceneObject(It.Is<SceneObject>(so => so.ElementBase.Iid == updatedElementWithShape.Iid)), Times.AtLeastOnce);
                this.babylonInterop.Verify(x => x.SetVisibility(It.Is<SceneObject>(so => so.ElementBase.Iid == updatedElementWithShape.Iid), false), Times.Once);

                // Without shape should not trigger CanvasViewModel Add/Remove
                this.babylonInterop.Verify(x => x.ClearSceneObject(It.Is<SceneObject>(so => so.ElementBase.Iid == updatedElementWithoutShape.Iid)), Times.Never);
                this.babylonInterop.Verify(x => x.AddSceneObject(It.Is<SceneObject>(so => so.ElementBase.Iid == updatedElementWithoutShape.Iid)), Times.Never);
            });
        }
    }
}
