﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSubscriptionTable.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Common.Rows;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that display information related to owned <see cref="ParameterOrOverrideBase" /> where other domains placed
    /// <see cref="ParameterSubscription" />
    /// </summary>
    public partial class DomainOfExpertiseSubscriptionTable
    {
        /// <summary>
        /// The <see cref="IDomainOfExpertiseSubscriptionTableViewModel" />
        /// </summary>
        [Parameter]
        public IDomainOfExpertiseSubscriptionTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{TValue}" /> to call when the user click on a
        /// <see cref="ParameterOrOverrideBase" /> that has a missing value
        /// </summary>
        [Parameter]
        public EventCallback<ParameterOrOverrideBase> OnMissingValueClick { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.Disposables.Add(this.ViewModel.Rows.CountChanged.Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Handles the click on the row inside the gird
        /// </summary>
        /// <param name="clickEvent">The <see cref="GridRowClickEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnRowClick(GridRowClickEventArgs clickEvent)
        {
            if (clickEvent.Grid.GetDataItem(clickEvent.VisibleIndex) is OwnedParameterOrOverrideBaseRowViewModel { HasMissingValues: true } ownedParameter)
            {
                await this.InvokeAsync(() => this.OnMissingValueClick.InvokeAsync(ownedParameter.Parameter));
            }
        }
    }
}
