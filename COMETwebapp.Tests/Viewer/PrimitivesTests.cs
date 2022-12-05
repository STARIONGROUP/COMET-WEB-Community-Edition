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

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;

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
        private List<BasicPrimitive> positionables;
        private List<Primitive> primitives;
        private ElementDefinition elementDef;
        private ElementUsage elementUsage;
        private readonly Uri uri = new Uri("http://test.com");
        private DomainOfExpertise domain;
        private IShapeFactory shapeFactory;
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
            JSInterop.JsRuntime = this.context.JSInterop.JSRuntime;

            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);

            this.context.Services.AddTransient<ISceneProvider, SceneProvider>();
            this.context.Services.AddSingleton<IShapeFactory>(new ShapeFactory());

            var renderer = this.context.RenderComponent<BabylonCanvas>();

            this.shapeFactory = renderer.Instance.ShapeFactory;

            this.positionables = new List<BasicPrimitive>();
            this.positionables.Add(new Cube());
            this.positionables.Add(new Cylinder());
            this.positionables.Add(new Sphere());
            this.positionables.Add(new Torus());

            this.primitives = new List<Primitive>(this.positionables);
            this.primitives.Add(new Line(new System.Numerics.Vector3(), new System.Numerics.Vector3(1, 1, 1)));
            this.primitives.Add(new CustomPrimitive(string.Empty, string.Empty));

            this.option = new Option(Guid.NewGuid(), cache, this.uri);

            var shapeKindParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "box" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var shapeKindParameterType = new EnumerationParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Shape Kind", ShortName = SceneProvider.ShapeKindShortName, };
            var shapeKindParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = shapeKindParameterType };
            shapeKindParameter.ValueSet.Add(shapeKindParameterValueSet);

            var colorParameterValueSet = new ParameterValueSet(Guid.NewGuid(), cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "255:155:25" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            var colorParameterType = new TextParameterType(Guid.NewGuid(), cache, this.uri) { Name = "Color", ShortName = SceneProvider.ColorShortName, };
            var colorParameter = new Parameter(Guid.NewGuid(), cache, this.uri) { ParameterType = colorParameterType };
            colorParameter.ValueSet.Add(colorParameterValueSet);

            this.elementDef = new ElementDefinition(Guid.NewGuid(), cache, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), cache, this.uri) { ElementDefinition = this.elementDef, Owner = this.domain };
            this.elementDef.ContainedElement.Add(this.elementUsage);
            this.elementDef.Parameter.Add(shapeKindParameter);
            this.elementDef.Parameter.Add(colorParameter);
        }

        [Test]
        public void VerifyPrimitiveData()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            Assert.IsNotNull(basicShape);
            Assert.IsNotNull(basicShape.ElementUsage);
            Assert.IsNotNull(basicShape.SelectedOption);
            Assert.IsNotNull(basicShape.States);
            Assert.IsTrue(basicShape.Type != string.Empty);
        }

        [Test]
        public void VerifyThatPrimitiveCanBeCreatedByElementUsage()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            Assert.IsNotNull(basicShape);
            Assert.IsTrue(basicShape is BasicPrimitive);
        }

        [Test]
        public void VerifyThatPrimitivesHaveValidPropertyName()
        {
            foreach (var primitive in this.primitives)
            {
                Assert.AreEqual(primitive.GetType().Name, primitive.Type);
            }

            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            var parameters = basicShape.ElementUsage.GetParametersInUse();
            Assert.IsNotNull(parameters);
            Assert.IsTrue(parameters.Count() > 0);
            var parameter = parameters.FirstOrDefault(x => x.ParameterType.ShortName == SceneProvider.ShapeKindShortName);
            Assert.IsNotNull(parameter);
        }

        [Test]
        public void VerifyThatValueSetsCanBeRetrievedFromPrimitive()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            var valueSets = basicShape.GetValueSets();
            Assert.IsNotNull(valueSets);
            Assert.IsTrue(valueSets.Count > 0);
            foreach (var primitive in this.positionables)
            {
                Assert.AreEqual(0.0, primitive.X, delta);
                Assert.AreEqual(0.0, primitive.Y, delta);
                Assert.AreEqual(0.0, primitive.Z, delta);

                ValueArray<string> valueArray = new ValueArray<string>(new string[] { "1", "1", "1" });

                var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
                {
                    ValueSwitch = ParameterSwitchKind.MANUAL,
                    Manual = valueArray,
                };
                primitive.SetPosition(parameterValueSet);

                Assert.AreEqual(1.0, primitive.X, delta);
                Assert.AreEqual(1.0, primitive.Y, delta);
                Assert.AreEqual(1.0, primitive.Z, delta);
            }

        }

        [Test]
        public void VerifyThatCanGetValueSets()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            var valueSets = basicShape.GetValueSets();
            Assert.IsNotNull(valueSets);
        }

        [Test]
        public void VerifyThatColorCanBeSetFromElementUsage()
        {
            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            basicShape.SetColor();
            Assert.AreNotEqual(Primitive.DefaultColor, basicShape.Color);
            Assert.AreEqual(new Vector3(255, 155, 25), basicShape.Color);
        }

        [Test]
        public void VerifyThatColorCanBeUpdated()
        {

            var basicShape = this.shapeFactory.CreatePrimitiveFromElementUsage(this.elementUsage, this.option, new List<ActualFiniteState>());
            Mock<IValueSet> valueSet = new Mock<IValueSet>();
            ValueArray<string> newValueArray = new ValueArray<string>(new List<string>() { "#CCCDDD" });
            valueSet.Setup(x => x.ActualValue).Returns(newValueArray);

            var colorBefore = basicShape.Color;

            basicShape.UpdatePropertyWithParameterData(SceneProvider.ColorShortName, valueSet.Object);

            Assert.AreNotEqual(colorBefore, basicShape.Color);
        }

        [Test]
        public void VerifyThatTransformationsCanBeReseted()
        {
            var cube = new Cube();
            Assert.AreEqual(0.0, cube.X, delta);
            Assert.AreEqual(0.0, cube.Y, delta);
            Assert.AreEqual(0.0, cube.Z, delta);

            ValueArray<string> valueArray = new ValueArray<string>(new string[] { "1", "2", "3" });

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = valueArray,
            };
            cube.SetPosition(parameterValueSet);

            Assert.AreEqual(1.0, cube.X, delta);
            Assert.AreEqual(2.0, cube.Y, delta);
            Assert.AreEqual(3.0, cube.Z, delta);

            valueArray = new ValueArray<string>(new string[] { "0", "0", "0" });

            parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = valueArray,
            };
            cube.SetPosition(parameterValueSet);

            Assert.AreEqual(0.0, cube.X, delta);
            Assert.AreEqual(0.0, cube.Y, delta);
            Assert.AreEqual(0.0, cube.Z, delta);
        }
    }
}
