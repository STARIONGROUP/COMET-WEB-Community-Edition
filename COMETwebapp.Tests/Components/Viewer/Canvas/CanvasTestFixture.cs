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
    using System.Threading.Tasks;

    using Bunit;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CanvasTestFixture
    {
        private TestContext context;
        private CanvasComponent canvas;
        private ISelectionMediator selectionMediator;

        [SetUp]
        public void SetUp()
        {
            context = new TestContext();
            context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionService>();
            context.Services.AddSingleton(session.Object);
            context.Services.AddTransient<ISceneSettings, SceneSettings>();
            context.Services.AddTransient<ISelectionMediator, SelectionMediator>();

            var renderer = context.RenderComponent<CanvasComponent>();
            canvas = renderer.Instance;
            selectionMediator = canvas.SelectionMediator;
        }

        [Test]
        public void VerifyThatMouseEventsWorks()
        {
            Assert.That(canvas.IsMouseDown, Is.False);
            canvas.OnMouseDown(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(canvas.IsMouseDown, Is.True);
            canvas.OnMouseMove(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(canvas.IsMouseDown, Is.EqualTo(canvas.IsMovingScene));
            canvas.OnMouseUp(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(canvas.IsMouseDown, Is.False);
        }

        [Test]
        public void VerifyThatOnTreeVisibiliyChangedWorks()
        {
            var cube = new Cube(1, 1, 1);
            var sceneObject = new SceneObject(cube);
            var treeNode = new TreeNode(sceneObject);
            var beforeSelectedObject = canvas.SelectedSceneObject;
            selectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            var afterSelectedObject = canvas.SelectedSceneObject;
            Assert.That(beforeSelectedObject, Is.Not.EqualTo(afterSelectedObject));
        }

        [Test]
        public void VerifyThatOnTreeSelectionChangedWorks()
        {
            var cube = new Cube(1, 1, 1);
            var sceneObject = new SceneObject(cube);
            var treeNode = new TreeNode(sceneObject);
            var beforeSelectedObject = canvas.SelectedSceneObject;
            selectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            var afterSelectedObject = canvas.SelectedSceneObject;
            Assert.That(beforeSelectedObject, Is.Not.EqualTo(afterSelectedObject));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeAdded()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await canvas.AddSceneObject(sceneObject);

            Assert.That(canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectCanBeAdded()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await canvas.AddTemporarySceneObject(sceneObject);

            Assert.That(canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatSceneObjectsCanBeCleared()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await canvas.AddSceneObject(sceneObject);
            Assert.That(canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));

            await canvas.ClearSceneObjects();
            Assert.That(canvas.GetAllSceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectsCanBeCleared()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await canvas.AddTemporarySceneObject(sceneObject);
            Assert.That(canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));

            await canvas.ClearTemporarySceneObjects();
            Assert.That(canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedByID()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await canvas.AddSceneObject(sceneObject);
            var retrieved = canvas.GetSceneObjectById(sceneObject.ID);
            Assert.That(retrieved, Is.Not.Null);
        }
    }
}
