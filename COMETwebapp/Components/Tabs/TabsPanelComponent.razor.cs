// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsPanelComponent.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Tabs
{
    using System.Text;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Core component for the Tabs page
    /// </summary>
    public partial class TabsPanelComponent : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the custom css class to be used in the container component
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the tab handler to be used
        /// </summary>
        [Parameter]
        public TabPanelInformation Panel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITabsViewModel" />
        /// </summary>
        [Parameter]
        public ITabsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed when the open tab button is clicked
        /// </summary>
        [Parameter]
        public Action OnOpenTabClick { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed when the open new tab for the selected model button is clicked
        /// </summary>
        [Parameter]
        public EventCallback<TabbedApplicationInformation> OnCreateTabForModel { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed when the remove tab button is clicked
        /// </summary>
        [Parameter]
        public EventCallback<TabbedApplicationInformation> OnRemoveTabClick { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed when the tab is clicked
        /// </summary>
        [Parameter]
        public EventCallback<(TabbedApplicationInformation, TabPanelInformation)> OnTabClick { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the split view button should be visible
        /// </summary>
        [Parameter]
        public bool IsSplitViewVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the split view button should be enabled
        /// </summary>
        [Parameter]
        public bool IsSplitViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService" />
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Sorts the tabs by the means of drag and drop
        /// </summary>
        /// <param name="oldIndex">The dragged tab old index</param>
        /// <param name="newIndex">The dragged tab new index</param>
        /// <returns>A <see cref="Task" /></returns>
        private void SortTabs(int oldIndex, int newIndex)
        {
            this.Panel.OpenTabs.Move(oldIndex, newIndex);
        }

        /// <summary>
        /// Handles the logic to organize data when a tab is moved from one panel to another
        /// </summary>
        /// <param name="oldIndex">The dragged tab old panel index</param>
        /// <param name="newIndex">The dragged tab new panel index</param>
        private void OnMovedTab(int oldIndex, int newIndex)
        {
            var tab = this.Panel.OpenTabs.Items[oldIndex];

            if (this.Panel == this.ViewModel.MainPanel)
            {
                this.ViewModel.SidePanel.OpenTabs.Insert(newIndex, tab);
            }
            else
            {
                this.ViewModel.MainPanel.OpenTabs.Insert(newIndex, tab);
            }

            this.Panel.OpenTabs.Remove(tab);
        }

        /// <summary>
        /// Gets the tab text for the given object of interest
        /// </summary>
        /// <param name="tab">The tab to get its text</param>
        /// <returns>The tab text</returns>
        private static string GetTabText(TabbedApplicationInformation tab)
        {
            var applicationName = Applications.ExistingApplications.OfType<TabbedApplication>().First(x => x.ComponentType == tab.ComponentType).Name;

            return tab.ObjectOfInterest switch
            {
                Iteration iteration => $"{applicationName} · {((EngineeringModel)iteration.Container).EngineeringModelSetup.Name} - It. {iteration.IterationSetup.IterationNumber}",
                EngineeringModel engineeringModel => $"{applicationName} · {engineeringModel.EngineeringModelSetup.Name}",
                _ => applicationName
            };
        }

        /// <summary>
        /// Gets the tab caption text for the given object of interest
        /// </summary>
        /// <param name="objectOfInterest">The object of interest to get its tab caption text</param>
        /// <returns>The tab caption</returns>
        private string GetCaptionText(object objectOfInterest)
        {
            var modelName = new StringBuilder();
            Iteration iterationOfInterest = null;

            switch (objectOfInterest)
            {
                case Iteration iteration:
                    modelName.Append(((EngineeringModel)iteration.Container).EngineeringModelSetup.Name + " - " + iteration.IterationSetup.IterationNumber);
                    iterationOfInterest = iteration;
                    break;
                case EngineeringModel engineeringModel:
                    modelName.Append(engineeringModel.EngineeringModelSetup.Name);
                    iterationOfInterest = engineeringModel.Iteration.First(x => x.IterationSetup.FrozenOn == null);
                    break;
            }

            modelName.Append(" - ");

            if (iterationOfInterest == null)
            {
                return modelName.ToString();
            }

            var domainOfExpertiseShortName = this.SessionService.GetDomainOfExpertise(iterationOfInterest).ShortName;
            modelName.Append(domainOfExpertiseShortName);

            return modelName.ToString();
        }

        /// <summary>
        /// Adds a new side panel to the tabs page
        /// </summary>
        private void AddSidePanel()
        {
            var currentTab = this.ViewModel.MainPanel.CurrentTab;
            this.ViewModel.SidePanel.OpenTabs.Add(currentTab);
            this.ViewModel.SidePanel.CurrentTab = currentTab;
            this.ViewModel.MainPanel.OpenTabs.Remove(currentTab);
        }

        /// <summary>
        /// Gets the tooltip explanation text for the split view button
        /// </summary>
        /// <returns>The tooltip string</returns>
        private string GetSplitViewTooltip()
        {
            return this.IsSplitViewEnabled
                ? "Split View"
                : "Split view requires at least two open tabs";
        }
    }
}
