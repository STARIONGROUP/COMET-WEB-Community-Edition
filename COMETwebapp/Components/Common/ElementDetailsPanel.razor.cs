// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDetailsPanel.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Common
{
    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.Common;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Code-behind partial class for the <see cref="ElementDetailsPanel" /> component. Renders the
    /// editable element details panel — the add-buttons row, the <c>DetailsPanelEditor</c> card, and all
    /// associated CRUD popups — bound to an <see cref="IElementDetailsPanelViewModel" />.
    /// </summary>
    public partial class ElementDetailsPanel
    {
        /// <summary>
        /// Gets or sets the <see cref="IElementDetailsPanelViewModel" /> that drives this panel.
        /// </summary>
        [Parameter]
        public IElementDetailsPanelViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters
        /// from its parent in the render tree. Subscribes to the three mode flags that control popup
        /// visibility so the component re-renders when they change.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override Task OnInitializedAsync()
        {
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnCreationMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnAddingParameterMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnEditMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnEditParameterMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnEditSubscriptionMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnParameterGroupEditMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedElement)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            return base.OnInitializedAsync();
        }
    }
}
