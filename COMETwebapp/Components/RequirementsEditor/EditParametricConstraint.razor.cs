// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParametricConstraint.razor.cs" company="Starion Group S.A.">
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

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Hosts the parametric-constraint expression tree editor: it renders the tree (or a "add first expression" prompt),
    /// handles the tree actions bubbled from <see cref="BooleanExpressionNode" />, and drives a small dialog to edit a
    /// relational expression's parameter type, operator, value and scale. On save it writes the tree back into the
    /// <see cref="ParametricConstraint" /> and raises <see cref="OnSaved" />.
    /// </summary>
    public partial class EditParametricConstraint
    {
        /// <summary>
        /// The available relational operators, paired with their display symbol.
        /// </summary>
        private static readonly OperatorOption[] OperatorOptions = Enum.GetValues<RelationalOperatorKind>()
            .Select(x => new OperatorOption(x, x.ToScientificNotationString()))
            .ToArray();

        /// <summary>
        /// The view model owning the edited tree.
        /// </summary>
        private readonly EditParametricConstraintViewModel viewModel = new();

        /// <summary>
        /// The relational row currently being edited in the relational dialog.
        /// </summary>
        private RelationalExpressionRow editingRelationalRow;

        /// <summary>
        /// The parent group a newly-created relational row is added to on save (null for the root).
        /// </summary>
        private CompositeExpressionRow relationalTargetParent;

        /// <summary>
        /// True when the relational dialog is adding a new expression rather than editing an existing one.
        /// </summary>
        private bool isNewRelational;

        /// <summary>
        /// The parameter type staged in the relational dialog.
        /// </summary>
        private ParameterType relationalParameterType;

        /// <summary>
        /// The operator staged in the relational dialog.
        /// </summary>
        private RelationalOperatorKind relationalOperator = RelationalOperatorKind.EQ;

        /// <summary>
        /// The value staged in the relational dialog.
        /// </summary>
        private string relationalValue = string.Empty;

        /// <summary>
        /// The scale staged in the relational dialog.
        /// </summary>
        private MeasurementScale relationalScale;

        /// <summary>
        /// Whether the root-level "add" dropdown menu (shown when the root is a single relational expression) is open.
        /// </summary>
        private bool rootAddMenuOpen;

        /// <summary>
        /// Gets or sets the <see cref="ParametricConstraint" /> being edited.
        /// </summary>
        [Parameter]
        public ParametricConstraint Constraint { get; set; }

        /// <summary>
        /// Gets or sets the parameter types available when editing a relational expression.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ParameterType> AvailableParameterTypes { get; set; } = [];

        /// <summary>
        /// Gets or sets the callback raised when the constraint has been saved into <see cref="Constraint" />.
        /// </summary>
        [Parameter]
        public EventCallback OnSaved { get; set; }

        /// <summary>
        /// Gets or sets the callback raised when editing is cancelled.
        /// </summary>
        [Parameter]
        public EventCallback OnCancelled { get; set; }

        /// <summary>
        /// Gets a value indicating whether the relational dialog is open.
        /// </summary>
        public bool IsRelationalDialogOpen { get; private set; }

        /// <summary>
        /// Gets the scales selectable for the relational dialog's quantity-kind parameter type.
        /// </summary>
        private IEnumerable<MeasurementScale> AvailableRelationalScales => (this.relationalParameterType as QuantityKind)?.AllPossibleScale ?? [];

        /// <summary>
        /// Gets a value indicating whether the relational dialog can be confirmed.
        /// </summary>
        private bool CanConfirmRelational => this.relationalParameterType != null && !string.IsNullOrWhiteSpace(this.relationalValue);

        /// <summary>
        /// The unique DOM id for the root-level "add" dropdown arrow button, generated once at field
        /// initialization so it is stable and immutable for the component's lifetime.
        /// </summary>
        private readonly string uniqueRootAddId = $"pc-root-add-{Guid.NewGuid():N}";

        /// <summary>
        /// Gets the CSS selector that targets the root-level "add" dropdown arrow button by its unique
        /// DOM id, used as the dropdown's <c>PositionTarget</c>.
        /// </summary>
        private string RootAddSelector => $"#{this.uniqueRootAddId}";

        /// <summary>
        /// Loads the tree from the constraint when the component is initialized.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.viewModel.AvailableParameterTypes = this.AvailableParameterTypes;
            this.viewModel.LoadFrom(this.Constraint);
        }

        /// <summary>
        /// Opens the relational dialog to add the first expression to an empty constraint.
        /// </summary>
        private void AddFirstExpression()
        {
            this.OpenRelationalDialog(null, null);
        }

        /// <summary>
        /// Opens the relational dialog to add a relational expression to the given group.
        /// </summary>
        /// <param name="parent">The group to add to.</param>
        private void HandleAddRelational(CompositeExpressionRow parent)
        {
            this.OpenRelationalDialog(null, parent);
        }

        /// <summary>
        /// Adds a nested group to the given group.
        /// </summary>
        /// <param name="parent">The group to add to.</param>
        private void HandleAddGroup(CompositeExpressionRow parent)
        {
            this.viewModel.AddGroup(parent);
        }

        /// <summary>
        /// Closes the root add menu and opens the relational dialog to add a second expression at the root (wrapping the
        /// existing single expression and the new one in an AND group).
        /// </summary>
        private void RootAddExpression()
        {
            this.rootAddMenuOpen = false;
            this.HandleAddRelational(null);
        }

        /// <summary>
        /// Closes the root add menu and adds a nested group at the root (wrapping the existing single expression and the
        /// new group in an AND group).
        /// </summary>
        private void RootAddGroup()
        {
            this.rootAddMenuOpen = false;
            this.HandleAddGroup(null);
        }

        /// <summary>
        /// Opens the relational dialog to edit an existing relational expression.
        /// </summary>
        /// <param name="row">The relational row to edit.</param>
        private void HandleEditRelational(RelationalExpressionRow row)
        {
            this.OpenRelationalDialog(row, row.Parent);
        }

        /// <summary>
        /// Configures and opens the relational dialog.
        /// </summary>
        /// <param name="existing">The row to edit, or null to add a new one.</param>
        /// <param name="parent">The parent group of the row.</param>
        private void OpenRelationalDialog(RelationalExpressionRow existing, CompositeExpressionRow parent)
        {
            this.isNewRelational = existing == null;
            this.editingRelationalRow = existing ?? new RelationalExpressionRow();
            this.relationalTargetParent = parent;
            this.relationalParameterType = this.editingRelationalRow.ParameterType;
            this.relationalOperator = this.editingRelationalRow.Operator;
            this.relationalValue = this.editingRelationalRow.Value;
            this.relationalScale = this.editingRelationalRow.Scale;
            this.IsRelationalDialogOpen = true;
        }

        /// <summary>
        /// Handles a change of the relational dialog's parameter type, resetting the scale to the type's default.
        /// </summary>
        /// <param name="parameterType">The selected parameter type.</param>
        private void OnRelationalParameterTypeChanged(ParameterType parameterType)
        {
            this.relationalParameterType = parameterType;
            this.relationalScale = (parameterType as QuantityKind)?.DefaultScale;
        }

        /// <summary>
        /// Commits the relational dialog onto its row, adding a new row to the tree when applicable.
        /// </summary>
        private void ConfirmRelational()
        {
            this.editingRelationalRow.ParameterType = this.relationalParameterType;
            this.editingRelationalRow.Operator = this.relationalOperator;
            this.editingRelationalRow.Value = this.relationalValue;
            this.editingRelationalRow.Scale = this.relationalScale;

            if (this.isNewRelational)
            {
                this.viewModel.AddNode(this.relationalTargetParent, this.editingRelationalRow);
            }

            this.IsRelationalDialogOpen = false;
        }

        /// <summary>
        /// Closes the relational dialog without applying changes.
        /// </summary>
        private void CancelRelational()
        {
            this.IsRelationalDialogOpen = false;
        }

        /// <summary>
        /// Handles the relational popup being closed via its close button.
        /// </summary>
        /// <param name="visible">The new visibility of the popup.</param>
        private void OnRelationalDialogVisibleChanged(bool visible)
        {
            if (!visible)
            {
                this.IsRelationalDialogOpen = false;
            }
        }

        /// <summary>
        /// Writes the tree back into the constraint and raises <see cref="OnSaved" />.
        /// </summary>
        private async Task SaveAsync()
        {
            this.viewModel.BuildInto(this.Constraint);
            await this.OnSaved.InvokeAsync();
        }

        /// <summary>
        /// A relational operator paired with its display symbol, for the operator combo.
        /// </summary>
        /// <param name="Kind">The operator.</param>
        /// <param name="Symbol">The display symbol.</param>
        private sealed record OperatorOption(RelationalOperatorKind Kind, string Symbol);
    }
}
