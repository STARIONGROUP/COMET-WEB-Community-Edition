// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsPanelComponentTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.Tabs
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Components.Tabs;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;
    using COMETwebapp.ViewModels.Pages;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class TabsPanelComponentTestFixture
    {
        private TestContext context;
        private IRenderedComponent<TabsPanelComponent> renderer;
        private Mock<ITabsViewModel> viewModel;
        private Mock<IEngineeringModelBodyViewModel> engineeringModelBodyViewModel;
        private Iteration iteration;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var engineeringModelBodyApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.Url == WebAppConstantValues.EngineeringModelPage);

            var optionsTableViewModel = new Mock<IOptionsTableViewModel>();
            optionsTableViewModel.Setup(x => x.Rows).Returns(new SourceList<OptionRowViewModel>());
            optionsTableViewModel.Setup(x => x.CurrentThing).Returns(new Option());
            this.engineeringModelBodyViewModel = new Mock<IEngineeringModelBodyViewModel>();
            this.engineeringModelBodyViewModel.Setup(x => x.OptionsTableViewModel).Returns(optionsTableViewModel.Object);

            this.iteration = new Iteration
            {
                IterationSetup = new IterationSetup
                {
                    Container = new EngineeringModelSetup()
                }
            };

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            this.viewModel = new Mock<ITabsViewModel>();
            var openTabs = new SourceList<TabbedApplicationInformation>();
            openTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.viewModel.Setup(x => x.OpenTabs).Returns(openTabs);
            this.viewModel.Setup(x => x.CurrentTab).Returns(openTabs.Items.First());
            this.viewModel.Setup(x => x.SelectedApplication).Returns(engineeringModelBodyApplication);
            this.viewModel.Setup(x => x.SidePanels).Returns(new SourceList<TabPanelInformation>());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(this.engineeringModelBodyViewModel.Object);
            this.context.Services.AddSingleton(configuration.Object);

            this.renderer = this.context.RenderComponent<TabsPanelComponent>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.Handler, this.viewModel.Object);
                parameters.Add(p => p.CssClass, "css-test-class");
                parameters.Add(p => p.IsSidePanelAvailable, true);
                parameters.Add(p => p.Tabs, this.viewModel.Object.OpenTabs.Items.ToList());
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyAddSidePanel()
        {
            var sidePanelButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "new-side-panel-button");
            await this.renderer.InvokeAsync(sidePanelButton.Instance.Click.InvokeAsync);

            this.viewModel.VerifySet(x => x.CurrentTab = null, Times.Once);
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain("css-test-class"));
            });
        }
    }
}
