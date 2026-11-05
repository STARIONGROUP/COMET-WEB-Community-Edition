// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.VandV
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.VandV;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class VandVRegisterViewModelTestFixture
    {
        private VandVRegisterScenario scenario;
        private VandVRegisterViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.scenario = new VandVRegisterScenario();
            this.viewModel = new VandVRegisterViewModel(new Mock<ILogger>().Object);
            this.viewModel.SetModel(this.scenario.BuildModel());
            this.viewModel.SelectedSpecification = null;
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySetModel()
        {
            using var fresh = new VandVRegisterViewModel(new Mock<ILogger>().Object);
            fresh.SetModel(this.scenario.BuildModel());
            var initialScope = fresh.SelectedSpecification;
            var rowsOfInitialScope = fresh.Rows.Count(x => x.Kind == VandVRegisterRowKind.Specification);

            var rows = this.viewModel.Rows;
            var r1 = rows.Single(x => x.Thing == this.scenario.FirstRequirement);
            var r3 = rows.Single(x => x.Thing == this.scenario.ThirdRequirement);

            Assert.Multiple(() =>
            {
                Assert.That(initialScope, Is.SameAs(this.scenario.Specification), "the first specification is selected, as on the Requirements tab");
                Assert.That(rowsOfInitialScope, Is.EqualTo(1));
                Assert.That(this.viewModel.AvailableSpecifications, Is.EqualTo(new[] { this.scenario.Specification, this.scenario.SecondSpecification }), "the V&V plan and reports are no scope");
                Assert.That(Describe(rows), Is.EqualTo(new[]
                {
                    "Specification SPEC 0", "Requirement R1 1", "Item VNV_R1_1 2", "Item VNV_R1_2 2", "Group G1 1", "Requirement R2 2", "Item VNV_R2_1 3",
                    "Group G11 2", "Requirement R3 3", "Specification SPEC2 0", "Requirement R4 1"
                }), "items start collapsed, the empty group G2 and the deprecated R0 are left out");

                Assert.That(rows[0].Coverage, Is.EqualTo("0/3 verified"), "R1 still has an open item");
                Assert.That(r1.Coverage, Is.EqualTo("2 item(s): 1 passed, 1 open"));
                Assert.That(r1.RollUp, Is.EqualTo(new VandVRollUp(1, 0, 1)));
                Assert.That(r3.Coverage, Is.EqualTo("Not covered"));
                Assert.That(r3.HasChildren, Is.False);
                Assert.That(rows.Single(x => x.Thing == this.scenario.PassedItem).Outcome, Is.EqualTo(VandVItemOutcome.Passed));
                Assert.That(rows.Single(x => x.Thing == this.scenario.PerformedItem).HasChildren, Is.True, "its activity's steps can be expanded");
            });

            this.viewModel.SetModel(this.scenario.BuildModel());
            var allKept = this.viewModel.SelectedSpecification;

            this.viewModel.SelectedSpecification = this.scenario.SecondSpecification;
            this.viewModel.SetModel(this.scenario.BuildModel());
            var keptScope = this.viewModel.SelectedSpecification;

            this.viewModel.SetModel(VandVModel.Build(null, null));

            Assert.Multiple(() =>
            {
                Assert.That(allKept, Is.Null, "a reload keeps showing all specifications");
                Assert.That(keptScope, Is.SameAs(this.scenario.SecondSpecification), "a reload keeps the scope");
                Assert.That(this.viewModel.SelectedSpecification, Is.Null, "a scope that no longer exists is dropped");
                Assert.That(this.viewModel.Rows, Is.Empty);
            });

            this.viewModel.SetModel(this.scenario.BuildModel());
            this.viewModel.SelectedStages = ["in_service", "Removed gate"];
            this.viewModel.SetModel(this.scenario.BuildModel());

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedStages, Is.EqualTo(new[] { "in_service" }), "a gate that still exists is kept whatever its spelling, a removed one is dropped");
                Assert.That(this.viewModel.SelectedSpecification, Is.SameAs(this.scenario.Specification), "coming back from no iteration selects the first specification again");
            });
        }

        [Test]
        public void VerifySetModelOfAnotherIteration()
        {
            this.viewModel.Toggle(this.Row(this.scenario.Group));
            this.viewModel.ToggleTreeNode(this.scenario.Group.Iid);

            var other = new VandVRegisterScenario();
            this.viewModel.SetModel(other.BuildModel());
            var scopeOfOther = this.viewModel.SelectedSpecification;

            this.viewModel.SetModel(this.scenario.BuildModel());

            Assert.Multiple(() =>
            {
                Assert.That(scopeOfOther, Is.SameAs(other.Specification), "another iteration selects its first specification");
                Assert.That(this.viewModel.SelectedSpecification, Is.SameAs(this.scenario.Specification));
                Assert.That(this.Row(this.scenario.Group).IsExpanded, Is.True, "the expansion of another iteration is forgotten");
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.scenario.Group.Iid), Is.False);
            });
        }

        [Test]
        public void VerifyRowsAreBuiltWhenRead()
        {
            this.viewModel.SetModel(this.scenario.BuildModel());
            this.scenario.Data.SetValue(this.scenario.PassedItem, VandVConstants.Status, "Failed");
            var statusOfFirstRead = this.Row(this.scenario.PassedItem).Fields.Single(x => x.Label == "Status").Value;
            var firstRead = this.viewModel.Rows;

            this.scenario.Data.SetValue(this.scenario.PassedItem, VandVConstants.Status, "Passed");

            Assert.Multiple(() =>
            {
                Assert.That(statusOfFirstRead, Is.EqualTo("Failed"), "a reload builds nothing until someone reads the rows");
                Assert.That(this.viewModel.Rows, Is.SameAs(firstRead), "and they are only built once");
            });
        }

        [Test]
        public void VerifyBuildError()
        {
            var broken = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.scenario.Data.ParameterTypes[VandVConstants.Level], Value = null };
            this.scenario.PassedItem.ParameterValue.Add(broken);
            this.viewModel.SetModel(this.scenario.BuildModel());

            var rows = this.viewModel.Rows;
            var groups = this.viewModel.GetGroups(this.scenario.Specification);
            var error = this.viewModel.BuildError;

            this.scenario.PassedItem.ParameterValue.Remove(broken);
            this.viewModel.SearchText = "R1";

            Assert.Multiple(() =>
            {
                Assert.That(rows, Is.Empty, "a failing build must never reach the render, where it would end the circuit");
                Assert.That(groups, Is.Empty);
                Assert.That(error, Does.Contain("could not be built"));
                Assert.That(this.viewModel.Rows, Is.Not.Empty, "the next change builds again");
                Assert.That(this.viewModel.BuildError, Is.Null);
            });
        }

        [Test]
        public void VerifyToggle()
        {
            this.viewModel.Toggle(this.Row(this.scenario.PerformedItem));
            var steps = this.viewModel.Rows.Where(x => x.Kind == VandVRegisterRowKind.Step).ToList();

            this.viewModel.Toggle(this.Row(this.scenario.Specification));

            Assert.Multiple(() =>
            {
                Assert.That(steps.Select(x => x.Thing), Is.EqualTo(new Thing[] { this.scenario.PassedStep, this.scenario.FailedStep }));
                Assert.That(steps.All(x => x.IsInherited && x.Depth == 4), Is.True, "the activity's steps, below the item");
                Assert.That(this.Row(this.scenario.Specification).IsExpanded, Is.False);
                Assert.That(this.viewModel.Rows.Any(x => x.Thing == this.scenario.FirstRequirement), Is.False, "a collapsed specification hides its content");
                Assert.That(this.viewModel.Rows.Any(x => x.Thing == this.scenario.FourthRequirement), Is.True);
            });
        }

        [Test]
        public void VerifySelectedSpecification()
        {
            this.viewModel.SelectedSpecification = this.scenario.SecondSpecification;
            var onlySecond = Describe(this.viewModel.Rows);

            this.viewModel.SelectedSpecification = this.scenario.SecondSpecification;

            Assert.Multiple(() =>
            {
                Assert.That(onlySecond, Is.EqualTo(new[] { "Specification SPEC2 0", "Requirement R4 1" }));
                Assert.That(this.viewModel.SelectedSpecification, Is.Null, "selecting the selected specification again shows them all");
                Assert.That(this.viewModel.Rows.Count(x => x.Kind == VandVRegisterRowKind.Specification), Is.EqualTo(2));
            });
        }

        [Test]
        public void VerifySearchText()
        {
            this.viewModel.SearchText = "r2";
            var byRequirement = Describe(this.viewModel.Rows);

            this.viewModel.SearchText = "VNV_R1_2";
            var byItem = Describe(this.viewModel.Rows);

            this.viewModel.SearchText = "  ";

            Assert.Multiple(() =>
            {
                Assert.That(byRequirement, Is.EqualTo(new[] { "Specification SPEC 0", "Group G1 1", "Requirement R2 2", "Item VNV_R2_1 3" }), "case-insensitive, groups without a match are left out");
                Assert.That(byItem, Is.EqualTo(new[] { "Specification SPEC 0", "Requirement R1 1", "Item VNV_R1_2 2" }), "a requirement found through one item shows that item");
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(11), "a blank search shows everything");
            });
        }

        [Test]
        public void VerifySelectedStages()
        {
            this.viewModel.SelectedStages = ["CDR"];

            Assert.That(Describe(this.viewModel.Rows), Is.EqualTo(new[]
            {
                "Specification SPEC 0", "Requirement R1 1", "Item VNV_R1_1 2", "Group G1 1", "Requirement R2 2", "Item VNV_R2_1 3"
            }), "the stage inherited from the activity counts, requirements without an item at the gate are left out");

            Assert.Multiple(() =>
            {
                Assert.That(this.Row(this.scenario.FirstRequirement).Coverage, Is.EqualTo("2 item(s): 1 passed, 1 open"), "the coverage counts every item, not just the ones the filter shows");
                Assert.That(this.Row(this.scenario.FirstRequirement).RollUp, Is.EqualTo(new VandVRollUp(1, 0, 1)));
                Assert.That(this.Row(this.scenario.Specification).Coverage, Is.EqualTo("0/3 verified"), "the coverage of a specification does not change with what the filter shows");
            });

            this.viewModel.SelectedStages = ["CDR", "PDR"];
            var bothGates = Describe(this.viewModel.Rows);

            this.viewModel.SelectedStages = ["PDR", "SRR"];
            var oneGateWithoutItems = Describe(this.viewModel.Rows);

            this.viewModel.SelectedStages = [];

            Assert.Multiple(() =>
            {
                Assert.That(bothGates, Is.EqualTo(new[]
                {
                    "Specification SPEC 0", "Requirement R1 1", "Item VNV_R1_1 2", "Item VNV_R1_2 2", "Group G1 1", "Requirement R2 2", "Item VNV_R2_1 3"
                }), "an item at any of the selected gates is shown");

                Assert.That(oneGateWithoutItems, Is.EqualTo(new[] { "Specification SPEC 0", "Requirement R1 1", "Item VNV_R1_2 2" }));
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(11), "no selected gate is no filter");
            });

            var stage = (EnumerationParameterType)this.scenario.Data.ParameterTypes[VandVConstants.Stage];
            stage.ValueDefinition.Add(new EnumerationValueDefinition { Iid = Guid.NewGuid(), Name = "Mission Review", ShortName = "MR" });
            this.scenario.Data.SetValue(this.scenario.OpenItem, VandVConstants.Stage, "MR");
            this.viewModel.SetModel(this.scenario.BuildModel());
            this.viewModel.SelectedStages = ["Mission Review"];

            Assert.That(Describe(this.viewModel.Rows), Is.EqualTo(new[] { "Specification SPEC 0", "Requirement R1 1", "Item VNV_R1_2 2" }), "a gate stored by short name matches the name offered");
        }

        [Test]
        public void VerifySelectedFields()
        {
            this.viewModel.Toggle(this.Row(this.scenario.PerformedItem));
            var allLabels = this.FieldLabels(this.scenario.PassedItem);
            var stepsWithAllFields = this.viewModel.Rows.Count(x => x.Kind == VandVRegisterRowKind.Step);

            this.viewModel.SelectedFields = ["Status", "Method"];
            var chosenLabels = this.FieldLabels(this.scenario.PassedItem);
            var stepsWithoutProcedure = this.viewModel.Rows.Count(x => x.Kind == VandVRegisterRowKind.Step);
            var hasChildrenWithoutProcedure = this.Row(this.scenario.PerformedItem).HasChildren;
            var preset = this.viewModel.ActivePreset;

            this.viewModel.SelectedFields = ["Status", VandVRegisterFields.ProcedureField];
            var stepsWithProcedure = this.viewModel.Rows.Count(x => x.Kind == VandVRegisterRowKind.Step);

            Assert.Multiple(() =>
            {
                Assert.That(allLabels, Is.EqualTo(VandVRegisterFields.AllFields), "no choice shows them all");
                Assert.That(allLabels, Does.Not.Contain("Owner"), "the owner is a badge on the row");
                Assert.That(stepsWithAllFields, Is.EqualTo(2));
                Assert.That(chosenLabels, Is.EqualTo(new[] { "Method", "Status" }), "in display order, whatever order they were chosen in");
                Assert.That(stepsWithoutProcedure, Is.Zero, "steps belong to the procedure");
                Assert.That(hasChildrenWithoutProcedure, Is.False);
                Assert.That(preset, Is.Null, "a custom combination is no preset");
                Assert.That(stepsWithProcedure, Is.EqualTo(2));
            });
        }

        [Test]
        public void VerifyApplyPreset()
        {
            var initialPreset = this.viewModel.ActivePreset;

            this.viewModel.ApplyPreset(VandVViewPreset.Planning);
            var planning = this.viewModel.ActivePreset;
            var planningLabels = this.FieldLabels(this.scenario.PassedItem);

            this.viewModel.ApplyPreset(VandVViewPreset.Procedure);
            var procedure = this.viewModel.ActivePreset;
            var procedureLabels = this.FieldLabels(this.scenario.PassedItem);

            this.viewModel.ApplyPreset(VandVViewPreset.Execution);
            var execution = this.viewModel.ActivePreset;

            this.viewModel.ApplyPreset(VandVViewPreset.Compliance);
            var compliance = this.viewModel.ActivePreset;

            this.viewModel.ApplyPreset(VandVViewPreset.Register);

            Assert.Multiple(() =>
            {
                Assert.That(initialPreset, Is.EqualTo(VandVViewPreset.Register));
                Assert.That(planning, Is.EqualTo(VandVViewPreset.Planning));
                Assert.That(planningLabels, Does.Contain("Method").And.Not.Contain("Status").And.Not.Contain("Procedure"));
                Assert.That(procedure, Is.EqualTo(VandVViewPreset.Procedure));
                Assert.That(procedureLabels, Is.EqualTo(new[] { "Procedure" }));
                Assert.That(execution, Is.EqualTo(VandVViewPreset.Execution));
                Assert.That(compliance, Is.EqualTo(VandVViewPreset.Compliance));
                Assert.That(this.viewModel.SelectedFields, Is.Empty, "the register view shows everything");
                Assert.That(this.viewModel.ActivePreset, Is.EqualTo(VandVViewPreset.Register));
            });
        }

        [Test]
        public void VerifyFocus()
        {
            this.viewModel.Toggle(this.Row(this.scenario.Group));
            this.viewModel.SelectedSpecification = this.scenario.SecondSpecification;
            this.viewModel.SearchText = "R4";
            this.viewModel.SelectedStages = ["PDR"];

            this.viewModel.Focus(this.scenario.SecondRequirement);
            var rows = this.viewModel.Rows;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SearchText, Is.Null);
                Assert.That(this.viewModel.SelectedStages, Is.Empty);
                Assert.That(this.viewModel.SelectedSpecification, Is.SameAs(this.scenario.Specification));
                Assert.That(this.viewModel.ScrollTarget, Is.EqualTo(this.scenario.SecondRequirement.Iid));
                Assert.That(rows.Single(x => x.Thing == this.scenario.Group).IsExpanded, Is.True, "a collapsed ancestor is expanded again");
                Assert.That(rows.Single(x => x.Thing == this.scenario.PerformedItem).IsExpanded, Is.True, "its items are expanded");
                Assert.That(rows.Count(x => x.Kind == VandVRegisterRowKind.Step), Is.EqualTo(2));
            });

            this.viewModel.ClearScrollTarget();

            Assert.That(this.viewModel.ScrollTarget, Is.Null);

            this.viewModel.Focus(this.scenario.FirstRequirement);

            Assert.That(this.viewModel.SelectedSpecification, Is.SameAs(this.scenario.Specification), "focusing inside the selected specification keeps it selected");

            this.viewModel.SelectedSpecification = this.scenario.SecondSpecification;
            this.viewModel.Focus(this.scenario.DeprecatedRequirement);
            var deprecated = (this.viewModel.SelectedSpecification, this.viewModel.ScrollTarget);

            this.viewModel.Focus(new Requirement { Iid = Guid.NewGuid() });
            var orphan = (this.viewModel.SelectedSpecification, this.viewModel.ScrollTarget);

            this.viewModel.Focus(null);

            Assert.Multiple(() =>
            {
                Assert.That(deprecated, Is.EqualTo(((RequirementsSpecification)null, (Guid?)null)), "not in the register: the whole register is shown, nothing to scroll to");
                Assert.That(orphan, Is.EqualTo(((RequirementsSpecification)null, (Guid?)null)), "a requirement without a specification does not throw");
                Assert.That(this.viewModel.ScrollTarget, Is.Null);
                Assert.That(this.viewModel.Rows.Count(x => x.Kind == VandVRegisterRowKind.Specification), Is.EqualTo(2));
            });
        }

        [Test]
        public void VerifyNavigateToRequirement()
        {
            this.viewModel.NavigateToRequirement(this.scenario.FourthRequirement, null);
            var requirementTarget = this.viewModel.ScrollTarget;

            this.viewModel.NavigateToRequirement(this.scenario.OpenItem, null);
            var itemTarget = this.viewModel.ScrollTarget;
            var itemScope = this.viewModel.SelectedSpecification;

            this.viewModel.ClearScrollTarget();
            this.viewModel.SearchText = "R1";
            this.viewModel.NavigateToRequirement(this.scenario.Activity, null);

            var plan = (RequirementsSpecification)this.scenario.PassedItem.Container;
            var itemOfDeprecated = this.scenario.Data.AddRequirement(plan, "VNV_R0_1", VandVConstants.VnVItemCategory);
            this.scenario.Data.Link(itemOfDeprecated, this.scenario.DeprecatedRequirement, VandVConstants.VerifiesCategory);
            this.viewModel.SetModel(this.scenario.BuildModel());
            this.viewModel.NavigateToRequirement(itemOfDeprecated, null);

            Assert.Multiple(() =>
            {
                Assert.That(requirementTarget, Is.EqualTo(this.scenario.FourthRequirement.Iid));
                Assert.That(itemTarget, Is.EqualTo(this.scenario.FirstRequirement.Iid), "an item goes to the requirement it covers");
                Assert.That(itemScope, Is.SameAs(this.scenario.Specification));
                Assert.That(this.viewModel.ScrollTarget, Is.Null, "an activity has no row to go to");
                Assert.That(this.viewModel.SearchText, Is.EqualTo("R1"), "so nothing is reset either");
                Assert.That(this.viewModel.SelectedSpecification, Is.SameAs(this.scenario.Specification), "not by an item covering a deprecated requirement either");
            });

            this.scenario.Data.Link(this.scenario.PassedItem, this.scenario.SecondRequirement, VandVConstants.VerifiesCategory);
            this.scenario.Data.Link(itemOfDeprecated, this.scenario.FourthRequirement, VandVConstants.VerifiesCategory);
            this.viewModel.SetModel(this.scenario.BuildModel());

            this.viewModel.NavigateToRequirement(this.scenario.PassedItem, this.scenario.SecondRequirement);
            var fromTheSecondCovered = this.viewModel.ScrollTarget;

            this.viewModel.NavigateToRequirement(this.scenario.PassedItem, this.scenario.Activity);
            var fromElsewhere = this.viewModel.ScrollTarget;

            this.viewModel.NavigateToRequirement(this.scenario.PassedItem, this.scenario.PerformedItem);
            var fromTheRowOfAnItem = this.viewModel.ScrollTarget;

            this.viewModel.NavigateToRequirement(itemOfDeprecated, null);

            Assert.Multiple(() =>
            {
                Assert.That(fromTheSecondCovered, Is.EqualTo(this.scenario.SecondRequirement.Iid), "an item covering several requirements stays on the one the link was clicked on");
                Assert.That(fromElsewhere, Is.EqualTo(this.scenario.FirstRequirement.Iid));
                Assert.That(fromTheRowOfAnItem, Is.EqualTo(this.scenario.SecondRequirement.Iid), "on the row of an item, the requirement that row sits under");
                Assert.That(this.viewModel.ScrollTarget, Is.EqualTo(this.scenario.FourthRequirement.Iid), "a deprecated first link does not hide a live second one");
            });
        }

        [Test]
        public void VerifyNavigateToGroup()
        {
            this.viewModel.Toggle(this.Row(this.scenario.Group));
            this.viewModel.Toggle(this.Row(this.scenario.Specification));

            this.viewModel.NavigateToGroup(this.scenario.SubGroup);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ScrollTarget, Is.EqualTo(this.scenario.SubGroup.Iid));
                Assert.That(this.Row(this.scenario.Specification).IsExpanded, Is.True);
                Assert.That(this.Row(this.scenario.Group).IsExpanded, Is.True, "a collapsed ancestor would leave nothing to scroll to");
                Assert.That(this.Row(this.scenario.SubGroup).IsExpanded, Is.True);
            });
        }

        [Test]
        public void VerifyGetGroups()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetGroups(this.scenario.Specification), Is.EqualTo(new[] { this.scenario.Group }), "the empty group is left out");
                Assert.That(this.viewModel.GetGroups(this.scenario.Group), Is.EqualTo(new[] { this.scenario.SubGroup }));
                Assert.That(this.viewModel.GetGroups(this.scenario.SubGroup), Is.Empty);
            });

            this.viewModel.SearchText = "R2";
            var underSearch = this.viewModel.GetGroups(this.scenario.Group).ToList();
            var topUnderSearch = this.viewModel.GetGroups(this.scenario.Specification).ToList();

            this.viewModel.SearchText = "R1";

            Assert.Multiple(() =>
            {
                Assert.That(topUnderSearch, Is.EqualTo(new[] { this.scenario.Group }));
                Assert.That(underSearch, Is.Empty, "a group without a row is not offered");
                Assert.That(this.viewModel.GetGroups(this.scenario.Specification), Is.Empty);
            });
        }

        [Test]
        public void VerifyTreeNodes()
        {
            var iid = this.scenario.Group.Iid;
            var initial = this.viewModel.IsTreeNodeCollapsed(iid);
            this.viewModel.ToggleTreeNode(iid);
            var collapsed = this.viewModel.IsTreeNodeCollapsed(iid);

            this.viewModel.ToggleTreeNode(iid);

            Assert.Multiple(() =>
            {
                Assert.That(initial, Is.False);
                Assert.That(collapsed, Is.True);
                Assert.That(this.viewModel.IsTreeNodeCollapsed(iid), Is.False);
                Assert.That(this.viewModel, Is.Not.InstanceOf<IRequirementsTreeDragViewModel>(), "the register only reads the structure");
            });
        }

        [Test]
        public void VerifyDisplayOptions()
        {
            var defaults = (this.viewModel.DisplayMode, this.viewModel.TreeUsesShortName, this.viewModel.IndentGroups, this.viewModel.ShowOwner, this.viewModel.ShowCategory, this.viewModel.ShowTraceability, this.viewModel.IsTocCollapsed);
            var rowsBefore = this.viewModel.Rows;

            this.viewModel.DisplayMode = RequirementRowDisplayMode.NameAndDefinition;
            this.viewModel.TreeUsesShortName = false;
            this.viewModel.IndentGroups = false;
            this.viewModel.ShowOwner = false;
            this.viewModel.ShowCategory = false;
            this.viewModel.IsTocCollapsed = true;

            Assert.Multiple(() =>
            {
                Assert.That(defaults, Is.EqualTo((RequirementRowDisplayMode.ShortNameNameAndDefinition, true, true, true, true, false, false)), "the same defaults as the Requirements tab");
                Assert.That(this.viewModel.DisplayMode, Is.EqualTo(RequirementRowDisplayMode.NameAndDefinition));
                Assert.That(this.viewModel.TreeUsesShortName, Is.False);
                Assert.That(this.viewModel.IndentGroups, Is.False);
                Assert.That(this.viewModel.ShowOwner, Is.False);
                Assert.That(this.viewModel.ShowCategory, Is.False);
                Assert.That(this.viewModel.IsTocCollapsed, Is.True);
                Assert.That(this.viewModel.Rows, Is.SameAs(rowsBefore), "nothing in a row depends on these, so they do not rebuild");
            });
        }

        [Test]
        public void VerifyShowTraceability()
        {
            var rule = new BinaryRelationshipRule { Iid = Guid.NewGuid(), ShortName = "someRule" };
            this.scenario.Data.Library.Rule.Add(rule);
            this.viewModel.SetModel(this.scenario.BuildModel());
            var traceabilityOff = this.Row(this.scenario.FirstRequirement).Traceability;

            this.viewModel.ShowTraceability = true;
            var requirementLinks = this.Row(this.scenario.FirstRequirement).Traceability.SelectMany(x => x.RelatedThings).ToList();
            var itemLinks = this.Row(this.scenario.PassedItem).Traceability.SelectMany(x => x.RelatedThings).ToList();
            var lookedUp = this.Row(this.scenario.FirstRequirement).Traceability;

            this.viewModel.SearchText = "R1";
            var afterRebuild = this.Row(this.scenario.FirstRequirement).Traceability;

            this.viewModel.SetModel(this.scenario.BuildModel());
            var afterReload = this.Row(this.scenario.FirstRequirement).Traceability;

            Assert.Multiple(() =>
            {
                Assert.That(traceabilityOff, Is.Null, "nothing is computed while it is off");
                Assert.That(afterRebuild, Is.SameAs(lookedUp), "a rebuild does not scan the relationships again");
                Assert.That(afterReload, Is.Not.SameAs(lookedUp), "a reload does");
                Assert.That(requirementLinks, Does.Contain(this.scenario.PassedItem).And.Contain(this.scenario.OpenItem));
                Assert.That(itemLinks, Does.Contain(this.scenario.FirstRequirement));
                Assert.That(this.viewModel.Model.Rules, Does.Contain(rule), "the rules name what a relationship matches");
            });
        }

        [Test]
        public void VerifyRequirementFiledInAGroupOfAnotherSpecification()
        {
            this.scenario.FourthRequirement.Group = this.scenario.Group;
            this.viewModel.SetModel(this.scenario.BuildModel());
            var all = Describe(this.viewModel.Rows);

            Assert.Multiple(() =>
            {
                Assert.That(all.Count(x => x == "Requirement R4 1"), Is.EqualTo(1), "it stays directly under its own specification");
                Assert.That(all, Does.Not.Contain("Requirement R4 2"), "the group of the other specification does not list it");
                Assert.That(this.viewModel.GetGroups(this.scenario.SecondSpecification), Is.Empty);
            });
        }

        private static List<string> Describe(IEnumerable<VandVRegisterRow> rows)
        {
            return rows.Select(x => $"{x.Kind} {((DefinedThing)x.Thing).ShortName} {x.Depth}").ToList();
        }

        private VandVRegisterRow Row(Thing thing)
        {
            return this.viewModel.Rows.Single(x => x.Thing == thing);
        }

        private List<string> FieldLabels(Requirement item)
        {
            return this.Row(item).Fields.Select(x => x.Label).ToList();
        }
    }
}
