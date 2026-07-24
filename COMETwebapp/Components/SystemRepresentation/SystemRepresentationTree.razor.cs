// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationTree.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SystemRepresentation
{
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Partial class that represents the <see cref="SystemRepresentationTree" />
    /// </summary>
    public partial class SystemRepresentationTree
    {
        /// <summary>
        /// Gets or sets the <see cref="IProductTreeViewModel{T}" />
        /// </summary>
        [Parameter]
        public SystemRepresentationTreeViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="IJSRuntime" /> used to wire drag auto-scroll onto the tree container.
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Whether the "View" display-options dropdown is open.
        /// </summary>
        private bool viewMenuOpen;

        /// <summary>
        /// Wires the drag auto-scroll behaviour onto the tree's scroll container on first render, so a node dragged
        /// towards the top or bottom edge scrolls out-of-view rows into reach. Tolerates the JS interop being
        /// unavailable during pre-rendering or in tests.
        /// </summary>
        /// <param name="firstRender"><see langword="true" /> on the first render cycle.</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
            {
                return;
            }

            try
            {
                await this.JsRuntime.InvokeVoidAsync("cometDragScroll.init", "product-tree-nodes-section");
            }
            catch (Exception)
            {
                // JS interop failures during pre-rendering or test environments are non-fatal.
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.RootViewModel,
                    x => x.ViewModel.SelectedFilter,
                    x => x.ViewModel.SearchText)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.ShowName,
                    x => x.ViewModel.ShowOwner,
                    x => x.ViewModel.ShowCategories)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
