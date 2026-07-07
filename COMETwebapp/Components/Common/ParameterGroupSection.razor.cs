// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterGroupSection.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///     Partial class for the <see cref="ParameterGroupSection" /> component. Renders a single
    ///     <see cref="ParameterGroup" /> (or the "Ungrouped" pseudo-section when <see cref="Group" /> is
    ///     <c>null</c>) as a tinted container card. Nested child groups are rendered recursively by
    ///     including further <see cref="ParameterGroupSection" /> instances for each child, and direct-member
    ///     <see cref="ElementDefinitionDetailsRowViewModel" /> rows are rendered as <see cref="ParameterCard" />
    ///     cards. The section supports collapse / expand, drag-and-drop targeting, and optional
    ///     edit / delete affordances on the header.
    /// </summary>
    public partial class ParameterGroupSection
    {
        /// <summary>
        ///     Gets or sets the <see cref="ParameterGroup" /> rendered by this section, or <c>null</c> to
        ///     render the special "Ungrouped" section that collects rows not assigned to any group.
        /// </summary>
        [Parameter]
        public ParameterGroup Group { get; set; }

        /// <summary>
        ///     Gets or sets the full list of <see cref="ElementDefinitionDetailsRowViewModel" /> rows from
        ///     which this section selects its direct members. May be <c>null</c> — treated as empty.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ElementDefinitionDetailsRowViewModel> AllRows { get; set; }

        /// <summary>
        ///     Gets or sets the full list of <see cref="ParameterGroup" />s from which child groups are
        ///     derived. May be <c>null</c> — treated as empty.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ParameterGroup> AllGroups { get; set; }

        /// <summary>
        ///     Gets or sets the current search term used to filter cards. When non-empty, sections that have
        ///     no matching rows in any descendant are hidden and the section is forced open.
        /// </summary>
        [Parameter]
        public string SearchTerm { get; set; }

        /// <summary>
        ///     Gets or sets the nesting level of this section. The root (top-level) sections use level 0;
        ///     each recursive child increments by one.
        /// </summary>
        [Parameter]
        public int Level { get; set; }

        /// <summary>
        ///     Optional callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Invoked when the user clicks the edit affordance on a card, carrying the row's
        ///     <see cref="ParameterOrOverrideBase" />.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterOrOverrideBase> OnEditParameter { get; set; }

        /// <summary>
        ///     Optional callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Invoked when the user clicks the delete affordance on a <see cref="Parameter" /> card.
        /// </summary>
        [Parameter]
        public EventCallback<Parameter> OnDeleteParameter { get; set; }

        /// <summary>
        ///     Optional callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Invoked when the user clicks the subscribe affordance on a <see cref="Parameter" /> card.
        /// </summary>
        [Parameter]
        public EventCallback<Parameter> OnCreateSubscription { get; set; }

        /// <summary>
        ///     Optional callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Invoked when the user clicks the unsubscribe affordance on a <see cref="Parameter" /> card.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterSubscription> OnDeleteSubscription { get; set; }

        /// <summary>
        ///     Optional callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Invoked when the user clicks the create-override affordance on a <see cref="Parameter" /> card.
        ///     The tuple carries the source <see cref="Parameter" /> and its host <see cref="ElementUsage" />.
        /// </summary>
        [Parameter]
        public EventCallback<(Parameter Parameter, ElementUsage HostUsage)> OnCreateOverride { get; set; }

        /// <summary>
        ///     Optional callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Invoked when the user clicks the delete-override affordance on a <see cref="Parameter" /> card.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterOverride> OnDeleteOverride { get; set; }

        /// <summary>
        ///     Callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Raised when the user starts dragging a card; carries the row being dragged.
        /// </summary>
        [Parameter]
        public EventCallback<ElementDefinitionDetailsRowViewModel> OnCardDragStart { get; set; }

        /// <summary>
        ///     Callback forwarded to each <see cref="ParameterCard" /> rendered inside this section.
        ///     Raised when a card drag operation ends (successful drop or cancel).
        /// </summary>
        [Parameter]
        public EventCallback<ElementDefinitionDetailsRowViewModel> OnCardDragEnd { get; set; }

        /// <summary>
        ///     Callback raised when the user drops a dragged card onto this section. The payload is
        ///     <see cref="Group" /> (which may be <c>null</c> for the Ungrouped section), so the host
        ///     component can identify the target group and call the appropriate assignment pipeline.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterGroup> OnSectionDrop { get; set; }

        /// <summary>
        ///     Optional callback raised when the user clicks the pencil (edit) affordance on this section's
        ///     header. When not set, the edit button is not rendered. Only available on named groups
        ///     (<see cref="Group" /> is not <c>null</c>).
        /// </summary>
        [Parameter]
        public EventCallback<ParameterGroup> OnEditGroup { get; set; }

        /// <summary>
        ///     Optional callback raised when the user clicks the trash (delete) affordance on this section's
        ///     header. When not set, the delete button is not rendered. Only available on named groups
        ///     (<see cref="Group" /> is not <c>null</c>).
        /// </summary>
        [Parameter]
        public EventCallback<ParameterGroup> OnDeleteGroup { get; set; }

        /// <summary>
        ///     Callback raised when the user starts dragging this section's group header (named groups only).
        ///     Forwarded to recursive child sections. The payload is the <see cref="ParameterGroup" /> being
        ///     dragged so the host component can record which group is in flight.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterGroup> OnGroupDragStart { get; set; }

        /// <summary>
        ///     Callback raised when the user's group-header drag operation ends (successful drop or cancel).
        ///     Forwarded to recursive child sections. The host component uses this to clear the in-flight
        ///     group state.
        /// </summary>
        [Parameter]
        public EventCallback OnGroupDragEnd { get; set; }

        /// <summary>
        ///     Tracks whether the user has explicitly collapsed this section. When a search is active the
        ///     section is always rendered open regardless of this flag.
        /// </summary>
        private bool collapsed;

        /// <summary>
        ///     Drag enter/leave depth counter. Because <c>dragenter</c>/<c>dragleave</c> bubble from every
        ///     descendant element, a single boolean flag flickers as the pointer moves over child cards. The
        ///     counter increments on enter and decrements on leave, so the drop-target highlight stays on for
        ///     as long as the pointer is anywhere inside the section.
        /// </summary>
        private int dragDepth;

        /// <summary>
        ///     Gets a value indicating whether a dragged card is currently hovering over this section's drop
        ///     zone (the drag-enter depth is positive).
        /// </summary>
        private bool IsDropTarget => this.dragDepth > 0;

        /// <summary>
        ///     Gets a value indicating whether a search term is currently active (non-empty and non-whitespace).
        /// </summary>
        private bool IsSearchActive => !string.IsNullOrWhiteSpace(this.SearchTerm);

        /// <summary>
        ///     Gets a value indicating whether the body of this section is currently visible. When a search is
        ///     active the section is always expanded; otherwise it follows <see cref="collapsed" />.
        /// </summary>
        private bool IsExpanded => this.IsSearchActive || !this.collapsed;

        /// <summary>
        ///     Gets the display name of this section: the group's <see cref="ParameterGroup.Name" /> for a
        ///     named group, or "Ungrouped" for the pseudo-section.
        /// </summary>
        private string Name => this.Group?.Name ?? "Ungrouped";

        /// <summary>
        ///     Gets the <see cref="ElementDefinitionDetailsRowViewModel" /> rows that are direct members of
        ///     this section and that match the current search term. Returns an empty list when
        ///     <see cref="AllRows" /> is <c>null</c>.
        /// </summary>
        private IReadOnlyList<ElementDefinitionDetailsRowViewModel> DirectRows =>
            (this.AllRows ?? []).Where(r => this.Group is null
                ? r.Group is null
                : r.Group?.Iid == this.Group.Iid)
            .Where(this.MatchesSearch)
            .ToList();

        /// <summary>
        ///     Gets the immediate child <see cref="ParameterGroup" />s of <see cref="Group" />. Always
        ///     returns an empty list for the Ungrouped pseudo-section (<see cref="Group" /> is <c>null</c>)
        ///     or when <see cref="AllGroups" /> is <c>null</c>.
        /// </summary>
        private IReadOnlyList<ParameterGroup> ChildGroups =>
            this.Group is null
                ? []
                : (this.AllGroups ?? []).Where(g => g.ContainingGroup?.Iid == this.Group.Iid).ToList();

        /// <summary>
        ///     Gets a value indicating whether this section (or any of its descendants) has any content
        ///     that should be rendered under the current search filter. When no search is active, always
        ///     returns <c>true</c> so that empty groups still show their header. When searching, returns
        ///     <c>true</c> only when there is at least one matching direct row or a descendant group with
        ///     a matching row.
        /// </summary>
        private bool HasVisibleContent =>
            !this.IsSearchActive || this.DirectRows.Count > 0 || this.ChildGroups.Any(this.ChildHasVisibleContent);

        /// <summary>
        ///     Toggles the <see cref="collapsed" /> flag. Has no effect when a search is active because
        ///     the section is always forced open during a search.
        /// </summary>
        private void ToggleCollapsed()
        {
            this.collapsed = !this.collapsed;
        }

        /// <summary>
        ///     Handles the drop event on this section. Clears the drop-target highlight and raises
        ///     <see cref="OnSectionDrop" /> with <see cref="Group" /> as the payload (may be <c>null</c>
        ///     for the Ungrouped section).
        /// </summary>
        private async Task HandleDrop()
        {
            this.dragDepth = 0;
            await this.OnSectionDrop.InvokeAsync(this.Group);
        }

        /// <summary>
        ///     Handles a <c>dragenter</c> on the section (or any descendant, via bubbling) by incrementing the
        ///     drag-enter depth counter.
        /// </summary>
        private void OnDragEnter()
        {
            this.dragDepth++;
        }

        /// <summary>
        ///     Handles a <c>dragleave</c> on the section (or any descendant, via bubbling) by decrementing the
        ///     drag-enter depth counter, clamped at zero.
        /// </summary>
        private void OnDragLeave()
        {
            if (this.dragDepth > 0)
            {
                this.dragDepth--;
            }
        }

        /// <summary>
        ///     Returns <c>true</c> when <paramref name="row" /> matches the current search term, or when no
        ///     search term is active.
        /// </summary>
        /// <param name="row">The <see cref="ElementDefinitionDetailsRowViewModel" /> to evaluate.</param>
        /// <returns><c>true</c> when the row passes the filter.</returns>
        private bool MatchesSearch(ElementDefinitionDetailsRowViewModel row)
        {
            return !this.IsSearchActive
                   || row.ParameterTypeName.Contains(this.SearchTerm, StringComparison.OrdinalIgnoreCase)
                   || row.ShortName.Contains(this.SearchTerm, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Recursively determines whether <paramref name="childGroup" /> or any of its descendants has
        ///     at least one row that matches the current search term.
        /// </summary>
        /// <param name="childGroup">The <see cref="ParameterGroup" /> to check.</param>
        /// <returns><c>true</c> when a matching row exists anywhere in the subtree.</returns>
        private bool ChildHasVisibleContent(ParameterGroup childGroup)
        {
            var hasMatchingRows = (this.AllRows ?? [])
                .Any(r => r.Group?.Iid == childGroup.Iid && this.MatchesSearch(r));

            if (hasMatchingRows)
            {
                return true;
            }

            var grandChildren = (this.AllGroups ?? []).Where(g => g.ContainingGroup?.Iid == childGroup.Iid);
            return grandChildren.Any(this.ChildHasVisibleContent);
        }
    }
}
