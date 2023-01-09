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

    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.Interoperability;
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
        private ISceneSettings scene;
        private BabylonCanvas canvas;
        private IJSInterop JSInterop;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            var factory = new Mock<IShapeFactory>();
            this.context.Services.AddSingleton(factory.Object);

            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddTransient<IJSInterop, JSInterop>();

            var renderer = this.context.RenderComponent<BabylonCanvas>();
            canvas = renderer.Instance;
        }

        [Test]
        public void VerifyThatAxesAreAddedToScene()
        {
            this.canvas.ClearPrimitives();
            Assert.AreEqual(0, this.canvas.GetPrimitives().Count);
            this.canvas.AddWorldAxes();
            Assert.AreEqual(3, this.canvas.GetPrimitives().Count);
        }

        [Test]
        public void VerifyThatPrimitiveCanBeRetrievedById()
        {
            var primitive = new Cube(1, 1, 1);
            this.canvas.AddPrimitive(primitive);
            var retrieved = this.canvas.GetPrimitiveById(primitive.ID);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(primitive, retrieved);
        }

        [Test]
        public async void VerifyThatGetPrimitivesWorks()
        {
            await this.canvas.ClearPrimitives();

            var primitive1 = new Cube(1, 1, 1);
            var primitive2 = new Sphere(1);
            var primitive3 = new Cone(1, 1);

            await this.canvas.AddPrimitive(primitive1);
            await this.canvas.AddPrimitive(primitive2);
            await this.canvas.AddPrimitive(primitive3);

            var primitives = this.canvas.GetPrimitives();

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
            this.canvas.AddPrimitive(cube).Wait();
            var prim = this.canvas.GetPrimitiveById(cube.ID);
            Assert.AreEqual(cube, prim);
        }

        [Test]
        public void VerifyThatSetPositionWorks()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            cube.X = 1;
            cube.Y = 1;
            cube.Z = 1;

            Assert.AreEqual(1, cube.X);
            Assert.AreEqual(1, cube.Y);
            Assert.AreEqual(1, cube.Z);
        }

        [Test]
        public void VerifyThatSetRotationWorks()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            cube.RX = 1;
            cube.RY = 1;
            cube.RZ = 1;

            Assert.AreEqual(1, cube.RX);
            Assert.AreEqual(1, cube.RY);
            Assert.AreEqual(1, cube.RZ);
        }

        [Test]
        public void VerifyThatSceneCanBeCleared()
        {
            var cube1 = new Cube(1, 1, 1);
            var cube2 = new Cube(1, 1, 1);
            this.canvas.ClearPrimitives().Wait();
            this.canvas.AddPrimitive(cube1).Wait();
            this.canvas.AddPrimitive(cube2).Wait();
            Assert.AreEqual(2, this.canvas.GetPrimitives().Count);

            this.canvas.ClearPrimitives().Wait();
            Assert.AreEqual(0, this.canvas.GetPrimitives().Count);
        }
    }
}
