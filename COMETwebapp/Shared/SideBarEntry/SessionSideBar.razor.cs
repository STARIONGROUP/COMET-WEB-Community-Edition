// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionSideBar.razor.cs" company="Starion Group S.A.">
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
    using CDP4Dal;

    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Menu entry to access to the <see cref="ISession" /> content
    /// </summary>
    public partial class SessionSideBar
    {
        /// <summary>
        /// The <see cref="ISessionMenuViewModel" />
        /// </summary>
        [Inject]
        public ISessionMenuViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// The display text of the refresh button
        /// </summary>
        public string RefreshButtonText => this.IsRefreshing ? "Refreshing" : "Refresh";

        /// <summary>
        /// Value indicating if the menu is expanded or not
        /// </summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// Gets or sets the value to check if the session is being refresh
        /// </summary>
        public bool IsRefreshing { get; private set; }

        /// <summary>
        /// Logs out to the current <see cref="ISession" />
        /// </summary>
        public void Logout()
        {
            this.Expanded = false;
            this.NavigationManager.NavigateTo("/Logout");
        }

        /// <summary>
        /// Method executed everytime the refresh button is clicked
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnRefreshClick()
        {
            this.IsRefreshing = true;
            await this.ViewModel.RefreshSession();
            this.IsRefreshing = false;
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.NotificationService.NotificationCount)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Expands the dropdown present in the navbar
        /// </summary>
        private void ExpandDropdown()
        {
            this.Expanded = true;
            this.InvokeAsync(this.StateHasChanged);
        }
    }
}
