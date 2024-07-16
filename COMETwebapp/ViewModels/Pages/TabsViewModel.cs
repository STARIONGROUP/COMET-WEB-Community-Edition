// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model;

    using DynamicData;

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
        /// Backing field for <see cref="CurrentTab" />
        /// </summary>
        private TabbedApplicationInformation currentTab;

        /// <summary>
        /// Backing field for <see cref="SelectedApplication" />
        /// </summary>
        private TabbedApplication selectedApplication;

        /// <summary>
        /// Initializes a new instance of <see cref="TabsViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider" /></param>
        public TabsViewModel(ISessionService sessionService, IServiceProvider serviceProvider)
        {
            this.sessionService = sessionService;
            this.serviceProvider = serviceProvider;
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedApplication).Subscribe(_ => this.OnSelectedApplicationChange()));
            this.Disposables.Add(this.sessionService.OpenIterations.CountChanged.Subscribe(this.CloseTabIfIterationClosed));
            this.Disposables.Add(this.OpenTabs.Connect().WhereReasonsAre(ListChangeReason.Remove, ListChangeReason.RemoveRange).Subscribe(this.OnOpenTabRemoved));
        }

        /// <summary>
        /// Gets the collection of all <see cref="TabbedApplicationInformation" />
        /// </summary>
        public SourceList<TabbedApplicationInformation> OpenTabs { get; } = new();

        /// <summary>
        /// Gets the collection of all <see cref="TabPanelInformation" />s
        /// </summary>
        public SourceList<TabPanelInformation> SidePanels { get; } = new();

        /// <summary>
        /// Gets or sets the current tab
        /// </summary>
        public TabbedApplicationInformation CurrentTab
        {
            get => this.currentTab;
            set => this.RaiseAndSetIfChanged(ref this.currentTab, value);
        }

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
        /// Creates a new tab and sets it to current
        /// </summary>
        /// <param name="application">The <see cref="TabbedApplication" /> for which the tab will be created</param>
        /// <param name="objectOfInterestId">
        /// The id of the object of interest, which can be an <see cref="Iteration" /> or an
        /// <see cref="EngineeringModel" />
        /// </param>
        /// <param name="sidePanel">The panel to open the new tab in</param>
        public void CreateNewTab(TabbedApplication application, Guid objectOfInterestId, TabPanelInformation sidePanel = null)
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

            if (thingOfInterest != null)
            {
                this.OpenTabs.Add(tabToCreate);
            }

            this.SelectedApplication = application;

            if (sidePanel == null)
            {
                this.CurrentTab = tabToCreate;
            }
            else
            {
                sidePanel.CurrentTab = tabToCreate;
                tabToCreate.Panel = sidePanel;
            }
        }

        /// <summary>
        /// Method executed everytime the <see cref="SelectedApplication" /> changes
        /// </summary>
        private void OnSelectedApplicationChange()
        {
            if (this.SelectedApplication == null)
            {
                return;
            }

            this.CurrentTab = this.OpenTabs.Items.FirstOrDefault(x => x.ComponentType == this.SelectedApplication.ComponentType && x.Panel == null);
        }

        /// <summary>
        /// Closes a tab if its iteration has been closed
        /// </summary>
        /// <param name="numberOfIterations">The new number of open iterations</param>
        private void CloseTabIfIterationClosed(int numberOfIterations)
        {
            var iterationTabsToClose = this.OpenTabs.Items
                .Where(x => x.ObjectOfInterest is Iteration && !this.sessionService.OpenIterations.Items.Contains(x.ObjectOfInterest))
                .ToList();

            var engineeringModelTabsToClose = this.OpenTabs.Items
                .Where(x => x.ObjectOfInterest is EngineeringModel && !this.sessionService.OpenEngineeringModels.Contains(x.ObjectOfInterest))
                .ToList();

            this.OpenTabs.RemoveMany([.. iterationTabsToClose, .. engineeringModelTabsToClose]);

            if (numberOfIterations == 0)
            {
                this.CurrentTab = null;
            }
        }

        /// <summary>
        /// Method executed when one or more open tabs are removed
        /// </summary>
        /// <param name="changeSet">The change set containing the removed <see cref="TabbedApplicationInformation" /></param>
        private void OnOpenTabRemoved(IChangeSet<TabbedApplicationInformation> changeSet)
        {
            foreach (var result in changeSet.ToList())
            {
                if (result.Range.Count > 0)
                {
                    foreach (var tabToRemove in result.Range)
                    {
                        tabToRemove.ApplicationBaseViewModel.IsAllowedToDispose = true;
                    }
                }
                else
                {
                    result.Item.Current.ApplicationBaseViewModel.IsAllowedToDispose = true;
                }
            }

            this.SetCurrentTabAfterTabRemoval(changeSet, this);

            foreach (var panel in this.SidePanels.Items)
            {
                this.SetCurrentTabAfterTabRemoval(changeSet, panel);
            }
        }

        /// <summary>
        /// Sets the current tab in a <see cref="ITabHandler" /> after a tab removal, if needed
        /// </summary>
        /// <param name="changeSet">The change set to be used to check deletions</param>
        /// <param name="handler">The <see cref="ITabHandler" /> to set its current tab</param>
        private void SetCurrentTabAfterTabRemoval(IChangeSet<TabbedApplicationInformation> changeSet, ITabHandler handler)
        {
            var wasCurrentTabRemoved = changeSet
                .Select(x => x.Item.Current)
                .Contains(handler.CurrentTab);

            var selectedSidePanel = handler is TabPanelInformation ? handler : null;

            if (wasCurrentTabRemoved)
            {
                handler.CurrentTab = this.OpenTabs.Items.FirstOrDefault(x => x.ComponentType == this.SelectedApplication.ComponentType && x.Panel == selectedSidePanel);
            }

            if (selectedSidePanel != null && handler.CurrentTab == null)
            {
                this.SidePanels.Remove((TabPanelInformation)handler);
            }
        }
    }
}
