// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Pages
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Pages;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DynamicData;

    using Blazored.SessionStorage;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabsViewModelTestFixture
    {
        private TabsViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IServiceProvider> serviceProvider;
        private Mock<ISessionStorageService> sessionStorageService;
        private SourceList<Iteration> openIterations;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = new Mock<IServiceProvider>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionStorageService = new Mock<ISessionStorageService>();
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

            this.viewModel = new TabsViewModel(this.sessionService.Object, this.serviceProvider.Object, this.sessionStorageService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
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
        public void VerifyTabCreation()
        {
            var engineeringModelApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.CreateNewTab(engineeringModelApplication, Guid.Empty, this.viewModel.MainPanel);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
                Assert.That(this.viewModel.MainPanel.CurrentTab.ObjectOfInterest, Is.TypeOf<Iteration>());
                Assert.That(this.viewModel.SelectedApplication, Is.Not.Null);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
            });

            var bookEditorApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.BookEditorPage);
            this.viewModel.CreateNewTab(bookEditorApplication, Guid.Empty, this.viewModel.MainPanel);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab.ObjectOfInterest, Is.TypeOf<EngineeringModel>());
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(2));
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.EqualTo(removedTab));
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
            });

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

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
            });

            this.openIterations.Clear();
            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Null);
        }

        [Test]
        public void VerifyViewModelProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableApplications, Is.Not.Empty);
                Assert.That(this.viewModel.SelectedApplication, Is.Null);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(0));
                Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);
                Assert.That(this.viewModel.SwitchDomainViewModel, Is.Not.Null);
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.True);
                Assert.That(this.viewModel.SwitchDomainViewModel.AvailableDomains, Has.Member(domain));
                Assert.That(this.viewModel.SwitchDomainViewModel.SelectedDomainOfExpertise, Is.EqualTo(domain));
            });

            await this.viewModel.SwitchDomainViewModel.OnSubmit.InvokeAsync(domain);
            this.sessionService.Verify(x => x.SwitchDomain(iteration, domain), Times.Once);
            Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);

            this.viewModel.AskToSwitchDomain(this.viewModel.MainPanel);
            Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.True);
            await this.viewModel.SwitchDomainViewModel.OnCancel.InvokeAsync();
            Assert.That(this.viewModel.IsOnSwitchDomainMode, Is.False);
        }

        [Test]
        public async Task VerifyCheckAndRestoreSavedTabsAsync()
        {
            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync((List<SavedTabDto>)null);

            await this.viewModel.CheckAndRestoreSavedTabsAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.RestoreTabsPopupViewModel, Is.Not.Null);
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.ShowCloseButton, Is.False);
            }

            var savedTabs = new List<SavedTabDto>
            {
                new() { ApplicationName = "EngineeringModelBody", ObjectOfInterestId = Guid.NewGuid(), IsSidePanel = false }
            };

            this.sessionStorageService.Setup(x => x.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey, CancellationToken.None))
                .ReturnsAsync(savedTabs);

            this.viewModel = new TabsViewModel(this.sessionService.Object, this.serviceProvider.Object, this.sessionStorageService.Object, this.messageBus);

            await this.viewModel.CheckAndRestoreSavedTabsAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.RestoreTabsPopupViewModel.ContentText, Does.Contain("restore your 1 previous tab"));
            }

            await this.viewModel.RestoreTabsPopupViewModel.OnCancel.InvokeAsync();
            Assert.That(this.viewModel.RestoreTabsPopupViewModel.IsVisible, Is.False);
        }

        [Test]
        public async Task VerifyRestoreSavedTabsAsync()
        {
            var iterationId = Guid.NewGuid();
            var iteration = new Iteration { Iid = iterationId };
            var iterationSetup = new IterationSetup();
            iteration.IterationSetup = iterationSetup;
            this.openIterations.Add(iteration);

            var savedTabs = new List<SavedTabDto>
            {
                new() { ApplicationName = "Engineering Model", ObjectOfInterestId = iterationId, IsSidePanel = false }
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
            }
        }

        [Test]
        public async Task VerifySessionClosed()
        {
            this.viewModel.MainPanel.OpenTabs.Add(new TabbedApplicationInformation(new Mock<IEngineeringModelBodyViewModel>().Object, typeof(EngineeringModelBody), this.openIterations.Items.First()));
            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));

            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.Closed));
            await Task.Delay(100);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(0));
                this.sessionStorageService.Verify(x => x.RemoveItemAsync(WebAppConstantValues.SavedTabsKey, CancellationToken.None), Times.AtLeastOnce);
            }
        }
    }
}
