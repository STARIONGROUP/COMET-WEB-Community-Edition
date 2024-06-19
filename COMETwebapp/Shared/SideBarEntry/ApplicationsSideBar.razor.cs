// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationsSideBar.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Shared.SideBarEntry
{
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.StringTableService;

    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Pages;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Routing;

    using ReactiveUI;

    /// <summary>
    /// Side bar entry to list the available <see cref="Application" />(s)
    /// </summary>
    public partial class ApplicationsSideBar
    {
        /// <summary>
        /// Gets or sets the <see cref="IRegistrationService" />
        /// </summary>
        [Inject]
        internal IRegistrationService RegistrationService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// The <see cref="IStringTableService" />
        /// </summary>
        [Inject]
        public IStringTableService ConfigurationService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITabsViewModel" />
        /// </summary>
        [Inject]
        public ITabsViewModel TabsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="Application" /> navigated
        /// </summary>
        private Application CurrentApplication { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.NavigationManager.LocationChanged += this.OnLocationChanged;
            this.Disposables.Add(this.WhenAnyValue(x => x.TabsViewModel.SelectedApplication).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.NavigationManager.LocationChanged -= this.OnLocationChanged;
        }

        /// <summary>
        /// Method executed everytime a navigation is done
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The <see cref="LocationChangedEventArgs" /></param>
        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            var currentUri = new Uri(e.Location);
            var pageName = currentUri.AbsolutePath.TrimStart('/');
            this.CurrentApplication = this.RegistrationService.RegisteredApplications.FirstOrDefault(x => x.Url == pageName);
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Navigates to the selected tabbed application
        /// </summary>
        /// <param name="application">The <see cref="TabbedApplication" /> to navigate to</param>
        private void NavigateToTabbedApplication(TabbedApplication application)
        {
            this.TabsViewModel.SelectedApplication = application;
            this.NavigationManager.NavigateTo(WebAppConstantValues.TabsPage);
        }

        /// <summary>
        /// Checks if the given application is the current one
        /// </summary>
        /// <param name="application">The <see cref="Application" /></param>
        /// <returns>The condition to check if is current application</returns>
        private bool IsCurrentApplication(Application application)
        {
            if (this.CurrentApplication?.Url == WebAppConstantValues.TabsPage || this.CurrentApplication == null)
            {
                return this.TabsViewModel.SelectedApplication == application;
            }

            return this.CurrentApplication == application;
        }

        /// <summary>
        /// Checks if the given application sidebar item should be enabled
        /// </summary>
        /// <param name="application">The <see cref="Application" /></param>
        /// <returns>The condition to check if is enabled</returns>
        private bool IsApplicationEnabled(Application application)
        {
            if (!this.AuthorizedMenuEntryViewModel.IsAuthenticated)
            {
                return false;
            }

            if (this.CurrentApplication?.Url == WebAppConstantValues.TabsPage)
            {
                return this.TabsViewModel.SelectedApplication != application;
            }

            return this.CurrentApplication != application;
        }
    }
}
