// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiOptionSelectorTestFixture.cs" company="Starion Group S.A.">
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
    public class MultiOptionSelectorTestFixture
    {
        private BunitContext context;
        private Mock<IMultiOptionSelectorViewModel> viewModel;
        private List<Option> availableOptions;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.availableOptions = new List<Option>
            {
                new() { Iid = Guid.NewGuid(), Name = "Option A" },
                new() { Iid = Guid.NewGuid(), Name = "Option B" }
            };

            this.viewModel = new Mock<IMultiOptionSelectorViewModel>();
            this.viewModel.Setup(x => x.AvailableOptions).Returns(this.availableOptions);
            this.viewModel.SetupProperty(x => x.SelectedOptions, Enumerable.Empty<Option>());
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyMultiOptionSelectorComponent()
        {
            var renderer = this.context.Render<MultiOptionSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            Assert.That(renderer.FindComponents<FilterTagBox<Option>>(), Is.Empty,
                "without a CurrentIteration the selector must not render.");

            this.viewModel.Setup(x => x.CurrentIteration).Returns(new Iteration { Iid = Guid.NewGuid() });
            renderer.Render();

            var filterTagBox = renderer.FindComponent<FilterTagBox<Option>>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("h6").TextContent, Is.EqualTo("Filter on Option:"));
                Assert.That(filterTagBox.Instance.Data, Is.EqualTo(this.availableOptions));
            });

            // Invoked on FilterTagBox rather than the nested DxTagBox: DevExpress does not echo the bound
            // EventCallback back onto its own instance under bunit.
            var newSelection = new List<Option> { this.availableOptions[0] };
            await renderer.InvokeAsync(() => filterTagBox.Instance.ValuesChanged.InvokeAsync(newSelection));

            Assert.That(this.viewModel.Object.SelectedOptions, Is.EqualTo(newSelection));

            renderer.Render(parameters => parameters.Add(p => p.DisplayText, string.Empty));

            Assert.That(renderer.FindAll("h6"), Is.Empty);
        }
    }
}
