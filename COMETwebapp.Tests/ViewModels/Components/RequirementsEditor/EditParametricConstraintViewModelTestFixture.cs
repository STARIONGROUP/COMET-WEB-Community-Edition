// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParametricConstraintViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using NUnit.Framework;

    [TestFixture]
    public class EditParametricConstraintViewModelTestFixture
    {
        private EditParametricConstraintViewModel viewModel;
        private TextParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            this.viewModel = new EditParametricConstraintViewModel();
            this.viewModel.InitializeForNew();
            this.parameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "a", Name = "Acceleration" };
        }

        private RelationalExpressionRow NewRelational(string value)
        {
            return new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.EQ, Value = value };
        }

        [Test]
        public void VerifyAddingASecondExpressionWrapsTheRootInAGroup()
        {
            Assert.That(this.viewModel.CanSave, Is.False, "An empty constraint is not saveable.");

            var first = this.NewRelational("5");
            this.viewModel.AddNode(null, first);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.RootExpression, Is.SameAs(first));
                Assert.That(this.viewModel.CanSave, Is.True, "A single valid relational expression is saveable.");
            });

            var second = this.NewRelational("6");
            this.viewModel.AddNode(null, second);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.RootExpression, Is.InstanceOf<CompositeExpressionRow>());
                var group = (CompositeExpressionRow)this.viewModel.RootExpression;
                Assert.That(group.Operator, Is.EqualTo(LogicalOperatorKind.And));
                Assert.That(group.Terms, Is.EqualTo(new BooleanExpressionRow[] { first, second }));
                Assert.That(first.Parent, Is.SameAs(group));
                Assert.That(this.viewModel.CanSave, Is.True);
            });
        }

        [Test]
        public void VerifyGroupValidationRequiresTwoValidTerms()
        {
            var group = new CompositeExpressionRow();
            this.viewModel.AddNode(null, group);

            Assert.That(group.IsValid, Is.False, "An empty group is invalid.");

            var incomplete = new RelationalExpressionRow { ParameterType = this.parameterType };
            this.viewModel.AddNode(group, incomplete);
            this.viewModel.AddNode(group, this.NewRelational("5"));

            Assert.That(group.IsValid, Is.False, "A group with an incomplete relational term (no value) is invalid.");

            incomplete.Value = "3";
            Assert.That(group.IsValid, Is.True);
        }

        [Test]
        public void VerifyRemovingATermCollapsesTheGroup()
        {
            var first = this.NewRelational("5");
            var second = this.NewRelational("6");
            this.viewModel.AddNode(null, first);
            this.viewModel.AddNode(null, second);

            this.viewModel.Remove(first);

            Assert.That(this.viewModel.RootExpression, Is.SameAs(second), "A group left with one child collapses to that child.");
        }

        [Test]
        public void VerifyMoveUpAndDownReorderSiblings()
        {
            var first = this.NewRelational("1");
            var second = this.NewRelational("2");
            var third = this.NewRelational("3");
            this.viewModel.AddNode(null, first);
            this.viewModel.AddNode(null, second);
            var group = (CompositeExpressionRow)this.viewModel.RootExpression;
            this.viewModel.AddNode(group, third);

            this.viewModel.MoveUp(third);
            Assert.That(group.Terms, Is.EqualTo(new BooleanExpressionRow[] { first, third, second }));

            this.viewModel.MoveDown(first);
            Assert.That(group.Terms, Is.EqualTo(new BooleanExpressionRow[] { third, first, second }));
        }

        [Test]
        public void VerifyMovingCrossesNestedGroupBoundaries()
        {
            var leaf1 = this.NewRelational("1");
            var leaf2 = this.NewRelational("2");
            this.viewModel.AddNode(null, leaf1);
            this.viewModel.AddNode(null, leaf2);

            var root = (CompositeExpressionRow)this.viewModel.RootExpression;
            var group = this.viewModel.AddGroup(root);
            this.viewModel.AddNode(group, this.NewRelational("3"));
            this.viewModel.AddNode(group, this.NewRelational("4"));

            // Moving leaf2 down, with the group as its lower neighbour, descends it into the group (at the front).
            this.viewModel.MoveDown(leaf2);

            Assert.Multiple(() =>
            {
                Assert.That(leaf2.Parent, Is.SameAs(group));
                Assert.That(group.Terms.First(), Is.SameAs(leaf2));
                Assert.That(root.Terms, Does.Not.Contain(leaf2));
            });

            // Moving it up again, from the top of the group, pops it back out just before the group.
            this.viewModel.MoveUp(leaf2);

            Assert.Multiple(() =>
            {
                Assert.That(leaf2.Parent, Is.SameAs(root));
                Assert.That(group.Terms, Does.Not.Contain(leaf2));
                Assert.That(root.Terms.IndexOf(leaf2), Is.LessThan(root.Terms.IndexOf(group)));
            });
        }

        [Test]
        public void VerifyMovingOutOfATwoTermGroupKeepsTheGroup()
        {
            var a = this.NewRelational("a");
            var b = this.NewRelational("b");
            this.viewModel.AddNode(null, a);
            this.viewModel.AddNode(null, b);

            var root = (CompositeExpressionRow)this.viewModel.RootExpression;
            var group = this.viewModel.AddGroup(root);
            var c = this.NewRelational("c");
            var d = this.NewRelational("d");
            this.viewModel.AddNode(group, c);
            this.viewModel.AddNode(group, d);

            // Popping the first term out of the group must NOT remove the group (its remaining term stays in it).
            this.viewModel.MoveUp(c);

            Assert.Multiple(() =>
            {
                Assert.That(root.Terms, Does.Contain(group));
                Assert.That(group.Terms, Is.EqualTo(new[] { d }));
                Assert.That(d.Parent, Is.SameAs(group));
                Assert.That(c.Parent, Is.SameAs(root));
                Assert.That(root.Terms.IndexOf(c), Is.LessThan(root.Terms.IndexOf(group)));
            });
        }

        [Test]
        public void VerifyBuildIntoProducesTheSdkExpressionGraph()
        {
            var first = this.NewRelational("5");
            var second = this.NewRelational("6");
            this.viewModel.AddNode(null, first);
            this.viewModel.AddNode(null, second);
            EditParametricConstraintViewModel.ToggleNot(second);
            EditParametricConstraintViewModel.SetOperator((CompositeExpressionRow)this.viewModel.RootExpression, LogicalOperatorKind.Or);

            var constraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            this.viewModel.BuildInto(constraint);

            Assert.Multiple(() =>
            {
                Assert.That(constraint.TopExpression, Is.InstanceOf<OrExpression>());
                Assert.That(constraint.Expression.OfType<RelationalExpression>().Count(), Is.EqualTo(2));
                Assert.That(constraint.Expression.OfType<NotExpression>().Count(), Is.EqualTo(1), "The negated term is wrapped in a NotExpression.");
                Assert.That(((OrExpression)constraint.TopExpression).Term, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void VerifyLoadFromRoundTripsANegatedGroup()
        {
            var source = this.NewRelational("5");
            var negated = this.NewRelational("6");
            this.viewModel.AddNode(null, source);
            this.viewModel.AddNode(null, negated);
            EditParametricConstraintViewModel.ToggleNot(negated);

            var constraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            this.viewModel.BuildInto(constraint);

            var loaded = new EditParametricConstraintViewModel();
            loaded.LoadFrom(constraint);

            Assert.Multiple(() =>
            {
                Assert.That(loaded.RootExpression, Is.InstanceOf<CompositeExpressionRow>());
                var group = (CompositeExpressionRow)loaded.RootExpression;
                Assert.That(group.Terms, Has.Count.EqualTo(2));
                Assert.That(group.Terms.OfType<RelationalExpressionRow>().Count(x => x.IsNegated), Is.EqualTo(1), "The NotExpression is restored as a negated relational row.");
                Assert.That(loaded.CanSave, Is.True);
            });
        }

        [Test]
        public void VerifyGetSummaryRendersTheTree()
        {
            var first = this.NewRelational("5");
            var second = this.NewRelational("6");
            this.viewModel.AddNode(null, first);
            this.viewModel.AddNode(null, second);
            EditParametricConstraintViewModel.ToggleNot(second);

            Assert.That(this.viewModel.GetSummary(this.viewModel.RootExpression), Is.EqualTo("(a = 5) AND (NOT (a = 6))"));
        }
    }
}
