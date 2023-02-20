// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscribedTable.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Component that display information related to <see cref="ParameterSubscription" /> owned by the current
    /// <see cref="DomainOfExpertise" />
    /// </summary>
    public partial class SubscribedTable
    {
        /// <summary>
        /// A reference to the <see cref="DxPopup" /> used for the <see cref="SubscribedParameterEvolution" />
        /// </summary>
        private DxPopup parameterEvolutionPopup;

        /// <summary>
        /// The <see cref="ParameterSubscriptionRowViewModel" /> used for the <see cref="ParameterSubscriptionRowViewModel" />
        /// </summary>
        private ParameterSubscriptionRowViewModel parameterSubscriptionRow;

        /// <summary>
        /// The <see cref="ISubscribedTableViewModel" />
        /// </summary>
        [Parameter]
        public ISubscribedTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.Disposables.Add(this.ViewModel.Rows.CountChanged.Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.DidSubscriptionsChanged)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Shows the evolution of a <see cref="ParameterSubscriptionValueSet" /> inside a chart
        /// </summary>
        /// <param name="valueSet">The interested <see cref="ParameterSubscriptionRowViewModel" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ShowEvolution(ParameterSubscriptionRowViewModel valueSet)
        {
            this.parameterSubscriptionRow = valueSet;
            await this.parameterEvolutionPopup.ShowAsync();
            await this.InvokeAsync(this.StateHasChanged);
        }
    }
}
