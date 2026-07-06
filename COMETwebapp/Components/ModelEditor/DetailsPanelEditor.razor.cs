// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsPanelEditor.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    ///     Partial class for the component <see cref="DetailsPanelEditor" />. Renders the element summary
    ///     card at the top, then a collapsible grouped layout of parameter cards — one
    ///     <see cref="Components.Common.ParameterGroupSection" /> per top-level <see cref="ParameterGroup" />
    ///     (which recursively renders nested sub-groups) followed by a trailing "Ungrouped" section.
    ///     Parameters can be dragged from a card and dropped onto a section header to reassign group
    ///     membership. Parameter groups can be dragged by their header and dropped onto another section to
    ///     nest them, or onto the Ungrouped section to move them to the top level. A search box at the top
    ///     filters cards by parameter type name or short name.
    /// </summary>
    public partial class DetailsPanelEditor : IAsyncDisposable
    {
        /// <summary>
        ///     The <see cref="IElementDefinitionDetailsViewModel" /> for the component.
        /// </summary>
        [Parameter]
        public IElementDefinitionDetailsViewModel ViewModel { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the per-card delete affordance for a
        ///     <see cref="Parameter" />. When unset, the delete affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<Parameter> OnDeleteParameter { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the per-card subscribe affordance for a
        ///     <see cref="Parameter" /> not owned by the currently logged-in
        ///     <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" /> and not yet subscribed to by it.
        ///     When unset, the subscribe affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<Parameter> OnCreateSubscription { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the per-card unsubscribe affordance to remove
        ///     an existing <see cref="ParameterSubscription" /> belonging to the currently logged-in
        ///     <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />. When unset, the unsubscribe
        ///     affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterSubscription> OnDeleteSubscription { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the per-card create-override affordance for a
        ///     <see cref="Parameter" /> on the currently selected <see cref="ElementUsage" />. The tuple
        ///     payload carries both the source <see cref="Parameter" /> and the host
        ///     <see cref="ElementUsage" /> on which the new <see cref="ParameterOverride" /> is to be created.
        ///     When unset, the create-override affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<(Parameter Parameter, ElementUsage HostUsage)> OnCreateOverride { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the per-card delete-override affordance to
        ///     remove an existing <see cref="ParameterOverride" /> from the currently selected
        ///     <see cref="ElementUsage" />. When unset, the delete-override affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterOverride> OnDeleteOverride { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the Edit affordance on the summary card.
        ///     When the selected node is an <see cref="ElementUsage" /> this edits the usage; when it is
        ///     an <see cref="ElementDefinition" /> it edits the definition.
        ///     When unset, the Edit button is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback OnEditElement { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the "Edit Element Definition" affordance on
        ///     the summary card. Only rendered when the selected node is an <see cref="ElementUsage" />,
        ///     to allow editing its referenced <see cref="ElementDefinition" /> separately.
        ///     When unset, the button is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback OnEditDefinition { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the Delete affordance on the summary card.
        ///     When unset, the Delete button is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback OnDeleteElement { get; set; }

        /// <summary>
        ///     Disables the Delete button on the summary card when the selected element is the iteration's
        ///     <see cref="Iteration.TopElement" />.
        /// </summary>
        [Parameter]
        public bool DisableDelete { get; set; }

        /// <summary>
        ///     The list of <see cref="ParameterGroup" />s available for the currently selected
        ///     <see cref="ElementDefinition" />. Used to build the collapsible section headers.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ParameterGroup> AvailableParameterGroups { get; set; } = [];

        /// <summary>
        ///     Optional callback invoked when the user drops a parameter card onto a different group section
        ///     (or onto the Ungrouped section). The tuple carries the <see cref="Parameter" /> and the target
        ///     <see cref="ParameterGroup" /> (which may be <c>null</c> to ungroup).
        /// </summary>
        [Parameter]
        public EventCallback<(Parameter Parameter, ParameterGroup Group)> OnAssignParameterGroup { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user drops a <see cref="ParameterGroup" /> header onto
        ///     another group section (to nest it) or onto the Ungrouped section (to move it to the top level).
        ///     The tuple carries the group being moved and the new containing group (which may be <c>null</c>
        ///     to move to the top level).
        /// </summary>
        [Parameter]
        public EventCallback<(ParameterGroup Group, ParameterGroup NewContaining)> OnReassignGroupContaining { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the edit (pencil) affordance on a group section
        ///     header. When unset, the pencil button is not rendered on section headers.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterGroup> OnEditParameterGroup { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the delete (trash) affordance on a group section
        ///     header. When unset, the trash button is not rendered on section headers.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterGroup> OnDeleteParameterGroup { get; set; }

        /// <summary>
        ///     The <see cref="IJSRuntime" /> used to initialise and dispose the masonry layout module.
        /// </summary>
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        /// <summary>
        ///     Backing field for <see cref="SearchTerm" />.
        /// </summary>
        private string searchTerm;

        /// <summary>
        ///     Gets or sets the current search term used to filter parameter cards. Setting this property
        ///     triggers a component re-render via <see cref="Microsoft.AspNetCore.Components.ComponentBase.StateHasChanged" />.
        /// </summary>
        private string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                this.searchTerm = value;
                this.StateHasChanged();
            }
        }

        /// <summary>
        ///     The row whose card is currently being dragged, or <c>null</c> when no drag is in progress.
        /// </summary>
        private ElementDefinitionDetailsRowViewModel draggedRow;

        /// <summary>
        ///     The <see cref="ParameterGroup" /> whose header is currently being dragged, or <c>null</c>
        ///     when no group-drag is in progress.
        /// </summary>
        private ParameterGroup draggedGroup;

        /// <summary>
        ///     Reference to the <c>.param-groups-container</c> DOM element, used to initialise the
        ///     masonry layout module.
        /// </summary>
        private ElementReference groupsContainer;

        /// <summary>
        ///     Tracks whether the masonry module has been initialised for the currently-rendered groups
        ///     container. The container only exists while an element is selected, so masonry cannot be
        ///     initialised on the very first render (nothing is selected yet) — it is initialised the first
        ///     time the container is present and re-initialised whenever it reappears after a deselection.
        /// </summary>
        private bool masonryInitialised;

        /// <summary>
        ///     Gets a flat view of all <see cref="ElementDefinitionDetailsRowViewModel" /> rows from the
        ///     view model, or an empty list when the view model has no rows.
        /// </summary>
        private IReadOnlyList<ElementDefinitionDetailsRowViewModel> AllRows =>
            (this.ViewModel.Rows ?? []).ToList();

        /// <summary>
        ///     Gets the top-level <see cref="ParameterGroup" />s — those whose
        ///     <see cref="ParameterGroup.ContainingGroup" /> is <c>null</c>.
        /// </summary>
        private IReadOnlyList<ParameterGroup> TopLevelGroups =>
            this.AvailableParameterGroups.Where(g => g.ContainingGroup == null).ToList();

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its initial parameters
        ///     from its parent in the render tree.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override Task OnInitializedAsync()
        {
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedSystemNode)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            return base.OnInitializedAsync();
        }

        /// <summary>
        ///     Method invoked each time the component has been rendered. On the first render, initialises
        ///     the masonry layout module so that the group-card container is laid out as a masonry grid.
        /// </summary>
        /// <param name="firstRender">
        ///     <c>true</c> on the very first render of this component instance.
        /// </param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            var containerPresent = this.ViewModel?.SelectedSystemNode != null;

            if (containerPresent && !this.masonryInitialised)
            {
                try
                {
                    await this.JsRuntime.InvokeVoidAsync("cometMasonry.init", this.groupsContainer);
                    this.masonryInitialised = true;
                }
                catch (Exception)
                {
                    // Ignore JS interop exceptions (e.g. during pre-render or unit tests without
                    // a configured JS runtime).
                }
            }
            else if (!containerPresent && this.masonryInitialised)
            {
                // The container was removed (selection cleared); allow re-initialisation when it reappears.
                this.masonryInitialised = false;
            }
        }

        /// <summary>
        ///     Asynchronously disposes the masonry layout module attached to the groups container.
        /// </summary>
        /// <returns>A <see cref="ValueTask" /> representing the asynchronous dispose.</returns>
        public async ValueTask DisposeAsync()
        {
            try
            {
                await this.JsRuntime.InvokeVoidAsync("cometMasonry.dispose", this.groupsContainer);
            }
            catch (Exception)
            {
                // Ignore JS interop exceptions on teardown (circuit may already be disconnected).
            }

            this.Dispose();
        }

        /// <summary>
        ///     Called when a parameter card drag operation starts. Records <paramref name="row" /> as the
        ///     card currently being dragged.
        /// </summary>
        /// <param name="row">The row whose card started being dragged.</param>
        private void OnCardDragStartCallback(ElementDefinitionDetailsRowViewModel row)
        {
            this.draggedRow = row;
        }

        /// <summary>
        ///     Called when a parameter card drag operation ends (successful drop or cancel). Clears the
        ///     dragged-row state.
        /// </summary>
        /// <param name="row">The row whose card drag ended (ignored; only used to match the callback signature).</param>
        private void OnCardDragEndCallback(ElementDefinitionDetailsRowViewModel row)
        {
            this.draggedRow = null;
        }

        /// <summary>
        ///     Called when a <see cref="ParameterGroup" /> header drag operation starts. Records
        ///     <paramref name="group" /> as the group currently being dragged.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> whose header started being dragged.</param>
        private void OnGroupDragStartCallback(ParameterGroup group)
        {
            this.draggedGroup = group;
        }

        /// <summary>
        ///     Called when a <see cref="ParameterGroup" /> header drag operation ends (successful drop or
        ///     cancel). Clears the dragged-group state.
        /// </summary>
        private void OnGroupDragEndCallback()
        {
            this.draggedGroup = null;
        }

        /// <summary>
        ///     Called when a card or group header is dropped onto a
        ///     <see cref="Components.Common.ParameterGroupSection" />.
        ///     When a group header is being dragged, invokes <see cref="OnReassignGroupContaining" /> to
        ///     move the group into (or out of) the target. When a parameter card is being dragged, invokes
        ///     <see cref="OnAssignParameterGroup" /> to reassign the parameter. Resets drag state in both cases.
        /// </summary>
        /// <param name="targetGroup">
        ///     The target <see cref="ParameterGroup" /> raised by the section, or <c>null</c> when dropped
        ///     onto the Ungrouped section.
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        private async Task HandleSectionDrop(ParameterGroup targetGroup)
        {
            if (this.draggedGroup != null)
            {
                var group = this.draggedGroup;
                this.draggedGroup = null;
                await this.OnReassignGroupContaining.InvokeAsync((group, targetGroup));
            }
            else if (this.draggedRow != null && this.OnAssignParameterGroup.HasDelegate)
            {
                var parameter = this.draggedRow.Parameter;
                this.draggedRow = null;
                await this.OnAssignParameterGroup.InvokeAsync((parameter, targetGroup));
            }
            else
            {
                this.draggedRow = null;
            }
        }
    }
}
