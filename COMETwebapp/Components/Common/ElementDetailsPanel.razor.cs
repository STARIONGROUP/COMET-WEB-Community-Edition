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
    using COMET.Web.Common.Services.SessionManagement;

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
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. Create and delete controls bind their enabled state to
        /// the inverse of this, so the data can still be inspected but never modified
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Gets or sets the <see cref="IElementDetailsPanelViewModel" /> that drives this panel.
        /// </summary>
        [Parameter]
        public IElementDetailsPanelViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets optional content rendered on the element summary header row, next to the edit/delete
        /// buttons (used by the Model Editor to place the panel-collapse chevron on the same line).
        /// </summary>
        [Parameter]
        public RenderFragment DetailsHeaderActions { get; set; }

        /// <summary>
        /// Gets or sets the current search term used to filter the parameter cards rendered by the
        /// <see cref="COMETwebapp.Components.ModelEditor.DetailsPanelEditor" />. Held here so the search box can share the action-bar row.
        /// </summary>
        private string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "New" dropdown menu is currently open.
        /// </summary>
        private bool IsNewMenuOpen { get; set; }

        /// <summary>
        /// The unique DOM id for the "New" dropdown trigger button, generated once at field initialization
        /// so it is stable and immutable for the component's lifetime.
        /// </summary>
        private readonly string uniqueNewButtonId = $"element-details-new-button-{Guid.NewGuid():N}";

        /// <summary>
        /// Gets the CSS selector that targets the "New" dropdown trigger button by its unique DOM id,
        /// used as the dropdown's <c>PositionTarget</c>.
        /// </summary>
        private string NewButtonSelector => $"#{this.uniqueNewButtonId}";

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters
        /// from its parent in the render tree. Subscribes to the three mode flags that control popup
        /// visibility so the component re-renders when they change.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override Task OnInitializedAsync()
        {
            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.IsOnCreationMode,
                    x => x.ViewModel.IsOnAddingParameterMode,
                    x => x.ViewModel.IsOnEditMode,
                    x => x.ViewModel.IsOnEditParameterMode,
                    x => x.ViewModel.IsOnEditSubscriptionMode,
                    x => x.ViewModel.IsOnParameterGroupEditMode,
                    x => x.ViewModel.SelectedElement)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Closes the "New" dropdown menu and invokes the supplied action. Bound to each item of the
        /// dropdown so that choosing an item both triggers its popup and dismisses the menu.
        /// </summary>
        /// <param name="action">The action to invoke, e.g. one of the <c>Open*Popup</c> methods on <see cref="ViewModel" />.</param>
        private void OnNewMenuItemClicked(Action action)
        {
            this.IsNewMenuOpen = false;
            action.Invoke();
        }
    }
}
