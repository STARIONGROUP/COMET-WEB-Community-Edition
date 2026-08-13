// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Pages
{
    using System.Reactive.Linq;

    using Blazored.SessionStorage;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model;
    using COMETwebapp.Utilities;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="TabsViewModel" /> contains logic and behavior that are required to support multi-tabs application
    /// </summary>
    public class TabsViewModel : DisposableObject, ITabsViewModel
    {
        /// <summary>
        /// Gets the injected <see cref="IServiceProvider" />
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Gets the injected <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Gets the injected <see cref="ISessionStorageService" />
        /// </summary>
        private readonly ISessionStorageService sessionStorageService;

        /// <summary>
        /// Gets the injected <see cref="ILogger{TabsViewModel}" />
        /// </summary>
        private readonly ILogger<TabsViewModel> logger;

        /// <summary>
        /// The currently selected <see cref="Iteration" /> for domain switching
        /// </summary>
        private Iteration selectedDomainSwitchIteration;

        /// <summary>
        /// Backing field for <see cref="IsOnSwitchDomainMode" />
        /// </summary>
        private bool isOnSwitchDomainMode;

        /// <summary>
        /// Indicates whether tab restoration is currently in progress
        /// </summary>
        private bool isRestoringSavedTabs;

        /// <summary>
        /// Indicates whether the saved tabs check has already been performed in the current session
        /// </summary>
        private bool hasCheckedForSavedTabs;

        /// <summary>
        /// Backing field for <see cref="SelectedApplication" />
        /// </summary>
        private TabbedApplication selectedApplication;

        /// <summary>
        /// Initializes a new instance of <see cref="TabsViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider" /></param>
        /// <param name="sessionStorageService">The <see cref="ISessionStorageService"/></param>
        /// <param name="logger">The <see cref="ILogger{TabsViewModel}"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public TabsViewModel(ISessionService sessionService, IServiceProvider serviceProvider, ISessionStorageService sessionStorageService, ILogger<TabsViewModel> logger, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;
            this.serviceProvider = serviceProvider;
            this.sessionStorageService = sessionStorageService;
            this.logger = logger;

            var eventCallbackFactory = new EventCallbackFactory();

            this.SwitchDomainViewModel = new SwitchDomainViewModel
            {
                OnSubmit = eventCallbackFactory.Create<DomainOfExpertise>(this, this.SwitchDomain),
                OnCancel = eventCallbackFactory.Create(this, () => this.IsOnSwitchDomainMode = false)
            };

            this.RestoreTabsPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Restore previous tabs",
                ShowCloseButton = false,
                OnConfirm = eventCallbackFactory.Create(this, this.RestoreSavedTabsAsync),
                OnCancel = eventCallbackFactory.Create(this, this.DiscardSavedTabsAsync)
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedApplication).Subscribe(_ => this.OnSelectedApplicationChange()));
            this.Disposables.Add(this.WhenAnyValue(x => x.MainPanel.CurrentTab).Subscribe(_ => this.OnCurrentTabChange(this.MainPanel)));
            this.Disposables.Add(this.WhenAnyValue(x => x.SidePanel.CurrentTab).Subscribe(_ => this.OnCurrentTabChange(this.SidePanel)));
            this.Disposables.Add(this.sessionService.OpenIterations.CountChanged.Subscribe(this.CloseTabIfIterationClosed));
            this.Disposables.Add(this.MainPanel.OpenTabs.Connect().SubscribeAsync(_ => this.SaveOpenTabsToSessionStorageAsync()));
            this.Disposables.Add(this.SidePanel.OpenTabs.Connect().SubscribeAsync(_ => this.SaveOpenTabsToSessionStorageAsync()));
            this.Disposables.Add(messageBus.Listen<SessionEvent>().Where(x => x.Status == SessionStatus.Closed).SubscribeAsync(_ => this.OnSessionClosedAsync()));
        }

        /// <summary>
        /// Gets the collection of all <see cref="TabbedApplicationInformation" />
        /// </summary>
        private IEnumerable<TabbedApplicationInformation> OpenTabs => [.. this.MainPanel.OpenTabs.Items, .. this.SidePanel.OpenTabs.Items];

        /// <summary>
        /// Gets the side tab panel information
        /// </summary>
        public TabPanelInformation SidePanel { get; } = new();

        /// <summary>
        /// Gets the main tab panel information
        /// </summary>
        public TabPanelInformation MainPanel { get; } = new();

        /// <summary>
        /// Gets the collection of available <see cref="TabbedApplication" />
        /// </summary>
        public IEnumerable<TabbedApplication> AvailableApplications => Applications.ExistingApplications.OfType<TabbedApplication>();

        /// <summary>
        /// Gets or sets the current selected <see cref="TabbedApplication" />
        /// </summary>
        public TabbedApplication SelectedApplication
        {
            get => this.selectedApplication;
            set => this.RaiseAndSetIfChanged(ref this.selectedApplication, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the domain switch popup dialog is visible
        /// </summary>
        public bool IsOnSwitchDomainMode
        {
            get => this.isOnSwitchDomainMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnSwitchDomainMode, value);
        }

        /// <summary>
        /// Gets the <see cref="ISwitchDomainViewModel" /> for domain switching
        /// </summary>
        public ISwitchDomainViewModel SwitchDomainViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> for restoring previous tabs
        /// </summary>
        public IConfirmCancelPopupViewModel RestoreTabsPopupViewModel { get; }

        /// <summary>
        /// Creates a new tab and sets it to current
        /// </summary>
        /// <param name="application">The <see cref="TabbedApplication" /> for which the tab will be created</param>
        /// <param name="objectOfInterestId">
        /// The id of the object of interest, which can be an <see cref="Iteration" /> or an
        /// <see cref="EngineeringModel" />
        /// </param>
        /// <param name="panel">The panel to open the new tab in</param>
        public void CreateNewTab(TabbedApplication application, Guid objectOfInterestId, TabPanelInformation panel)
        {
            if (this.serviceProvider.GetService(application.ViewModelType) is not IApplicationBaseViewModel viewModel)
            {
                return;
            }

            viewModel.IsAllowedToDispose = false;
            object thingOfInterest = default;

            if (application.ThingTypeOfInterest == typeof(Iteration))
            {
                thingOfInterest = this.sessionService.OpenIterations.Items.FirstOrDefault(x => x.Iid == objectOfInterestId);
            }

            if (application.ThingTypeOfInterest == typeof(EngineeringModel))
            {
                thingOfInterest = this.sessionService.OpenEngineeringModels.FirstOrDefault(x => x.Iid == objectOfInterestId);
            }

            var tabToCreate = new TabbedApplicationInformation(viewModel, application.ComponentType, thingOfInterest);
            panel.OpenTabs.Add(tabToCreate);
            this.SelectedApplication = application;
            panel.CurrentTab = tabToCreate;
        }

        /// <summary>
        /// Method executed everytime the <see cref="SelectedApplication" /> changes
        /// </summary>
        private void OnSelectedApplicationChange()
        {
            if (this.SelectedApplication == null || this.MainPanel.CurrentTab?.ComponentType == this.SelectedApplication?.ComponentType)
            {
                return;
            }

            var mainPanelTabForCurrentApplication = this.MainPanel.OpenTabs.Items.FirstOrDefault(x => x.ComponentType == this.SelectedApplication.ComponentType);
            var sidePanelTabForCurrentApplication = this.SidePanel.OpenTabs.Items.FirstOrDefault(x => x.ComponentType == this.SelectedApplication.ComponentType);

            if (mainPanelTabForCurrentApplication != null)
            {
                this.MainPanel.CurrentTab = mainPanelTabForCurrentApplication;
            }

            if (sidePanelTabForCurrentApplication != null)
            {
                this.SidePanel.CurrentTab = sidePanelTabForCurrentApplication;
            }

            if (sidePanelTabForCurrentApplication == null && mainPanelTabForCurrentApplication == null)
            {
                this.MainPanel.CurrentTab = null;
            }
        }

        /// <summary>
        /// Method executed everytime the <see cref="TabPanelInformation.CurrentTab" /> changes
        /// </summary>
        /// <param name="panel">The panel where the current tab changed</param>
        private void OnCurrentTabChange(TabPanelInformation panel)
        {
            if (panel.CurrentTab == null)
            {
                return;
            }

            this.SelectedApplication = Applications.ExistingApplications.OfType<TabbedApplication>().FirstOrDefault(x => x.ComponentType == panel.CurrentTab.ComponentType);
        }

        /// <summary>
        /// Closes a tab if its iteration has been closed
        /// </summary>
        /// <param name="numberOfIterations">The new number of open iterations</param>
        private void CloseTabIfIterationClosed(int numberOfIterations)
        {
            var iterationTabsToClose = this.OpenTabs
                .Where(x => x.ObjectOfInterest is Iteration && !this.sessionService.OpenIterations.Items.Contains(x.ObjectOfInterest))
                .ToList();

            var engineeringModelTabsToClose = this.OpenTabs
                .Where(x => x.ObjectOfInterest is EngineeringModel && !this.sessionService.OpenEngineeringModels.Contains(x.ObjectOfInterest))
                .ToList();

            List<TabbedApplicationInformation> thingTabsToClose = [.. iterationTabsToClose, .. engineeringModelTabsToClose];
            this.MainPanel.CloseTabs(thingTabsToClose);
            this.SidePanel.CloseTabs(thingTabsToClose);
        }

        /// <summary>
        /// Gets the active <see cref="DomainOfExpertise" /> for the given tab panel's current tab
        /// </summary>
        /// <param name="panel">The <see cref="TabPanelInformation" /></param>
        /// <returns>The active <see cref="DomainOfExpertise" />, or null if none</returns>
        public DomainOfExpertise GetCurrentDomainOfExpertise(TabPanelInformation panel)
        {
            var iteration = GetIterationFromTab(panel.CurrentTab);

            return iteration != null 
                ? this.sessionService.GetDomainOfExpertise(iteration) 
                : null;
        }

        /// <summary>
        /// Opens the domain switch dialog for the given tab panel's current tab
        /// </summary>
        /// <param name="panel">The <see cref="TabPanelInformation" /></param>
        public void AskToSwitchDomain(TabPanelInformation panel)
        {
            var iteration = GetIterationFromTab(panel.CurrentTab);

            if (iteration == null)
            {
                return;
            }

            this.selectedDomainSwitchIteration = iteration;
            this.SwitchDomainViewModel.AvailableDomains = this.sessionService.GetModelDomains((EngineeringModelSetup)iteration.IterationSetup.Container);
            this.SwitchDomainViewModel.SelectedDomainOfExpertise = this.sessionService.GetDomainOfExpertise(iteration);
            this.IsOnSwitchDomainMode = true;
        }

        /// <summary>
        /// Checks if there are saved tabs in session storage and prompts the user to restore them
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        public async Task CheckAndRestoreSavedTabsAsync()
        {
            if (this.isRestoringSavedTabs || this.hasCheckedForSavedTabs)
            {
                return;
            }

            this.hasCheckedForSavedTabs = true;

            var savedTabs = await this.sessionStorageService.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey);

            if (savedTabs is { Count: > 0 })
            {
                this.RestoreTabsPopupViewModel.ContentText = $"Would you like to restore your {savedTabs.Count} previous tab{(savedTabs.Count > 1 ? "s" : string.Empty)}?";
                this.RestoreTabsPopupViewModel.IsVisible = true;
            }
        }

        /// <summary>
        /// Switches the <see cref="DomainOfExpertise" /> for the selected iteration
        /// </summary>
        /// <param name="domainOfExpertise">The selected <see cref="DomainOfExpertise" /></param>
        private void SwitchDomain(DomainOfExpertise domainOfExpertise)
        {
            this.sessionService.SwitchDomain(this.selectedDomainSwitchIteration, domainOfExpertise);
            this.IsOnSwitchDomainMode = false;
        }

        /// <summary>
        /// Gets the active <see cref="Iteration" /> from a <see cref="TabbedApplicationInformation" />
        /// </summary>
        /// <param name="tab">The <see cref="TabbedApplicationInformation" /></param>
        /// <returns>The active <see cref="Iteration" />, or null</returns>
        private static Iteration GetIterationFromTab(TabbedApplicationInformation tab)
        {
            return tab?.ObjectOfInterest switch
            {
                Iteration iteration => iteration,
                EngineeringModel engineeringModel => engineeringModel.Iteration.FirstOrDefault(x => x.IterationSetup.FrozenOn == null),
                _ => null
            };
        }

        /// <summary>
        /// Saves all open tabs from MainPanel and SidePanel to session storage
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task SaveOpenTabsToSessionStorageAsync()
        {
            if (this.isRestoringSavedTabs || !this.sessionService.IsSessionOpen)
            {
                return;
            }

            var savedTabs = new List<SavedTabDto>();

            foreach (var tab in this.OpenTabs.Distinct())
            {
                var app = this.AvailableApplications.FirstOrDefault(x => x.ComponentType == tab.ComponentType);

                if (app == null)
                {
                    continue;
                }

                var objectOfInterestId = (tab.ObjectOfInterest as Thing)?.Iid ?? Guid.Empty;
                var isSidePanel = this.SidePanel.OpenTabs.Items.Contains(tab);

                var iterationSetupIid = tab.ObjectOfInterest switch
                {
                    Iteration iteration => iteration.IterationSetup.Iid,
                    EngineeringModelSetup engineeringModelSetup => engineeringModelSetup.IterationSetup.Find(x => x.FrozenOn == null)?.Iid ?? Guid.Empty,
                    _ => Guid.Empty
                };

                var iterationOfIterationSetup = this.sessionService.OpenIterations.Items.FirstOrDefault(x => x.IterationSetup.Iid == iterationSetupIid);
                var domainId = iterationOfIterationSetup != null ? this.sessionService.GetDomainOfExpertise(iterationOfIterationSetup)?.Iid ?? Guid.Empty : Guid.Empty;

                savedTabs.Add(new SavedTabDto
                {
                    ObjectOfInterestId = objectOfInterestId,
                    ApplicationName = app.Name,
                    IterationSetupId = iterationSetupIid,
                    DomainId = domainId,
                    IsSidePanel = isSidePanel
                });
            }

            await this.sessionStorageService.SetItemAsync(WebAppConstantValues.SavedTabsKey, savedTabs);
        }

        /// <summary>
        /// Restores open tabs saved in session storage
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task RestoreSavedTabsAsync()
        {
            if (this.isRestoringSavedTabs)
            {
                return;
            }

            this.isRestoringSavedTabs = true;
           
            try
            {
                var savedTabs = await this.sessionStorageService.GetItemAsync<List<SavedTabDto>>(WebAppConstantValues.SavedTabsKey);
                await this.sessionStorageService.RemoveItemAsync(WebAppConstantValues.SavedTabsKey);

                if (savedTabs is { Count: > 0 })
                {
                    foreach (var savedTab in savedTabs)
                    {
                        var app = this.AvailableApplications.FirstOrDefault(x => x.Name == savedTab.ApplicationName);

                        if (app == null)
                        {
                            continue;
                        }

                        var targetPanel = savedTab.IsSidePanel ? this.SidePanel : this.MainPanel;

                        await this.OpenThingOfInterest(savedTab.IterationSetupId, savedTab.DomainId);
                        this.CreateNewTab(app, savedTab.ObjectOfInterestId, targetPanel);
                    }
                }
                
                await this.SaveOpenTabsToSessionStorageAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while restoring saved tabs from session storage.");
            }
            finally
            {
                this.isRestoringSavedTabs = false;
                this.RestoreTabsPopupViewModel.IsVisible = false;
            }
        }

        /// <summary>
        /// Opens the thing of interest (Iteration or EngineeringModel) based on its ID
        /// </summary>
        /// <param name="iterationSetupIid">The ID of the iteration setup to open</param>
        /// <param name="domainId">The ID of the domain to select</param>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task OpenThingOfInterest(Guid iterationSetupIid, Guid domainId)
        {
            if (iterationSetupIid == Guid.Empty)
            {
                return;
            }

            var isIterationAlreadyOpen = this.sessionService.OpenIterations.Items.Any(x => x.IterationSetup.Iid == iterationSetupIid);

            if (isIterationAlreadyOpen)
            {
                return;
            }

            var iterationSetup = this.sessionService
                .GetSiteDirectory()
                .Model
                .SelectMany(x => x.IterationSetup)
                .FirstOrDefault(x => x.Iid == iterationSetupIid);

            if (iterationSetup == null)
            {
                return;
            }

            var modelSetup = (EngineeringModelSetup)iterationSetup.Container;
            var allDomains = this.sessionService.GetAvailableDomains(modelSetup);

            var domainToUse = allDomains.FirstOrDefault(x => x.Iid == domainId) 
                               ?? modelSetup.ActiveDomain.Find(x => x == this.sessionService.Session.ActivePerson.DefaultDomain)
                               ?? modelSetup.ActiveDomain.FirstOrDefault();

            if (domainToUse == null)
            {
                return;
            }

            await this.sessionService.ReadIteration(iterationSetup, domainToUse);
        }

        /// <summary>
        /// Discards saved tabs in session storage
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task DiscardSavedTabsAsync()
        {
            this.RestoreTabsPopupViewModel.IsVisible = false;
            await this.sessionStorageService.RemoveItemAsync(WebAppConstantValues.SavedTabsKey);
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.Closed" /> message by closing all open tabs and clearing saved tabs from session storage
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task OnSessionClosedAsync()
        {
            this.MainPanel.OpenTabs.Clear();
            this.SidePanel.OpenTabs.Clear();
            await this.sessionStorageService.RemoveItemAsync(WebAppConstantValues.SavedTabsKey);
        }
    }
}
