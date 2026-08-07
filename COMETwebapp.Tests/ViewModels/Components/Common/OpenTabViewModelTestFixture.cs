// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTabViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Common
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Common.OpenTab;
    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OpenTabViewModelTestFixture
    {
        private OpenTabViewModel viewModel;
        private Mock<ICacheService> cacheService;
        private Mock<ISessionService> sessionService;
        private Mock<IConfigurationService> configurationService;
        private Mock<ITabsViewModel> tabsViewModel;
        private SourceList<Iteration> alreadyOpenIterations;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.configurationService = new Mock<IConfigurationService>();
            this.cacheService = new Mock<ICacheService>();
            this.tabsViewModel = new Mock<ITabsViewModel>();

            var alreadyOpenIterationSetup = new IterationSetup { Iid = Guid.NewGuid() };

            var alreadyOpenIteration = new Iteration
            {
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = new EngineeringModelSetup
                    {
                        IterationSetup = { alreadyOpenIterationSetup }
                    }
                },
                Iid = Guid.NewGuid(),
                IterationSetup = alreadyOpenIterationSetup
            };

            this.alreadyOpenIterations = new SourceList<Iteration>();
            this.alreadyOpenIterations.Add(alreadyOpenIteration);

            this.sessionService.Setup(x => x.OpenIterations).Returns(this.alreadyOpenIterations);
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns([]);
            this.sessionService.Setup(x => x.ReadEngineeringModels(It.IsAny<IEnumerable<EngineeringModelSetup>>())).Returns(Task.FromResult(new Result()));
            this.sessionService.Setup(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>())).Returns(Task.FromResult(new Result<Iteration>()));

            this.viewModel = new OpenTabViewModel(this.sessionService.Object, this.configurationService.Object, this.tabsViewModel.Object, this.cacheService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel?.Dispose();
            this.alreadyOpenIterations?.Dispose();
        }

        /// <summary>
        /// Verifies that an <see cref="Iteration" /> that is already open can still be selected when opening a tab, so that the
        /// same iteration can be shown in several views at once, for instance the Model Editor next to the System
        /// Representation. Only opening a <i>model</i> refuses an already open iteration.
        /// </summary>
        [Test]
        public void VerifyAlreadyOpenIterationCanStillBeSelectedForANewTab()
        {
            var alreadyOpenIteration = this.alreadyOpenIterations.Items[0];
            var engineeringModelSetup = ((EngineeringModel)alreadyOpenIteration.Container).EngineeringModelSetup;

            // The "is this iteration already open" check matches IterationSetup.IterationIid against Iteration.Iid, so the
            // setup has to point back at its iteration for the iteration to count as open at all.
            alreadyOpenIteration.IterationSetup.IterationIid = alreadyOpenIteration.Iid;

            this.viewModel.SelectedEngineeringModel = engineeringModelSetup;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableIterationSetups.Select(x => x.IterationSetupId),
                    Does.Contain(alreadyOpenIteration.IterationSetup.Iid),
                    "An already open iteration must remain selectable when opening a new tab on it.");

                Assert.That(this.viewModel.SelectedIterationSetup, Is.Not.Null,
                    "Without a selected iteration the Open Tab button stays disabled, which would block opening a second view on the iteration.");
            });
        }

        /// <summary>
        /// Verifies the path that the Open Tab component actually takes: it calls <c>InitializesProperties</c>, which restores the
        /// last used iteration from the browser session cache. That iteration is normally the one that is currently open, and it
        /// must still be restored here, otherwise the Open Tab button stays disabled and no second view can be opened on it.
        /// </summary>
        [Test]
        public void VerifyInitializesPropertiesRestoresTheAlreadyOpenIteration()
        {
            var alreadyOpenIteration = this.alreadyOpenIterations.Items[0];
            var engineeringModelSetup = ((EngineeringModel)alreadyOpenIteration.Container).EngineeringModelSetup;

            alreadyOpenIteration.IterationSetup.IterationIid = alreadyOpenIteration.Iid;

            var cachedIterationData = new IterationData(alreadyOpenIteration.IterationSetup);

            this.sessionService.Setup(x => x.GetParticipantModels()).Returns([engineeringModelSetup]);

            object engineeringModelSetting = engineeringModelSetup;
            object iterationSetting = cachedIterationData;

            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedEngineeringModel, out engineeringModelSetting)).Returns(true);
            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedIterationData, out iterationSetting)).Returns(true);

            this.viewModel.InitializesProperties();

            Assert.That(this.viewModel.SelectedIterationSetup?.IterationSetupId, Is.EqualTo(cachedIterationData.IterationSetupId),
                "The already open iteration must be restored, so that another view can be opened on it.");
        }

        [Test]
        public async Task VerifyOpenIterationAndModel()
        {
            var panel = new TabPanelInformation();
            await this.viewModel.OpenTab(panel);

            Assert.Multiple(() =>
            {
                this.tabsViewModel.VerifySet(x => x.SelectedApplication = this.viewModel.SelectedApplication, Times.Never);
                this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Never);
            });

            var toBeOpenedIterationSetup = new IterationSetup { Iid = Guid.NewGuid() };

            var toBeOpenedIteration = new Iteration
            {
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = new EngineeringModelSetup
                    {
                        IterationSetup = { toBeOpenedIterationSetup }
                    }
                },
                Iid = Guid.NewGuid(),
                IterationSetup = toBeOpenedIterationSetup
            };

            var engineeringModelBodyApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.SelectedApplication = engineeringModelBodyApplication;
            this.viewModel.SelectedEngineeringModel = ((EngineeringModel)toBeOpenedIteration.Container).EngineeringModelSetup;
            this.viewModel.SelectedIterationSetup = new IterationData(toBeOpenedIterationSetup);
            this.viewModel.SelectedDomainOfExpertise = new DomainOfExpertise();

            //Make sure that toBeOpenedIteration is added to sessionService.OpenIterations when new Iteration is open
            this.sessionService.Setup(x => x.ReadIteration(toBeOpenedIterationSetup, It.IsAny<DomainOfExpertise>()))
                .Returns(Task.FromResult(new Result<Iteration>()))
                .Callback(() =>
                {
                    this.alreadyOpenIterations.Add(toBeOpenedIteration);
                });

            await this.viewModel.OpenTab(panel);

            Assert.Multiple(() =>
            {
                this.tabsViewModel.Verify(x => x.CreateNewTab(It.IsAny<TabbedApplication>(), It.IsAny<Guid>(), It.IsAny<TabPanelInformation>()), Times.Exactly(2));
                this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Once);
            });

            var newEngineeringModel = new EngineeringModel { EngineeringModelSetup = this.viewModel.SelectedEngineeringModel };
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns([newEngineeringModel]);

            await this.viewModel.OpenTab(panel);
            this.sessionService.Verify(x => x.SwitchDomain(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>()), Times.Once);

            // Verify that for a non-iteration application (e.g., Book Editor), the active iteration setup is opened and used
            var frozenIterationSetup = new IterationSetup { Iid = Guid.NewGuid(), FrozenOn = DateTime.UtcNow };
            var activeIterationSetup = new IterationSetup { Iid = Guid.NewGuid(), FrozenOn = null };
            
            var modelSetupForBookEditor = new EngineeringModelSetup
            {
                IterationSetup = { frozenIterationSetup, activeIterationSetup }
            };

            var bookEditorApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.Url == WebAppConstantValues.BookEditorPage);
            this.viewModel.SelectedApplication = bookEditorApplication;
            this.viewModel.SelectedEngineeringModel = modelSetupForBookEditor;
            this.viewModel.SelectedIterationSetup = new IterationData(frozenIterationSetup);
            this.viewModel.SelectedDomainOfExpertise = new DomainOfExpertise();

            this.sessionService.Setup(x => x.ReadIteration(activeIterationSetup, It.IsAny<DomainOfExpertise>())).ReturnsAsync(new Result<Iteration>());
            await this.viewModel.OpenTab(panel);

            using (Assert.EnterMultipleScope())
            {
                this.sessionService.Verify(x => x.ReadIteration(activeIterationSetup, It.IsAny<DomainOfExpertise>()), Times.Once);
                Assert.That(this.viewModel.SelectedIterationSetup.IterationSetupId, Is.EqualTo(activeIterationSetup.Iid));
            }
        }

        /// <summary>
        /// Verifies that multiple iterations of the same engineering model can be opened.
        /// </summary>
        [Test]
        public async Task VerifyOpenMultipleIterationsOfSameModel()
        {
            var panel = new TabPanelInformation();
            var firstIteration = this.alreadyOpenIterations.Items[0];
            var engineeringModelSetup = ((EngineeringModel)firstIteration.Container).EngineeringModelSetup;

            var secondIterationSetup = new IterationSetup { Iid = Guid.NewGuid(), IterationIid = Guid.NewGuid() };
            engineeringModelSetup.IterationSetup.Add(secondIterationSetup);

            var secondIteration = new Iteration
            {
                Container = firstIteration.Container,
                Iid = secondIterationSetup.IterationIid,
                IterationSetup = secondIterationSetup
            };

            var engineeringModelBodyApplication = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.SelectedApplication = engineeringModelBodyApplication;
            this.viewModel.SelectedEngineeringModel = engineeringModelSetup;
            this.viewModel.SelectedIterationSetup = new IterationData(secondIterationSetup);
            this.viewModel.SelectedDomainOfExpertise = new DomainOfExpertise();

            this.sessionService.Setup(x => x.ReadIteration(secondIterationSetup, It.IsAny<DomainOfExpertise>()))
                .Returns(Task.FromResult(new Result<Iteration>()))
                .Callback(() =>
                {
                    this.alreadyOpenIterations.Add(secondIteration);
                });

            await this.viewModel.OpenTab(panel);

            using (Assert.EnterMultipleScope())
            {
                this.sessionService.Verify(x => x.ReadIteration(secondIterationSetup, It.IsAny<DomainOfExpertise>()), Times.Once);
                this.sessionService.Verify(x => x.SwitchDomain(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>()), Times.Never);
                this.tabsViewModel.Verify(x => x.CreateNewTab(engineeringModelBodyApplication, secondIteration.Iid, panel), Times.Once);
            }
        }

        [Test]
        public void VerifyNavigateToOpenTab()
        {
            // When no application or model is selected, navigation fails
            var mainPanel = new TabPanelInformation();
            var sidePanel = new TabPanelInformation();
            this.tabsViewModel.Setup(x => x.MainPanel).Returns(mainPanel);
            this.tabsViewModel.Setup(x => x.SidePanel).Returns(sidePanel);

            Assert.That(this.viewModel.NavigateToOpenTab(), Is.False);

            // When an iteration view is selected but no tabs are currently open for it
            var alreadyOpenIteration = this.alreadyOpenIterations.Items[0];
            alreadyOpenIteration.IterationSetup.IterationIid = alreadyOpenIteration.Iid;
            var engineeringModelSetup = ((EngineeringModel)alreadyOpenIteration.Container).EngineeringModelSetup;

            var application = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.ThingTypeOfInterest == typeof(Iteration));
            this.viewModel.SelectedApplication = application;
            this.viewModel.SelectedEngineeringModel = engineeringModelSetup;
            this.viewModel.SelectedIterationSetup = new IterationData(alreadyOpenIteration.IterationSetup);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasOpenTab, Is.False);
                Assert.That(this.viewModel.NavigateToOpenTab(), Is.False);
            }

            // When a matching iteration tab is open in the main panel, navigate to it
            var dummyViewModel = new Mock<IApplicationBaseViewModel>();
            var existingTab = new TabbedApplicationInformation(dummyViewModel.Object, application.ComponentType, alreadyOpenIteration);
            mainPanel.OpenTabs.Add(existingTab);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasOpenTab, Is.True);
                Assert.That(this.viewModel.NavigateToOpenTab(), Is.True);
                Assert.That(mainPanel.CurrentTab, Is.EqualTo(existingTab));
            }

            // When a matching iteration tab is open in the side panel, navigate to it
            mainPanel.OpenTabs.Remove(existingTab);
            mainPanel.CurrentTab = null;
            sidePanel.OpenTabs.Add(existingTab);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.NavigateToOpenTab(), Is.True);
                Assert.That(sidePanel.CurrentTab, Is.EqualTo(existingTab));
            }

            // When an engineering-model-level view tab is open in the side panel, navigate to it
            var engModelApp = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.ThingTypeOfInterest == typeof(EngineeringModel));
            this.viewModel.SelectedApplication = engModelApp;

            var openEngModel = (EngineeringModel)alreadyOpenIteration.Container;
            openEngModel.Iid = Guid.NewGuid();
            engineeringModelSetup.EngineeringModelIid = openEngModel.Iid;
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns([openEngModel]);

            var engModelTab = new TabbedApplicationInformation(dummyViewModel.Object, engModelApp.ComponentType, openEngModel);
            sidePanel.OpenTabs.Add(engModelTab);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasOpenTab, Is.True);
                Assert.That(this.viewModel.NavigateToOpenTab(), Is.True);
            }
        }
    }
}
