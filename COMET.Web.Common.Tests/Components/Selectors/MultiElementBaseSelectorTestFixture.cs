// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiElementBaseSelectorTestFixture.cs" company="Starion Group S.A.">
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

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MultiElementBaseSelectorTestFixture
    {
        private BunitContext context;
        private Mock<IMultiElementBaseSelectorViewModel> viewModel;
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

            this.viewModel = new Mock<IMultiElementBaseSelectorViewModel>();
            this.viewModel.Setup(x => x.AvailableElements).Returns(this.availableElements);
            this.viewModel.SetupProperty(x => x.SelectedElementBases, Enumerable.Empty<ElementBase>());
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyMultiElementBaseSelectorComponent()
        {
            var renderer = this.context.Render<MultiElementBaseSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            Assert.That(renderer.FindComponents<FilterTagBox<ElementBase>>(), Is.Empty,
                "without a CurrentIteration the selector must not render.");

            this.viewModel.Setup(x => x.CurrentIteration).Returns(new Iteration { Iid = Guid.NewGuid() });
            renderer.Render();

            var filterTagBox = renderer.FindComponent<FilterTagBox<ElementBase>>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("h6").TextContent, Is.EqualTo("Filter on Element:"));
                Assert.That(filterTagBox.Instance.Data, Is.EqualTo(this.availableElements));
            });

            // Invoked on FilterTagBox rather than the nested DxTagBox: DevExpress does not echo the bound
            // EventCallback back onto its own instance under bunit.
            var newSelection = new List<ElementBase> { this.availableElements[0] };
            await renderer.InvokeAsync(() => filterTagBox.Instance.ValuesChanged.InvokeAsync(newSelection));

            Assert.That(this.viewModel.Object.SelectedElementBases, Is.EqualTo(newSelection));

            renderer.Render(parameters => parameters.Add(p => p.DisplayText, string.Empty));

            Assert.That(renderer.FindAll("h6"), Is.Empty);
        }
    }
}
