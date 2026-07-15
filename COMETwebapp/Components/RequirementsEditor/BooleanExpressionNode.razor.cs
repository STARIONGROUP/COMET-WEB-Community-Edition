// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BooleanExpressionNode.razor.cs" company="Starion Group S.A.">
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
    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Recursively renders one node of the parametric-constraint expression tree: a relational leaf, or an AND/OR/XOR
    /// group with its child nodes. It is presentation-only — every action is bubbled up through an
    /// <see cref="EventCallback" /> to the hosting <see cref="EditParametricConstraint" />, which mutates the tree.
    /// </summary>
    public partial class BooleanExpressionNode
    {
        /// <summary>
        /// The logical operators offered on a group's header.
        /// </summary>
        private static readonly LogicalOperatorKind[] Operators = [LogicalOperatorKind.And, LogicalOperatorKind.Or, LogicalOperatorKind.Xor];

        /// <summary>
        /// Whether this group's "add" dropdown menu is open.
        /// </summary>
        private bool addMenuOpen;

        /// <summary>
        /// Gets a value indicating whether the row can move up. It cannot only when it is the very first item of the root
        /// group (anywhere else, moving up either swaps, descends into the group above, or pops out of a nested group).
        /// </summary>
        private bool CanMoveUp => this.Row.Parent != null && !(this.Row.Parent.Parent == null && this.Row.Parent.Terms.IndexOf(this.Row) == 0);

        /// <summary>
        /// Gets a value indicating whether the row can move down. It cannot only when it is the very last item of the root group.
        /// </summary>
        private bool CanMoveDown => this.Row.Parent != null && !(this.Row.Parent.Parent == null && this.Row.Parent.Terms.IndexOf(this.Row) == this.Row.Parent.Terms.Count - 1);

        /// <summary>
        /// Gets the element id of this group's "add" button, used to position the dropdown menu.
        /// </summary>
        private string AddButtonId => $"pc-add-{this.Row.Id:N}";

        /// <summary>
        /// Gets the CSS selector targeting this group's "add" button.
        /// </summary>
        private string AddButtonSelector => $"#{this.AddButtonId}";

        /// <summary>
        /// Closes the add menu and raises the request to add a relational expression to the given <paramref name="group" />.
        /// </summary>
        /// <param name="group">The group to add to.</param>
        /// <returns>A <see cref="Task" /></returns>
        private Task AddExpressionClicked(CompositeExpressionRow group)
        {
            this.addMenuOpen = false;
            return this.OnAddRelational.InvokeAsync(group);
        }

        /// <summary>
        /// Closes the add menu and raises the request to add a nested group to the given <paramref name="group" />.
        /// </summary>
        /// <param name="group">The group to add to.</param>
        /// <returns>A <see cref="Task" /></returns>
        private Task AddGroupClicked(CompositeExpressionRow group)
        {
            this.addMenuOpen = false;
            return this.OnAddGroup.InvokeAsync(group);
        }

        /// <summary>
        /// Gets or sets the row rendered by this node.
        /// </summary>
        [Parameter]
        public BooleanExpressionRow Row { get; set; }

        /// <summary>
        /// Gets or sets the view model used to compute the node's summary.
        /// </summary>
        [Parameter]
        public EditParametricConstraintViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is the root of the tree (root has no move/reorder actions).
        /// </summary>
        [Parameter]
        public bool IsRoot { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to add a relational expression to a group.
        /// </summary>
        [Parameter]
        public EventCallback<CompositeExpressionRow> OnAddRelational { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to add a nested group to a group.
        /// </summary>
        [Parameter]
        public EventCallback<CompositeExpressionRow> OnAddGroup { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to edit a relational expression.
        /// </summary>
        [Parameter]
        public EventCallback<RelationalExpressionRow> OnEditRelational { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to remove a node.
        /// </summary>
        [Parameter]
        public EventCallback<BooleanExpressionRow> OnRemove { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to move a node up among its siblings.
        /// </summary>
        [Parameter]
        public EventCallback<BooleanExpressionRow> OnMoveUp { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to move a node down among its siblings.
        /// </summary>
        [Parameter]
        public EventCallback<BooleanExpressionRow> OnMoveDown { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to toggle a node's negation.
        /// </summary>
        [Parameter]
        public EventCallback<BooleanExpressionRow> OnToggleNot { get; set; }

        /// <summary>
        /// Gets or sets the callback raised to change a group's logical operator.
        /// </summary>
        [Parameter]
        public EventCallback<(CompositeExpressionRow Group, LogicalOperatorKind Operator)> OnSetOperator { get; set; }
    }
}
