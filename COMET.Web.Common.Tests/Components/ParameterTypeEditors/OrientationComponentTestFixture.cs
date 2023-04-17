// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrientationComponentTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components.ParameterTypeEditors
{
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.Types;

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

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
