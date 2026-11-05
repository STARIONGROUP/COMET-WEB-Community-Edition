// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IVandVRegisterViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.VandV
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    /// <summary>
    /// View model of the V&amp;V register (the Verification Control Document): the requirements to be covered, by
    /// specification and group, each with the V&amp;V items covering it and their procedure steps. It hosts the shared
    /// table-of-contents tree: selecting the selected specification again shows all specifications.
    /// </summary>
    public interface IVandVRegisterViewModel : IRequirementsTreeViewModel
    {
        /// <summary>
        /// Gets the V&amp;V snapshot the register is built from.
        /// </summary>
        VandVModel Model { get; }

        /// <summary>
        /// Gets the rows to render, in order.
        /// </summary>
        IReadOnlyList<VandVRegisterRow> Rows { get; }

        /// <summary>
        /// Gets the message shown when the rows could not be built, or null.
        /// </summary>
        string BuildError { get; }

        /// <summary>
        /// Gets or sets the text the requirements and items are searched for, by short name and name.
        /// </summary>
        string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the stage gates the items are filtered on; empty means no filter.
        /// </summary>
        IEnumerable<string> SelectedStages { get; set; }

        /// <summary>
        /// Gets or sets the labels of the fields shown on an item; empty shows them all.
        /// </summary>
        IEnumerable<string> SelectedFields { get; set; }

        /// <summary>
        /// Gets the preset the selected fields currently match, or null for a custom combination.
        /// </summary>
        VandVViewPreset? ActivePreset { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the table of contents is collapsed.
        /// </summary>
        bool IsTocCollapsed { get; set; }

        /// <summary>
        /// Gets or sets the way the rows are labelled: by short name, by name or by both.
        /// </summary>
        RequirementRowDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether specifications, groups and requirements are indented by their depth.
        /// </summary>
        bool IndentGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the relationships of each requirement and item are shown.
        /// </summary>
        bool ShowTraceability { get; set; }

        /// <summary>
        /// Gets the iid of the row to scroll to on the next render, or null.
        /// </summary>
        Guid? ScrollTarget { get; }

        /// <summary>
        /// Selects the fields of the given view.
        /// </summary>
        /// <param name="preset">The <see cref="VandVViewPreset" />.</param>
        void ApplyPreset(VandVViewPreset preset);

        /// <summary>
        /// Expands or collapses the given row.
        /// </summary>
        /// <param name="row">The <see cref="VandVRegisterRow" />.</param>
        void Toggle(VandVRegisterRow row);

        /// <summary>
        /// Clears the search and filters, scopes the register to the requirement's specification, expands the requirement
        /// and its items, and flags it as the <see cref="ScrollTarget" />. A requirement that is not in the register (for
        /// example in a deprecated specification) just shows the whole register.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        void Focus(Requirement requirement);

        /// <summary>
        /// Follows a traceability link: focuses the requirement, or a requirement an item covers, preferring the one the link
        /// was clicked on. A link to anything else, such as an activity, has no row to go to and does nothing.
        /// </summary>
        /// <param name="requirement">The linked requirement.</param>
        /// <param name="origin">The requirement or item whose row holds the link.</param>
        void NavigateToRequirement(Requirement requirement, Requirement origin);

        /// <summary>
        /// Clears the <see cref="ScrollTarget" /> once the row was scrolled to.
        /// </summary>
        void ClearScrollTarget();

        /// <summary>
        /// Rebuilds the register from a new snapshot, keeping the scope, filters and expansion state.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        void SetModel(VandVModel model);
    }
}
