// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationSelectorTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IterationSelectorTestFixture
    {
        private BunitContext context;
        private Mock<IIterationSelectorViewModel> viewModel;
        private List<IterationData> iterations;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var modelSetup = new EngineeringModelSetup { Iid = Guid.NewGuid(), Name = "model" };
            var iterationSetupA = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 1, Container = modelSetup };
            var iterationSetupB = new IterationSetup { Iid = Guid.NewGuid(), IterationNumber = 2, Container = modelSetup };

            this.iterations = new List<IterationData>
            {
                new(iterationSetupA),
                new(iterationSetupB)
            };

            this.viewModel = new Mock<IIterationSelectorViewModel>();
            this.viewModel.Setup(x => x.AvailableIterations).Returns(this.iterations);
            this.viewModel.Setup(x => x.Submit()).Returns(Task.CompletedTask);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyIterationSelectorComponent()
        {
            var renderer = this.context.Render<IterationSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            var listBox = renderer.FindComponent<DxListBox<IterationData, IterationData>>();
            var submitButton = renderer.FindComponent<DxButton>();

            Assert.Multiple(() =>
            {
                Assert.That(listBox.Instance.Data.Count(), Is.EqualTo(2));
                Assert.That(submitButton.Instance.Enabled, Is.False);
            });

            this.viewModel.Setup(x => x.SelectedIteration).Returns(this.iterations[^1]);
            renderer.Render();

            Assert.That(submitButton.Instance.Enabled, Is.True);

            await renderer.InvokeAsync(submitButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.Submit(), Times.Once);
        }
    }
}
