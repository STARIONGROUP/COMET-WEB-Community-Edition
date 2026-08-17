// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Pages
{
    using Blazored.SessionStorage;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabsViewModelTestFixture
    {
        private TabsViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IServiceProvider> serviceProvider;
        private Mock<ISessionStorageService> sessionStorageService;
        private Mock<ILogger<TabsViewModel>> logger;
        private SourceList<Iteration> openIterations;
        private CDPMessageBus messageBus;

        private void CreateViewModel()
        {
            this.viewModel?.Dispose();
            this.viewModel = new TabsViewModel(this.sessionService.Object, this.serviceProvider.Object, this.sessionStorageService.Object, this.logger.Object, this.messageBus);
        }

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = new Mock<IServiceProvider>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionStorageService = new Mock<ISessionStorageService>();
            this.logger = new Mock<ILogger<TabsViewModel>>();
            this.openIterations = new SourceList<Iteration>();
            this.messageBus = new CDPMessageBus();

            var iteration = new Iteration();
            var iterationSetup = new IterationSetup();
            var modelSetup = new EngineeringModelSetup();
            var engineeringModel = new EngineeringModel();

            iteration.IterationSetup = iterationSetup;
            iterationSetup.Container = modelSetup;

            modelSetup.IterationSetup.Add(iterationSetup);
            engineeringModel.EngineeringModelSetup = modelSetup;

            this.openIterations.Add(iteration);
            var engineeringModels = new List<EngineeringModel> { engineeringModel };
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns(engineeringModels);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns((DomainOfExpertise)null);
            this.sessionStorageService.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
            this.serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(new Mock<IApplicationBaseViewModel>().Object);

            this.CreateViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public async Task VerifyCheckAndRestoreSavedTabsAsync()
        {
            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync((List<SavedTabDto>)null);

            await this.viewModel.CheckAndRestoreSavedTabsAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.ShowCloseButton, Is.False);
            }

            var savedTabs = new List<SavedTabDto>
            {
                new() { ApplicationName = nameof(EngineeringModelBody), ObjectOfInterestId = Guid.NewGuid(), IsSidePanel = false },
                new() { ApplicationName = nameof(EngineeringModelBody), ObjectOfInterestId = Guid.NewGuid(), IsSidePanel = true }
            };

            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync(savedTabs);

            this.CreateViewModel();
            await this.viewModel.CheckAndRestoreSavedTabsAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.ContentText, Does.Contain("restore your 2 previous tabs"));
            }

            this.viewModel.RestoreTabsPopupViewModel.IsVisible = false;
            await this.viewModel.CheckAndRestoreSavedTabsAsync();
            Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);

            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException());

            this.CreateViewModel();
            await this.viewModel.CheckAndRestoreSavedTabsAsync();

            Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyOnSelectedApplicationDoesNotCrossPanel()
        {
            var engineeringModelApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            var bookEditorApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.BookEditorPage);

            this.viewModel.CreateNewTab(engineeringModelApplication, Guid.Empty, this.viewModel.MainPanel);
            var mainTab = this.viewModel.MainPanel.CurrentTab;

            this.viewModel.CreateNewTab(bookEditorApplication, Guid.Empty, this.viewModel.SidePanel);
            var sideTab = this.viewModel.SidePanel.CurrentTab;

            // Simulate clicking the SidePanel tab
            this.viewModel.SidePanel.CurrentTab = sideTab;
            this.viewModel.MainPanel.CurrentTab = mainTab;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.EqualTo(mainTab));
                Assert.That(this.viewModel.SidePanel.CurrentTab, Is.EqualTo(sideTab));
            }
        }

        [Test]
        public async Task VerifyDomainSwitching()
        {
            var emptyPanel = new TabPanelInformation();
            Assert.That(this.viewModel.GetCurrentDomainOfExpertise(emptyPanel), Is.Null);

            this.viewModel.AskToSwitchDomain(emptyPanel);
            Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);

            var iteration = new Iteration
            {
                IterationSetup = new IterationSetup
                {
                    Container = new EngineeringModelSetup()
                }
            };

            var domain = new DomainOfExpertise { Name = "Domain 1" };
            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(domain);
            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>())).Returns([domain]);

            var tab = new TabbedApplicationInformation(new Mock<IApplicationBaseViewModel>().Object, typeof(EngineeringModelBody), iteration);
            this.viewModel.MainPanel.OpenTabs.Add(tab);
            this.viewModel.MainPanel.CurrentTab = tab;

            var activeDomain = this.viewModel.GetCurrentDomainOfExpertise(this.viewModel.MainPanel);
            Assert.That(activeDomain, Is.EqualTo(domain));

            this.viewModel.AskToSwitchDomain(this.viewModel.MainPanel);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.True);
                Assert.That(this.viewModel.SwitchDomainViewModel.AvailableDomains, Has.Member(domain));
                Assert.That(this.viewModel.SwitchDomainViewModel.SelectedDomainOfExpertise, Is.EqualTo(domain));
            }

            await this.viewModel.SwitchDomainViewModel.OnSubmit.InvokeAsync(domain);

            using (Assert.EnterMultipleScope())
            {
                this.sessionService.Verify(x => x.SwitchDomain(iteration, domain), Times.Once);
                Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);
            }

            this.viewModel.AskToSwitchDomain(this.viewModel.MainPanel);
            Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.True);
            await this.viewModel.SwitchDomainViewModel.OnCancel.InvokeAsync();
            Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);
        }

        [Test]
        public void VerifyOnSelectedApplication()
        {
            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Null);

            var iteration = new Iteration();
            var iterationSetup = new IterationSetup();
            iteration.IterationSetup = iterationSetup;

            this.viewModel.MainPanel.OpenTabs.Add(new TabbedApplicationInformation(new Mock<IEngineeringModelBodyViewModel>().Object, typeof(EngineeringModelBody), iteration));
            this.viewModel.SelectedApplication = this.viewModel.AvailableApplications.FirstOrDefault(x => x.Url == WebAppConstantValues.EngineeringModelPage);

            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
        }

        [Test]
        public async Task VerifyOpenThingOfInterest()
        {
            var iterationSetupId = Guid.NewGuid();
            var domainId = Guid.NewGuid();
            var domain = new DomainOfExpertise { Iid = domainId };
            var defaultDomain = new DomainOfExpertise { Iid = Guid.NewGuid() };

            var iterationSetup = new IterationSetup { Iid = iterationSetupId };
            var modelSetup = new EngineeringModelSetup();
            modelSetup.IterationSetup.Add(iterationSetup);
            iterationSetup.Container = modelSetup;

            var siteDirectory = new SiteDirectory();
            siteDirectory.Model.Add(modelSetup);

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetAvailableDomains(modelSetup)).Returns([domain]);

            var person = new Person { DefaultDomain = defaultDomain };
            var session = new Mock<ISession>();
            session.Setup(x => x.ActivePerson).Returns(person);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            var savedTabs = new List<SavedTabDto>
            {
                new() { ApplicationName = "Engineering Model", IterationSetupId = Guid.Empty, DomainId = domainId },
                new() { ApplicationName = "Engineering Model", IterationSetupId = iterationSetupId, DomainId = domainId },
                new() { ApplicationName = "Engineering Model", IterationSetupId = Guid.NewGuid(), DomainId = domainId }
            };

            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync(savedTabs);

            await this.viewModel.CheckAndRestoreSavedTabsAsync();
            await this.viewModel.RestoreTabsPopupViewModel.OnConfirm.InvokeAsync();

            this.sessionService.Verify(x => x.ReadIteration(iterationSetup, domain), Times.Once);

            var unknownDomainId = Guid.NewGuid();
            modelSetup.ActiveDomain.Add(defaultDomain);
            this.sessionService.Setup(x => x.GetAvailableDomains(modelSetup)).Returns([]);

            var fallbackSavedTabs = new List<SavedTabDto>
            {
                new() { ApplicationName = "Engineering Model", IterationSetupId = iterationSetupId, DomainId = unknownDomainId }
            };

            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync(fallbackSavedTabs);

            this.CreateViewModel();
            await this.viewModel.CheckAndRestoreSavedTabsAsync();
            await this.viewModel.RestoreTabsPopupViewModel.OnConfirm.InvokeAsync();

            this.sessionService.Verify(x => x.ReadIteration(iterationSetup, defaultDomain), Times.Once);

            modelSetup.ActiveDomain.Clear();
            this.sessionService.Setup(x => x.GetAvailableDomains(modelSetup)).Returns([]);

            this.CreateViewModel();
            await this.viewModel.CheckAndRestoreSavedTabsAsync();
            await this.viewModel.RestoreTabsPopupViewModel.OnConfirm.InvokeAsync();

            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task VerifyRestoreSavedTabsAsync()
        {
            var iterationId = Guid.NewGuid();
            var iteration = new Iteration { Iid = iterationId, IterationSetup = new IterationSetup() };
            this.openIterations.Add(iteration);

            var savedTabs = new List<SavedTabDto>
            {
                new() { ApplicationName = "Engineering Model", ObjectOfInterestId = iterationId, IsSidePanel = false },
                new() { ApplicationName = "Engineering Model", ObjectOfInterestId = iterationId, IsSidePanel = true },
                new() { ApplicationName = "UnknownApp", ObjectOfInterestId = iterationId, IsSidePanel = false }
            };

            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync(savedTabs);

            await this.viewModel.CheckAndRestoreSavedTabsAsync();
            Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.True);

            await this.viewModel.RestoreTabsPopupViewModel.OnConfirm.InvokeAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.SidePanel.OpenTabs, Has.Count.EqualTo(1));
            }

            this.sessionStorageService.SetupSequence(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync(savedTabs)
                .ThrowsAsync(new InvalidOperationException());

            this.CreateViewModel();
            await this.viewModel.CheckAndRestoreSavedTabsAsync();
            await this.viewModel.RestoreTabsPopupViewModel.OnConfirm.InvokeAsync();

            Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifySaveOpenTabsToSessionStorageAsync()
        {
            // Case 1: Session not open guard clause
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(false);
            var iteration = this.openIterations.Items[0];
            var tab = new TabbedApplicationInformation(new Mock<IApplicationBaseViewModel>().Object, typeof(EngineeringModelBody), iteration);

            this.viewModel.MainPanel.OpenTabs.Add(tab);
            await Task.Yield();

            this.sessionStorageService.Verify(x => x.SetItemAsync(WebAppConstantValues.SavedTabsKey, It.IsAny<List<SavedTabDto>>(), CancellationToken.None), Times.Never);

            // Case 2: Session open with EngineeringModelSetup and Iteration tabs
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            var engModelSetup = new EngineeringModelSetup();
            engModelSetup.IterationSetup.Add(iteration.IterationSetup);
            var engModelTab = new TabbedApplicationInformation(new Mock<IApplicationBaseViewModel>().Object, typeof(EngineeringModelBody), engModelSetup);

            this.viewModel.SidePanel.OpenTabs.Add(engModelTab);
            await Task.Yield();

            this.sessionStorageService.Verify(x => x.SetItemAsync(WebAppConstantValues.SavedTabsKey, It.IsAny<List<SavedTabDto>>(), CancellationToken.None), Times.AtLeastOnce);
        }

        [Test]
        public async Task VerifySessionClosed()
        {
            this.viewModel.MainPanel.OpenTabs.Add(new TabbedApplicationInformation(new Mock<IEngineeringModelBodyViewModel>().Object, typeof(EngineeringModelBody), this.openIterations.Items[0]));
            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));

            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.Closed));
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(0));
                this.sessionStorageService.Verify(x => x.RemoveItemAsync(WebAppConstantValues.SavedTabsKey, CancellationToken.None), Times.AtLeastOnce);
            }
        }

        [Test]
        public void VerifyTabCreation()
        {
            var engineeringModelApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.CreateNewTab(engineeringModelApplication, Guid.Empty, this.viewModel.MainPanel);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
                Assert.That(this.viewModel.MainPanel.CurrentTab.ObjectOfInterest, Is.TypeOf<Iteration>());
                Assert.That(this.viewModel.SelectedApplication, Is.Not.Null);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
            }

            var bookEditorApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.BookEditorPage);
            this.viewModel.CreateNewTab(bookEditorApplication, Guid.Empty, this.viewModel.MainPanel);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab.ObjectOfInterest, Is.TypeOf<EngineeringModel>());
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void VerifyTabRemoval()
        {
            var engineeringModelApplication1 = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            var engineeringModelApplication2 = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.CreateNewTab(engineeringModelApplication1, Guid.Empty, this.viewModel.MainPanel);
            this.viewModel.CreateNewTab(engineeringModelApplication2, Guid.Empty, this.viewModel.MainPanel);

            var removedTab = this.viewModel.MainPanel.OpenTabs.Items[1];

            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.EqualTo(removedTab));
            this.viewModel.MainPanel.OpenTabs.Remove(removedTab);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.EqualTo(removedTab));
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
            }

            this.viewModel.CreateNewTab(engineeringModelApplication2, Guid.Empty, this.viewModel.MainPanel);
            this.viewModel.CreateNewTab(engineeringModelApplication2, Guid.Empty, this.viewModel.MainPanel);
            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(3));

            this.viewModel.MainPanel.OpenTabs.RemoveRange(1, 2);
            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyTabsOnSessionChanges()
        {
            var engineeringModelApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            var iteration = new Iteration();

            this.viewModel.CreateNewTab(engineeringModelApplication, iteration.Iid, this.viewModel.MainPanel);
            this.openIterations.Add(iteration);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
            }

            this.openIterations.Clear();
            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Null);
        }

        [Test]
        public void VerifyViewModelProperties()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.AvailableApplications, Is.Not.Empty);
                Assert.That(this.viewModel.SelectedApplication, Is.Null);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(0));
                Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);
                Assert.That(this.viewModel.SwitchDomainViewModel, Is.Not.Null);
            }
        }
    }
}
