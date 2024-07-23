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
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Components.Shared;
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
        private TabPanelInformation mainPanel;
        private TabPanelInformation sidePanel;

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

            var engineeringModelSetup = new EngineeringModelSetup();

            this.iteration = new Iteration
            {
                IterationSetup = new IterationSetup
                {
                    Container = engineeringModelSetup
                },
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = engineeringModelSetup
                }
            };

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            this.viewModel = new Mock<ITabsViewModel>();
            var openTabs = new SourceList<TabbedApplicationInformation>();
            openTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.sidePanel = new TabPanelInformation();

            this.mainPanel = new TabPanelInformation
            {
                OpenTabs = openTabs,
                CurrentTab = openTabs.Items.First()
            };

            this.viewModel.Setup(x => x.MainPanel).Returns(this.mainPanel);
            this.viewModel.Setup(x => x.SidePanel).Returns(this.sidePanel);
            this.viewModel.Setup(x => x.SelectedApplication).Returns(engineeringModelBodyApplication);

            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise());

            this.context.Services.AddSingleton(sessionService.Object);
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(this.engineeringModelBodyViewModel.Object);
            this.context.Services.AddSingleton(configuration.Object);

            this.renderer = this.context.RenderComponent<TabsPanelComponent>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.Panel, this.mainPanel);
                parameters.Add(p => p.CssClass, "css-test-class");
                parameters.Add(p => p.IsSidePanelAvailable, true);
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

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Object.SidePanel.OpenTabs, Has.Count.GreaterThan(0));
                Assert.That(this.viewModel.Object.SidePanel.CurrentTab, Is.Not.Null);
                Assert.That(this.viewModel.Object.MainPanel.OpenTabs, Has.Count.EqualTo(0));
            });
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

        [Test]
        public async Task VerifyTabsOrdering()
        {
            var newTab = new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration);
            this.mainPanel.OpenTabs.Add(newTab);

            Assert.Multiple(() =>
            {
                Assert.That(this.mainPanel.OpenTabs, Has.Count.EqualTo(2));
                Assert.That(this.sidePanel.OpenTabs, Has.Count.EqualTo(0));
                Assert.That(this.mainPanel.OpenTabs.Items.First(), Is.Not.EqualTo(newTab));
                Assert.That(this.mainPanel.OpenTabs.Items.ElementAt(1), Is.EqualTo(newTab));
            });

            var sortableList = this.renderer.FindComponent<SortableList<TabbedApplicationInformation>>();
            await this.renderer.InvokeAsync(() => sortableList.Instance.OnUpdate.InvokeAsync((0, 1)));

            Assert.Multiple(() =>
            {
                Assert.That(this.mainPanel.OpenTabs, Has.Count.EqualTo(2));
                Assert.That(this.mainPanel.OpenTabs.Items.First(), Is.EqualTo(newTab));
                Assert.That(this.mainPanel.OpenTabs.Items.ElementAt(1), Is.Not.EqualTo(newTab));
            });

            await this.renderer.InvokeAsync(() => sortableList.Instance.OnRemove.InvokeAsync((0, 0)));
            this.sidePanel.CurrentTab = this.sidePanel.OpenTabs.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(this.mainPanel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.sidePanel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.mainPanel.OpenTabs.Items.First(), Is.Not.EqualTo(newTab));
                Assert.That(this.sidePanel.OpenTabs.Items.First(), Is.EqualTo(newTab));
            });

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.Panel, this.sidePanel);
            });

            sortableList = this.renderer.FindComponent<SortableList<TabbedApplicationInformation>>();
            await this.renderer.InvokeAsync(() => sortableList.Instance.OnRemove.InvokeAsync((0, 0)));

            Assert.Multiple(() =>
            {
                Assert.That(this.mainPanel.OpenTabs, Has.Count.EqualTo(2));
                Assert.That(this.sidePanel.OpenTabs, Has.Count.EqualTo(0));
                Assert.That(this.mainPanel.OpenTabs.Items.First(), Is.EqualTo(newTab));
            });
        }
    }
}
