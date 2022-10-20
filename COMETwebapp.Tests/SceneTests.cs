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
namespace COMETwebapp.Tests.IntegrationTests
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

        [SetUp]        
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.JsRuntime = this.context.JSInterop.JSRuntime;          
            
            var session = new Mock<ISessionAnchor>();
            this.context.Services.AddSingleton(session.Object);

            var renderer = this.context.RenderComponent<BabylonCanvas>();          
        }

        [Test]
        public void VerifyThatPrimitivesAreAddedToScene()
        {
            Cube cube = new Cube(0, 0, 0, 1, 1, 1);
            Scene.AddPrimitive(cube);
            var prim = Scene.GetPrimitiveById(cube.ID);
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
            Scene.ClearPrimitives();
            Scene.AddPrimitive(cube1);
            Scene.AddPrimitive(cube2);
            Assert.AreEqual(2, Scene.GetPrimitives().Count);

            Scene.ClearPrimitives();
            Assert.AreEqual(0, Scene.GetPrimitives().Count);
        }
    }
}
