// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterFieldsTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.VandV;

    using NUnit.Framework;

    [TestFixture]
    public class VandVRegisterFieldsTestFixture
    {
        private static readonly string[] PlanningLabels = ["Method", "Stage gate", "Level", "Criticality", "Activity", "Planned", "Acceptance"];

        private VandVRegisterScenario scenario;

        [SetUp]
        public void SetUp()
        {
            this.scenario = new VandVRegisterScenario();
        }

        [Test]
        public void VerifyGetPresetLabel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVRegisterFields.GetPresetLabel(VandVViewPreset.Register), Is.EqualTo("Register (VCD)"));
                Assert.That(VandVRegisterFields.GetPresetLabel(VandVViewPreset.Compliance), Is.EqualTo("Compliance"));
            });
        }
        [Test]
        public void VerifyGetFields()
        {
            var model = this.scenario.BuildModel();
            var item = this.scenario.PassedItem;

            Assert.Multiple(() =>
            {
                Assert.That(VandVRegisterFields.GetFields(model, item, []).Select(x => x.Label), Is.EqualTo(VandVRegisterFields.AllFields), "every field the filter offers exists, in display order");
                Assert.That(VandVRegisterFields.GetFields(model, item, ["Status", "Method"]).Select(x => x.Label), Is.EqualTo(new[] { "Method", "Status" }));
                Assert.That(VandVRegisterFields.GetFields(model, item, ["Unknown"]), Is.Empty);
                Assert.That(VandVRegisterFields.GetFields(model, item, [VandVRegisterFields.ProcedureField]).Select(x => x.Label), Is.EqualTo(new[] { VandVRegisterFields.ProcedureField }));
            });
        }

        [Test]
        public void VerifyGetPresetFields()
        {
            var model = this.scenario.BuildModel();
            var item = this.scenario.PassedItem;

            Assert.Multiple(() =>
            {
                Assert.That(VandVRegisterFields.GetPresetFields(VandVViewPreset.Register), Is.Empty, "empty shows them all");
                Assert.That(VandVRegisterFields.GetPresetFields(VandVViewPreset.Planning), Is.EqualTo(VandVRegisterFields.GetPlanningFields(model, item).Select(x => x.Label)));
                Assert.That(VandVRegisterFields.GetPresetFields(VandVViewPreset.Procedure), Is.EqualTo(new[] { VandVRegisterFields.ProcedureField }));
                Assert.That(VandVRegisterFields.GetPresetFields(VandVViewPreset.Execution), Is.EqualTo(VandVRegisterFields.GetExecutionFields(model, item).Select(x => x.Label)));
                Assert.That(VandVRegisterFields.GetPresetFields(VandVViewPreset.Compliance), Is.EqualTo(VandVRegisterFields.GetComplianceFields(model, item).Select(x => x.Label)));
            });
        }

        [Test]
        public void VerifyGetPlanningFields()
        {
            this.scenario.Data.SetValue(this.scenario.OpenItem, VandVConstants.ActivityNumber, "LEGACY_7");
            var model = this.scenario.BuildModel();
            var performed = VandVRegisterFields.GetPlanningFields(model, this.scenario.PerformedItem).ToDictionary(x => x.Label);
            var own = VandVRegisterFields.GetPlanningFields(model, this.scenario.OpenItem).ToDictionary(x => x.Label);

            Assert.Multiple(() =>
            {
                Assert.That(performed.Keys, Is.EqualTo(PlanningLabels));
                Assert.That(performed["Method"], Is.EqualTo(new VandVField("Method", "Test", "ACT_1")), "inherited from the activity");
                Assert.That(performed["Stage gate"].InheritedFrom, Is.EqualTo("ACT_1"));
                Assert.That(performed["Activity"].Value, Is.EqualTo("ACT_1"));
                Assert.That(performed["Acceptance"].Value, Is.Null, "acceptance never derives");
                Assert.That(own["Stage gate"], Is.EqualTo(new VandVField("Stage gate", "PDR")), "the item's own value");
                Assert.That(own["Activity"].Value, Is.EqualTo("LEGACY_7"), "the legacy activity number when no activity performs it");
            });
        }

        [Test]
        public void VerifyGetExecutionFields()
        {
            var model = this.scenario.BuildModel();
            var fields = VandVRegisterFields.GetExecutionFields(model, this.scenario.PassedItem);

            Assert.Multiple(() =>
            {
                Assert.That(fields.Select(x => x.Label), Is.EqualTo(new[] { "Status", "Actual", "Result", "Evidence" }));
                Assert.That(fields[0].Value, Is.EqualTo("Passed"));
                Assert.That(fields[1].Value, Is.Null);
            });
        }

        [Test]
        public void VerifyGetComplianceFields()
        {
            var model = this.scenario.BuildModel();
            var closed = VandVRegisterFields.GetComplianceFields(model, this.scenario.PassedItem);
            var open = VandVRegisterFields.GetComplianceFields(model, this.scenario.OpenItem);
            this.scenario.Data.SetValue(this.scenario.OpenItem, VandVConstants.Closed, "true");
            var closedWithoutReason = VandVRegisterFields.GetComplianceFields(model, this.scenario.OpenItem);

            Assert.Multiple(() =>
            {
                Assert.That(closed.Select(x => x.Label), Is.EqualTo(new[] { "Compliance", "Close-out", "Closure" }));
                Assert.That(closed[1].Value, Is.EqualTo("Closed: Accepted at CDR"));
                Assert.That(open[1].Value, Is.EqualTo("Open"));
                Assert.That(closedWithoutReason[1].Value, Is.EqualTo("Closed: no reason given"));
            });
        }

        [Test]
        public void VerifyGetSteps()
        {
            var ownStep = this.scenario.Data.AddRequirement((RequirementsSpecification)this.scenario.Activity.Container, "VNV_R1_1_S01", VandVConstants.VnVStepCategory);
            this.scenario.Data.Link(this.scenario.PassedItem, ownStep, VandVConstants.HasStepCategory);
            var model = this.scenario.BuildModel();

            var inherited = VandVRegisterFields.GetSteps(model, this.scenario.PerformedItem);
            var own = VandVRegisterFields.GetSteps(model, this.scenario.PassedItem);
            var none = VandVRegisterFields.GetSteps(model, this.scenario.OpenItem);

            Assert.Multiple(() =>
            {
                Assert.That(inherited.Steps, Is.EqualTo(new[] { this.scenario.PassedStep, this.scenario.FailedStep }));
                Assert.That(inherited.IsInherited, Is.True);
                Assert.That(own.Steps, Is.EqualTo(new[] { ownStep }));
                Assert.That(own.IsInherited, Is.False);
                Assert.That(none.Steps, Is.Empty);
                Assert.That(none.IsInherited, Is.False);
            });
        }

        [Test]
        public void VerifyGetProcedureSummary()
        {
            var model = this.scenario.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVRegisterFields.GetProcedureSummary(model, this.scenario.PerformedItem), Is.EqualTo("2 of 2 step(s) recorded, 1 failed (from activity)"));
                Assert.That(VandVRegisterFields.GetProcedureSummary(model, this.scenario.OpenItem), Is.EqualTo("No procedure"));
            });

            this.scenario.Data.SetValue(this.scenario.FailedStep, VandVConstants.StepResult, "Not_Run");

            Assert.That(VandVRegisterFields.GetProcedureSummary(model, this.scenario.PerformedItem), Is.EqualTo("1 of 2 step(s) recorded (from activity)"), "Not Run is not recorded");
        }

        [Test]
        public void VerifyGetStepLabel()
        {
            var unnumbered = new Requirement { Iid = Guid.NewGuid(), Name = "Step 3" };

            Assert.Multiple(() =>
            {
                Assert.That(VandVRegisterFields.GetStepLabel(this.scenario.PassedStep), Is.EqualTo("1. Weigh the unit"));
                Assert.That(VandVRegisterFields.GetStepLabel(unnumbered), Is.EqualTo("Step 3"));
            });
        }
    }
}
