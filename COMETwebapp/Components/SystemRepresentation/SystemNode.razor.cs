// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemNode.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;

    using ViewModels.Components.SystemRepresentation;
    using ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the baseNode component
    /// </summary>
    public partial class SystemNode
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a
        /// session opened from an ECSS-E-TM-10-25 Annex C3 archive. The node is not draggable and drops onto it
        /// are ignored when this is <see langword="true" />, so the tree can still be inspected but never modified.
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Gets or sets the <see cref="SystemNodeViewModel" /> for this node.
        /// </summary>
        [Parameter]
        public SystemNodeViewModel ViewModel { get; set; }

        /// <summary>
        /// Level of the tree. Increases by one for each nested element
        /// </summary>
        [Parameter]
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SystemRepresentationTreeViewModel" /> cascaded from the tree root,
        /// used to read display-option toggles (<see cref="SystemRepresentationTreeViewModel.ShowName" />,
        /// <see cref="SystemRepresentationTreeViewModel.ShowOwner" />,
        /// <see cref="SystemRepresentationTreeViewModel.ShowCategories" />) and to co-ordinate
        /// drag-and-drop state.
        /// </summary>
        [CascadingParameter]
        public SystemRepresentationTreeViewModel TreeViewModel { get; set; }

        /// <summary>
        /// The subscription that re-renders this node when its draw/expand/select state or the shared
        /// drag-over state changes. Re-created (and the previous one disposed) on every
        /// <see cref="OnParametersSet" /> because <see cref="Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize{TItem}" />
        /// reuses the same component instance with a different <see cref="ViewModel" />.
        /// </summary>
        private IDisposable nodeStateSubscription;

        /// <summary>
        /// Gets a value indicating whether this node is currently the valid drop target during an
        /// in-progress drag operation.
        /// </summary>
        public bool IsValidDropTarget
            => this.TreeViewModel is not null
               && this.TreeViewModel.DragOverNode == this.ViewModel
               && SystemRepresentationTreeViewModel.CanDrop(this.TreeViewModel.DraggedNode, this.ViewModel);

        /// <summary>
        /// Gets the number of elements the dragged node would bring if it were dropped here. The
        /// <see cref="ElementDefinition" /> itself counts as one new <see cref="ElementUsage" />, plus
        /// each of its contained usages and each of its parameters. Returns 0 when no drag is in progress
        /// or when the dragged node's <see cref="COMETwebapp.ViewModels.Components.Shared.BaseNodeViewModel{T}.Thing" />
        /// cannot be resolved to an <see cref="ElementDefinition" />.
        /// </summary>
        public int DraggedItemImpactCount
        {
            get
            {
                if (this.TreeViewModel?.DraggedNode is null)
                {
                    return 0;
                }

                var ed = this.TreeViewModel.DraggedNode.Thing as ElementDefinition
                         ?? (this.TreeViewModel.DraggedNode.Thing as ElementUsage)?.ElementDefinition;

                return ed is null ? 0 : 1 + ed.ContainedElement.Count + ed.Parameter.Count;
            }
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.ViewModel.Level = this.Level;

            this.nodeStateSubscription?.Dispose();

            this.nodeStateSubscription = this.WhenAnyValue(
                    x => x.ViewModel.IsDrawn,
                    x => x.ViewModel.IsExpanded,
                    x => x.ViewModel.IsSelected,
                    x => x.TreeViewModel.DragOverNode)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged));

            this.Disposables.Add(this.nodeStateSubscription);
        }

        /// <summary>
        /// Toggles this node's expansion, routing through the cascaded tree view model so the state is
        /// persisted across tree rebuilds. Falls back to a direct set when no tree view model is cascaded.
        /// </summary>
        private void ToggleExpansion()
        {
            if (this.TreeViewModel is not null)
            {
                this.TreeViewModel.SetNodeExpansion(this.ViewModel, !this.ViewModel.IsExpanded);
            }
            else
            {
                this.ViewModel.IsExpanded = !this.ViewModel.IsExpanded;
            }
        }

        /// <summary>
        /// Marks this node as the dragged node in the shared <see cref="TreeViewModel" />.
        /// </summary>
        private void HandleDragStart()
        {
            if (this.TreeViewModel is null || this.IsReadOnly)
            {
                return;
            }

            this.TreeViewModel.DraggedNode = this.ViewModel;
        }

        /// <summary>
        /// Clears the dragged/hover state in the shared <see cref="TreeViewModel" /> when the drag ends.
        /// </summary>
        private void HandleDragEnd()
        {
            if (this.TreeViewModel is null)
            {
                return;
            }

            this.TreeViewModel.DraggedNode = null;
            this.TreeViewModel.DragOverNode = null;
        }

        /// <summary>
        /// Highlights this node as the drop target when the dragged node may validly be dropped here.
        /// </summary>
        private void HandleDragEnter()
        {
            if (this.TreeViewModel is null)
            {
                return;
            }

            if (SystemRepresentationTreeViewModel.CanDrop(this.TreeViewModel.DraggedNode, this.ViewModel))
            {
                this.TreeViewModel.DragOverNode = this.ViewModel;
            }
        }

        /// <summary>
        /// Handles a drop event: clears the hover state and invokes <see cref="SystemRepresentationTreeViewModel.OnDrop" />
        /// when the drop is valid.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        private async Task HandleDropAsync()
        {
            if (this.TreeViewModel is null || this.IsReadOnly)
            {
                return;
            }

            var dragged = this.TreeViewModel.DraggedNode;
            this.TreeViewModel.DragOverNode = null;
            this.TreeViewModel.DraggedNode = null;

            if (SystemRepresentationTreeViewModel.CanDrop(dragged, this.ViewModel))
            {
                await this.TreeViewModel.OnDrop.InvokeAsync((dragged, this.ViewModel));
            }
        }
    }
}
