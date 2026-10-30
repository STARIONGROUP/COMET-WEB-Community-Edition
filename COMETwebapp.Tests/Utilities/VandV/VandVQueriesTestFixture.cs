// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVQueriesTestFixture.cs" company="Starion Group S.A.">
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

    using COMETwebapp.Utilities.VandV;

    using NUnit.Framework;

    [TestFixture]
    public class VandVQueriesTestFixture
    {
        private VandVTestData data;
        private RequirementsSpecification specification;
        private RequirementsSpecification vandVSpecification;
        private RequirementsSpecification report;
        private Requirement requirement;
        private Requirement item;
        private Requirement activity;
        private BinaryRelationship verifiesLink;

        [SetUp]
        public void SetUp()
        {
            this.data = new VandVTestData();
            this.specification = this.data.AddSpecification("SPEC");
            this.requirement = this.data.AddRequirement(this.specification, "R1", VandVTestData.RequirementCategoryShortName);
            this.vandVSpecification = this.data.AddSpecification(VandVConstants.VandVSpecificationShortName);
            this.item = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_1", VandVConstants.VnVItemCategory);
            this.report = this.data.AddSpecification("FAT_REPORT", VandVConstants.VnVReportCategory);
            this.activity = this.data.AddRequirement(this.report, "ACT_1", VandVConstants.VnVActivityCategory);
            this.verifiesLink = this.data.Link(this.item, this.requirement, VandVConstants.VerifiesCategory);
            this.data.Link(this.item, this.activity, VandVConstants.PerformedByCategory);
        }

        [Test]
        public void VerifyIsCategorizedAs()
        {
            var subCategorized = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_2", VandVConstants.ValidationItemCategory);

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsCategorizedAs(this.item, VandVConstants.VnVItemCategory), Is.True);
                Assert.That(VandVQueries.IsCategorizedAs(subCategorized, VandVConstants.VnVItemCategory), Is.True, "through the super category");
                Assert.That(VandVQueries.IsCategorizedAs(this.requirement, VandVConstants.VnVItemCategory), Is.False);
                Assert.That(VandVQueries.IsCategorizedAs(null, VandVConstants.VnVItemCategory), Is.False);
            });
        }

        [Test]
        public void VerifyIsItem()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsItem(this.item), Is.True);
                Assert.That(VandVQueries.IsItem(this.activity), Is.False);
                Assert.That(VandVQueries.IsItem(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsStep()
        {
            var step = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_1_S01", VandVConstants.VnVStepCategory);

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsStep(step), Is.True);
                Assert.That(VandVQueries.IsStep(this.item), Is.False);
                Assert.That(VandVQueries.IsStep(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsActivity()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsActivity(this.activity), Is.True);
                Assert.That(VandVQueries.IsActivity(this.item), Is.False);
                Assert.That(VandVQueries.IsActivity(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsReport()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsReport(this.report), Is.True);
                Assert.That(VandVQueries.IsReport(this.vandVSpecification), Is.False);
                Assert.That(VandVQueries.IsReport(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsVandVSpecification()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsVandVSpecification(this.vandVSpecification), Is.True);
                Assert.That(VandVQueries.IsVandVSpecification(this.specification), Is.False);
                Assert.That(VandVQueries.IsVandVSpecification(new RequirementsSpecification { ShortName = "vnv" }), Is.False, "short names are case-sensitive");
                Assert.That(VandVQueries.IsVandVSpecification(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsCoverageLink()
        {
            var validates = this.data.Link(this.item, this.requirement, VandVConstants.ValidatesCategory);
            var performedBy = this.data.Iteration.Relationship.OfType<BinaryRelationship>().Single(x => x.Target == this.activity);

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsCoverageLink(this.verifiesLink), Is.True);
                Assert.That(VandVQueries.IsCoverageLink(validates), Is.True);
                Assert.That(VandVQueries.IsCoverageLink(performedBy), Is.False);
                Assert.That(VandVQueries.IsCoverageLink(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsVandVThing()
        {
            var step = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_1_S01", VandVConstants.VnVStepCategory);
            var plainRequirementInPlan = this.data.AddRequirement(this.vandVSpecification, "NOTE_1");
            var itemInNormalSpecification = this.data.AddRequirement(this.specification, "VNV_STRAY_1", VandVConstants.VnVItemCategory);
            var groupInPlan = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "CDR" };
            this.vandVSpecification.Group.Add(groupInPlan);
            var groupInSpecification = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "OPS" };
            this.specification.Group.Add(groupInSpecification);
            var constraintOfItem = new ParametricConstraint { Iid = Guid.NewGuid() };
            this.item.ParametricConstraint.Add(constraintOfItem);
            var otherRequirement = this.data.AddRequirement(this.specification, "R2", VandVTestData.RequirementCategoryShortName);
            var ordinaryVerifies = this.data.Link(otherRequirement, this.requirement, VandVConstants.VerifiesCategory);
            var parameter = new Parameter { Iid = Guid.NewGuid() };
            var coversParameter = this.data.Link(this.item, parameter, VandVConstants.CoversParameterCategory);
            var hasStepBetweenRequirements = this.data.Link(this.requirement, otherRequirement, VandVConstants.HasStepCategory);
            var multiWithItem = new MultiRelationship { Iid = Guid.NewGuid(), RelatedThing = { this.requirement, this.item } };
            var multiWithoutItem = new MultiRelationship { Iid = Guid.NewGuid(), RelatedThing = { this.requirement, otherRequirement } };
            var multiWithVandVCategory = new MultiRelationship { Iid = Guid.NewGuid(), RelatedThing = { this.requirement, otherRequirement }, Category = { this.data.Categories[VandVConstants.PerformedByCategory] } };
            var cycleStart = new BinaryRelationship { Iid = Guid.NewGuid(), Target = this.requirement };
            var cycleEnd = new BinaryRelationship { Iid = Guid.NewGuid(), Source = cycleStart, Target = cycleStart };
            cycleStart.Source = cycleEnd;

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsVandVThing(null), Is.False);
                Assert.That(VandVQueries.IsVandVThing(this.vandVSpecification), Is.True);
                Assert.That(VandVQueries.IsVandVThing(this.report), Is.True);
                Assert.That(VandVQueries.IsVandVThing(this.specification), Is.False);
                Assert.That(VandVQueries.IsVandVThing(this.item), Is.True);
                Assert.That(VandVQueries.IsVandVThing(step), Is.True);
                Assert.That(VandVQueries.IsVandVThing(this.activity), Is.True);
                Assert.That(VandVQueries.IsVandVThing(plainRequirementInPlan), Is.True, "anything filed in the V&V plan");
                Assert.That(VandVQueries.IsVandVThing(itemInNormalSpecification), Is.True, "an item moved into a normal specification");
                Assert.That(VandVQueries.IsVandVThing(this.requirement), Is.False);
                Assert.That(VandVQueries.IsVandVThing(groupInPlan), Is.True);
                Assert.That(VandVQueries.IsVandVThing(groupInSpecification), Is.False);
                Assert.That(VandVQueries.IsVandVThing(constraintOfItem), Is.True);
                Assert.That(VandVQueries.IsVandVThing(this.verifiesLink), Is.True, "a coverage link from an item");
                Assert.That(VandVQueries.IsVandVThing(ordinaryVerifies), Is.False, "verifies between two requirements is traceability");
                Assert.That(VandVQueries.IsVandVThing(coversParameter), Is.True);
                Assert.That(VandVQueries.IsVandVThing(hasStepBetweenRequirements), Is.True, "a V&V-only category");
                Assert.That(VandVQueries.IsVandVThing(multiWithItem), Is.True);
                Assert.That(VandVQueries.IsVandVThing(multiWithoutItem), Is.False);
                Assert.That(VandVQueries.IsVandVThing(multiWithVandVCategory), Is.True);
                Assert.That(VandVQueries.IsVandVThing(cycleStart), Is.False, "relationships pointing at each other do not recurse forever");
                Assert.That(VandVQueries.IsVandVThing(parameter), Is.False);
                Assert.That(VandVQueries.IsVandVThing(this.data.Iteration), Is.False);
            });
        }

        [Test]
        public void VerifyGetSimpleParameterValue()
        {
            this.data.SetValue(this.item, VandVConstants.Method, "Test");

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetSimpleParameterValue(this.item, VandVConstants.Method).Value[0], Is.EqualTo("Test"));
                Assert.That(VandVQueries.GetSimpleParameterValue(this.item, VandVConstants.Stage), Is.Null);
                Assert.That(VandVQueries.GetSimpleParameterValue(null, VandVConstants.Method), Is.Null);
            });
        }

        [Test]
        public void VerifyGetValue()
        {
            this.data.SetValue(this.item, VandVConstants.Method, "Test");
            this.data.SetValue(this.item, VandVConstants.Stage, "-");
            this.data.SetValue(this.item, VandVConstants.Level, "  ");

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetValue(this.item, VandVConstants.Method), Is.EqualTo("Test"));
                Assert.That(VandVQueries.GetValue(this.item, VandVConstants.Stage), Is.Null, "the - placeholder is unset");
                Assert.That(VandVQueries.GetValue(this.item, VandVConstants.Level), Is.Null);
                Assert.That(VandVQueries.GetValue(this.item, VandVConstants.Result), Is.Null);
            });
        }

        [Test]
        public void VerifyGetDisplayValue()
        {
            this.data.SetValue(this.item, VandVConstants.Compliance, "Non_Compliant");
            this.data.SetValue(this.item, VandVConstants.Status, "In Progress");
            this.data.SetValue(this.item, VandVConstants.Method, "Telepathy");
            this.data.SetValue(this.item, VandVConstants.Result, "Non_Compliant");

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetDisplayValue(this.item, VandVConstants.Compliance), Is.EqualTo("Non-Compliant"), "a short name shows as its name");
                Assert.That(VandVQueries.GetDisplayValue(this.item, VandVConstants.Status), Is.EqualTo("In Progress"));
                Assert.That(VandVQueries.GetDisplayValue(this.item, VandVConstants.Method), Is.EqualTo("Telepathy"), "an undefined value shows as stored");
                Assert.That(VandVQueries.GetDisplayValue(this.item, VandVConstants.Result), Is.EqualTo("Non_Compliant"), "text is never resolved");
                Assert.That(VandVQueries.GetDisplayValue(this.item, VandVConstants.Stage), Is.Null);
            });
        }

        [Test]
        public void VerifyGetBooleanValue()
        {
            string[] trueValues = ["true", "TRUE", " yes ", "1", "-1"];
            string[] falseValues = ["false", "0", "no"];

            Assert.Multiple(() =>
            {
                foreach (var value in trueValues)
                {
                    this.data.SetValue(this.item, VandVConstants.Closed, value);
                    Assert.That(VandVQueries.GetBooleanValue(this.item, VandVConstants.Closed), Is.True, value);
                }

                foreach (var value in falseValues)
                {
                    this.data.SetValue(this.item, VandVConstants.Closed, value);
                    Assert.That(VandVQueries.GetBooleanValue(this.item, VandVConstants.Closed), Is.False, value);
                }

                Assert.That(VandVQueries.GetBooleanValue(this.activity, VandVConstants.Closed), Is.False);
            });
        }

        [Test]
        public void VerifyGetEffectiveSource()
        {
            this.data.SetValue(this.activity, VandVConstants.Method, "Test");
            this.data.SetValue(this.activity, VandVConstants.Stage, "CDR");
            this.data.SetValue(this.item, VandVConstants.Stage, "PDR");
            this.data.SetValue(this.activity, VandVConstants.AcceptanceCriteria, "Activity criteria");
            var unperformed = this.data.AddRequirement(this.vandVSpecification, "VNV_R1_2", VandVConstants.VnVItemCategory);
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetEffectiveSource(model, this.item, VandVConstants.Method), Is.SameAs(this.activity), "inherited");
                Assert.That(VandVQueries.GetEffectiveSource(model, this.item, VandVConstants.Stage), Is.SameAs(this.item), "the item's own value wins");
                Assert.That(VandVQueries.GetEffectiveSource(model, this.item, VandVConstants.AcceptanceCriteria), Is.SameAs(this.item), "acceptance never derives");
                Assert.That(VandVQueries.GetEffectiveSource(model, this.item, VandVConstants.Level), Is.SameAs(this.item), "neither states it");
                Assert.That(VandVQueries.GetEffectiveSource(model, unperformed, VandVConstants.Method), Is.SameAs(unperformed));
            });
        }

        [Test]
        public void VerifyGetEffectiveValue()
        {
            this.data.SetValue(this.activity, VandVConstants.Status, "Passed");
            this.data.SetValue(this.activity, VandVConstants.Compliance, "Compliant");
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetEffectiveValue(model, this.item, VandVConstants.Status), Is.EqualTo("Passed"));
                Assert.That(VandVQueries.GetEffectiveValue(model, this.item, VandVConstants.Compliance), Is.Null, "compliance never derives");
            });
        }

        [Test]
        public void VerifyGetEffectiveDisplayValue()
        {
            this.data.SetValue(this.activity, VandVConstants.Stage, "In_Service");
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetEffectiveDisplayValue(model, this.item, VandVConstants.Stage), Is.EqualTo("In-Service"));
                Assert.That(VandVQueries.GetEffectiveDisplayValue(model, this.item, VandVConstants.Method), Is.Null);
            });
        }

        [Test]
        public void VerifyGetCompliance()
        {
            var unset = VandVQueries.GetCompliance(this.item);
            this.data.SetValue(this.item, VandVConstants.Compliance, "Partially_Compliant");

            Assert.Multiple(() =>
            {
                Assert.That(unset, Is.EqualTo(VandVConstants.NotAssessed));
                Assert.That(VandVQueries.GetCompliance(this.item), Is.EqualTo("Partially_Compliant"));
            });
        }

        [Test]
        public void VerifyIsShortfall()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsShortfall("Partially Compliant"), Is.True);
                Assert.That(VandVQueries.IsShortfall("Non_Compliant"), Is.True);
                Assert.That(VandVQueries.IsShortfall(VandVConstants.ComplianceCompliant), Is.False);
                Assert.That(VandVQueries.IsShortfall(VandVConstants.NotAssessed), Is.False);
                Assert.That(VandVQueries.IsShortfall(null), Is.False);
            });
        }

        [Test]
        public void VerifyIsClosed()
        {
            var unset = VandVQueries.IsClosed(this.item);
            this.data.SetValue(this.item, VandVConstants.Closed, "true");

            Assert.Multiple(() =>
            {
                Assert.That(unset, Is.False);
                Assert.That(VandVQueries.IsClosed(this.item), Is.True);
            });
        }

        [Test]
        public void VerifyReferences()
        {
            var both = this.data.Annotate<ReviewItemDiscrepancy>(this.item, AnnotationStatusKind.OPEN);
            var primaryOnly = new ReviewItemDiscrepancy { PrimaryAnnotatedThing = new ModellingThingReference(this.item) };
            var relatedOnly = new ReviewItemDiscrepancy();
            relatedOnly.RelatedThing.Add(new ModellingThingReference(this.item));

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.References(both, this.item), Is.True);
                Assert.That(VandVQueries.References(primaryOnly, this.item), Is.True);
                Assert.That(VandVQueries.References(relatedOnly, this.item), Is.True);
                Assert.That(VandVQueries.References(both, this.requirement), Is.False);
                Assert.That(VandVQueries.References(new ReviewItemDiscrepancy(), this.item), Is.False);
                Assert.That(VandVQueries.References(null, this.item), Is.False);
                Assert.That(VandVQueries.References(both, null), Is.False);
            });
        }

        [Test]
        public void VerifyIsReviewRequest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.IsReviewRequest(new ReviewItemDiscrepancy()), Is.True);
                Assert.That(VandVQueries.IsReviewRequest(new RequestForDeviation()), Is.True);
                Assert.That(VandVQueries.IsReviewRequest(new RequestForWaiver()), Is.True);
                Assert.That(VandVQueries.IsReviewRequest(new ChangeRequest()), Is.False);
                Assert.That(VandVQueries.IsReviewRequest(null), Is.False);
            });
        }

        [Test]
        public void VerifyHasOpenReviewRequest()
        {
            var annotations = this.data.EngineeringModel.ModellingAnnotation;
            var none = VandVQueries.HasOpenReviewRequest(annotations, this.item);

            this.data.Annotate<ReviewItemDiscrepancy>(this.item, AnnotationStatusKind.CLOSED);
            this.data.Annotate<ChangeRequest>(this.item, AnnotationStatusKind.OPEN);
            this.data.Annotate<RequestForWaiver>(this.requirement, AnnotationStatusKind.OPEN);
            var resolvedOrNotARequest = VandVQueries.HasOpenReviewRequest(annotations, this.item);

            this.data.Annotate<RequestForDeviation>(this.item, AnnotationStatusKind.OPEN);

            Assert.Multiple(() =>
            {
                Assert.That(none, Is.False);
                Assert.That(resolvedOrNotARequest, Is.False, "a closed RID, an open change request and a request on another thing");
                Assert.That(VandVQueries.HasOpenReviewRequest(annotations, this.item), Is.True);
                Assert.That(VandVQueries.HasOpenReviewRequest(annotations, this.requirement), Is.True);
                Assert.That(VandVQueries.HasOpenReviewRequest(null, this.item), Is.False);
            });
        }

        [Test]
        public void VerifyHasAcceptedConcession()
        {
            var annotations = this.data.EngineeringModel.ModellingAnnotation;
            this.data.Annotate<RequestForWaiver>(this.item, AnnotationStatusKind.OPEN);
            this.data.Annotate<ReviewItemDiscrepancy>(this.item, AnnotationStatusKind.CLOSED);
            var openOrNotAConcession = VandVQueries.HasAcceptedConcession(annotations, this.item);

            this.data.Annotate<RequestForDeviation>(this.item, AnnotationStatusKind.DONE);
            this.data.Annotate<RequestForWaiver>(this.activity, AnnotationStatusKind.CLOSED);

            Assert.Multiple(() =>
            {
                Assert.That(openOrNotAConcession, Is.False, "an open waiver and a closed RID");
                Assert.That(VandVQueries.HasAcceptedConcession(annotations, this.item), Is.True);
                Assert.That(VandVQueries.HasAcceptedConcession(annotations, this.activity), Is.True);
                Assert.That(VandVQueries.HasAcceptedConcession(annotations, this.requirement), Is.False);
                Assert.That(VandVQueries.HasAcceptedConcession(null, this.item), Is.False);
            });
        }

        [Test]
        public void VerifyGetItemOutcome()
        {
            var open = this.data.AddRequirement(this.vandVSpecification, "OPEN", VandVConstants.VnVItemCategory);
            var passed = this.CreateItem("PASSED", (VandVConstants.Status, "Passed"));
            var notApplicable = this.CreateItem("NA", (VandVConstants.Status, "Not_Applicable"));
            var failed = this.CreateItem("FAILED", (VandVConstants.Status, "Failed"));
            var shortfall = this.CreateItem("SHORT", (VandVConstants.Status, "Passed"), (VandVConstants.Compliance, "Partially Compliant"));
            var closedCompliant = this.CreateItem("CLOSED_OK", (VandVConstants.Closed, "true"), (VandVConstants.Status, "Failed"), (VandVConstants.Compliance, "Compliant"));
            var closedShortfall = this.CreateItem("CLOSED_SHORT", (VandVConstants.Closed, "true"), (VandVConstants.Compliance, "Non_Compliant"));
            var conceded = this.CreateItem("CONCEDED", (VandVConstants.Closed, "true"), (VandVConstants.Compliance, "Non-Compliant"));
            var pendingConcession = this.CreateItem("PENDING", (VandVConstants.Closed, "true"), (VandVConstants.Compliance, "Non-Compliant"));
            this.data.Annotate<RequestForWaiver>(conceded, AnnotationStatusKind.CLOSED);
            this.data.Annotate<RequestForWaiver>(pendingConcession, AnnotationStatusKind.OPEN);
            this.data.SetValue(this.activity, VandVConstants.Status, "Passed");
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetItemOutcome(model, open), Is.EqualTo(VandVItemOutcome.Open));
                Assert.That(VandVQueries.GetItemOutcome(model, passed), Is.EqualTo(VandVItemOutcome.Passed));
                Assert.That(VandVQueries.GetItemOutcome(model, notApplicable), Is.EqualTo(VandVItemOutcome.Passed));
                Assert.That(VandVQueries.GetItemOutcome(model, this.item), Is.EqualTo(VandVItemOutcome.Passed), "status inherited from the activity");
                Assert.That(VandVQueries.GetItemOutcome(model, failed), Is.EqualTo(VandVItemOutcome.Failed));
                Assert.That(VandVQueries.GetItemOutcome(model, shortfall), Is.EqualTo(VandVItemOutcome.Failed), "a passed test can still fall short");
                Assert.That(VandVQueries.GetItemOutcome(model, closedCompliant), Is.EqualTo(VandVItemOutcome.Passed), "close-out wins over the status");
                Assert.That(VandVQueries.GetItemOutcome(model, closedShortfall), Is.EqualTo(VandVItemOutcome.Failed));
                Assert.That(VandVQueries.GetItemOutcome(model, conceded), Is.EqualTo(VandVItemOutcome.Passed));
                Assert.That(VandVQueries.GetItemOutcome(model, pendingConcession), Is.EqualTo(VandVItemOutcome.Failed));
            });
        }

        [Test]
        public void VerifyGetRollUp()
        {
            var passed = this.CreateItem("PASSED", (VandVConstants.Status, "Passed"));
            var failed = this.CreateItem("FAILED", (VandVConstants.Status, "Failed"));
            var model = this.data.BuildModel();

            var mixed = VandVQueries.GetRollUp(model, [passed, failed, this.item]);
            var allPassed = VandVQueries.GetRollUp(model, [passed]);
            var none = VandVQueries.GetRollUp(model, []);

            Assert.Multiple(() =>
            {
                Assert.That(mixed, Is.EqualTo(new VandVRollUp(1, 1, 1)));
                Assert.That(mixed.Total, Is.EqualTo(3));
                Assert.That(mixed.IsVerified, Is.False);
                Assert.That(allPassed.IsVerified, Is.True);
                Assert.That(none.Total, Is.Zero);
                Assert.That(none.IsVerified, Is.False, "an uncovered requirement is not verified");
            });
        }

        [Test]
        public void VerifyGetCoverage()
        {
            var uncovered = this.data.AddRequirement(this.specification, "R2", VandVTestData.RequirementCategoryShortName);
            this.data.SetValue(this.activity, VandVConstants.Status, "Passed");
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.GetCoverage(model, this.requirement), Is.EqualTo(new VandVRollUp(1, 0, 0)));
                Assert.That(VandVQueries.GetCoverage(model, uncovered), Is.Null);
                Assert.That(VandVQueries.GetCoverage(model, null), Is.Null);
            });
        }

        [Test]
        public void VerifyFormatRequirementCoverage()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.FormatRequirementCoverage(new VandVRollUp(0, 0, 0)), Is.EqualTo("Not covered"));
                Assert.That(VandVQueries.FormatRequirementCoverage(new VandVRollUp(1, 0, 1)), Is.EqualTo("2 item(s): 1 passed, 1 open"));
                Assert.That(VandVQueries.FormatRequirementCoverage(new VandVRollUp(0, 2, 0)), Is.EqualTo("2 item(s): 2 failed"));
                Assert.That(VandVQueries.FormatRequirementCoverage(new VandVRollUp(1, 1, 1)), Is.EqualTo("3 item(s): 1 passed, 1 failed, 1 open"));
            });
        }

        [Test]
        public void VerifyFormatContainerCoverage()
        {
            var secondRequirement = this.data.AddRequirement(this.specification, "R2", VandVTestData.RequirementCategoryShortName);
            var uncovered = this.data.AddRequirement(this.specification, "R3", VandVTestData.RequirementCategoryShortName);
            var failed = this.CreateItem("FAILED", (VandVConstants.Status, "Failed"));
            this.data.Link(failed, secondRequirement, VandVConstants.VerifiesCategory);
            this.data.SetValue(this.activity, VandVConstants.Status, "Passed");
            var model = this.data.BuildModel();

            Assert.Multiple(() =>
            {
                Assert.That(VandVQueries.FormatContainerCoverage(model, []), Is.Empty);
                Assert.That(VandVQueries.FormatContainerCoverage(model, [this.requirement]), Is.EqualTo("1/1 verified"));
                Assert.That(VandVQueries.FormatContainerCoverage(model, [this.requirement, secondRequirement, uncovered]), Is.EqualTo("1/3 verified, 1 failed"));
            });
        }

        [Test]
        public void VerifySuggestItemShortName()
        {
            var free = VandVQueries.SuggestItemShortName(this.data.Iteration, this.data.AddRequirement(this.specification, "R9", VandVTestData.RequirementCategoryShortName));
            var dotted = this.data.AddRequirement(this.specification, "REQ.1", VandVTestData.RequirementCategoryShortName);
            var unnamed = this.data.AddRequirement(this.specification, " ", VandVTestData.RequirementCategoryShortName);

            Assert.Multiple(() =>
            {
                Assert.That(free, Is.EqualTo("VNV_R9_1"));
                Assert.That(VandVQueries.SuggestItemShortName(this.data.Iteration, this.requirement), Is.EqualTo("VNV_R1_2"), "VNV_R1_1 is taken");
                Assert.That(VandVQueries.SuggestItemShortName(this.data.Iteration, dotted), Is.EqualTo("VNV_REQ1_1"));
                Assert.That(VandVQueries.SuggestItemShortName(this.data.Iteration, unnamed), Is.EqualTo("VNV_REQ_1"));
            });
        }

        [Test]
        public void VerifySuggestActivityShortName()
        {
            var second = VandVQueries.SuggestActivityShortName(this.data.Iteration);
            this.data.AddRequirement(this.vandVSpecification, "ACT_2", VandVConstants.VnVActivityCategory);

            Assert.Multiple(() =>
            {
                Assert.That(second, Is.EqualTo("ACT_2"));
                Assert.That(VandVQueries.SuggestActivityShortName(this.data.Iteration), Is.EqualTo("ACT_3"));
            });
        }

        private Requirement CreateItem(string shortName, params (string ShortName, string Value)[] values)
        {
            var created = this.data.AddRequirement(this.vandVSpecification, shortName, VandVConstants.VnVItemCategory);

            foreach (var (parameterTypeShortName, value) in values)
            {
                this.data.SetValue(created, parameterTypeShortName, value);
            }

            return created;
        }
    }
}
