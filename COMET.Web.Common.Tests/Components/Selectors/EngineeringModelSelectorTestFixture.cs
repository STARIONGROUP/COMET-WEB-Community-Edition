// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelSelectorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class EngineeringModelSelectorTestFixture
    {
        private TestContext context;
        private Mock<IEngineeringModelSelectorViewModel> viewModel;
        private List<EngineeringModelSetup> setups;
        
        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.setups = new List<EngineeringModelSetup>();
            this.viewModel = new Mock<IEngineeringModelSelectorViewModel>();
            this.viewModel.Setup(x => x.AvailableEngineeringModelSetups).Returns(this.setups);
            this.setups.Add(new EngineeringModelSetup(){Name = "model A"});
            this.setups.Add(new EngineeringModelSetup(){Name = "model B"});
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyEngineeringModelSelectorComponent()
        {
            var renderer = this.context.RenderComponent<EngineeringModelSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            Assert.That(renderer.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));

            var listBox = renderer.FindComponent<DxListBox<EngineeringModelSetup, EngineeringModelSetup>>();
            var submitButton = renderer.FindComponent<DxButton>();

            Assert.Multiple(() =>
            {
                Assert.That(listBox.Instance.Data.Count(), Is.EqualTo(2));
                Assert.That(submitButton.Instance.Enabled, Is.False);
            });

            this.viewModel.Setup(x => x.SelectedEngineeringModelSetup).Returns(this.setups[^1]);
            renderer.Render();

            Assert.That(submitButton.Instance.Enabled, Is.True);

            await renderer.InvokeAsync(submitButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.Submit(), Times.Once);
        }
    }
}
