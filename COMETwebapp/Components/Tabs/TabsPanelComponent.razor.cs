// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsPanelComponent.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Tabs
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;

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
        public ITabHandler Handler { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITabsViewModel" />
        /// </summary>
        [Parameter]
        public ITabsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the tabs to be displayed
        /// </summary>
        [Parameter]
        public List<TabbedApplicationInformation> Tabs { get; set; } = new();

        /// <summary>
        /// Gets or sets the method to be executed when the open tab button is clicked
        /// </summary>
        [Parameter]
        public Action OnOpenTabClick { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed when the remove tab button is clicked
        /// </summary>
        [Parameter]
        public EventCallback<TabbedApplicationInformation> OnRemoveTabClick { get; set; }

        /// <summary>
        /// Gets or sets the method to be executed when the tab is clicked
        /// </summary>
        [Parameter]
        public EventCallback<(TabbedApplicationInformation, ITabHandler)> OnTabClick { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if the side panel should be available
        /// </summary>
        [Parameter]
        public bool IsSidePanelAvailable { get; set; }

        /// <summary>
        /// Gets the tab text for the given object of interest
        /// </summary>
        /// <param name="objectOfInterest">The object of interest to get its tab text</param>
        /// <returns>The tab text</returns>
        private static string GetTabText(object objectOfInterest)
        {
            return objectOfInterest switch
            {
                Iteration iteration => iteration.QueryName(),
                EngineeringModel engineeringModel => engineeringModel.EngineeringModelSetup.Name,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Adds a new side panel to the tabs page
        /// </summary>
        private void AddNewSidePanel()
        {
            var currentTab = this.ViewModel.CurrentTab;

            var newPanel = new TabPanelInformation
            {
                CurrentTab = currentTab
            };

            currentTab.Panel = newPanel;
            this.ViewModel.SidePanels.Add(newPanel);
            this.ViewModel.CurrentTab = this.ViewModel.OpenTabs.Items.LastOrDefault(x => x.ComponentType == this.ViewModel.SelectedApplication.ComponentType && x.Panel == null);
        }
    }
}
