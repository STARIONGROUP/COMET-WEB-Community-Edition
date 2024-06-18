// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Tabs.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="Tabs" /> page
    /// </summary>
    public partial class Tabs
    {
        /// <summary>
        /// Gets or sets the injected <see cref="ITabsViewModel" />
        /// </summary>
        [Inject]
        public ITabsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the open tab component visibility
        /// </summary>
        public bool IsOpenTabVisible { get; private set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.SelectedApplication,
                    x => x.ViewModel.CurrentTab)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.ViewModel.OpenTabs.CountChanged.SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method executed when a tab is clicked
        /// </summary>
        /// <param name="tabbedApplicationInformation">The tab to be set</param>
        private void OnTabClick(TabbedApplicationInformation tabbedApplicationInformation)
        {
            this.ViewModel.CurrentTab = tabbedApplicationInformation;
        }

        /// <summary>
        /// Method executed when the remove tab button is clicked
        /// </summary>
        /// <param name="tabbedApplicationInformation">The tab to be removed</param>
        private void OnRemoveTabClick(TabbedApplicationInformation tabbedApplicationInformation)
        {
            this.ViewModel.OpenTabs.Remove(tabbedApplicationInformation);
        }

        /// <summary>
        /// Sets the open tab popup visibility
        /// </summary>
        /// <param name="visibility">The visibility to be set</param>
        private void SetOpenTabVisibility(bool visibility)
        {
            this.IsOpenTabVisible = visibility;
            this.InvokeAsync(this.StateHasChanged);
        }

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
                CDP4Common.EngineeringModelData.EngineeringModel engineeringModel => engineeringModel.EngineeringModelSetup.Name,
                _ => string.Empty
            };
        }
    }
}
