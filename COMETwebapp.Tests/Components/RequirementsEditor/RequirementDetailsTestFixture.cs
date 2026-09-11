// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementDetailsTestFixture.cs" company="Starion Group S.A.">
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
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using DynamicData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementDetailsTestFixture
    {
        private BunitContext context;
        private CDPMessageBus messageBus;
        private RequirementsEditorBodyViewModel viewModel;
        private Requirement requirement;
        private Requirement linkedRequirement;
        private Requirement deprecatedRequirement;
        private AndExpression andExpression;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.messageBus = new CDPMessageBus();

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var massType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var lengthType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "l", Name = "length" };
            var kilogram = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg", Name = "kilogram" };

            var boundRelational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LE, Scale = kilogram, Value = new ValueArray<string>(["100"]) };
            var notInner = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = lengthType, RelationalOperator = RelationalOperatorKind.GE, Value = new ValueArray<string>(["2"]) };
            var orLeftExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.EQ, Value = new ValueArray<string>(["1"]) };
            var orRightExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.NE, Value = new ValueArray<string>(["0"]) };
            var xorLeftExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LT, Value = new ValueArray<string>(["9"]) };
            var xorRightExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.GT, Value = new ValueArray<string>(["3"]) };

            var notExpression = new NotExpression { Iid = Guid.NewGuid(), Term = notInner };
            var orExpression = new OrExpression { Iid = Guid.NewGuid(), Term = { orLeftExpression, orRightExpression } };
            var xorExpression = new ExclusiveOrExpression { Iid = Guid.NewGuid(), Term = { xorLeftExpression, xorRightExpression } };
            this.andExpression = new AndExpression { Iid = Guid.NewGuid(), Term = { notExpression, orExpression, xorExpression, boundRelational } };

            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), TopExpression = this.andExpression };
            constraint.Expression.AddRange([this.andExpression, notExpression, orExpression, xorExpression, boundRelational, notInner, orLeftExpression, orRightExpression, xorLeftExpression, xorRightExpression]);

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement", Owner = domain };
            this.requirement.ParametricConstraint.Add(constraint);

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = massType };
            parameter.ValueSet.Add(new ParameterValueSet { Iid = Guid.NewGuid(), Published = new ValueArray<string>(["42"]) });
            elementDefinition.Parameter.Add(parameter);

            this.linkedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R2", Name = "Second requirement" };
            this.deprecatedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R0", Name = "Old requirement", IsDeprecated = true };

            var specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "SPEC", Name = "Specification" };
            specification.Requirement.AddRange([this.requirement, this.linkedRequirement, this.deprecatedRequirement]);

            var iteration = new Iteration { Iid = Guid.NewGuid() };
            iteration.RequirementsSpecification.Add(specification);
            iteration.Element.Add(elementDefinition);
            iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = parameter, Target = boundRelational });
            iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.requirement, Target = elementDefinition });
            iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.linkedRequirement, Target = this.requirement });
            iteration.Relationship.Add(new MultiRelationship { Iid = Guid.NewGuid(), RelatedThing = { this.requirement, this.deprecatedRequirement } });

            var openIterations = new SourceList<Iteration>();
            openIterations.Add(iteration);
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.OpenIterations).Returns(openIterations);
            var session = new Mock<ISession>();
            session.Setup(x => x.OpenReferenceDataLibraries).Returns([]);
            sessionService.Setup(x => x.Session).Returns(session.Object);

            this.viewModel = new RequirementsEditorBodyViewModel(sessionService.Object, this.messageBus, new ShowHideDeprecatedThingsService(), new Mock<ILogger<RequirementsEditorBodyViewModel>>().Object, new Mock<IExportService>().Object)
            {
                CurrentThing = iteration,
                ShowParametricConstraints = true,
                ShowTraceability = true
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        private IRenderedComponent<RequirementDetails> Render()
        {
            return this.context.Render<RequirementDetails>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel)
                .Add(p => p.Requirement, this.requirement));
        }

        [Test]
        public void VerifyConstraintTreeRenders()
        {
            var component = this.Render();
            var markup = component.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("Constraints"));
                Assert.That(markup, Does.Contain("AND"));
                Assert.That(markup, Does.Contain("OR"));
                Assert.That(markup, Does.Contain("XOR"));
                Assert.That(markup, Does.Contain("NOT"));
                Assert.That(markup, Does.Contain("≤"), "the relational operator symbol must render");
                Assert.That(markup, Does.Contain("SAT.m"), "the bound parameter model code must render");
                Assert.That(markup, Does.Contain("= 42"), "the bound parameter published value must render");
                Assert.That(component.FindAll(".req-expr-toggle"), Is.Not.Empty);
            });
        }

        [Test]
        public void VerifyTraceabilityRenders()
        {
            var component = this.Render();
            var markup = component.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("Traceability"));
                Assert.That(markup, Does.Contain("→"), "an outgoing relationship arrow must render");
                Assert.That(markup, Does.Contain("←"), "an incoming relationship arrow must render");
                Assert.That(markup, Does.Contain("↔"), "a multi relationship arrow must render");
                Assert.That(markup, Does.Contain("Satellite"), "the related element definition label must render");
                Assert.That(markup, Does.Contain("SPEC · R2"), "a related requirement renders as a specification-qualified link");
                Assert.That(component.FindAll(".req-link-req.req-row-deprecated"), Is.Not.Empty, "a deprecated linked requirement is styled");
            });
        }

        [Test]
        public void VerifyCollapsingAnExpressionHidesItsChildren()
        {
            var component = this.Render();

            Assert.That(this.viewModel.IsExpressionCollapsed(this.andExpression.Iid), Is.False);

            // the top AND node's toggle is the first one
            component.FindAll(".req-expr-toggle")[0].Click();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsExpressionCollapsed(this.andExpression.Iid), Is.True);
                Assert.That(component.Markup, Does.Contain("Collapsed.svg"), "the collapsed node shows the collapsed chevron");
            });
        }

        [Test]
        public void VerifyClickingARequirementLinkNavigates()
        {
            var component = this.Render();

            component.Find(".req-link-req").Click();

            Assert.That(this.viewModel.ScrollTarget, Is.Not.Null, "clicking a requirement link navigates to it");
        }

        [Test]
        public async Task VerifyParameterLinkPicker()
        {
            var massType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var relational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), TopExpression = relational };
            constraint.Expression.Add(relational);

            var linkedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R3", Name = "Third requirement" };
            linkedRequirement.ParametricConstraint.Add(constraint);

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            var boundParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = massType };
            var unboundParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = massType };
            elementDefinition.Parameter.AddRange([boundParameter, unboundParameter]);

            var mockedViewModel = new Mock<IRequirementsEditorBodyViewModel>();
            mockedViewModel.Setup(x => x.ShowParametricConstraints).Returns(true);
            mockedViewModel.Setup(x => x.GetTopExpressions(constraint)).Returns([relational]);
            mockedViewModel.Setup(x => x.GetBoundParameters(relational)).Returns([boundParameter]);
            mockedViewModel.Setup(x => x.GetPublishedValue(It.IsAny<ParameterOrOverrideBase>())).Returns((string)null);
            mockedViewModel.Setup(x => x.GetLinkableParameters(relational)).Returns([boundParameter, unboundParameter]);
            mockedViewModel.Setup(x => x.UpdateParameterLinksAsync(relational, It.IsAny<IReadOnlyCollection<ParameterOrOverrideBase>>())).Returns(Task.FromResult(Result.Ok()));

            var component = this.context.Render<RequirementDetails>(parameters => parameters
                .Add(p => p.ViewModel, mockedViewModel.Object)
                .Add(p => p.Requirement, linkedRequirement));

            var linkButton = component.Find("button.req-expr-link");
            linkButton.Click();

            Assert.Multiple(() =>
            {
                Assert.That(component.Instance.IsLinkDialogOpen, Is.True);
                Assert.That(component.FindComponents<DxCheckBox<bool>>(), Has.Count.EqualTo(2), "both candidates are offered by the picker");
                Assert.That(component.FindComponents<DxCheckBox<bool>>()[0].Instance.Checked, Is.True, "the already-bound parameter is pre-checked");
                Assert.That(component.FindComponents<DxCheckBox<bool>>()[1].Instance.Checked, Is.False, "the unbound candidate is not pre-checked");
            });

            var okButton = component.FindComponents<DxButton>().First(x => x.Instance.Text == "OK");
            await component.InvokeAsync(okButton.Instance.Click.InvokeAsync);

            mockedViewModel.Verify(x => x.UpdateParameterLinksAsync(relational, It.IsAny<IReadOnlyCollection<ParameterOrOverrideBase>>()), Times.Once);
            Assert.That(component.Instance.IsLinkDialogOpen, Is.False, "confirming closes the picker");
        }

        [Test]
        public async Task VerifyTogglingACandidateStagesAndUnstagesIt()
        {
            var massType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var relational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), TopExpression = relational };
            constraint.Expression.Add(relational);

            var linkedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R3", Name = "Third requirement" };
            linkedRequirement.ParametricConstraint.Add(constraint);

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            var boundParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = massType };
            var unboundParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = massType };
            elementDefinition.Parameter.AddRange([boundParameter, unboundParameter]);

            IReadOnlyCollection<ParameterOrOverrideBase> capturedSelection = null;

            var mockedViewModel = new Mock<IRequirementsEditorBodyViewModel>();
            mockedViewModel.Setup(x => x.ShowParametricConstraints).Returns(true);
            mockedViewModel.Setup(x => x.GetTopExpressions(constraint)).Returns([relational]);
            mockedViewModel.Setup(x => x.GetBoundParameters(relational)).Returns([boundParameter]);
            mockedViewModel.Setup(x => x.GetPublishedValue(It.IsAny<ParameterOrOverrideBase>())).Returns((string)null);
            mockedViewModel.Setup(x => x.GetLinkableParameters(relational)).Returns([boundParameter, unboundParameter]);
            mockedViewModel.Setup(x => x.UpdateParameterLinksAsync(relational, It.IsAny<IReadOnlyCollection<ParameterOrOverrideBase>>()))
                .Callback<RelationalExpression, IReadOnlyCollection<ParameterOrOverrideBase>>((_, selected) => capturedSelection = selected)
                .Returns(Task.FromResult(Result.Ok()));

            var component = this.context.Render<RequirementDetails>(parameters => parameters
                .Add(p => p.ViewModel, mockedViewModel.Object)
                .Add(p => p.Requirement, linkedRequirement));

            component.Find("button.req-expr-link").Click();

            Assert.That(component.Markup, Does.Contain("(Satellite)"), "the picker labels each candidate with its owning element");

            await component.InvokeAsync(() => component.FindComponents<DxCheckBox<bool>>()[1].Instance.CheckedChanged.InvokeAsync(true));
            Assert.That(component.FindComponents<DxCheckBox<bool>>()[1].Instance.Checked, Is.True, "checking a candidate stages it");

            await component.InvokeAsync(() => component.FindComponents<DxCheckBox<bool>>()[1].Instance.CheckedChanged.InvokeAsync(false));
            Assert.That(component.FindComponents<DxCheckBox<bool>>()[1].Instance.Checked, Is.False, "unchecking a candidate unstages it");

            var okButton = component.FindComponents<DxButton>().First(x => x.Instance.Text == "OK");
            await component.InvokeAsync(okButton.Instance.Click.InvokeAsync);

            Assert.That(capturedSelection, Is.EquivalentTo(new ParameterOrOverrideBase[] { boundParameter }), "only the still-bound parameter is submitted after staging then unstaging the other candidate");
        }

        [Test]
        public async Task VerifyCancelLinksClosesPickerWithoutSaving()
        {
            var massType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var relational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), TopExpression = relational };
            constraint.Expression.Add(relational);

            var linkedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R3", Name = "Third requirement" };
            linkedRequirement.ParametricConstraint.Add(constraint);

            var mockedViewModel = new Mock<IRequirementsEditorBodyViewModel>();
            mockedViewModel.Setup(x => x.ShowParametricConstraints).Returns(true);
            mockedViewModel.Setup(x => x.GetTopExpressions(constraint)).Returns([relational]);
            mockedViewModel.Setup(x => x.GetBoundParameters(relational)).Returns([]);
            mockedViewModel.Setup(x => x.GetPublishedValue(It.IsAny<ParameterOrOverrideBase>())).Returns((string)null);
            mockedViewModel.Setup(x => x.GetLinkableParameters(relational)).Returns([]);

            var component = this.context.Render<RequirementDetails>(parameters => parameters
                .Add(p => p.ViewModel, mockedViewModel.Object)
                .Add(p => p.Requirement, linkedRequirement));

            component.Find("button.req-expr-link").Click();
            Assert.That(component.Instance.IsLinkDialogOpen, Is.True);

            var cancelButton = component.FindComponents<DxButton>().First(x => x.Instance.Text == "Cancel");
            await component.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(component.Instance.IsLinkDialogOpen, Is.False, "cancelling closes the picker");
                mockedViewModel.Verify(x => x.UpdateParameterLinksAsync(It.IsAny<RelationalExpression>(), It.IsAny<IReadOnlyCollection<ParameterOrOverrideBase>>()), Times.Never);
            });
        }

        [Test]
        public async Task VerifyClosingPickerViaPopupVisibleChanged()
        {
            var massType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var relational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), TopExpression = relational };
            constraint.Expression.Add(relational);

            var linkedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R3", Name = "Third requirement" };
            linkedRequirement.ParametricConstraint.Add(constraint);

            var mockedViewModel = new Mock<IRequirementsEditorBodyViewModel>();
            mockedViewModel.Setup(x => x.ShowParametricConstraints).Returns(true);
            mockedViewModel.Setup(x => x.GetTopExpressions(constraint)).Returns([relational]);
            mockedViewModel.Setup(x => x.GetBoundParameters(relational)).Returns([]);
            mockedViewModel.Setup(x => x.GetPublishedValue(It.IsAny<ParameterOrOverrideBase>())).Returns((string)null);
            mockedViewModel.Setup(x => x.GetLinkableParameters(relational)).Returns([]);

            var component = this.context.Render<RequirementDetails>(parameters => parameters
                .Add(p => p.ViewModel, mockedViewModel.Object)
                .Add(p => p.Requirement, linkedRequirement));

            component.Find("button.req-expr-link").Click();
            Assert.That(component.Instance.IsLinkDialogOpen, Is.True);

            var popup = component.FindComponent<DxPopup>();
            await component.InvokeAsync(() => popup.Instance.VisibleChanged.InvokeAsync(false));

            Assert.That(component.Instance.IsLinkDialogOpen, Is.False, "closing the popup via its close button closes the picker");
        }

        [Test]
        public void VerifyEmptyCandidatesShowsPlaceholder()
        {
            var massType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var relational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), TopExpression = relational };
            constraint.Expression.Add(relational);

            var linkedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R3", Name = "Third requirement" };
            linkedRequirement.ParametricConstraint.Add(constraint);

            var mockedViewModel = new Mock<IRequirementsEditorBodyViewModel>();
            mockedViewModel.Setup(x => x.ShowParametricConstraints).Returns(true);
            mockedViewModel.Setup(x => x.GetTopExpressions(constraint)).Returns([relational]);
            mockedViewModel.Setup(x => x.GetBoundParameters(relational)).Returns([]);
            mockedViewModel.Setup(x => x.GetPublishedValue(It.IsAny<ParameterOrOverrideBase>())).Returns((string)null);
            mockedViewModel.Setup(x => x.GetLinkableParameters(relational)).Returns([]);

            var component = this.context.Render<RequirementDetails>(parameters => parameters
                .Add(p => p.ViewModel, mockedViewModel.Object)
                .Add(p => p.Requirement, linkedRequirement));

            component.Find("button.req-expr-link").Click();

            Assert.Multiple(() =>
            {
                Assert.That(component.Markup, Does.Contain("No element has a parameter of this type."));
                Assert.That(component.FindComponents<DxCheckBox<bool>>(), Is.Empty);
            });
        }
    }
}
