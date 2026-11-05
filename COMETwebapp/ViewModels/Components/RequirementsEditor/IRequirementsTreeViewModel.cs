// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRequirementsTreeViewModel.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// What the shared table-of-contents tree and the owner and category pills need from their host, so a page can host them
    /// without implementing the whole Requirements Editor contract.
    /// </summary>
    public interface IRequirementsTreeViewModel
    {
        /// <summary>
        /// Gets the specifications offered in the tree.
        /// </summary>
        IEnumerable<RequirementsSpecification> AvailableSpecifications { get; }

        /// <summary>
        /// Gets or sets the selected specification, the one whose groups the tree shows.
        /// </summary>
        RequirementsSpecification SelectedSpecification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner pill is shown.
        /// </summary>
        bool ShowOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown.
        /// </summary>
        bool ShowCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tree labels specifications and groups by their short name (true) or
        /// their name (false).
        /// </summary>
        bool TreeUsesShortName { get; set; }

        /// <summary>
        /// Gets the groups to show below the given specification or group.
        /// </summary>
        /// <param name="container">The specification or group.</param>
        /// <returns>The groups.</returns>
        IEnumerable<RequirementsGroup> GetGroups(RequirementsContainer container);

        /// <summary>
        /// Determines whether the tree node with the given iid is collapsed.
        /// </summary>
        /// <param name="iid">The iid of the specification or group.</param>
        /// <returns>true when collapsed.</returns>
        bool IsTreeNodeCollapsed(Guid iid);

        /// <summary>
        /// Toggles the collapsed state of the tree node with the given iid.
        /// </summary>
        /// <param name="iid">The iid of the specification or group.</param>
        void ToggleTreeNode(Guid iid);

        /// <summary>
        /// Navigates the page to the given group, as clicked in the tree.
        /// </summary>
        /// <param name="group">The group.</param>
        void NavigateToGroup(RequirementsGroup group);
    }
}
