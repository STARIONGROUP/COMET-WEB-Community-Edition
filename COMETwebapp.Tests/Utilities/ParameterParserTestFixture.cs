// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterParseTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Tests.Utilities
{
    using System.Collections.Concurrent;
    using System.Numerics;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Utilities;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Utilities;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterParserTestFixture
    {
        private readonly Uri uri = new("http://test.com");
        private Option option;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
        }

        [Test]
        public void VerifyThatColorParserWorks()
        {
            var colorParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "255:155:25" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var colorParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Color", ShortName = ConstantValues.ColorShortName };
            var colorParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = colorParameterType };
            colorParameter.ValueSet.Add(colorParameterValueSet);

            var color = ParameterParser.ColorParser(colorParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ParameterParser.ColorParser(null, this.option, new List<ActualFiniteState>()));
                Assert.That(color, Is.EqualTo(new Vector3(255, 155, 25)));
            });
        }

        [Test]
        public void VerifyThatDoubleParserWorks()
        {
            var doubleParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var doubleParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Width", ShortName = SceneSettings.WidthShortName };
            var doubleParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = doubleParameterType };
            doubleParameter.ValueSet.Add(doubleParameterValueSet);

            var width = ParameterParser.DoubleParser(doubleParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ParameterParser.DoubleParser(null, this.option, new List<ActualFiniteState>()));
                Assert.That(width, Is.EqualTo(3.15));
            });
        }

        [Test]
        public void VerifyThatOrientationParserWorks()
        {
            var orientationParameterValueSet1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "0", "0", "0" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var orientationParameterValueSet2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "1", "0", "0", "0", "1", "0", "0", "0", "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var orientationParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Orientation", ShortName = ConstantValues.OrientationShortName };
            var orientationParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = orientationParameterType };
            orientationParameter.ValueSet.Add(orientationParameterValueSet1);

            var orientation1 = ParameterParser.OrientationParser(orientationParameterValueSet1);
            orientationParameter.ValueSet.Clear();

            orientationParameter.ValueSet.Add(orientationParameterValueSet2);
            var orientation2 = ParameterParser.OrientationParser(orientationParameterValueSet2);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ParameterParser.OrientationParser(null, this.option, new List<ActualFiniteState>()));
                Assert.That(orientation1.X, Is.EqualTo(0));
                Assert.That(orientation1.Y, Is.EqualTo(0));
                Assert.That(orientation1.Z, Is.EqualTo(0));

                Assert.That(orientation2.X, Is.EqualTo(0));
                Assert.That(orientation2.Y, Is.EqualTo(0));
                Assert.That(orientation2.Z, Is.EqualTo(0));
            });
        }

        [Test]
        public void VerifyThatPositionParserWorks()
        {
            var positionParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "0", "0", "0" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var positionParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Position", ShortName = ConstantValues.PositionShortName };
            var positionParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = positionParameterType };
            positionParameter.ValueSet.Add(positionParameterValueSet);

            var position = ParameterParser.PositionParser(positionParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ParameterParser.PositionParser(null, this.option, new List<ActualFiniteState>()));
                Assert.That(position, Is.EqualTo(Vector3.Zero));
            });
        }

        [Test]
        public void VerifyThatShapeParserWorks()
        {
            var shapeKindParameterType = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Shape Kind", ShortName = SceneSettings.ShapeKindShortName };
            var shapeKindParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = shapeKindParameterType };

            var boxParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "box" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var coneParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "cone" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var cylinderParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "cylinder" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var sphereParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "sphere" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var torusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "torus" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var triprismParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "triprism" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var discParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "disc" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var hexagonalPrismParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "hexagonalprism" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var rectangleParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "rectangle" }), ValueSwitch = ParameterSwitchKind.MANUAL };
            var triangleParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { Manual = new ValueArray<string>(new List<string> { "triangle" }), ValueSwitch = ParameterSwitchKind.MANUAL };

            shapeKindParameter.ValueSet.Add(boxParameterValueSet);
            var box = ParameterParser.ShapeKindParser(boxParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(coneParameterValueSet);
            var cone = ParameterParser.ShapeKindParser(coneParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(cylinderParameterValueSet);
            var cylinder = ParameterParser.ShapeKindParser(cylinderParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(sphereParameterValueSet);
            var sphere = ParameterParser.ShapeKindParser(sphereParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(torusParameterValueSet);
            var torus = ParameterParser.ShapeKindParser(torusParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(triprismParameterValueSet);
            var trisprism = ParameterParser.ShapeKindParser(triprismParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(discParameterValueSet);
            var disc = ParameterParser.ShapeKindParser(discParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(hexagonalPrismParameterValueSet);
            var hexagonalPrism = ParameterParser.ShapeKindParser(hexagonalPrismParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(rectangleParameterValueSet);
            var rectangle = ParameterParser.ShapeKindParser(rectangleParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            shapeKindParameter.ValueSet.Add(triangleParameterValueSet);
            var triangle = ParameterParser.ShapeKindParser(triangleParameterValueSet);
            shapeKindParameter.ValueSet.Clear();

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ParameterParser.ShapeKindParser(null, this.option, new List<ActualFiniteState>()));
                Assert.That(box, Is.Not.Null);
                Assert.That(cone, Is.Not.Null);
                Assert.That(cylinder, Is.Not.Null);
                Assert.That(sphere, Is.Not.Null);
                Assert.That(torus, Is.Not.Null);
                Assert.That(trisprism, Is.Not.Null);
                Assert.That(disc, Is.Not.Null);
                Assert.That(hexagonalPrism, Is.Not.Null);
                Assert.That(rectangle, Is.Not.Null);
                Assert.That(triangle, Is.Not.Null);
            });
        }
    }
}
