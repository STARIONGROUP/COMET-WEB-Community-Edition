// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Tests.Viewer
{
    using System.Threading.Tasks;
    
    using Bunit;
    
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;
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
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddTransient<IJSInterop, JSInterop>();
            this.context.Services.AddTransient<ISelectionMediator, SelectionMediator>();
           
            var renderer = this.context.RenderComponent<CanvasComponent>();
            this.canvas = renderer.Instance;
            this.selectionMediator = this.canvas.SelectionMediator;
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
            var beforeSelectedObject = this.canvas.SelectedSceneObject;
            this.selectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            var afterSelectedObject = this.canvas.SelectedSceneObject;
            Assert.That(beforeSelectedObject, Is.Not.EqualTo(afterSelectedObject));
        }

        [Test]
        public void VerifyThatOnTreeSelectionChangedWorks()
        {
            var cube = new Cube(1,1,1);
            var sceneObject = new SceneObject(cube);
            var treeNode = new TreeNode(sceneObject);
            var beforeSelectedObject = this.canvas.SelectedSceneObject;
            this.selectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            var afterSelectedObject = this.canvas.SelectedSceneObject;
            Assert.That(beforeSelectedObject, Is.Not.EqualTo(afterSelectedObject));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeAdded()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);

            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectCanBeAdded()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddTemporarySceneObject(sceneObject);

            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatSceneObjectsCanBeCleared()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));

            await this.canvas.ClearSceneObjects();
            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectsCanBeCleared()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddTemporarySceneObject(sceneObject);
            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));

            await this.canvas.ClearTemporarySceneObjects();
            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedByID()
        {
            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            var retrieved = this.canvas.GetSceneObjectById(sceneObject.ID);
            Assert.That(retrieved, Is.Not.Null);
        }
    }
}
