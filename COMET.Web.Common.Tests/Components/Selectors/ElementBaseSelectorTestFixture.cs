// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementBaseSelectorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.Components.Selectors
{
    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementBaseSelectorTestFixture
    {
        private BunitContext context;
        private Mock<IElementBaseSelectorViewModel> viewModel;
        private List<ElementBase> availableElements;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.availableElements = new List<ElementBase>
            {
                new ElementDefinition { Iid = Guid.NewGuid(), Name = "Element A" },
                new ElementDefinition { Iid = Guid.NewGuid(), Name = "Element B" }
            };

            this.viewModel = new Mock<IElementBaseSelectorViewModel>();
            this.viewModel.Setup(x => x.AvailableElements).Returns(this.availableElements);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyElementBaseSelectorComponent()
        {
            var renderer = this.context.Render<ElementBaseSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            Assert.That(renderer.FindComponents<DxComboBox<ElementBase, ElementBase>>(), Is.Empty,
                "without a CurrentIteration the selector must not render.");

            this.viewModel.Setup(x => x.CurrentIteration).Returns(new Iteration { Iid = Guid.NewGuid() });
            renderer.Render();

            // The DxComboBox is asserted by presence only: DevExpress echoes neither Data nor ValueChanged back
            // onto its own instance under bunit.
            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("h6").TextContent, Is.EqualTo("Filter on Element:"));
                Assert.That(renderer.FindComponent<DxComboBox<ElementBase, ElementBase>>(), Is.Not.Null);
            });

            renderer.Render(parameters => parameters.Add(p => p.DisplayText, string.Empty));

            Assert.That(renderer.FindAll("h6"), Is.Empty);
        }
    }
}
