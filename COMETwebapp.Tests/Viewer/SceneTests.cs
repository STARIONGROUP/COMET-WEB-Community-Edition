// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SceneTests.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    /// <summary>
    /// Scene tests that verifies the correct behavior of JSInterop
    /// </summary>
    [TestFixture]
    public class SceneTests
    {
        private TestContext context;
        private BabylonCanvas canvas;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddTransient<IJSInterop, JSInterop>();

            var renderer = this.context.RenderComponent<BabylonCanvas>();
            canvas = renderer.Instance;
        }

        [Test]
        public async Task VerifyThatPrimitiveCanBeRetrievedById()
        {
            var sceneObj = new SceneObject(new Cube(1, 1, 1));

            await this.canvas.AddSceneObject(sceneObj);

            var retrieved = this.canvas.GetSceneObjectById(sceneObj.ID);

            Assert.Multiple(() =>
            {
                Assert.That(retrieved, Is.Not.Null);
                Assert.That(sceneObj, Is.EqualTo(retrieved));
            });
        }

        [Test]
        public async Task VerifyThatGetSceneObjectsWorks()
        {
            await this.canvas.ClearSceneObjects();

            var sceneObj1 = new SceneObject(new Cube(1, 1, 1));
            var sceneObj2 = new SceneObject(new Sphere(1));
            var sceneObj3 = new SceneObject(new Cone(1, 1));

            await this.canvas.AddSceneObject(sceneObj1);
            await this.canvas.AddSceneObject(sceneObj2);
            await this.canvas.AddSceneObject(sceneObj3);

            var primitives = this.canvas.GetAllSceneObjects();

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

        [Test]
        public async Task VerifyThatPrimitivesAreAddedToScene()
        {
            var sceneObject = new SceneObject(new Cube(0, 0, 0, 1, 1, 1));
            await this.canvas.AddSceneObject(sceneObject);
            var sceneObjectRetrieved = this.canvas.GetSceneObjectById(sceneObject.ID);
            Assert.That(sceneObject, Is.EqualTo(sceneObjectRetrieved));
        }

        [Test]
        public void VerifyThatSetPositionWorks()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            cube.X = 1;
            cube.Y = 1;
            cube.Z = 1;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, cube.X);
                Assert.AreEqual(1, cube.Y);
                Assert.AreEqual(1, cube.Z);
            });
        }

        [Test]
        public void VerifyThatSetRotationWorks()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            cube.RX = 1;
            cube.RY = 1;
            cube.RZ = 1;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, cube.RX);
                Assert.AreEqual(1, cube.RY);
                Assert.AreEqual(1, cube.RZ);
            });
        }

        [Test]
        public async Task VerifyThatSceneCanBeCleared()
        {
            var cube1 = new Cube(1, 1, 1);
            var cube2 = new Cube(1, 1, 1);
            await this.canvas.ClearSceneObjects();
            await this.canvas.AddSceneObject(new SceneObject(cube1));
            await this.canvas.AddSceneObject(new SceneObject(cube2));
            Assert.AreEqual(2, this.canvas.GetAllSceneObjects().Count);

            await this.canvas.ClearSceneObjects();
            Assert.AreEqual(0, this.canvas.GetAllSceneObjects().Count);
        }
    }
}
