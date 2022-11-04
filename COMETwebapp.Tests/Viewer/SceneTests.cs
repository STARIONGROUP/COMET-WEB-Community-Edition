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
    using Bunit;

    using COMETwebapp.Components.Viewer;
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
        private ISceneProvider scene;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.JsRuntime = context.JSInterop.JSRuntime;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            var factory = new Mock<IShapeFactory>();
            this.context.Services.AddSingleton(factory.Object);

            this.context.Services.AddTransient<ISceneProvider, Scene>();

            var renderer = this.context.RenderComponent<BabylonCanvas>();

            this.scene = renderer.Instance.Scene;
        }

        [Test]
        public void VerifyThatAxesAreAddedToScene()
        {
            this.scene.ClearPrimitives();
            Assert.AreEqual(0, this.scene.GetPrimitives().Count);
            this.scene.AddWorldAxes();
            Assert.AreEqual(3, this.scene.GetPrimitives().Count);
        }

        [Test]
        public void VerifyThatPrimitiveCanBeRetrievedById()
        {
            var primitive = new Cube(1, 1, 1);
            this.scene.AddPrimitive(primitive);
            var retrieved = this.scene.GetPrimitiveById(primitive.ID);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(primitive, retrieved);
        }

        [Test]
        public void VerifyThatGetPrimitivesWorks()
        {
            this.scene.ClearPrimitives();

            var primitive1 = new Cube(1, 1, 1);
            var primitive2 = new Sphere(1);
            var primitive3 = new Cone(1, 1);

            this.scene.AddPrimitive(primitive1);
            this.scene.AddPrimitive(primitive2);
            this.scene.AddPrimitive(primitive3);

            var primitives = this.scene.GetPrimitives();

            Assert.AreEqual(3, primitives.Count);

            var retrieved1 = primitives.Exists(x => x == primitive1);
            var retrieved2 = primitives.Exists(x => x == primitive2);
            var retrieved3 = primitives.Exists(x => x == primitive3);

            Assert.IsTrue(retrieved1);
            Assert.IsTrue(retrieved2);
            Assert.IsTrue(retrieved3);
        }

        [Test]
        public void VerifyThatPrimitivesAreAddedToScene()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            this.scene.AddPrimitive(cube).Wait();
            var prim = this.scene.GetPrimitiveById(cube.ID);
            Assert.AreEqual(cube, prim);
        }

        [Test]
        public void VerifyThatSetPositionWorks()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            cube.SetTranslation(1, 1, 1);
            Assert.AreEqual(1, cube.X);
            Assert.AreEqual(1, cube.Y);
            Assert.AreEqual(1, cube.Z);
        }

        [Test]
        public void VerifyThatSetRotationWorks()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            cube.SetRotation(1, 1, 1);
            Assert.AreEqual(1, cube.RX);
            Assert.AreEqual(1, cube.RY);
            Assert.AreEqual(1, cube.RZ);
        }

        [Test]
        public void VerifyThatSceneCanBeCleared()
        {
            var cube1 = new Cube(1, 1, 1);
            var cube2 = new Cube(1, 1, 1);
            this.scene.ClearPrimitives().Wait();
            this.scene.AddPrimitive(cube1).Wait();
            this.scene.AddPrimitive(cube2).Wait();
            Assert.AreEqual(2, this.scene.GetPrimitives().Count);

            this.scene.ClearPrimitives().Wait();
            Assert.AreEqual(0, this.scene.GetPrimitives().Count);
        }
    }
}
