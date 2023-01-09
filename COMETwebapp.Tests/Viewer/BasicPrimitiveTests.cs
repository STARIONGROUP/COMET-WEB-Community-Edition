// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicPrimitiveTests.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;


    /// <summary>
    /// Basic Primitives tests that verifies the correct behavior of JSInterop
    /// </summary>
    [TestFixture]
    public class BasicPrimitiveTests
    {
        private TestContext context;
        private List<Primitive> positionables;
        private ElementDefinition elementDef;
        private ElementUsage elementUsage;
        private readonly Uri uri = new Uri("http://test.com");
        private DomainOfExpertise domain;
        private IShapeFactory shapeFactory;
        private Option option;

        [SetUp]
        public void SetUp()
        {
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.domain = new DomainOfExpertise(Guid.NewGuid(), cache, this.uri) { Name = "domain" };

            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);

            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddSingleton<IShapeFactory>(new ShapeFactory());
            this.context.Services.AddTransient<IJSInterop, JSInterop>();

            var renderer = this.context.RenderComponent<BabylonCanvas>();

            this.shapeFactory = renderer.Instance.ShapeFactory;

            this.positionables = new List<Primitive>();
            this.positionables.Add(new Cube(1, 1, 1));
            this.positionables.Add(new Cylinder(1, 1));
            this.positionables.Add(new Sphere(1));
            this.positionables.Add(new Torus(2, 1));

            this.option = new Option(Guid.NewGuid(), cache, this.uri);

            var shapeKindParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "box" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var shapeKindParameterType = new EnumerationParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Shape Kind", ShortName = SceneSettings.ShapeKindShortName, };
            var shapeKindParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = shapeKindParameterType };
            shapeKindParameter.ValueSet.Add(shapeKindParameterValueSet);

            var positionParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "1", "1", "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var positionParameterType = new CompoundParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Coordinates", ShortName = SceneSettings.PositionShortName, };
            var positionParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = positionParameterType };
            positionParameter.ValueSet.Add(positionParameterValueSet);

            var orientationParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "0.5", "0", "0.8660254", "0", "1", "0", "-0.8660254", "0", "0.5" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var orientationParameterType = new ArrayParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Orientation", ShortName = SceneSettings.OrientationShortName, };
            var orientationParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = orientationParameterType };
            orientationParameter.ValueSet.Add(orientationParameterValueSet);


            this.elementDef = new ElementDefinition(Guid.NewGuid(), cache, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), cache, this.uri) { ElementDefinition = this.elementDef, Owner = this.domain };
            this.elementDef.ContainedElement.Add(this.elementUsage);
            this.elementDef.Parameter.Add(shapeKindParameter);
            this.elementDef.Parameter.Add(positionParameter);
            this.elementDef.Parameter.Add(orientationParameter);
        }

        [Test]
        public void VerifyThatPrimitiveCanBePositionedByElementUsage()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            Assert.IsNotNull(basicShape);

            Assert.AreNotEqual(0.0, basicShape.X);
            Assert.AreNotEqual(0.0, basicShape.Y);
            Assert.AreNotEqual(0.0, basicShape.Z);
        }

        [Test]
        public void VerifyThatPrimitiveCanBeRotatedByElementUsage()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            Assert.IsNotNull(basicShape);

            Assert.AreEqual(0.0, basicShape.RX);
            Assert.AreNotEqual(0.0, basicShape.RY);
            Assert.AreEqual(0.0, basicShape.RZ);
        }

        [Test]
        public void VerifyThatPrimitivesCanBeTranslated()
        {
            foreach (var primitive in this.positionables)
            {
                Assert.AreEqual(0.0, primitive.X);
                Assert.AreEqual(0.0, primitive.Y);
                Assert.AreEqual(0.0, primitive.Z);

                primitive.X = 1;
                primitive.Y = 1;
                primitive.Z = 1;

                Assert.AreEqual(1.0, primitive.X);
                Assert.AreEqual(1.0, primitive.Y);
                Assert.AreEqual(1.0, primitive.Z);
            }
        }

        [Test]
        public void VerifyThatPrimitivesCanBeRotated()
        {
            foreach (var primitive in this.positionables)
            {
                Assert.AreEqual(0.0, primitive.RX);
                Assert.AreEqual(0.0, primitive.RY);
                Assert.AreEqual(0.0, primitive.RZ);

                primitive.RX = 1;
                primitive.RY = 1;
                primitive.RZ = 1;

                Assert.AreEqual(1.0, primitive.RX);
                Assert.AreEqual(1.0, primitive.RY);
                Assert.AreEqual(1.0, primitive.RZ);
            }
        }

        [Test]
        public void VerifyThatTransformationsCanBeReseted()
        {
            var cube = new Cube(1, 1, 1);
            Assert.AreEqual(0.0, cube.X);
            Assert.AreEqual(0.0, cube.Y);
            Assert.AreEqual(0.0, cube.Z);

            cube.X = 1;
            cube.Y = 1;
            cube.Z = 1;

            Assert.AreEqual(1.0, cube.X);
            Assert.AreEqual(2.0, cube.Y);
            Assert.AreEqual(3.0, cube.Z);

            cube.ResetTransformations();
            Assert.AreEqual(0.0, cube.X);
            Assert.AreEqual(0.0, cube.Y);
            Assert.AreEqual(0.0, cube.Z);
        }
    }
}
