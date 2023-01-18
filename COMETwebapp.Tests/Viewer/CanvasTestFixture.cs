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
    using Bunit;
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CanvasTestFixture
    {
        private TestContext context;
        private CanvasComponent canvas;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddTransient<IJSInterop, JSInterop>();

            var renderer = this.context.RenderComponent<CanvasComponent>();
            this.canvas = renderer.Instance;
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeAdded()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);

            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectCanBeAdded()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddTemporarySceneObject(sceneObject);

            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatSceneObjectsCanBeCleared()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(1));

            await this.canvas.ClearSceneObjects();
            Assert.That(this.canvas.GetAllSceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatTemporarySceneObjectsCanBeCleared()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddTemporarySceneObject(sceneObject);
            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(1));

            await this.canvas.ClearTemporarySceneObjects();
            Assert.That(this.canvas.GetAllTemporarySceneObjects(), Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedByID()
        {
            var sceneObject = new Model.SceneObject(new Cube(1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            var retrieved = this.canvas.GetSceneObjectById(sceneObject.ID);
            Assert.That(retrieved, Is.Not.Null);
        }
    }
}
