// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionMenu.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Shared.TopMenuEntry
{
    using CDP4Dal;

    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Menu entry to access to the <see cref="ISession" /> content
    /// </summary>
    public partial class SessionMenu
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
        /// <returns>A <see cref="Task"/></returns>
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
    }
}
