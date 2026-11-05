// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRequirementsTreeDragViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;

    using FluentResults;

    /// <summary>
    /// A host of the table-of-contents tree whose groups can be dragged to re-parent them. The tree only offers dragging
    /// when its host implements this.
    /// </summary>
    public interface IRequirementsTreeDragViewModel : IRequirementsTreeViewModel
    {
        /// <summary>
        /// Gets or sets the group being dragged in the tree.
        /// </summary>
        RequirementsGroup DraggedGroup { get; set; }

        /// <summary>
        /// Gets or sets the container the drag is currently over, highlighted as a drop target.
        /// </summary>
        RequirementsContainer DragOverContainer { get; set; }

        /// <summary>
        /// Determines whether the group can be moved into the target.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="target">The target specification or group.</param>
        /// <returns>true when the move is allowed.</returns>
        bool CanMoveGroup(RequirementsGroup group, RequirementsContainer target);

        /// <summary>
        /// Moves the group into the target.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="target">The target specification or group.</param>
        /// <returns>The <see cref="Result" /> of the write.</returns>
        Task<Result> MoveGroupAsync(RequirementsGroup group, RequirementsContainer target);
    }
}
