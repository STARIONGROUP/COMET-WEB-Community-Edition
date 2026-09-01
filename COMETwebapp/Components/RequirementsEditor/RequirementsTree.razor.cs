// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsTree.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Recursive table-of-contents tree for the Requirements Editor: it lists the specifications and, under the
    /// selected one, its <see cref="RequirementsGroup" /> hierarchy used to navigate the document.
    /// </summary>
    public partial class RequirementsTree
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a
        /// session opened from an ECSS-E-TM-10-25 Annex C3 archive. A group is not draggable and drops onto a
        /// container are ignored when this is <see langword="true" />, so the tree can still be inspected but
        /// never modified.
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the child <see cref="RequirementsGroup" />s to render at this level. When <c>null</c>, this
        /// instance renders the specification roots.
        /// </summary>
        [Parameter]
        public IEnumerable<RequirementsGroup> Groups { get; set; }

        /// <summary>
        /// Records the <paramref name="group" /> being dragged so a subsequent drop can re-parent it.
        /// </summary>
        /// <param name="group">The dragged <see cref="RequirementsGroup" />.</param>
        private void OnGroupDragStart(RequirementsGroup group)
        {
            if (this.IsReadOnly)
            {
                return;
            }

            this.ViewModel.DraggedGroup = group;
        }

        /// <summary>
        /// Marks the given <paramref name="target" /> as the hovered container so it is highlighted while the drag is
        /// over it (only the hovered target is highlighted, and only the invalid-target check gates the highlight).
        /// </summary>
        /// <param name="target">The <see cref="RequirementsContainer" /> the drag entered.</param>
        private void OnDragEnter(RequirementsContainer target)
        {
            this.ViewModel.DragOverContainer = target;
        }

        /// <summary>
        /// Clears the drag state when the drag ends (whether or not it resulted in a drop).
        /// </summary>
        private void OnGroupDragEnd()
        {
            this.ViewModel.DraggedGroup = null;
            this.ViewModel.DragOverContainer = null;
        }

        /// <summary>
        /// Drops the dragged group onto the given <paramref name="target" /> container, re-parenting it (the move is
        /// validated and silently ignored when it is not allowed, e.g. onto itself or a descendant).
        /// </summary>
        /// <param name="target">The target <see cref="RequirementsContainer" /> (a specification or a group).</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnDropOnContainer(RequirementsContainer target)
        {
            var dragged = this.ViewModel.DraggedGroup;
            this.ViewModel.DraggedGroup = null;
            this.ViewModel.DragOverContainer = null;

            if (!this.IsReadOnly && dragged != null && this.ViewModel.CanMoveGroup(dragged, target))
            {
                await this.ViewModel.MoveGroupAsync(dragged, target);
            }
        }
    }
}
