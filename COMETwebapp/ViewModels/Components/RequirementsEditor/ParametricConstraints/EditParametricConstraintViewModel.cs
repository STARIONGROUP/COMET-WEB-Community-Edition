// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParametricConstraintViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Utilities;

    /// <summary>
    /// Drives the parametric-constraint tree editor: it owns the in-memory <see cref="RootExpression" /> tree of
    /// <see cref="BooleanExpressionRow" />s, the operations that mutate it (add, remove, move, negate, change operator),
    /// its validation, and the conversion to and from the SDK <see cref="BooleanExpression" /> graph.
    /// </summary>
    public class EditParametricConstraintViewModel
    {
        /// <summary>
        /// Gets the root of the expression tree, or null when the constraint has no expression yet.
        /// </summary>
        public BooleanExpressionRow RootExpression { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" />s available when editing a relational expression.
        /// </summary>
        public IReadOnlyList<ParameterType> AvailableParameterTypes { get; set; } = [];

        /// <summary>
        /// Gets a value indicating whether the tree is a complete, valid expression that can be saved.
        /// </summary>
        public bool CanSave => this.RootExpression != null && this.RootExpression.IsValid;

        /// <summary>
        /// Resets the editor to an empty constraint.
        /// </summary>
        public void InitializeForNew()
        {
            this.RootExpression = null;
        }

        /// <summary>
        /// Loads the tree from an existing <paramref name="constraint" />.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /> to edit.</param>
        public void LoadFrom(ParametricConstraint constraint)
        {
            var topLevelExpressions = constraint.Expression.GetTopLevelExpressions();
            var top = constraint.TopExpression ?? (topLevelExpressions.Count == 0 ? null : topLevelExpressions[0]);
            this.RootExpression = top == null ? null : ConvertToRow(top, null, false);
        }

        /// <summary>
        /// Adds the given <paramref name="node" /> under <paramref name="parent" />; when <paramref name="parent" /> is
        /// null it becomes (or is merged into) the root: an empty tree takes the node as its root, a leaf root is wrapped
        /// with the node in a new AND group, and a group root receives the node as a new term.
        /// </summary>
        /// <param name="parent">The parent group, or null to add at the root.</param>
        /// <param name="node">The node to add.</param>
        public void AddNode(CompositeExpressionRow parent, BooleanExpressionRow node)
        {
            if (parent != null)
            {
                node.Parent = parent;
                parent.Terms.Add(node);
                return;
            }

            switch (this.RootExpression)
            {
                case null:
                    this.RootExpression = node;
                    return;
                case CompositeExpressionRow rootComposite:
                    node.Parent = rootComposite;
                    rootComposite.Terms.Add(node);
                    return;
                default:
                    var wrapper = new CompositeExpressionRow { Operator = LogicalOperatorKind.And };
                    var previousRoot = this.RootExpression;
                    previousRoot.Parent = wrapper;
                    node.Parent = wrapper;
                    wrapper.Terms.Add(previousRoot);
                    wrapper.Terms.Add(node);
                    this.RootExpression = wrapper;
                    return;
            }
        }

        /// <summary>
        /// Adds a new empty AND group under <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">The parent group.</param>
        /// <returns>The created group.</returns>
        public CompositeExpressionRow AddGroup(CompositeExpressionRow parent)
        {
            var group = new CompositeExpressionRow { Operator = LogicalOperatorKind.And };
            this.AddNode(parent, group);
            return group;
        }

        /// <summary>
        /// Removes the given <paramref name="node" />, collapsing a group left with a single child and pruning one left empty.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public void Remove(BooleanExpressionRow node)
        {
            if (node == this.RootExpression)
            {
                this.RootExpression = null;
                return;
            }

            var parent = node.Parent;

            if (parent == null)
            {
                return;
            }

            parent.Terms.Remove(node);

            if (parent.Terms.Count == 1)
            {
                this.CollapseGroup(parent);
            }
            else if (parent.Terms.Count == 0)
            {
                this.Remove(parent);
            }
        }

        /// <summary>
        /// Moves the given <paramref name="node" /> one step up in the tree: it swaps with a leaf sibling above, descends
        /// into the group directly above, or — when it is the first child of a group — pops out just before that group in
        /// the grandparent.
        /// </summary>
        /// <param name="node">The node to move.</param>
        public void MoveUp(BooleanExpressionRow node)
        {
            var parent = node.Parent;

            if (parent == null)
            {
                return;
            }

            var index = parent.Terms.IndexOf(node);

            if (index > 0)
            {
                if (parent.Terms[index - 1] is CompositeExpressionRow groupAbove)
                {
                    parent.Terms.RemoveAt(index);
                    node.Parent = groupAbove;
                    groupAbove.Terms.Add(node);
                }
                else
                {
                    (parent.Terms[index], parent.Terms[index - 1]) = (parent.Terms[index - 1], parent.Terms[index]);
                    return;
                }
            }
            else if (parent.Parent != null)
            {
                PromoteOut(node, parent, before: true);
            }
            else
            {
                return;
            }

            this.CleanupAfterMove(parent);
        }

        /// <summary>
        /// Moves the given <paramref name="node" /> one step down in the tree: it swaps with a leaf sibling below, descends
        /// into the group directly below, or — when it is the last child of a group — pops out just after that group in
        /// the grandparent.
        /// </summary>
        /// <param name="node">The node to move.</param>
        public void MoveDown(BooleanExpressionRow node)
        {
            var parent = node.Parent;

            if (parent == null)
            {
                return;
            }

            var index = parent.Terms.IndexOf(node);

            if (index < parent.Terms.Count - 1)
            {
                if (parent.Terms[index + 1] is CompositeExpressionRow groupBelow)
                {
                    parent.Terms.RemoveAt(index);
                    node.Parent = groupBelow;
                    groupBelow.Terms.Insert(0, node);
                }
                else
                {
                    (parent.Terms[index], parent.Terms[index + 1]) = (parent.Terms[index + 1], parent.Terms[index]);
                    return;
                }
            }
            else if (parent.Parent != null)
            {
                PromoteOut(node, parent, before: false);
            }
            else
            {
                return;
            }

            this.CleanupAfterMove(parent);
        }

        /// <summary>
        /// Toggles the negation (NOT) of the given <paramref name="node" />.
        /// </summary>
        /// <param name="node">The node to negate.</param>
        public static void ToggleNot(BooleanExpressionRow node)
        {
            node.IsNegated = !node.IsNegated;
        }

        /// <summary>
        /// Sets the logical operator of the given <paramref name="group" />.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="operatorKind">The operator to set.</param>
        public static void SetOperator(CompositeExpressionRow group, LogicalOperatorKind operatorKind)
        {
            group.Operator = operatorKind;
        }

        /// <summary>
        /// Gets a one-line human-readable summary of the given <paramref name="node" /> subtree.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The summary string.</returns>
        public string GetSummary(BooleanExpressionRow node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            string inner;

            if (node is RelationalExpressionRow relational)
            {
                var scale = relational.Scale == null ? string.Empty : $" {relational.Scale.ShortName}";
                var parameterType = relational.ParameterType?.ShortName ?? "?";
                var value = string.IsNullOrWhiteSpace(relational.Value) ? "?" : relational.Value;
                inner = $"{parameterType} {relational.Operator.ToScientificNotationString()} {value}{scale}";
            }
            else
            {
                var group = (CompositeExpressionRow)node;

                var separator = group.Operator switch
                {
                    LogicalOperatorKind.Or => " OR ",
                    LogicalOperatorKind.Xor => " XOR ",
                    _ => " AND "
                };

                inner = group.Terms.Count == 0 ? "empty group" : string.Join(separator, group.Terms.Select(x => $"({this.GetSummary(x)})"));
            }

            return node.IsNegated ? $"NOT ({inner})" : inner;
        }

        /// <summary>
        /// Rebuilds the <paramref name="constraint" />'s <see cref="ParametricConstraint.Expression" /> graph and
        /// <see cref="ParametricConstraint.TopExpression" /> from the current tree.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /> to write into.</param>
        /// <returns>The created <see cref="BooleanExpression" />s (already added to the constraint).</returns>
        public IReadOnlyList<BooleanExpression> BuildInto(ParametricConstraint constraint)
        {
            var existing = new Dictionary<Guid, BooleanExpression>();

            foreach (var expression in constraint.Expression)
            {
                existing[expression.Iid] = expression;
            }

            constraint.Expression.Clear();

            if (this.RootExpression == null)
            {
                constraint.TopExpression = null;
                return [];
            }

            var created = new List<BooleanExpression>();
            var top = BuildExpression(this.RootExpression, created, existing, []);

            foreach (var expression in created)
            {
                constraint.Expression.Add(expression);
            }

            constraint.TopExpression = top;
            return created;
        }

        /// <summary>
        /// Pops the given <paramref name="node" /> out of <paramref name="parent" /> into the grandparent, placed just
        /// before (<paramref name="before" /> is true) or after the parent group.
        /// </summary>
        /// <param name="node">The node to move out.</param>
        /// <param name="parent">The group the node currently belongs to.</param>
        /// <param name="before">True to place the node before the parent group, false to place it after.</param>
        private static void PromoteOut(BooleanExpressionRow node, CompositeExpressionRow parent, bool before)
        {
            var grandparent = parent.Parent;
            var parentIndex = grandparent.Terms.IndexOf(parent);
            parent.Terms.Remove(node);
            node.Parent = grandparent;
            grandparent.Terms.Insert(before ? parentIndex : parentIndex + 1, node);
        }

        /// <summary>
        /// Prunes a group left completely empty after a node was moved out of it. A group left with a single child is
        /// deliberately kept (shown as an incomplete group) rather than collapsed, so moving an expression out of a group
        /// does not silently dissolve the group.
        /// </summary>
        /// <param name="group">The group the node was moved out of.</param>
        private void CleanupAfterMove(CompositeExpressionRow group)
        {
            if (group.Terms.Count == 0)
            {
                this.Remove(group);
            }
        }

        /// <summary>
        /// Replaces the given <paramref name="group" /> (left with a single child) by that child in its parent or as the root.
        /// </summary>
        /// <param name="group">The group to collapse.</param>
        private void CollapseGroup(CompositeExpressionRow group)
        {
            var remaining = group.Terms[0];
            var grandparent = group.Parent;

            if (grandparent == null)
            {
                remaining.Parent = null;
                this.RootExpression = remaining;
                return;
            }

            var index = grandparent.Terms.IndexOf(group);
            grandparent.Terms[index] = remaining;
            remaining.Parent = grandparent;
        }

        /// <summary>
        /// Recursively converts a <paramref name="row" /> to its SDK <see cref="BooleanExpression" />, collecting every
        /// created expression into <paramref name="created" /> and wrapping negated rows in a <see cref="NotExpression" />.
        /// </summary>
        /// <param name="row">The row to convert.</param>
        /// <param name="created">The accumulator of created expressions.</param>
        /// <param name="existing">The expressions the constraint held before the rebuild.</param>
        /// <param name="fresh">The identifiers of the expressions minted during this rebuild.</param>
        /// <returns>The created (possibly negated) expression.</returns>
        private static BooleanExpression BuildExpression(BooleanExpressionRow row, List<BooleanExpression> created, Dictionary<Guid, BooleanExpression> existing, HashSet<Guid> fresh)
        {
            BooleanExpression expression;

            if (row is RelationalExpressionRow relational)
            {
                var relationalExpression = TryReuse<RelationalExpression>(relational.SourceIid, existing) ?? Mint(new RelationalExpression { Iid = Guid.NewGuid() }, fresh);
                relationalExpression.ParameterType = relational.ParameterType;
                relationalExpression.RelationalOperator = relational.Operator;
                relationalExpression.Value = new ValueArray<string>([relational.Value]);
                relationalExpression.Scale = relational.Scale;
                expression = relationalExpression;
            }
            else
            {
                var group = (CompositeExpressionRow)row;
                var terms = group.Terms.Select(x => BuildExpression(x, created, existing, fresh)).ToList();
                var reusable = terms.TrueForAll(x => !fresh.Contains(x.Iid));

                expression = group.Operator switch
                {
                    LogicalOperatorKind.Or => Reuse<OrExpression>(group.SourceIid, existing, reusable) ?? Mint(new OrExpression { Iid = Guid.NewGuid() }, fresh),
                    LogicalOperatorKind.Xor => Reuse<ExclusiveOrExpression>(group.SourceIid, existing, reusable) ?? Mint(new ExclusiveOrExpression { Iid = Guid.NewGuid() }, fresh),
                    _ => Reuse<AndExpression>(group.SourceIid, existing, reusable) ?? Mint(new AndExpression { Iid = Guid.NewGuid() }, fresh)
                };

                switch (expression)
                {
                    case AndExpression andExpression:
                        andExpression.Term.Clear();
                        andExpression.Term.AddRange(terms);
                        break;
                    case OrExpression orExpression:
                        orExpression.Term.Clear();
                        orExpression.Term.AddRange(terms);
                        break;
                    case ExclusiveOrExpression exclusiveOrExpression:
                        exclusiveOrExpression.Term.Clear();
                        exclusiveOrExpression.Term.AddRange(terms);
                        break;
                }
            }

            created.Add(expression);

            if (!row.IsNegated)
            {
                return expression;
            }

            var notExpression = Reuse<NotExpression>(row.SourceNotIid, existing, !fresh.Contains(expression.Iid)) ?? Mint(new NotExpression { Iid = Guid.NewGuid() }, fresh);
            notExpression.Term = expression;
            created.Add(notExpression);
            return notExpression;
        }

        /// <summary>
        /// Records the given freshly minted <paramref name="expression" /> in <paramref name="fresh" />.
        /// </summary>
        /// <typeparam name="T">The expression type.</typeparam>
        /// <param name="expression">The newly created expression.</param>
        /// <param name="fresh">The identifiers of the expressions minted during this rebuild.</param>
        /// <returns>The given <paramref name="expression" />.</returns>
        private static T Mint<T>(T expression, HashSet<Guid> fresh) where T : BooleanExpression
        {
            fresh.Add(expression.Iid);
            return expression;
        }

        /// <summary>
        /// Returns the existing expression with the given <paramref name="sourceIid" /> when <paramref name="reusable" /> is
        /// true and it can be updated in place, otherwise null so that a new expression is minted.
        /// </summary>
        /// <typeparam name="T">The expected expression type.</typeparam>
        /// <param name="sourceIid">The identifier of the expression this row was loaded from, or null.</param>
        /// <param name="existing">The expressions the constraint held before the rebuild.</param>
        /// <param name="reusable">Whether the expression may keep its identity.</param>
        /// <returns>The reusable expression, or null.</returns>
        private static T Reuse<T>(Guid? sourceIid, Dictionary<Guid, BooleanExpression> existing, bool reusable) where T : BooleanExpression
        {
            return reusable ? TryReuse<T>(sourceIid, existing) : null;
        }

        /// <summary>
        /// Returns the existing expression with the given <paramref name="sourceIid" /> when it is present and of the
        /// requested type <typeparamref name="T" /> (so it can be updated in place), otherwise null.
        /// </summary>
        /// <typeparam name="T">The expected expression type.</typeparam>
        /// <param name="sourceIid">The identifier of the expression this row was loaded from, or null.</param>
        /// <param name="existing">The expressions the constraint held before the rebuild.</param>
        /// <returns>The reusable expression, or null.</returns>
        private static T TryReuse<T>(Guid? sourceIid, Dictionary<Guid, BooleanExpression> existing) where T : BooleanExpression
        {
            return sourceIid.HasValue && existing.TryGetValue(sourceIid.Value, out var expression) && expression is T typed ? typed : null;
        }

        /// <summary>
        /// Recursively converts an SDK <paramref name="expression" /> to a <see cref="BooleanExpressionRow" />, unwrapping
        /// a <see cref="NotExpression" /> into the negation of its term.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <param name="parent">The parent group of the produced row.</param>
        /// <param name="negated">Whether the produced row is negated.</param>
        /// <param name="notIid">The identifier of the wrapping <see cref="NotExpression" /> when negated, else null.</param>
        /// <returns>The produced row.</returns>
        private static BooleanExpressionRow ConvertToRow(BooleanExpression expression, CompositeExpressionRow parent, bool negated, Guid? notIid = null)
        {
            switch (expression)
            {
                case NotExpression { Term: not null } notExpression:
                    return ConvertToRow(notExpression.Term, parent, true, notExpression.Iid);

                case RelationalExpression relationalExpression:
                    return new RelationalExpressionRow
                    {
                        Parent = parent,
                        IsNegated = negated,
                        SourceIid = relationalExpression.Iid,
                        SourceNotIid = notIid,
                        ParameterType = relationalExpression.ParameterType,
                        Operator = relationalExpression.RelationalOperator,
                        Value = string.Join(", ", relationalExpression.Value),
                        Scale = relationalExpression.Scale
                    };

                default:
                    var operatorKind = expression switch
                    {
                        OrExpression => LogicalOperatorKind.Or,
                        ExclusiveOrExpression => LogicalOperatorKind.Xor,
                        _ => LogicalOperatorKind.And
                    };

                    var group = new CompositeExpressionRow { Parent = parent, IsNegated = negated, Operator = operatorKind, SourceIid = expression.Iid, SourceNotIid = notIid };

                    foreach (var term in GetTerms(expression))
                    {
                        group.Terms.Add(ConvertToRow(term, group, false));
                    }

                    return group;
            }
        }

        /// <summary>
        /// Gets the child terms of the given <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The child expressions.</returns>
        private static List<BooleanExpression> GetTerms(BooleanExpression expression)
        {
            return expression switch
            {
                AndExpression andExpression => andExpression.Term,
                OrExpression orExpression => orExpression.Term,
                ExclusiveOrExpression exclusiveOrExpression => exclusiveOrExpression.Term,
                NotExpression { Term: not null } notExpression => [notExpression.Term],
                _ => []
            };
        }
    }
}
