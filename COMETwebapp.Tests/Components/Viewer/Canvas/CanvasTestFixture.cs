// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.Viewer.Canvas
{
    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Microsoft.Extensions.DependencyInjection;
    
    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CanvasTestFixture
    {
        private TestContext context;
        private Canvas3D canvas;
        private ICanvasViewModel viewModel;

        private Mock<IBabylonInterop> babylonInterop;
        private Mock<ISelectionMediator> selectionMediator;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            this.context.ConfigureDevExpressBlazor();

            this.babylonInterop = new Mock<IBabylonInterop>();
            this.selectionMediator = new Mock<ISelectionMediator>();

            this.viewModel = new CanvasViewModel(this.babylonInterop.Object, this.selectionMediator.Object);
            this.context.Services.AddSingleton(this.viewModel);

            var rendererComponent = this.context.RenderComponent<Canvas3D>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel);
            });

            this.canvas = rendererComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.canvas.ViewModel, Is.Not.Null);
                Assert.That(this.canvas.IsMouseDown, Is.False);
                Assert.That(this.canvas.IsMovingScene, Is.False);
            });
        }

        [Test]
        public void VerifyThatMouseEventsWorks()
        {
            Assert.That(this.canvas.IsMouseDown, Is.False);
            this.canvas.OnMouseDown(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(this.canvas.IsMouseDown, Is.True);
            this.canvas.OnMouseMove(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(this.canvas.IsMouseDown, Is.EqualTo(this.canvas.IsMovingScene));
            this.canvas.OnMouseUp(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            Assert.That(this.canvas.IsMouseDown, Is.False);
        }
    }
}
