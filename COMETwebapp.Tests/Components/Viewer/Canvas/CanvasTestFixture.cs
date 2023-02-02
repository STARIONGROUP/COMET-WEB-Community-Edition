// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Tests.Components.Viewer.Canvas
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;
    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CanvasTestFixture
    {
        private TestContext context;
        private CanvasComponent canvas;
        private ICanvasViewModel viewModel;

        private Mock<IBabylonInterop> babylonInterop;
        private Mock<ISelectionMediator> selectionMediator;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            this.context.ConfigureDevExpressBlazor();

            this.babylonInterop = new Mock<IBabylonInterop>();
            this.selectionMediator = new Mock<ISelectionMediator>();

            this.viewModel = new CanvasViewModel(this.babylonInterop.Object, this.selectionMediator.Object);
            this.context.Services.AddSingleton(this.viewModel);
        }

        [Test]
        public void VerifyThatMouseEventsWorks()
        {
            Assert.That(this.canvas.IsMouseDown, Is.False);
            this.canvas.OnMouseDown(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(this.canvas.IsMouseDown, Is.True);
            this.canvas.OnMouseMove(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(this.canvas.IsMouseDown, Is.EqualTo(this.canvas.IsMovingScene));
            this.canvas.OnMouseUp(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(this.canvas.IsMouseDown, Is.False);
        }

        [Test]
        public void VerifyThatOnTreeVisibiliyChangedWorks()
        {
            var cube = new Cube(1, 1, 1);
            var sceneObject = new SceneObject(cube);
            var treeNode = new TreeNode(sceneObject);
            var beforeSelectedObject = this.viewModel.SelectionMediator.SelectedSceneObject;
            //this.viewModel.SelectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            var afterSelectedObject = this.viewModel.SelectionMediator.SelectedSceneObject;
            Assert.That(beforeSelectedObject, Is.Not.EqualTo(afterSelectedObject));
        }

        [Test]
        public void VerifyThatOnTreeSelectionChangedWorks()
        {
            var cube = new Cube(1, 1, 1);
            var sceneObject = new SceneObject(cube);
            var treeNode = new TreeNode(sceneObject);
            var beforeSelectedObject = this.viewModel.SelectionMediator.SelectedSceneObject;
            //this.viewModel.SelectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            var afterSelectedObject = this.viewModel.SelectionMediator.SelectedSceneObject;
            Assert.That(beforeSelectedObject, Is.Not.EqualTo(afterSelectedObject));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeAdded()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddSceneObject(sceneObject);

            Assert.That(this.viewModel.GetAllSceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectCanBeAdded()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddTemporarySceneObject(sceneObject);

            Assert.That(this.viewModel.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatSceneObjectsCanBeCleared()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddSceneObject(sceneObject);
            Assert.That(this.viewModel.GetAllSceneObjects(), Has.Count.EqualTo(1));

            await this.viewModel.ClearSceneObjects();
            Assert.That(this.viewModel.GetAllSceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectsCanBeCleared()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddTemporarySceneObject(sceneObject);
            Assert.That(this.viewModel.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));

            await this.viewModel.ClearTemporarySceneObjects();
            Assert.That(this.viewModel.GetAllTemporarySceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedByID()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.viewModel.AddSceneObject(sceneObject);
            var retrieved = this.viewModel.GetSceneObjectById(sceneObject.ID);
            Assert.Multiple(() =>
            {
                Assert.That(retrieved, Is.Not.Null);
                Assert.That(sceneObject, Is.EqualTo(retrieved));
            });
        }

        [Test]
        public async Task VerifyThatGetAllSceneObjectsWorks()
        {
            await this.viewModel.ClearSceneObjects();

            var sceneObj1 = new SceneObject(new Cube(1, 1, 1));
            var sceneObj2 = new SceneObject(new Sphere(1));
            var sceneObj3 = new SceneObject(new Cone(1, 1));

            await this.viewModel.AddSceneObject(sceneObj1);
            await this.viewModel.AddSceneObject(sceneObj2);
            await this.viewModel.AddSceneObject(sceneObj3);

            var primitives = this.viewModel.GetAllSceneObjects();

            Assert.AreEqual(3, primitives.Count);

            var retrieved1 = primitives.Any(x => x == sceneObj1);
            var retrieved2 = primitives.Any(x => x == sceneObj2);
            var retrieved3 = primitives.Any(x => x == sceneObj3);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(retrieved1);
                Assert.IsTrue(retrieved2);
                Assert.IsTrue(retrieved3);
            });
        }
    }
}
