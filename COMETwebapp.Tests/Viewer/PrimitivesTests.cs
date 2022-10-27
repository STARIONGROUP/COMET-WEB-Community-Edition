// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveTests.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    using Bunit;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    /// <summary>
    /// Primitives tests that verifies the correct behavior of JSInterop
    /// </summary>
    [TestFixture]
    public class PrimitivesTests
    {
        private TestContext context;
        private List<PositionablePrimitive> positionables;
        private List<Primitive> primitives;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.JsRuntime = this.context.JSInterop.JSRuntime;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);
            var factory = new Mock<IShapeFactory>();
            this.context.Services.AddSingleton(factory.Object);

            var renderer = this.context.RenderComponent<BabylonCanvas>();
            this.positionables = new List<PositionablePrimitive>();
            this.positionables.Add(new Cone(1, 2));
            this.positionables.Add(new Cube(1, 1, 1));
            this.positionables.Add(new Cylinder(1, 1));
            this.positionables.Add(new Sphere(1));
            this.positionables.Add(new Torus(2, 1));

            this.primitives = new List<Primitive>(this.positionables);
            this.primitives.Add(new Line(new System.Numerics.Vector3(), new System.Numerics.Vector3(1, 1, 1)));
            this.primitives.Add(new CustomPrimitive(string.Empty, string.Empty));
        }

        [Test]
        public void VerifyThatPrimitivesHaveValidPropertyName()
        {
            foreach (var primitive in this.primitives)
            {
                Assert.AreEqual(primitive.GetType().Name, primitive.Type);
            }
        }

        [Test]
        public void VerifyThatPrimitivesCanBeTranslated()
        {
            foreach (var primitive in this.positionables)
            {
                Assert.AreEqual(0, primitive.X);
                Assert.AreEqual(0, primitive.Y);
                Assert.AreEqual(0, primitive.Z);

                primitive.SetTranslation(1, 1, 1);

                Assert.AreEqual(1, primitive.X);
                Assert.AreEqual(1, primitive.Y);
                Assert.AreEqual(1, primitive.Z);
            }
        }

        [Test]
        public void VerifyThatPrimitivesCanBeRotated()
        {
            foreach (var primitive in this.positionables)
            {
                Assert.AreEqual(0, primitive.RX);
                Assert.AreEqual(0, primitive.RY);
                Assert.AreEqual(0, primitive.RZ);

                primitive.SetRotation(1, 1, 1);

                Assert.AreEqual(1, primitive.RX);
                Assert.AreEqual(1, primitive.RY);
                Assert.AreEqual(1, primitive.RZ);
            }
        }

        [Test]
        public void VerifyThatTransformationsCanBeReseted()
        {
            var cube = new Cube(1, 1, 1);
            Assert.AreEqual(0, cube.X);
            Assert.AreEqual(0, cube.Y);
            Assert.AreEqual(0, cube.Z);

            cube.SetTranslation(1, 2, 3);
            Assert.AreEqual(1, cube.X);
            Assert.AreEqual(2, cube.Y);
            Assert.AreEqual(3, cube.Z);

            cube.ResetTransformations();
            Assert.AreEqual(0, cube.X);
            Assert.AreEqual(0, cube.Y);
            Assert.AreEqual(0, cube.Z);
        }
    }
}
