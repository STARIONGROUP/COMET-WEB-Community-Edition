// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SwitchDomainTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SwitchDomainTestFixture
    {
        private BunitContext context;
        private Mock<ISwitchDomainViewModel> viewModel;
        private List<DomainOfExpertise> availableDomains;
        private DomainOfExpertise submittedDomain;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.availableDomains = new List<DomainOfExpertise>
            {
                new() { Iid = Guid.NewGuid(), Name = "Domain A" },
                new() { Iid = Guid.NewGuid(), Name = "Domain B" }
            };

            this.viewModel = new Mock<ISwitchDomainViewModel>();
            this.viewModel.Setup(x => x.AvailableDomains).Returns(this.availableDomains);
            this.viewModel.Setup(x => x.SelectedDomainOfExpertise).Returns(this.availableDomains[0]);
            this.viewModel.Setup(x => x.OnSubmit).Returns(new EventCallbackFactory().Create<DomainOfExpertise>(this, domain => this.submittedDomain = domain));
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifySwitchDomainComponent()
        {
            var renderer = this.context.Render<SwitchDomain>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            // The DxComboBox is asserted by presence only: DevExpress echoes neither Data nor ValueChanged back
            // onto its own instance under bunit, so behaviour is verified through the switch button instead.
            Assert.That(renderer.FindComponent<DxComboBox<DomainOfExpertise, DomainOfExpertise>>(), Is.Not.Null);

            var switchButton = renderer.FindComponent<DxButton>();
            await renderer.InvokeAsync(switchButton.Instance.Click.InvokeAsync);

            Assert.That(this.submittedDomain, Is.EqualTo(this.availableDomains[0]));
        }
    }
}
