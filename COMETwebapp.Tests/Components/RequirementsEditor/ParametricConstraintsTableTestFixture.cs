// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParametricConstraintsTableTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParametricConstraintsTableTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<ParametricConstraintsTable> renderer;
        private Requirement requirement;
        private TextParameterType parameterType;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.parameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "a", Name = "Acceleration" };

            var editor = new EditParametricConstraintViewModel();
            editor.InitializeForNew();
            editor.AddNode(null, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.GT, Value = "5" });
            editor.AddNode(null, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.LT, Value = "9" });

            var constraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            editor.BuildInto(constraint);

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R01", Name = "Requirement" };
            this.requirement.ParametricConstraint.Add(constraint);

            this.renderer = this.context.Render<ParametricConstraintsTable>(parameters => parameters
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.AvailableParameterTypes, new[] { (ParameterType)this.parameterType }));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyExistingConstraintSummaryIsRendered()
        {
            Assert.That(this.renderer.Markup, Does.Contain("a &gt; 5").Or.Contain("a > 5"));
        }

        [Test]
        public async Task VerifyAddOpensTheEditor()
        {
            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);

            Assert.That(this.renderer.Instance.IsEditorOpen, Is.True);
        }

        [Test]
        public async Task VerifyRemoveConstraint()
        {
            var removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeParametricConstraintButton");
            await this.renderer.InvokeAsync(removeButton.Instance.Click.InvokeAsync);

            var removalPopup = this.renderer.Instance.RemovalPopup;

            Assert.Multiple(() =>
            {
                Assert.That(removalPopup.IsVisible, Is.True, "The removal asks for confirmation first.");
                Assert.That(this.requirement.ParametricConstraint, Has.Count.EqualTo(1), "Nothing is removed before the user confirms.");
            });

            await this.renderer.InvokeAsync(removalPopup.Cancel);

            Assert.Multiple(() =>
            {
                Assert.That(removalPopup.IsVisible, Is.False);
                Assert.That(this.requirement.ParametricConstraint, Has.Count.EqualTo(1), "Cancelling keeps the constraint.");
            });

            await this.renderer.InvokeAsync(removeButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(removalPopup.Confirm);

            Assert.Multiple(() =>
            {
                Assert.That(this.requirement.ParametricConstraint, Is.Empty, "Confirming removes the constraint.");
                Assert.That(removalPopup.IsVisible, Is.False);
            });
        }

        [Test]
        public async Task VerifyNewConstraintIsAddedOnSave()
        {
            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            await this.renderer.InvokeAsync(this.renderer.Instance.HandleSaved);

            Assert.Multiple(() =>
            {
                Assert.That(this.requirement.ParametricConstraint, Has.Count.EqualTo(2), "The new constraint is appended on save.");
                Assert.That(this.renderer.Instance.IsEditorOpen, Is.False);
            });
        }

        [Test]
        public async Task VerifyEditingKeepsTheConstraintInPlace()
        {
            var original = this.requirement.ParametricConstraint.Single();

            await this.renderer.InvokeAsync(() => this.renderer.Instance.OpenEdit(original));
            await this.renderer.InvokeAsync(this.renderer.Instance.HandleSaved);

            // Editing keeps the same constraint (a real update), not a delete-and-recreate.
            Assert.That(this.requirement.ParametricConstraint.Single(), Is.SameAs(original));
        }

        [Test]
        public async Task VerifyHandleCancelledClosesTheEditorWithoutSaving()
        {
            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            Assert.That(this.renderer.Instance.IsEditorOpen, Is.True);

            await this.renderer.InvokeAsync(this.renderer.Instance.HandleCancelled);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsEditorOpen, Is.False);
                Assert.That(this.requirement.ParametricConstraint, Has.Count.EqualTo(1), "A cancelled add is not appended to the requirement.");
            });
        }

        [Test]
        public void VerifyControlsFollowTheParentWritePermission()
        {
            // These rows are parts of the Requirement and are saved atomically by the hosting form, so the permission is
            // decided once by that form and passed down. Without this a user could stage edits into a dialog whose Save
            // is withdrawn, and only discover it at the end.
            var addButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addParametricConstraintButton");
            var removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeParametricConstraintButton");

            Assert.Multiple(() =>
            {
                Assert.That(addButton.Instance.Enabled, Is.True, "the default keeps a host that does not set the flag unchanged");
                Assert.That(removeButton.Instance.Enabled, Is.True);
            });

            this.renderer.Render(parameters => parameters
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.AvailableParameterTypes, new[] { (ParameterType)this.parameterType })
                .Add(p => p.IsAllowedToWrite, false));

            addButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addParametricConstraintButton");
            removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeParametricConstraintButton");

            Assert.Multiple(() =>
            {
                Assert.That(addButton.Instance.Enabled, Is.False, "the user may not add constraints to a thing they cannot write");
                Assert.That(removeButton.Instance.Enabled, Is.False, "the user may not remove constraints from a thing they cannot write");
            });
        }

        [Test]
        public async Task VerifyClosingTheEditorPopupClosesIt()
        {
            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            this.renderer.Render();
            Assert.That(this.renderer.Instance.IsEditorOpen, Is.True);

            var popup = this.renderer.FindComponents<DxPopup>().First(x => x.Instance.HeaderText.Contains("Parametric Constraint"));
            await this.renderer.InvokeAsync(() => popup.Instance.VisibleChanged.InvokeAsync(false));

            Assert.That(this.renderer.Instance.IsEditorOpen, Is.False, "Closing the popup (e.g. via its close button) closes the editor.");
        }
    }
}
