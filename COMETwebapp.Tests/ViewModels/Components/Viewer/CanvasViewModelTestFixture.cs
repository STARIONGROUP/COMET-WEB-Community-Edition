// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="CanvasViewModelTestFixture.cs" company="RHEA System S.A."> 
//    Copyright (c) 2023-2024 RHEA System S.A. 
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar 
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

namespace COMETwebapp.Tests.ViewModels.Components.Viewer
{
    using System.Threading.Tasks;

    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CanvasViewModelTestFixture
    {
        private ICanvasViewModel viewModel;

        private Mock<IBabylonInterop> babylonInterop;
        private Mock<ISelectionMediator> selectionMediator;

        [SetUp]
        public void SetUp()
        {
            this.babylonInterop = new Mock<IBabylonInterop>();
            this.selectionMediator = new Mock<ISelectionMediator>();

            this.viewModel = new CanvasViewModel(this.babylonInterop.Object, this.selectionMediator.Object);
        }

        [Test]
        public async Task VerifyThatSceneObjectsCanBeAdded()
        {
            var sceneObject1 = new SceneObject(new Cube(1, 1, 1));
            var sceneObject2 = new SceneObject(new Sphere(1));

            await this.viewModel.AddSceneObject(sceneObject1);
            await this.viewModel.AddTemporarySceneObject(sceneObject2);

            var sceneObjects = this.viewModel.GetAllSceneObjects();
            var tempSceneObjects = this.viewModel.GetAllTemporarySceneObjects();

            Assert.Multiple(() =>
            {
                Assert.That(sceneObjects, Has.Count.EqualTo(1));
                Assert.That(tempSceneObjects, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyThatSceneCanBeCleared()
        {
            var sceneObject1 = new SceneObject(new Cube(1, 1, 1));
            var sceneObject2 = new SceneObject(new Sphere(1));

            await this.viewModel.AddSceneObject(sceneObject1);
            await this.viewModel.AddTemporarySceneObject(sceneObject2);

            var sceneObjects = this.viewModel.GetAllSceneObjects();
            var tempSceneObjects = this.viewModel.GetAllTemporarySceneObjects();

            Assert.Multiple(() =>
            {
                Assert.That(sceneObjects, Has.Count.EqualTo(1));
                Assert.That(tempSceneObjects, Has.Count.EqualTo(1));
            });

            sceneObjects = this.viewModel.GetAllSceneObjects();
            tempSceneObjects = this.viewModel.GetAllTemporarySceneObjects();

            await this.viewModel.ClearScene();

            Assert.Multiple(() =>
            {
                Assert.That(sceneObjects, Has.Count.EqualTo(0));
                Assert.That(tempSceneObjects, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public async Task VerifyThatSceneObjectCanBeRetrievedById()
        {
            var sceneObject1 = new SceneObject(new Cube(1, 1, 1));

            await this.viewModel.AddSceneObject(sceneObject1);

            var result = this.viewModel.GetSceneObjectById(sceneObject1.ID);

            Assert.That(result, Is.Not.Null);
        }
    }
}
