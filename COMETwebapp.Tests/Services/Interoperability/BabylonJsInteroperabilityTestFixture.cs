// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BabylonJsInteroperabilityTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Services.Interoperability
{
    using System;

    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BabylonJsInteroperabilityTestFixture
    {
        private ElementReference canvasReference;
        private Mock<IBabylonInterop> babylonInteropMock;

        [SetUp]
        public void SetUp()
        {
            this.canvasReference = new ElementReference();
            this.babylonInteropMock = new Mock<IBabylonInterop>();
            this.babylonInteropMock.Setup(x => x.InitCanvas(this.canvasReference, It.IsAny<bool>()));
            this.babylonInteropMock.Setup(x => x.AddSceneObject(null)).Throws<ArgumentNullException>();
            this.babylonInteropMock.Setup(x => x.ClearSceneObject(null)).Throws<ArgumentNullException>();
            this.babylonInteropMock.Setup(x => x.ClearSceneObjects(null)).Throws<ArgumentNullException>();
            this.babylonInteropMock.Setup(x => x.SetVisibility(null, It.IsAny<bool>())).Throws<ArgumentNullException>();
        }

        [Test]
        public void VerifyInitCanvas()
        {
            Assert.DoesNotThrow(() => this.babylonInteropMock.Object.InitCanvas(this.canvasReference, true));
            this.babylonInteropMock.Verify(x => x.InitCanvas(this.canvasReference, It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void VerifyAddSceneObject()
        {
            Assert.Throws<ArgumentNullException>(()=> this.babylonInteropMock.Object.AddSceneObject(null));
            Assert.DoesNotThrow(() => this.babylonInteropMock.Object.AddSceneObject(new SceneObject(new Cube(1, 1, 1))));
            this.babylonInteropMock.Verify(x => x.AddSceneObject(It.IsAny<SceneObject>()), Times.AtLeast(2));
        }

        [Test]
        public void VerifyClearSceneObjects()
        {
            Assert.Throws<ArgumentNullException>(() => this.babylonInteropMock.Object.ClearSceneObject(null));
            Assert.DoesNotThrow(() => this.babylonInteropMock.Object.ClearSceneObject(new SceneObject(new Cube(1, 1, 1))));
            this.babylonInteropMock.Verify(x => x.ClearSceneObject(It.IsAny<SceneObject>()), Times.AtLeast(2));
        }

        [Test]
        public void VerifySetVisibility()
        {
            Assert.Throws<ArgumentNullException>(() => this.babylonInteropMock.Object.SetVisibility(null,true));
            Assert.DoesNotThrow(() => this.babylonInteropMock.Object.SetVisibility(new SceneObject(new Cube(1,1,1)), true));
            this.babylonInteropMock.Verify(x => x.SetVisibility(It.IsAny<SceneObject>(), It.IsAny<bool>()), Times.AtLeast(2));
        }

        [Test]
        public void VerifyGetPrimitiveIdUnderMouseAsync()
        {
            Assert.DoesNotThrow(() => this.babylonInteropMock.Object.GetPrimitiveIdUnderMouseAsync());
            this.babylonInteropMock.Verify(x => x.GetPrimitiveIdUnderMouseAsync(), Times.Once());
        }
    }
}
