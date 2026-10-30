// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Utilities.VandV
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Utilities.VandV;

    using NUnit.Framework;

    [TestFixture]
    public class VandVModelTestFixture
    {
        private VandVTestData data;
        private RequirementsSpecification vandVSpecification;
        private RequirementsSpecification report;
        private Requirement firstRequirement;
        private Requirement secondRequirement;
        private Requirement thirdRequirement;
        private Requirement firstItem;
        private Requirement secondItem;
        private Requirement uncoveringItem;
        private Requirement itemInDeprecatedSpecification;
        private Requirement itemStep;
        private Requirement activity;
        private Requirement deprecatedActivity;
        private Requirement firstActivityStep;
        private Requirement secondActivityStep;
        private Requirement unnumberedActivityStep;
        private Parameter parameter;

        [SetUp]
        public void SetUp()
        {
            this.data = new VandVTestData();

            var specification = this.data.AddSpecification("SPEC");
            this.firstRequirement = this.data.AddRequirement(specification, "R1", VandVTestData.RequirementCategoryShortName);
            this.secondRequirement = this.data.AddRequirement(specification, "R2", VandVTestData.RequirementCategoryShortName);
            this.thirdRequirement = this.data.AddRequirement(specification, "R3", VandVTestData.RequirementCategoryShortName);
            this.data.AddRequirement(specification, "R0", VandVTestData.RequirementCategoryShortName).IsDeprecated = true;
            var deprecatedSpecification = this.data.AddSpecification("OLD");
            deprecatedSpecification.IsDeprecated = true;
            this.itemInDeprecatedSpecification = this.data.AddRequirement(deprecatedSpecification, "VNV_OLD_1", VandVConstants.VnVItemCategory);
            var stepInDeprecatedSpecification = this.data.AddRequirement(deprecatedSpecification, "OLD_S01", VandVConstants.VnVStepCategory);
            var deprecatedReport = this.data.AddSpecification("OLD_REPORT", VandVConstants.VnVReportCategory);
            deprecatedReport.IsDeprecated = true;
            var activityInDeprecatedReport = this.data.AddRequirement(deprecatedReport, "ACT_9", VandVConstants.VnVActivityCategory);

            this.vandVSpecification = this.data.AddSpecification(VandVConstants.VandVSpecificationShortName);
            this.secondItem = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_2", VandVConstants.VnVItemCategory);
            this.firstItem = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_1", VandVConstants.VnVItemCategory);
            this.uncoveringItem = this.data.AddRequirement(this.vandVSpecification, "VNV_X_1", VandVConstants.VerificationItemCategory);
            this.itemStep = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_1_S01", VandVConstants.VnVStepCategory);
            var deprecatedItem = this.data.AddRequirement(this.vandVSpecification, "VNV_R2_1", VandVConstants.VnVItemCategory);
            deprecatedItem.IsDeprecated = true;

            this.report = this.data.AddSpecification("FAT_REPORT", VandVConstants.VnVReportCategory);
            this.activity = this.data.AddRequirement(this.report, "ACT_1", VandVConstants.VnVActivityCategory);
            this.deprecatedActivity = this.data.AddRequirement(this.report, "ACT_0", VandVConstants.VnVActivityCategory);
            this.deprecatedActivity.IsDeprecated = true;
            this.unnumberedActivityStep = this.data.AddRequirement(this.report, "ACT_1_S00", VandVConstants.VnVStepCategory);
            this.secondActivityStep = this.data.AddRequirement(this.report, "ACT_1_S02", VandVConstants.VnVStepCategory);
            this.firstActivityStep = this.data.AddRequirement(this.report, "ACT_1_S01", VandVConstants.VnVStepCategory);
            var deprecatedStep = this.data.AddRequirement(this.report, "ACT_1_S03", VandVConstants.VnVStepCategory);
            deprecatedStep.IsDeprecated = true;
            this.data.SetValue(this.secondActivityStep, VandVConstants.StepNumber, "2");
            this.data.SetValue(this.firstActivityStep, VandVConstants.StepNumber, "1");
            this.data.SetValue(this.unnumberedActivityStep, VandVConstants.StepNumber, "x");

            this.data.Link(this.secondItem, this.firstRequirement, VandVConstants.ValidatesCategory);
            this.data.Link(this.firstItem, this.firstRequirement, VandVConstants.VerifiesCategory);
            this.data.Link(deprecatedItem, this.secondRequirement, VandVConstants.VerifiesCategory);
            this.data.Link(this.thirdRequirement, this.secondRequirement, VandVConstants.VerifiesCategory);
            this.data.Link(this.firstItem, this.deprecatedActivity, VandVConstants.PerformedByCategory);
            this.data.Link(this.firstItem, this.activity, VandVConstants.PerformedByCategory);
            this.data.Link(this.firstItem, this.itemStep, VandVConstants.HasStepCategory);
            this.data.Link(this.itemInDeprecatedSpecification, this.secondRequirement, VandVConstants.VerifiesCategory);
            this.data.Link(this.itemInDeprecatedSpecification, this.itemStep, VandVConstants.HasStepCategory);
            this.data.Link(this.itemInDeprecatedSpecification, new Parameter { Iid = Guid.NewGuid() }, VandVConstants.CoversParameterCategory);
            this.data.Link(deprecatedItem, this.activity, VandVConstants.PerformedByCategory);
            this.data.Link(this.secondItem, activityInDeprecatedReport, VandVConstants.PerformedByCategory);

            foreach (var step in new[] { this.unnumberedActivityStep, this.secondActivityStep, this.firstActivityStep, deprecatedStep, stepInDeprecatedSpecification })
            {
                this.data.Link(this.activity, step, VandVConstants.HasStepCategory);
            }

            this.parameter = new Parameter { Iid = Guid.NewGuid() };
            this.data.Link(this.firstItem, this.parameter, VandVConstants.CoversParameterCategory);
            this.data.Link(this.firstItem, new Option { Iid = Guid.NewGuid() }, VandVConstants.CoversOptionCategory);

            this.data.SetValue(this.activity, VandVConstants.Stage, "CDR");
            this.data.SetValue(this.secondItem, VandVConstants.Stage, "In_Service");
            this.data.SetValue(this.uncoveringItem, VandVConstants.Stage, "Orbit");
            this.data.Annotate<RequestForWaiver>(this.secondItem, AnnotationStatusKind.OPEN);
        }

        [Test]
        public void VerifyBuild()
        {
            var empty = VandVModel.Build(null, null);
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(empty.Iteration, Is.Null);
                Assert.That(empty.Items, Is.Empty);
                Assert.That(empty.RequirementsToCover, Is.Empty);
                Assert.That(empty.StageGates, Is.EqualTo(VandVConstants.DefaultStageGates));

                Assert.That(model.Iteration, Is.SameAs(this.data.Iteration));
                Assert.That(model.VandVSpecification, Is.SameAs(this.vandVSpecification));
                Assert.That(model.Reports, Is.EqualTo(new[] { this.report }));
                Assert.That(model.Items, Is.EqualTo(new[] { this.firstItem, this.secondItem, this.uncoveringItem }));
                Assert.That(model.Activities, Is.EqualTo(new[] { this.activity }));
                Assert.That(model.RequirementsToCover, Is.EqualTo(new[] { this.firstRequirement, this.secondRequirement, this.thirdRequirement }));
                Assert.That(model.Annotations, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void VerifyGetActivity()
        {
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.GetActivity(this.firstItem), Is.SameAs(this.activity), "the deprecated activity linked first is skipped");
                Assert.That(model.GetActivity(this.secondItem), Is.Null, "an activity in a deprecated report performs nothing");
                Assert.That(model.GetActivity(null), Is.Null);
            });
        }

        [Test]
        public void VerifyGetPerformedItems()
        {
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.GetPerformedItems(this.activity), Is.EqualTo(new[] { this.firstItem }), "the deprecated item it performs is excluded");
                Assert.That(model.GetPerformedItems(this.deprecatedActivity), Is.Empty);
                Assert.That(model.GetPerformedItems(null), Is.Empty);
            });
        }

        [Test]
        public void VerifyGetSteps()
        {
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.GetSteps(this.activity), Is.EqualTo(new[] { this.firstActivityStep, this.secondActivityStep, this.unnumberedActivityStep }),
                    "ordered by number, unparseable last, deprecated steps and steps in a deprecated specification excluded");

                Assert.That(model.GetSteps(this.firstItem), Is.EqualTo(new[] { this.itemStep }));
                Assert.That(model.GetSteps(this.itemInDeprecatedSpecification), Is.Empty, "an owner in a deprecated specification");
                Assert.That(model.GetSteps(this.firstRequirement), Is.Empty);
                Assert.That(model.GetSteps(null), Is.Empty);
            });
        }

        [Test]
        public void VerifyGetCoveringItems()
        {
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.GetCoveringItems(this.firstRequirement), Is.EqualTo(new[] { this.firstItem, this.secondItem }), "verifies and validates, ordered by short name");
                Assert.That(model.GetCoveringItems(this.secondRequirement), Is.Empty, "a deprecated item, an item in a deprecated specification and a link between two requirements are not coverage");
                Assert.That(model.GetCoveringItems(null), Is.Empty);
            });
        }

        [Test]
        public void VerifyGetCoveredRequirement()
        {
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(model.GetCoveredRequirement(this.firstItem), Is.SameAs(this.firstRequirement));
                Assert.That(model.GetCoveredRequirement(this.uncoveringItem), Is.Null);
                Assert.That(model.GetCoveredRequirement(this.itemInDeprecatedSpecification), Is.Null);
                Assert.That(model.GetCoveredRequirement(null), Is.Null);
            });
        }

        [Test]
        public void VerifyGetMeasurementLinks()
        {
            var model = this.data.BuildModel();
            var links = model.GetMeasurementLinks(this.firstItem);

            Assert.Multiple(() =>
            {
                Assert.That(links, Has.Count.EqualTo(2), "covers links only, not the coverage, performed by or step links");
                Assert.That(links[0].Target, Is.SameAs(this.parameter));
                Assert.That(links[1].Target, Is.InstanceOf<Option>());
                Assert.That(model.GetMeasurementLinks(this.secondItem), Is.Empty);
                Assert.That(model.GetMeasurementLinks(this.itemInDeprecatedSpecification), Is.Empty);
                Assert.That(model.GetMeasurementLinks(null), Is.Empty);
            });
        }

        [Test]
        public void VerifyStageGates()
        {
            var withDefaults = this.data.BuildModel();

            var stage = (EnumerationParameterType)this.data.ParameterTypes[VandVConstants.Stage];
            stage.ValueDefinition.Clear();
            stage.ValueDefinition.Add(new EnumerationValueDefinition { Iid = Guid.NewGuid(), Name = "PDR", ShortName = "PDR" });
            stage.ValueDefinition.Add(new EnumerationValueDefinition { Iid = Guid.NewGuid(), Name = "CDR", ShortName = "CDR" });
            var custom = this.data.BuildModel();

            stage.ValueDefinition.Clear();
            var emptyEnumeration = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(withDefaults.StageGates, Is.EqualTo(VandVConstants.DefaultStageGates.Append("Orbit")),
                    "In_Service matches In-Service, CDR is inherited from the activity, Orbit is not defined so it is appended");

                Assert.That(custom.StageGates, Is.EqualTo(new[] { "PDR", "CDR", "In_Service", "Orbit" }), "the RDL order is the project order");
                Assert.That(emptyEnumeration.StageGates, Is.EqualTo(VandVConstants.DefaultStageGates.Append("Orbit")));
                Assert.That(VandVModel.Build(this.data.Iteration, null).StageGates, Is.EqualTo(VandVConstants.DefaultStageGates.Append("Orbit")));
            });
        }
    }
}
