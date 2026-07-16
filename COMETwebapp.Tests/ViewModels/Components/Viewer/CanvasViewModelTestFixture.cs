// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.Viewer
{
    using System.Threading.Tasks;

    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="CanvasViewModel" />.
    /// </summary>
    [TestFixture]
    public class CanvasViewModelTestFixture
    {
        private CanvasViewModel viewModel;
        private Mock<IBabylonInterop> babylonInterop;
        private Mock<ISelectionMediator> selectionMediator;

        /// <summary>
        /// Set up the test run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.babylonInterop = new Mock<IBabylonInterop>();
            this.selectionMediator = new Mock<ISelectionMediator>();

            this.babylonInterop.Setup(x => x.InitCanvas(It.IsAny<ElementReference>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
            this.babylonInterop.Setup(x => x.AddSceneObject(It.IsAny<SceneObject>())).Returns(Task.CompletedTask);
            this.babylonInterop.Setup(x => x.ClearSceneObject(It.IsAny<SceneObject>())).Returns(Task.CompletedTask);
            this.babylonInterop.Setup(x => x.ClearSceneObjects(It.IsAny<IEnumerable<SceneObject>>())).Returns(Task.CompletedTask);
            this.babylonInterop.Setup(x => x.SetVisibility(It.IsAny<SceneObject>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
            this.babylonInterop.Setup(x => x.RegenerateMesh(It.IsAny<SceneObject>())).Returns(Task.CompletedTask);

            this.viewModel = new CanvasViewModel(this.babylonInterop.Object, this.selectionMediator.Object);
        }

        /// <summary>
        /// Verifies that scene objects can be added and temporary scene objects can be added.
        /// </summary>
        [Test]
        public async Task VerifyThatSceneObjectsCanBeAdded()
        {
            var sceneObject1 = new SceneObject(new Cube(1, 1, 1));
            var sceneObject2 = new SceneObject(new Sphere(1));

            await this.viewModel.AddSceneObject(sceneObject1);
            await this.viewModel.AddTemporarySceneObject(sceneObject2);

            var sceneObjects = this.viewModel.GetAllSceneObjects();
            var tempSceneObjects = this.viewModel.GetAllTemporarySceneObjects();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sceneObjects, Has.Count.EqualTo(1));
                Assert.That(tempSceneObjects, Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that the scene can be cleared.
        /// </summary>
        [Test]
        public async Task VerifyThatSceneCanBeCleared()
        {
            var sceneObject1 = new SceneObject(new Cube(1, 1, 1));
            var sceneObject2 = new SceneObject(new Sphere(1));

            await this.viewModel.AddSceneObject(sceneObject1);
            await this.viewModel.AddTemporarySceneObject(sceneObject2);

            var sceneObjects = this.viewModel.GetAllSceneObjects();
            var tempSceneObjects = this.viewModel.GetAllTemporarySceneObjects();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sceneObjects, Has.Count.EqualTo(1));
                Assert.That(tempSceneObjects, Has.Count.EqualTo(1));
            }

            await this.viewModel.ClearScene();

            sceneObjects = this.viewModel.GetAllSceneObjects();
            tempSceneObjects = this.viewModel.GetAllTemporarySceneObjects();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sceneObjects, Has.Count.EqualTo(0));
                Assert.That(tempSceneObjects, Has.Count.EqualTo(0));
            }
        }

        /// <summary>
        /// Verifies that a scene object can be retrieved by its ID.
        /// </summary>
        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedById()
        {
            var sceneObject1 = new SceneObject(new Cube(1, 1, 1));

            await this.viewModel.AddSceneObject(sceneObject1);

            var result = this.viewModel.GetSceneObjectById(sceneObject1.ID);

            Assert.That(result, Is.Not.Null);
        }

        /// <summary>
        /// Verifies the InitializeViewModel method.
        /// </summary>
        [Test]
        public void VerifyInitializeViewModel()
        {
            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel());
        }

        /// <summary>
        /// Verifies the RemoveSceneObject method.
        /// </summary>
        [Test]
        public async Task VerifyRemoveSceneObject()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddSceneObject(sceneObject);

            await this.viewModel.RemoveSceneObject(sceneObject);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetAllSceneObjects(), Has.Count.EqualTo(0));
                this.babylonInterop.Verify(x => x.ClearSceneObject(sceneObject), Times.Once);
            }

            // Verify null parameter does not throw
            Assert.DoesNotThrowAsync(async () => await this.viewModel.RemoveSceneObject(null));
        }

        /// <summary>
        /// Verifies the OnParameterChanged callback behaviour.
        /// </summary>
        [Test]
        public async Task VerifyParameterChangedCallback()
        {
            var original = new SceneObject(new Cube(1, 1, 1));
            var clone = original.Clone();
            this.selectionMediator.SetupGet(x => x.SelectedSceneObject).Returns(original);
            this.selectionMediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(clone);

            this.viewModel.InitializeViewModel();

            await this.selectionMediator.RaiseAsync(x => x.OnParameterChanged += null);
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
                Assert.That(this.viewModel.GetAllTemporarySceneObjects()[0], Is.EqualTo(clone));
                this.babylonInterop.Verify(x => x.SetVisibility(original, false), Times.Once);
            }
        }

        /// <summary>
        /// Verifies the OnParameterSubmitted callback behaviour.
        /// </summary>
        [Test]
        public async Task VerifyParameterSubmittedCallback()
        {
            var original = new SceneObject(new Cube(1, 1, 1));
            var clone = original.Clone();
            this.selectionMediator.SetupGet(x => x.SelectedSceneObject).Returns(original);
            this.selectionMediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(clone);

            this.viewModel.InitializeViewModel();

            await this.selectionMediator.RaiseAsync(x => x.OnParameterSubmitted += null);
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(original.Primitive, Is.EqualTo(clone.Primitive));
                this.babylonInterop.Verify(x => x.RegenerateMesh(original), Times.Once);
            }

            // Test scenario where shape type changed
            var newClone = new SceneObject(new Sphere(1));
            this.selectionMediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(newClone);

            await this.selectionMediator.RaiseAsync(x => x.OnParameterSubmitted += null);
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                this.babylonInterop.Verify(x => x.ClearSceneObject(original), Times.Once);
                this.babylonInterop.Verify(x => x.AddSceneObject(original), Times.Once);
            }
        }

        /// <summary>
        /// Verifies the OnTreeSelectionChanged callback behaviour.
        /// </summary>
        [Test]
        public async Task VerifyTreeSelectionChangedCallback()
        {
            var original = new SceneObject(new Cube(1, 1, 1));
            var clone = original.Clone();
            this.selectionMediator.SetupGet(x => x.SelectedSceneObject).Returns(original);
            this.selectionMediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(clone);
            this.selectionMediator.SetupGet(x => x.SceneObjectHasChanges).Returns(true);

            this.viewModel.InitializeViewModel();

            var nodeViewModel = new ViewerNodeViewModel(original)
            {
                IsSelected = true,
                IsSceneObjectVisible = true
            };
            
            await this.selectionMediator.RaiseAsync(x => x.OnTreeSelectionChanged += null, nodeViewModel);
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
                Assert.That(this.viewModel.GetAllTemporarySceneObjects()[0], Is.EqualTo(clone));
            }
        }

        /// <summary>
        /// Verifies the OnTreeVisibilityChanged callback behaviour.
        /// </summary>
        [Test]
        public async Task VerifyTreeVisibilityChangedCallback()
        {
            var original = new SceneObject(new Cube(1, 1, 1));
            var clone = original.Clone();
            this.selectionMediator.SetupGet(x => x.SelectedSceneObject).Returns(original);
            this.selectionMediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(clone);
            this.selectionMediator.SetupGet(x => x.SceneObjectHasChanges).Returns(true);

            this.viewModel.InitializeViewModel();

            var nodeViewModel = new ViewerNodeViewModel(original)
            {
                IsSceneObjectVisible = true
            };
           
            await this.selectionMediator.RaiseAsync(x => x.OnTreeVisibilityChanged += null, nodeViewModel);
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
                Assert.That(this.viewModel.GetAllTemporarySceneObjects()[0], Is.EqualTo(clone));
                this.babylonInterop.Verify(x => x.SetVisibility(original, true), Times.Once);
            }
        }

        /// <summary>
        /// Verifies the HandleMouseUp method.
        /// </summary>
        [Test]
        public async Task VerifyHandleMouseUp()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddSceneObject(sceneObject);

            this.babylonInterop.Setup(x => x.GetPrimitiveIdUnderMouseAsync()).ReturnsAsync(sceneObject.ID);
            this.selectionMediator.SetupGet(x => x.SceneObjectHasChanges).Returns(false);

            await this.viewModel.HandleMouseUp();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsOnChangePrimitiveMode, Is.False);
                this.selectionMediator.Verify(x => x.RaiseOnModelSelectionChanged(sceneObject), Times.Once);
            }
        }
    }
}
