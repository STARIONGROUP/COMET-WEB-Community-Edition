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
    using COMET.Web.Common.Services.SessionManagement;

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
        /// Gets or sets the selected side panel
        /// </summary>
        private TabPanelInformation SelectedSidePanel { get; set; }

        /// <summary>
        /// Collection of open tabs that belong from view model sourcelist
        /// </summary>
        private IEnumerable<TabbedApplicationInformation> OpenTabs => this.ViewModel.OpenTabs.Items;

        /// <summary>
        /// The model id to fill the opentab form, if needed
        /// </summary>
        private Guid ModelId { get; set; }

        /// <summary>
        /// The iteration id to fill the opentab form, if needed
        /// </summary>
        private Guid IterationId { get; set; }

        /// <summary>
        /// The domain id to fill the opentab form, if needed
        /// </summary>
        private Guid DomainId { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="ITabsViewModel" />
        /// </summary>
        [Inject]
        public ITabsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="ISessionService" />
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

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
        /// <param name="tabHandler">The tab handler to handle the tab click</param>
        private static void OnTabClick(TabbedApplicationInformation tabbedApplicationInformation, ITabHandler tabHandler)
        {
            tabHandler.CurrentTab = tabbedApplicationInformation;
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
        /// Method executed when the open tab button is clicked
        /// </summary>
        /// <param name="sidePanel">The side panel to be set, if any</param>
        private void OnOpenTabClick(TabPanelInformation sidePanel = null)
        {
            this.SelectedSidePanel = sidePanel;
            this.SetOpenTabVisibility(true);
        }

        /// <summary>
        /// Resets the preset id's used to fill the open tab form
        /// </summary>
        private void ResetOpenTabPopup()
        {
            this.DomainId = Guid.Empty;
            this.IterationId = Guid.Empty;
            this.ModelId = Guid.Empty;
        }

        /// <summary>
        /// Method executed when the button to open a new view of the selected model is clicked
        /// </summary>
        /// <param name="tabbedApplicationInformation">The selected tab that contains the model of interest</param>
        private void OnCreateTabForModel(TabbedApplicationInformation tabbedApplicationInformation)
        {
            var iterationOfInterest = tabbedApplicationInformation.ObjectOfInterest switch
            {
                Iteration iteration => iteration,
                CDP4Common.EngineeringModelData.EngineeringModel model => model.Iteration.First(x => x.IterationSetup.FrozenOn == null),
                _ => null
            };

            if (iterationOfInterest == null)
            {
                return;
            }

            this.IterationId = iterationOfInterest.Iid;
            this.ModelId = ((CDP4Common.EngineeringModelData.EngineeringModel)iterationOfInterest.Container).Iid;
            this.DomainId = this.SessionService.GetDomainOfExpertise(iterationOfInterest).Iid;
            this.SetOpenTabVisibility(true);
        }
    }
}
