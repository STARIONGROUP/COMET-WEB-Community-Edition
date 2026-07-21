// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SourceConfiguration.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RelationshipMatrix
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.Components;

    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="SourceConfiguration" /> component, rendering the configuration of one
    /// Relationship Matrix axis (rows or columns).
    /// </summary>
    public partial class SourceConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="ISourceConfigurationViewModel" /> to render.
        /// </summary>
        [Parameter]
        public ISourceConfigurationViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the heading rendered above this axis' configuration.
        /// </summary>
        [Parameter]
        public string Title { get; set; }

        /// <summary>
        /// Gets the class-kind options as a nullable-typed list. The <see cref="DxComboBox{TData,TValue}" /> only
        /// two-way binds a nullable <see cref="ClassKind" />? value when its data items are the same nullable type;
        /// binding it to the non-nullable <see cref="ISourceConfigurationViewModel.PossibleClassKinds" /> silently
        /// drops the selection, leaving <see cref="ISourceConfigurationViewModel.SelectedClassKind" /> null.
        /// </summary>
        private IReadOnlyList<ClassKind?> ClassKindOptions { get; set; } = [];

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters. The
        /// subscription is registered here (once) rather than in <c>OnParametersSet</c>, which Blazor re-invokes
        /// on every parent render and would accumulate duplicate subscriptions; the <see cref="ViewModel" /> is
        /// assigned once by the parent and never reassigned.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.ClassKindOptions = this.ViewModel.PossibleClassKinds.Select(x => (ClassKind?)x).ToList();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.AvailableOwners,
                    x => x.ViewModel.SelectedOwners,
                    x => x.ViewModel.SelectedClassKind,
                    x => x.ViewModel.CategorySelector.SelectedCategories)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
