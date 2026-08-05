// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BabylonJsInteroperabilityTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.Services.Interoperability
{
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BabylonJsInteroperabilityTestFixture
    {
        private Mock<IJSRuntime> jsRuntimeMock;
        private BabylonInterop babylonInterop;

        [SetUp]
        public void SetUp()
        {
            this.jsRuntimeMock = new Mock<IJSRuntime>();

            this.jsRuntimeMock
                .Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(Mock.Of<IJSVoidResult>());

            this.jsRuntimeMock
                .Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync((string)null);

            this.babylonInterop = new BabylonInterop(this.jsRuntimeMock.Object);
        }

        [Test]
        public async Task VerifyAddSceneObject()
        {
            Assert.That(async () => await this.babylonInterop.AddSceneObject(null), Throws.ArgumentNullException);

            await this.babylonInterop.AddSceneObject(new SceneObject(new Cube(1, 1, 1)));

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.AddSceneObjectFunction,
                    It.Is<object[]>(args => args.Length == 1)),
                Times.Once());
        }

        [Test]
        public async Task VerifyClearSceneObject()
        {
            Assert.That(async () => await this.babylonInterop.ClearSceneObject(null), Throws.ArgumentNullException);

            await this.babylonInterop.ClearSceneObject(new SceneObject(new Cube(1, 1, 1)));

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.DisposeAllFunction,
                    It.Is<object[]>(args => args.Length == 1)),
                Times.Once());
        }

        [Test]
        public async Task VerifyClearSceneObjects()
        {
            Assert.That(async () => await this.babylonInterop.ClearSceneObjects(null), Throws.ArgumentNullException);

            await this.babylonInterop.ClearSceneObjects(new List<SceneObject> { new(new Cube(1, 1, 1)) });

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.DisposeAllFunction,
                    It.Is<object[]>(args => args.Length == 1)),
                Times.Once());
        }

        [Test]
        public async Task VerifyGetPrimitiveIdUnderMouseAsync()
        {
            // Returns Guid.Empty when JS returns null
            var result = await this.babylonInterop.GetPrimitiveIdUnderMouseAsync();
            Assert.That(result, Is.EqualTo(Guid.Empty));

            // Returns Guid.Empty when JS returns an invalid string
            this.jsRuntimeMock
                .Setup(x => x.InvokeAsync<string>(BabylonInterop.GetPrimitiveIdUnderMouseFunction, It.IsAny<object[]>()))
                .ReturnsAsync("not-a-guid");

            result = await this.babylonInterop.GetPrimitiveIdUnderMouseAsync();
            Assert.That(result, Is.EqualTo(Guid.Empty));

            // Returns parsed Guid when JS returns a valid guid string
            var expected = Guid.NewGuid();

            this.jsRuntimeMock
                .Setup(x => x.InvokeAsync<string>(BabylonInterop.GetPrimitiveIdUnderMouseFunction, It.IsAny<object[]>()))
                .ReturnsAsync(expected.ToString());

            result = await this.babylonInterop.GetPrimitiveIdUnderMouseAsync();
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task VerifyInitCanvas()
        {
            var canvasReference = new ElementReference();
            await this.babylonInterop.InitCanvas(canvasReference, true);

            using var scope = Assert.EnterMultipleScope();

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.LoadBabylonScriptsFunction,
                    It.Is<object[]>(args => args.Length == 0)),
                Times.Once());

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.InitCanvasFunction,
                    It.Is<object[]>(args => args.Length == 2)),
                Times.Once());
        }

        [Test]
        public async Task VerifyRegenerateMesh()
        {
            await this.babylonInterop.RegenerateMesh(new SceneObject(new Cube(1, 1, 1)));

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.RegenMeshFunction,
                    It.Is<object[]>(args => args.Length == 1)),
                Times.Once());
        }

        [Test]
        public async Task VerifySetVisibility()
        {
            Assert.That(async () => await this.babylonInterop.SetVisibility(null, true), Throws.ArgumentNullException);

            var sceneObject = new SceneObject(new Cube(1, 1, 1));
            await this.babylonInterop.SetVisibility(sceneObject, true);

            this.jsRuntimeMock.Verify(
                x => x.InvokeAsync<IJSVoidResult>(
                    BabylonInterop.SetMeshVisibilityFunction,
                    It.Is<object[]>(args => args.Length == 2)),
                Times.Once());
        }
    }
}
