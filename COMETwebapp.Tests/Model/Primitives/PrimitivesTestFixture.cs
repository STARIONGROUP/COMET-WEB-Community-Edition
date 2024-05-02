// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitivesTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Model.Primitives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
   
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Model.Viewer.Primitives;

    using NUnit.Framework;

    [TestFixture]
    public class PrimitivesTestFixture
    {
        private readonly Uri uri = new("http://test.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
        }

        [Test]
        public void VerifyThatParseParameterWorksOnCone()
        {
            var cone = new Cone(0, 0, 0, 1, 1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            var heightParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var heightParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Height", ShortName = SceneSettings.HeightShortName, };
            var heightParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = heightParameterType };
            heightParameter.ValueSet.Add(heightParameterValueSet);

            cone.ParseParameter(radiusParameter, radiusParameterValueSet);
            cone.ParseParameter(heightParameter, heightParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(cone.Radius, Is.Not.EqualTo(1));
                Assert.That(cone.Height, Is.Not.EqualTo(1));
            });
        }

        [Test]
        public void VerifyThatParseParameterWorksOnCube()
        {
            var cube = new Cube(0, 0, 0, 1, 1, 1);

            var widthParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var widthParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Width", ShortName = SceneSettings.WidthShortName, };
            var widthParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = widthParameterType };
            widthParameter.ValueSet.Add(widthParameterValueSet);

            var heightParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var heightParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Height", ShortName = SceneSettings.HeightShortName, };
            var heightParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = heightParameterType };
            heightParameter.ValueSet.Add(heightParameterValueSet);

            var lengthParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var lengthParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Length", ShortName = SceneSettings.LengthShortName, };
            var lengthParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = lengthParameterType };
            lengthParameter.ValueSet.Add(lengthParameterValueSet);

            cube.ParseParameter(widthParameter, widthParameterValueSet);
            cube.ParseParameter(heightParameter, heightParameterValueSet);
            cube.ParseParameter(lengthParameter, lengthParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(cube.Width, Is.Not.EqualTo(1));
                Assert.That(cube.Height, Is.Not.EqualTo(1));
                Assert.That(cube.Depth, Is.Not.EqualTo(1));
            });
        }

        [Test]
        public void VerifyThatParseParameterWorksOnCylinder()
        {
            var cylinder = new Cylinder(0, 0, 0, 1, 1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            var heightParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var heightParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Height", ShortName = SceneSettings.HeightShortName, };
            var heightParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = heightParameterType };
            heightParameter.ValueSet.Add(heightParameterValueSet);

            cylinder.ParseParameter(radiusParameter, radiusParameterValueSet);
            cylinder.ParseParameter(heightParameter, heightParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(cylinder.Radius, Is.Not.EqualTo(1));
                Assert.That(cylinder.Height, Is.Not.EqualTo(1));
            });
        }

        [Test]
        public void VerifyThatParseParameterWorksOnDisc()
        {
            var disc = new Disc(1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            disc.ParseParameter(radiusParameter, radiusParameterValueSet);

            Assert.That(disc.Radius, Is.Not.EqualTo(1));
        }

        [Test]
        public void VerifyThatParseParameterWorksOnEquilateralTriangle()
        {
            var triangle = new EquilateralTriangle(1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            triangle.ParseParameter(radiusParameter, radiusParameterValueSet);

            Assert.That(triangle.Radius, Is.Not.EqualTo(1));
        }

        [Test]
        public void VerifyThatParseParameterWorksOnHexagonalPrism()
        {
            var hexagonalPrism = new HexagonalPrism(1, 1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            var heightParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var heightParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Height", ShortName = SceneSettings.HeightShortName, };
            var heightParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = heightParameterType };
            heightParameter.ValueSet.Add(heightParameterValueSet);

            hexagonalPrism.ParseParameter(radiusParameter, radiusParameterValueSet);
            hexagonalPrism.ParseParameter(heightParameter, heightParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(hexagonalPrism.Radius, Is.Not.EqualTo(1));
                Assert.That(hexagonalPrism.Height, Is.Not.EqualTo(1));
            });
        }

        [Test]
        public void VerifyThatParseParameterWorksOnRectangle()
        {
            var rectangle = new Rectangle(1, 1);

            var widthParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var widthParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Width", ShortName = SceneSettings.WidthShortName, };
            var widthParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = widthParameterType };
            widthParameter.ValueSet.Add(widthParameterValueSet);

            var heightParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var heightParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Height", ShortName = SceneSettings.HeightShortName, };
            var heightParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = heightParameterType };
            heightParameter.ValueSet.Add(heightParameterValueSet);

            rectangle.ParseParameter(widthParameter, widthParameterValueSet);
            rectangle.ParseParameter(heightParameter, heightParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(rectangle.Width, Is.Not.EqualTo(1));
                Assert.That(rectangle.Height, Is.Not.EqualTo(1));
            });
        }

        [Test]
        public void VerifyThatParseParameterWorksOnSphere()
        {
            var hexagonalPrism = new Sphere(1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            hexagonalPrism.ParseParameter(radiusParameter, radiusParameterValueSet);

            Assert.That(hexagonalPrism.Radius, Is.Not.EqualTo(1));
        }

        [Test]
        public void VerifyThatParseParameterWorksOnTorus()
        {
            var torus = new Torus(1, 1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            var thicknessParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var thicknessParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Thickness", ShortName = SceneSettings.ThicknessShortName, };
            var thicknessParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = thicknessParameterType };
            thicknessParameter.ValueSet.Add(thicknessParameterValueSet);

            torus.ParseParameter(radiusParameter, radiusParameterValueSet);
            torus.ParseParameter(thicknessParameter, thicknessParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(torus.Diameter, Is.Not.EqualTo(1));
                Assert.That(torus.Thickness, Is.Not.EqualTo(1));
            });
        }

        [Test]
        public void VerifyThatParseParameterWorksOnTriangularPrism()
        {
            var triangularPrism = new TriangularPrism(1, 1);

            var radiusParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var radiusParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Radius", ShortName = SceneSettings.DiameterShortName, };
            var radiusParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = radiusParameterType };
            radiusParameter.ValueSet.Add(radiusParameterValueSet);

            var heightParameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "3.15" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            
            var heightParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Height", ShortName = SceneSettings.HeightShortName, };
            var heightParameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = heightParameterType };
            heightParameter.ValueSet.Add(heightParameterValueSet);

            triangularPrism.ParseParameter(radiusParameter, radiusParameterValueSet);
            triangularPrism.ParseParameter(heightParameter, heightParameterValueSet);

            Assert.Multiple(() =>
            {
                Assert.That(triangularPrism.Radius, Is.Not.EqualTo(1));
                Assert.That(triangularPrism.Height, Is.Not.EqualTo(1));
            });
        }
    }


}
