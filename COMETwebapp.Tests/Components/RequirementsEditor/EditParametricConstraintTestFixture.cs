// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParametricConstraintTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RequirementsEditor
{
    using System.Linq;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class EditParametricConstraintTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<EditParametricConstraint> renderer;
        private TextParameterType alpha;
        private TextParameterType beta;
        private ParametricConstraint constraint;
        private Guid firstRelationalIid;
        private bool saved;
        private bool cancelled;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.alpha = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "alpha", Name = "Alpha" };
            this.beta = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "beta", Name = "Beta" };

            // AddNode(null, ...) is called three times in a row: the first sets the root, the second wraps the
            // single-relational root into a new AND group, and the third appends to that (now composite) root.
            var editor = new EditParametricConstraintViewModel();
            editor.InitializeForNew();
            editor.AddNode(null, new RelationalExpressionRow { ParameterType = this.alpha, Operator = RelationalOperatorKind.GT, Value = "5" });
            editor.AddNode(null, new RelationalExpressionRow { ParameterType = this.beta, Operator = RelationalOperatorKind.LT, Value = "10" });
            editor.AddNode(null, new RelationalExpressionRow { ParameterType = this.alpha, Operator = RelationalOperatorKind.EQ, Value = "3", IsNegated = true });

            this.constraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            editor.BuildInto(this.constraint);

            this.firstRelationalIid = this.constraint.Expression.OfType<RelationalExpression>()
                .Single(x => x.ParameterType == this.alpha && x.RelationalOperator == RelationalOperatorKind.GT).Iid;

            this.saved = false;
            this.cancelled = false;

            this.renderer = this.context.Render<EditParametricConstraint>(parameters => parameters
                .Add(p => p.Constraint, this.constraint)
                .Add(p => p.AvailableParameterTypes, new ParameterType[] { this.alpha, this.beta })
                .Add(p => p.OnSaved, EventCallback.Factory.Create(this, () => this.saved = true))
                .Add(p => p.OnCancelled, EventCallback.Factory.Create(this, () => this.cancelled = true)));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyExistingConstraintTreeIsRendered()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Markup, Does.Contain("alpha &gt; 5").Or.Contain("alpha > 5"));
                Assert.That(this.renderer.Markup, Does.Contain("beta &lt; 10").Or.Contain("beta < 10"));
                Assert.That(this.renderer.Markup, Does.Contain("NOT (alpha = 3)"));
                Assert.That(this.renderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Save").Instance.Enabled, Is.True, "The initial tree is fully valid.");
            });
        }

        [Test]
        public async Task VerifyAddExpressionOpensDialogAndCancelDiscardsIt()
        {
            var addExpressionButton = this.renderer.Find(".pc-split-main");
            addExpressionButton.Click();

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.True);
                Assert.That(this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Text == "Save").Instance.Enabled, Is.False, "A brand new expression has no parameter type yet.");
            });

            var cancelButton = this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Text == "Cancel");
            await this.renderer.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.False);
                Assert.That(this.renderer.Markup, Does.Contain("alpha &gt; 5").Or.Contain("alpha > 5"));
                Assert.That(this.renderer.Markup, Does.Contain("beta &lt; 10").Or.Contain("beta < 10"));
                Assert.That(this.renderer.Markup, Does.Contain("NOT (alpha = 3)"), "Cancelling must not add a new expression to the group.");
            });
        }

        [Test]
        public async Task VerifyAddGroupAddsEmptyNestedGroupAndDisablesSave()
        {
            var arrowButton = this.renderer.Find(".pc-split-arrow");
            arrowButton.Click();

            var addGroupButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Add Group");
            await this.renderer.InvokeAsync(addGroupButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Markup, Does.Contain("A group requires at least two expressions."));
                Assert.That(this.renderer.Markup, Does.Contain("title=\"Delete group\""), "A non-root group offers a delete action.");
                Assert.That(this.renderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Save").Instance.Enabled, Is.False, "An incomplete nested group makes the whole tree invalid.");
            });
        }

        [Test]
        public async Task VerifyEditRelationalPrefillsDialogAndCancelDiscardsChanges()
        {
            var editButton = this.renderer.FindAll("button[title='Edit expression']").First();
            editButton.Click();

            var comboBox = this.renderer.FindComponent<DxComboBox<ParameterType, ParameterType>>();
            var textBox = this.renderer.FindComponent<DxTextBox>();

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.True);
                Assert.That(comboBox.Instance.Value, Is.EqualTo(this.alpha));
                Assert.That(textBox.Instance.Text, Is.EqualTo("5"));
            });

            var cancelButton = this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Text == "Cancel");
            await this.renderer.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.False);
                Assert.That(this.renderer.Markup, Does.Contain("alpha &gt; 5").Or.Contain("alpha > 5"), "Cancelling must not change the original expression.");
            });
        }

        [Test]
        public async Task VerifyEditRelationalConfirmUpdatesValueAndPersistsOnSave()
        {
            var editButton = this.renderer.FindAll("button[title='Edit expression']").First();
            editButton.Click();

            var textBox = this.renderer.FindComponent<DxTextBox>();
            await this.renderer.InvokeAsync(() => textBox.Instance.TextChanged.InvokeAsync("99"));

            var confirmButton = this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Text == "Save");
            Assert.That(confirmButton.Instance.Enabled, Is.True, "The parameter type is already set, only the value changed.");
            await this.renderer.InvokeAsync(confirmButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.False);
                Assert.That(this.renderer.Markup, Does.Contain("alpha &gt; 99").Or.Contain("alpha > 99"));
            });

            var saveButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Save");
            await this.renderer.InvokeAsync(saveButton.Instance.Click.InvokeAsync);

            var updatedExpression = (RelationalExpression)this.constraint.Expression.Single(x => x.Iid == this.firstRelationalIid);

            Assert.Multiple(() =>
            {
                Assert.That(this.saved, Is.True);
                Assert.That(updatedExpression.Value.Single(), Is.EqualTo("99"), "The expression keeps its identity and gets its value updated in place.");
            });
        }

        [Test]
        public async Task VerifyToggleNotAndSavePersistsNegationIntoConstraint()
        {
            var notButton = this.renderer.FindAll(".pc-leaf .pc-not").First();
            Assert.That(notButton.ClassList, Does.Not.Contain("pc-toggle-on"), "The first expression is not negated initially.");

            notButton.Click();

            var toggledNotButton = this.renderer.FindAll(".pc-leaf .pc-not").First();
            Assert.That(toggledNotButton.ClassList, Does.Contain("pc-toggle-on"));

            var saveButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Save");
            await this.renderer.InvokeAsync(saveButton.Instance.Click.InvokeAsync);

            var notExpression = this.constraint.Expression.OfType<NotExpression>().Single(x => x.Term.Iid == this.firstRelationalIid);

            Assert.Multiple(() =>
            {
                Assert.That(this.saved, Is.True);
                Assert.That(((RelationalExpression)notExpression.Term).ParameterType, Is.EqualTo(this.alpha));
            });
        }

        [Test]
        public void VerifyMoveDownSwapsSiblingOrderInMarkup()
        {
            var before = this.renderer.Markup;
            Assert.That(before.IndexOf("alpha &gt; 5", StringComparison.Ordinal), Is.LessThan(before.IndexOf("beta &lt; 10", StringComparison.Ordinal)));

            var moveDownButton = this.renderer.FindAll("button[title='Move down']").First();
            moveDownButton.Click();

            var after = this.renderer.Markup;
            Assert.That(after.IndexOf("beta &lt; 10", StringComparison.Ordinal), Is.LessThan(after.IndexOf("alpha &gt; 5", StringComparison.Ordinal)), "Moving the first expression down swaps it with its sibling.");
        }

        [Test]
        public void VerifyRemoveButtonRemovesNode()
        {
            var deleteButton = this.renderer.FindAll("button[title='Delete']").First();
            deleteButton.Click();

            Assert.That(this.renderer.Markup, Does.Not.Contain("alpha &gt; 5").And.Not.Contain("alpha > 5"));
        }

        [Test]
        public async Task VerifyCancelButtonInvokesOnCancelled()
        {
            var cancelButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Cancel");
            await this.renderer.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.That(this.cancelled, Is.True);
        }

        [Test]
        public async Task VerifyAddFirstExpressionOpensDialogForEmptyConstraint()
        {
            var emptyConstraint = new ParametricConstraint { Iid = Guid.NewGuid() };

            var emptyRenderer = this.context.Render<EditParametricConstraint>(parameters => parameters
                .Add(p => p.Constraint, emptyConstraint)
                .Add(p => p.AvailableParameterTypes, new ParameterType[] { this.alpha, this.beta }));

            Assert.That(emptyRenderer.Markup, Does.Contain("No expression defined."));

            var addFirstButton = emptyRenderer.FindComponent<DxButton>();
            await emptyRenderer.InvokeAsync(addFirstButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(emptyRenderer.Instance.IsRelationalDialogOpen, Is.True);
                Assert.That(emptyRenderer.FindComponents<DxButton>().Last(x => x.Instance.Text == "Save").Instance.Enabled, Is.False);
            });

            var cancelButton = emptyRenderer.FindComponents<DxButton>().Last(x => x.Instance.Text == "Cancel");
            await emptyRenderer.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(emptyRenderer.Instance.IsRelationalDialogOpen, Is.False);
                Assert.That(emptyRenderer.Markup, Does.Contain("No expression defined."), "Cancelling leaves the constraint empty.");
            });
        }

        [Test]
        public async Task VerifyRootAddGroupWrapsSingleExpressionIntoGroup()
        {
            var singleRenderer = this.RenderSingleRelationalRoot();

            var arrowButton = singleRenderer.Find(".pc-split-arrow");
            arrowButton.Click();

            var addGroupButton = singleRenderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Add Group");
            await singleRenderer.InvokeAsync(addGroupButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(singleRenderer.Markup, Does.Contain("A group requires at least two expressions."));
                Assert.That(singleRenderer.FindComponents<DxButton>().First(x => x.Instance.Text == "Save").Instance.Enabled, Is.False);
            });
        }

        [Test]
        public void VerifyRootAddExpressionOpensDialogForSecondExpression()
        {
            var singleRenderer = this.RenderSingleRelationalRoot();

            var rootAddExpressionButton = singleRenderer.Find(".pc-split-main");
            rootAddExpressionButton.Click();

            Assert.That(singleRenderer.Instance.IsRelationalDialogOpen, Is.True);
        }

        [Test]
        public async Task VerifyClosingRelationalDialogThroughItsPopupClosesIt()
        {
            var addExpressionButton = this.renderer.Find(".pc-split-main");
            addExpressionButton.Click();
            Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.True);

            var popup = this.renderer.FindComponent<DxPopup>();
            await this.renderer.InvokeAsync(() => popup.Instance.VisibleChanged.InvokeAsync(false));

            Assert.That(this.renderer.Instance.IsRelationalDialogOpen, Is.False);
        }

        /// <summary>
        /// Renders a <see cref="EditParametricConstraint" /> over a constraint whose root is a single, valid
        /// relational expression, so its "pc-root-add" split button is shown.
        /// </summary>
        /// <returns>The rendered component.</returns>
        private IRenderedComponent<EditParametricConstraint> RenderSingleRelationalRoot()
        {
            var singleEditor = new EditParametricConstraintViewModel();
            singleEditor.InitializeForNew();
            singleEditor.AddNode(null, new RelationalExpressionRow { ParameterType = this.alpha, Operator = RelationalOperatorKind.GT, Value = "5" });

            var singleConstraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            singleEditor.BuildInto(singleConstraint);

            return this.context.Render<EditParametricConstraint>(parameters => parameters
                .Add(p => p.Constraint, singleConstraint));
        }
    }
}
