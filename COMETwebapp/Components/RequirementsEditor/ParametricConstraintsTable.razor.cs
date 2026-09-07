// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParametricConstraintsTable.razor.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Inline table that manages the <see cref="Requirement.ParametricConstraint" /> collection of the edited requirement:
    /// a grid of the constraints (rendered as a one-line summary) with an add/edit dialog hosting the
    /// <see cref="EditParametricConstraint" /> tree editor. Additions, edits and removals are staged on the in-memory
    /// requirement so they are committed together with it when the surrounding dialog is saved.
    /// </summary>
    public partial class ParametricConstraintsTable : DisposableComponent
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
        /// Gets or sets a value indicating whether the active user may write the parent <see cref="Requirement" /> this
        /// table edits part of. These constraints are parts of one aggregate saved atomically by the hosting form, so
        /// the permission is decided once by that form and passed down rather than evaluated per row. Defaults to true
        /// so a host that does not set it keeps its previous behaviour
        /// </summary>
        [Parameter]
        public bool IsAllowedToWrite { get; set; } = true;

        /// <summary>
        /// The <see cref="Requirement" /> whose parametric constraints are edited.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// The parameter types available when editing a relational expression.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ParameterType> AvailableParameterTypes { get; set; } = [];

        /// <summary>
        /// Gets the constraint currently being added or edited, or null when the editor is closed.
        /// </summary>
        public ParametricConstraint EditingConstraint { get; private set; }

        /// <summary>
        /// True when the editor holds a new constraint not yet added to the requirement.
        /// </summary>
        private bool isNewConstraint;

        /// <summary>
        /// Gets a value indicating whether the constraint editor dialog is open.
        /// </summary>
        public bool IsEditorOpen => this.EditingConstraint != null;

        /// <summary>
        /// The popup that asks the user to confirm the removal of a <see cref="ParametricConstraint" /> before it is applied.
        /// </summary>
        public ConfirmRemovalPopup<ParametricConstraint> RemovalPopup { get; private set; }

        /// <summary>
        /// Gets the constraint rows to display.
        /// </summary>
        /// <returns>The requirement's parametric constraints.</returns>
        private List<ParametricConstraint> GetRows()
        {
            return this.Requirement?.ParametricConstraint.ToList() ?? [];
        }

        /// <summary>
        /// Gets a one-line summary of the given <paramref name="constraint" />.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" />.</param>
        /// <returns>The summary string.</returns>
        private static string GetSummary(ParametricConstraint constraint)
        {
            var viewModel = new EditParametricConstraintViewModel();
            viewModel.LoadFrom(constraint);
            return viewModel.RootExpression == null ? "(empty)" : viewModel.GetSummary(viewModel.RootExpression);
        }

        /// <summary>
        /// Opens the editor to add a new constraint.
        /// </summary>
        public void OpenAdd()
        {
            this.isNewConstraint = true;
            this.EditingConstraint = new ParametricConstraint { Iid = Guid.NewGuid() };
        }

        /// <summary>
        /// Opens the editor to edit an existing constraint.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /> to edit.</param>
        public void OpenEdit(ParametricConstraint constraint)
        {
            this.isNewConstraint = false;
            this.EditingConstraint = constraint;
        }

        /// <summary>
        /// Removes the given <paramref name="constraint" /> from the requirement, once the user has confirmed the removal
        /// in the <see cref="RemovalPopup" />.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /> to remove.</param>
        public void Remove(ParametricConstraint constraint)
        {
            this.Requirement.ParametricConstraint.Remove(constraint);
        }

        /// <summary>
        /// Handles the tree editor saving: a new constraint is appended to the requirement; an edited existing one is kept
        /// in place (its rebuilt expressions preserve their identifiers, so the write is an update, not a delete-and-recreate).
        /// </summary>
        public void HandleSaved()
        {
            if (this.isNewConstraint)
            {
                this.Requirement.ParametricConstraint.Add(this.EditingConstraint);
            }

            this.EditingConstraint = null;
        }

        /// <summary>
        /// Closes the editor without applying changes.
        /// </summary>
        public void HandleCancelled()
        {
            this.EditingConstraint = null;
        }

        /// <summary>
        /// Handles the editor popup being closed via its close button.
        /// </summary>
        /// <param name="visible">The new visibility of the popup.</param>
        private void OnEditorVisibleChanged(bool visible)
        {
            if (!visible)
            {
                this.EditingConstraint = null;
            }
        }
    }
}
