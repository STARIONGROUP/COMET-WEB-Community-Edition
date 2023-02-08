﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrientationComponentTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Tests.Components.Viewer.PropertiesPanel
{
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.Types;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class OrientationComponentTestFixture
    {
        private TestContext context;
        private OrientationComponent orientation;
        private IRenderedComponent<OrientationComponent> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            this.context.JSInterop.SetupVoid("DxBlazor.AdaptiveDropDown.init");
            this.context.JSInterop.SetupVoid("DxBlazor.ScrollViewer.loadModule");
            this.context.ConfigureDevExpressBlazor();

            var orientationViewModel = new Mock<IOrientationViewModel>();
            orientationViewModel.Setup(x => x.AngleFormats).Returns(new List<AngleFormat>() { AngleFormat.Degrees, AngleFormat.Radians });
            orientationViewModel.Setup(x => x.CurrentValueSet.ActualValue).Returns(new ValueArray<string>(new List<string>() { "0", "0", "0", }));
            orientationViewModel.Setup(x => x.Orientation).Returns(Orientation.Identity());

            this.renderedComponent = this.context.RenderComponent<OrientationComponent>(parameters =>
            {
                parameters.Add(p => p.ViewModel, orientationViewModel.Object);
            });

            this.orientation = this.renderedComponent.Instance;
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
                Assert.That(this.orientation, Is.Not.Null);
                Assert.That(this.orientation.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatEulerTabIsSelected()
        {
            var component = this.renderedComponent.Find(".euler-tab");
            component.Click();
            var eulerTab = this.renderedComponent.Find("#euler-orientation-tab");
            
            Assert.That(eulerTab, Is.Not.Null);
        }

        [Test]
        public void VerifyThatMatrixTabIsSelected()
        {
            var component = this.renderedComponent.Find(".matrix-tab");
            component.Click();
            var matrixTab = this.renderedComponent.Find("#matrix-orientation-tab");

            Assert.That(matrixTab, Is.Not.Null);
        }
    }
}