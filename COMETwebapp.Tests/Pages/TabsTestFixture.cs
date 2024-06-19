// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Pages
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
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

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class TabsTestFixture
    {
        private TestContext context;
        private Mock<ITabsViewModel> viewModel;
        private Mock<IEngineeringModelBodyViewModel> engineeringModelBodyViewModel;
        private IRenderedComponent<Tabs> renderer;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();

            var engineeringModelBodyApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel = new Mock<ITabsViewModel>();
            this.viewModel.Setup(x => x.OpenTabs).Returns(new SourceList<TabbedApplicationInformation>());
            this.viewModel.Setup(x => x.SelectedApplication).Returns(engineeringModelBodyApplication);

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

            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(this.engineeringModelBodyViewModel.Object);
            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(new Mock<IOpenTabViewModel>().Object);
            this.context.Services.AddSingleton(new Mock<IOpenModelViewModel>().Object);
            this.context.Services.AddSingleton(new Mock<IStringTableService>().Object);

            this.renderer = this.context.RenderComponent<Tabs>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyTabComponents()
        {
            var openTabs = new SourceList<TabbedApplicationInformation>();
            openTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.viewModel.Setup(x => x.OpenTabs).Returns(openTabs);
            this.viewModel.Setup(x => x.CurrentTab).Returns(openTabs.Items.First());
            this.renderer.Render();

            var tabComponents = this.renderer.FindComponents<TabComponent>();
            var firstTab = tabComponents[0];

            await this.renderer.InvokeAsync(firstTab.Instance.OnClick.Invoke);
            this.viewModel.VerifySet(x => x.CurrentTab = openTabs.Items.First(), Times.Once);
            await this.renderer.InvokeAsync(firstTab.Instance.OnIconClick.Invoke);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.OpenTabs, Times.AtLeastOnce);
                Assert.That(this.renderer.Instance.IsOpenTabVisible, Is.False);
            });

            var secondTab = tabComponents[1];
            await this.renderer.InvokeAsync(secondTab.Instance.OnClick.Invoke);
            Assert.That(this.renderer.Instance.IsOpenTabVisible, Is.True);
        }

        [Test]
        public void VerifyTabsPage()
        {
            var tabComponents = this.renderer.FindComponents<TabComponent>();
            var openTab = this.renderer.FindComponents<OpenTab>();

            Assert.Multiple(() =>
            {
                Assert.That(tabComponents, Has.Count.EqualTo(0));
                Assert.That(openTab, Has.Count.EqualTo(1));
            });

            var openTabs = new SourceList<TabbedApplicationInformation>();
            openTabs.Add(new TabbedApplicationInformation(this.engineeringModelBodyViewModel.Object, typeof(EngineeringModelBody), this.iteration));
            this.viewModel.Setup(x => x.OpenTabs).Returns(openTabs);
            this.viewModel.Setup(x => x.CurrentTab).Returns(openTabs.Items.First());
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
