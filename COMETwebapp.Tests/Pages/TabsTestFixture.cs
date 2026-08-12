// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.Pages
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Components.Tabs;
    using COMETwebapp.Model;
    using COMETwebapp.Pages;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Common.OpenTab;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;
    using COMETwebapp.ViewModels.Pages;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabsTestFixture
    {
        private BunitContext context;
        private Mock<ITabsViewModel> viewModel;
        private Mock<IEngineeringModelBodyViewModel> engineeringModelBodyViewModel;
        private IRenderedComponent<Tabs> renderer;
        private Iteration iteration;
        private TabPanelInformation mainPanel;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();

            var engineeringModelBodyApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            var engineeringSetupModel = new EngineeringModelSetup();

            this.iteration = new Iteration
            {
                IterationSetup = new IterationSetup
                {
                    Container = engineeringSetupModel
                },
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = engineeringSetupModel
                }
            };

            var optionsTableViewModel = new Mock<IOptionsTableViewModel>();
            optionsTableViewModel.Setup(x => x.Rows).Returns(new SourceList<OptionRowViewModel>());
            optionsTableViewModel.Setup(x => x.CurrentThing).Returns(new Option());
            this.engineeringModelBodyViewModel = new Mock<IEngineeringModelBodyViewModel>();
            this.engineeringModelBodyViewModel.Setup(x => x.OptionsTableViewModel).Returns(optionsTableViewModel.Object);

            this.mainPanel = new TabPanelInformation();
            this.mainPanel.OpenTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.mainPanel.CurrentTab = this.mainPanel.OpenTabs.Items[0];

            this.viewModel = new Mock<ITabsViewModel>();
            this.viewModel.Setup(x => x.MainPanel).Returns(this.mainPanel);
            this.viewModel.Setup(x => x.SidePanel).Returns(new TabPanelInformation());
            this.viewModel.Setup(x => x.SelectedApplication).Returns(engineeringModelBodyApplication);
            this.viewModel.Setup(x => x.SwitchDomainViewModel).Returns(new Mock<ISwitchDomainViewModel>().Object);
            this.viewModel.Setup(x => x.RestoreTabsPopupViewModel).Returns(new ConfirmCancelPopupViewModel());

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise());

            this.messageBus = new CDPMessageBus();

            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(this.engineeringModelBodyViewModel.Object);
            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton<ICDPMessageBus>(this.messageBus);
            this.context.Services.AddSingleton(new Mock<IOpenTabViewModel>().Object);
            this.context.Services.AddSingleton(new Mock<IOpenModelViewModel>().Object);
            this.context.Services.AddSingleton(new Mock<IStringTableService>().Object);
            this.context.Services.AddSingleton(sessionService.Object);

            this.renderer = this.context.Render<Tabs>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifySidePanelIsOnlyAvailableWithMoreThanOneOpenTab()
        {
            var button = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "new-side-panel-button");
            Assert.That(button.Instance.Enabled, Is.False);

            this.mainPanel.OpenTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.renderer.Render();

            button = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "new-side-panel-button");
            Assert.That(button.Instance.Enabled, Is.True);
        }

        [Test]
        public void VerifyOnlyOneSwitchDomainPopupIsRenderedInSplitView()
        {
            this.mainPanel.OpenTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.viewModel.Object.SidePanel.OpenTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.viewModel.Object.SidePanel.CurrentTab = this.viewModel.Object.SidePanel.OpenTabs.Items[0];
            
            this.viewModel.Setup(x => x.IsOnSwitchDomainMode).Returns(true);

            this.renderer.Render();

            var switchDomainPopups = this.renderer.FindComponents<SwitchDomain>();
            Assert.That(switchDomainPopups, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyTabComponents()
        {
            var tabComponents = this.renderer.FindComponents<TabComponent>();
            var firstTab = tabComponents[0];
            var selectModelTab = tabComponents[1];

            await this.renderer.InvokeAsync(firstTab.Instance.OnClick.Invoke);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Object.MainPanel.CurrentTab, Is.EqualTo(this.mainPanel.OpenTabs.Items[0]));
                Assert.That(this.renderer.Instance.IsOpenTabVisible, Is.False);
            });

            await this.renderer.InvokeAsync(selectModelTab.Instance.OnClick.Invoke);
            Assert.That(this.renderer.Instance.IsOpenTabVisible, Is.True);

            await this.renderer.InvokeAsync(firstTab.Instance.OnIconClick.Invoke);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Object.MainPanel.OpenTabs, Has.Count.EqualTo(0));
                Assert.That(this.viewModel.Object.MainPanel.CurrentTab, Is.Null);
            });
        }

        [Test]
        public async Task VerifyTabCustomButton()
        {
            var tabCustomButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "tab-custom-option-button");
            await this.renderer.InvokeAsync(tabCustomButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsOpenTabVisible, Is.True);

            var openTabComponent = this.renderer.FindComponent<OpenTab>();
            await this.renderer.InvokeAsync(openTabComponent.Instance.OnCancel.Invoke);
            Assert.That(this.renderer.Instance.IsOpenTabVisible, Is.False);

            this.mainPanel.OpenTabs.ReplaceAt(0, new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), null));
            this.renderer.Render();
            var hasCustomOption = this.renderer.FindComponents<DxButton>().Any(x => x.Instance.Id == "tab-custom-option-button");
            Assert.That(hasCustomOption, Is.False);
        }

        [Test]
        public void VerifyTabsPage()
        {
            this.viewModel.Setup(x => x.MainPanel).Returns(new TabPanelInformation());
            this.renderer.Render();

            var tabComponents = this.renderer.FindComponents<TabComponent>();
            var openTab = this.renderer.FindComponents<OpenTab>();

            Assert.Multiple(() =>
            {
                Assert.That(tabComponents, Has.Count.EqualTo(0));
                Assert.That(openTab, Has.Count.EqualTo(1));
            });

            this.viewModel.Setup(x => x.MainPanel).Returns(this.mainPanel);
            this.renderer.Render();

            tabComponents = this.renderer.FindComponents<TabComponent>();
            openTab = this.renderer.FindComponents<OpenTab>();
            var componentOfSelectedTab = this.renderer.FindComponent<EngineeringModelBody>();

            Assert.Multiple(() =>
            {
                Assert.That(tabComponents, Has.Count.EqualTo(2));
                Assert.That(openTab, Has.Count.EqualTo(0));
                Assert.That(componentOfSelectedTab.Instance, Is.Not.Null);
            });
        }
    }
}
