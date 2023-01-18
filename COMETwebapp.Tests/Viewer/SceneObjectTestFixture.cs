// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SceneObjectTestFixture.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Numerics;

    using Bunit;
    
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
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
    /// Primitives tests that verifies the correct behavior of JSInterop
    /// </summary>
    [TestFixture]
    public class SceneObjectTestFixture
    {
        private TestContext context;
        private List<Primitive> positionables;
        private List<Primitive> primitives;
        private ElementDefinition elementDef;
        private ElementUsage elementUsage;
        private readonly Uri uri = new Uri("http://test.com");
        private DomainOfExpertise domain;
        private Option option;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private double delta = 0.001;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.domain = new DomainOfExpertise(Guid.NewGuid(), cache, this.uri) { Name = "domain" };

            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);

            this.context.Services.AddTransient<ISceneSettings, SceneSettings>();
            this.context.Services.AddTransient<IJSInterop, JSInterop>();

            this.positionables = new List<Primitive>();
            this.positionables.Add(new Cube(1, 1, 1));
            this.positionables.Add(new Cylinder(1, 1));
            this.positionables.Add(new Sphere(1));
            this.positionables.Add(new Torus(2, 1));

            this.primitives = new List<Primitive>(this.positionables);
            this.primitives.Add(new Line(new System.Numerics.Vector3(), new System.Numerics.Vector3(1, 1, 1)));
            this.primitives.Add(new CustomPrimitive(string.Empty, string.Empty));

            this.option = new Option(Guid.NewGuid(), cache, this.uri);

            var shapeKindParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "box" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var shapeKindParameterType = new EnumerationParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Shape Kind", ShortName = SceneSettings.ShapeKindShortName, };
            var shapeKindParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = shapeKindParameterType };
            shapeKindParameter.ValueSet.Add(shapeKindParameterValueSet);

            var colorParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "255:155:25" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var colorParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Color", ShortName = SceneSettings.ColorShortName, };
            var colorParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = colorParameterType };
            colorParameter.ValueSet.Add(colorParameterValueSet);

            var positionParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "0", "0", "0" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var positionParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Position", ShortName = SceneSettings.PositionShortName, };
            var positionParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = positionParameterType };
            positionParameter.ValueSet.Add(positionParameterValueSet);

            var orientationParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "0", "0", "0" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var orientationParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Orientation", ShortName = SceneSettings.OrientationShortName, };
            var orientationParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = orientationParameterType };
            orientationParameter.ValueSet.Add(orientationParameterValueSet);

            this.elementDef = new ElementDefinition(Guid.NewGuid(), cache, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), cache, this.uri) { ElementDefinition = this.elementDef, Owner = this.domain };
            this.elementDef.ContainedElement.Add(this.elementUsage);
            this.elementDef.Parameter.Add(shapeKindParameter);
            this.elementDef.Parameter.Add(colorParameter);
            this.elementDef.Parameter.Add(positionParameter);
            this.elementDef.Parameter.Add(orientationParameter);
        }

        [Test]
        public void VerifySceneObjectData()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());

            Assert.Multiple(() =>
            {
                Assert.That(sceneObject, Is.Not.Null);
                Assert.That(sceneObject.ElementUsage, Is.Not.Null);
                Assert.That(sceneObject.Option, Is.Not.Null);
                Assert.That(sceneObject.States, Is.Not.Null);
                Assert.That(sceneObject.Primitive, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatSceneObjectCanBeCreatedByElementUsage()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            Assert.That(sceneObject, Is.Not.Null);
        }

        [Test]
        public void VerifyThatPrimitivesHaveValidPropertyName()
        {
            foreach (var primitive in this.primitives)
            {
                Assert.AreEqual(primitive.GetType().Name, primitive.Type);
            }

            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var parameters = sceneObject.ParametersAsociated;
            Assert.IsNotNull(parameters);
            Assert.IsTrue(parameters.Count() > 0);
            var parameter = parameters.FirstOrDefault(x => x.ParameterType.ShortName == SceneSettings.ShapeKindShortName);
            Assert.IsNotNull(parameter);
        }

        [Test]
        public void VerifyThatValueSetsCanBeRetrievedFromSceneObjects()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var valueSets = sceneObject.GetValueSets();

            Assert.IsNotNull(valueSets);
            Assert.IsTrue(valueSets.Count > 0);
            foreach (var primitive in this.positionables)
            {
                Assert.AreEqual(0.0, primitive.X, delta);
                Assert.AreEqual(0.0, primitive.Y, delta);
                Assert.AreEqual(0.0, primitive.Z, delta);

                primitive.X = 1.0;
                primitive.Y = 1.0;
                primitive.Z = 1.0;

                Assert.AreEqual(1.0, primitive.X, delta);
                Assert.AreEqual(1.0, primitive.Y, delta);
                Assert.AreEqual(1.0, primitive.Z, delta);
            }

        }

        [Test]
        public void VerifyThatCanGetValueSets()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var valueSets = sceneObject.GetValueSets();
            Assert.IsNotNull(valueSets);
        }

        [Test]
        public void VerifyThatColorCanBeSetFromElementUsage()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(Primitive.DefaultColor, sceneObject.Primitive.Color);
                Assert.AreEqual(new Vector3(255, 155, 25), sceneObject.Primitive.Color);
            });         
        }

        [Test]
        public void VerifyThatColorCanBeUpdated()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var basicShape = sceneObject.Primitive;

            var colorParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "#CCCDDD" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var colorParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Color", ShortName = SceneSettings.ColorShortName, };
            var colorParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = colorParameterType };
            colorParameter.ValueSet.Add(colorParameterValueSet);

            var colorBefore = basicShape.Color;

            basicShape.ParseParameter(colorParameter, colorParameterValueSet);

            Assert.AreNotEqual(colorBefore, basicShape.Color);

            basicShape.SetColor(0, 0, 0);
            Assert.That(basicShape.Color, Is.EqualTo(Vector3.Zero));
        }

        [Test]
        public void VerifyThatPositionCanBeUpdated()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var basicShape = sceneObject.Primitive;

            var positionParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "1", "1", "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var positionParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Position", ShortName = SceneSettings.PositionShortName, };
            var positionParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = positionParameterType };
            positionParameter.ValueSet.Add(positionParameterValueSet);

            var x = basicShape.X;
            var y = basicShape.Y;
            var z = basicShape.Z;

            basicShape.ParseParameter(positionParameter, positionParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(x, Is.Not.EqualTo(basicShape.X));
                Assert.That(y, Is.Not.EqualTo(basicShape.Y));
                Assert.That(z, Is.Not.EqualTo(basicShape.Z));
            });
        }

        [Test]
        public void VerifyThatOrientationCanBeUpdated()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var basicShape = sceneObject.Primitive;

            var orientationParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "1", "1", "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var orientationParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Orientation", ShortName = SceneSettings.OrientationShortName, };
            var orientationParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = orientationParameterType };
            orientationParameter.ValueSet.Add(orientationParameterValueSet);

            var rx = basicShape.RX;
            var ry = basicShape.RY;
            var rz = basicShape.RZ;

            basicShape.ParseParameter(orientationParameter, orientationParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(rx, Is.Not.EqualTo(basicShape.RX));
                Assert.That(ry, Is.Not.EqualTo(basicShape.RY));
                Assert.That(rz, Is.Not.EqualTo(basicShape.RZ));
            });
        }

        [Test]
        public void VerifyThatTransformationsCanBeReseted()
        {
            var cube = new Cube(1.0, 1.0, 1.0);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0.0, cube.X, delta);
                Assert.AreEqual(0.0, cube.Y, delta);
                Assert.AreEqual(0.0, cube.Z, delta);
            });

            cube.X = 1.0;
            cube.Y = 2.0;
            cube.Z = 3.0;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1.0, cube.X, delta);
                Assert.AreEqual(2.0, cube.Y, delta);
                Assert.AreEqual(3.0, cube.Z, delta);
            });

            cube.ResetTransformations();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0.0, cube.X, delta);
                Assert.AreEqual(0.0, cube.Y, delta);
                Assert.AreEqual(0.0, cube.Z, delta);
            });
        }

        [Test]
        public void VerifyThatSceneObjectCanBeCloned()
        {
            var sceneObject = SceneObject.Create(this.elementUsage, this.option, new List<ActualFiniteState>());
            var newSceneObject = sceneObject.Clone();

            Assert.That(sceneObject, Is.Not.EqualTo(newSceneObject));
        }
    }
}
