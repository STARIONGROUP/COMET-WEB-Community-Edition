// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementAnnotationHelperTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor.Annotations
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementAnnotationHelper" />
    /// </summary>
    [TestFixture]
    public class RequirementAnnotationHelperTestFixture
    {
        /// <summary>
        /// The author of the annotations built by the tests
        /// </summary>
        private Participant author;

        /// <summary>
        /// The domain owning the annotations built by the tests
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// The annotated requirement
        /// </summary>
        private Requirement requirement;

        [SetUp]
        public void SetUp()
        {
            this.author = new Participant { Iid = Guid.NewGuid() };
            this.owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "REQ-1" };
        }

        [Test]
        public void VerifyGetKind()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.GetKind(new EngineeringModelDataNote()), Is.EqualTo(RequirementAnnotationKind.Note));
                Assert.That(RequirementAnnotationHelper.GetKind(new ChangeRequest()), Is.EqualTo(RequirementAnnotationKind.ChangeRequest));
                Assert.That(RequirementAnnotationHelper.GetKind(new ReviewItemDiscrepancy()), Is.EqualTo(RequirementAnnotationKind.ReviewItemDiscrepancy));
                Assert.That(RequirementAnnotationHelper.GetKind(new RequestForDeviation()), Is.EqualTo(RequirementAnnotationKind.RequestForDeviation));
                Assert.That(RequirementAnnotationHelper.GetKind(new RequestForWaiver()), Is.EqualTo(RequirementAnnotationKind.RequestForWaiver));
                Assert.That(RequirementAnnotationHelper.GetKind(new ChangeProposal()), Is.EqualTo(RequirementAnnotationKind.ChangeProposal));
                Assert.That(RequirementAnnotationHelper.GetKind(new ActionItem()), Is.Null);
            }
        }

        [Test]
        public void VerifyKindDescriptors()
        {
            var kinds = Enum.GetValues<RequirementAnnotationKind>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(kinds.Select(RequirementAnnotationHelper.GetLabel), Is.EqualTo(["Note", "Change Request", "Review Item Discrepancy", "Request for Deviation", "Request for Waiver", "Change Proposal"]));
                Assert.That(kinds.Select(RequirementAnnotationHelper.GetAcronym), Is.EqualTo(["Note", "CR", "RID", "RFD", "RFW", "CP"]));
                Assert.That(kinds.Select(RequirementAnnotationHelper.GetCssSuffix), Is.EqualTo(["note", "cr", "rid", "rfd", "rfw", "cp"]));
                Assert.That(kinds.Select(RequirementAnnotationHelper.GetClassKind), Is.EqualTo([ClassKind.EngineeringModelDataNote, ClassKind.ChangeRequest, ClassKind.ReviewItemDiscrepancy, ClassKind.RequestForDeviation, ClassKind.RequestForWaiver, ClassKind.ChangeProposal]));
                Assert.That(kinds.Select(RequirementAnnotationHelper.IsModellingAnnotationItem), Is.EqualTo([false, true, true, true, true, true]));
            }
        }

        [Test]
        public void VerifyGetPluralLabel()
        {
            var kinds = Enum.GetValues<RequirementAnnotationKind>();

            Assert.That(kinds.Select(RequirementAnnotationHelper.GetPluralLabel), Is.EqualTo(["Notes", "Change Requests", "Review Item Discrepancies", "Requests for Deviation", "Requests for Waiver", "Change Proposals"]));
        }

        [Test]
        public void VerifyGetClassificationLabel()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.GetClassificationLabel(AnnotationClassificationKind.MAJOR), Is.EqualTo("Major"));
                Assert.That(RequirementAnnotationHelper.GetClassificationLabel(AnnotationClassificationKind.MINOR), Is.EqualTo("Minor"));
            }
        }

        [Test]
        public void VerifyGetSummaryTooltip()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.ChangeRequest, 2, 1)), Is.EqualTo("1 of 2 Change Requests open"));
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.RequestForDeviation, 2, 1)), Is.EqualTo("1 of 2 Requests for Deviation open"));
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.ReviewItemDiscrepancy, 3, 3)), Is.EqualTo("3 of 3 Review Item Discrepancies open"));
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.ReviewItemDiscrepancy, 1, 0)), Is.EqualTo("0 of 1 Review Item Discrepancy open"));
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.RequestForWaiver, 1, 1)), Is.EqualTo("1 of 1 Request for Waiver open"));
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.Note, 1, 0)), Is.EqualTo("1 Note"));
                Assert.That(RequirementAnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(RequirementAnnotationKind.Note, 4, 0)), Is.EqualTo("4 Notes"));
            }
        }

        [Test]
        public void VerifyRequirementKindsAndStatuses()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.RequirementKinds, Is.EqualTo([RequirementAnnotationKind.Note, RequirementAnnotationKind.ChangeRequest, RequirementAnnotationKind.ReviewItemDiscrepancy, RequirementAnnotationKind.RequestForDeviation, RequirementAnnotationKind.RequestForWaiver]));
                Assert.That(RequirementAnnotationHelper.RequirementKinds, Does.Not.Contain(RequirementAnnotationKind.ChangeProposal));
                Assert.That(RequirementAnnotationHelper.Statuses, Is.EquivalentTo(Enum.GetValues<AnnotationStatusKind>()));
                Assert.That(RequirementAnnotationHelper.GetStatusLabel(AnnotationStatusKind.WONTFIX), Is.EqualTo("Won't fix"));
                Assert.That(RequirementAnnotationHelper.Statuses.Select(RequirementAnnotationHelper.GetStatusLabel), Is.EqualTo(["Open", "Done", "Closed", "Invalid", "Won't fix", "Not applicable"]));
                Assert.That(Enum.GetValues<AnnotationApprovalKind>().Select(RequirementAnnotationHelper.GetApprovalLabel), Is.EquivalentTo(["Accepted", "Rejected", "Withdrawn"]));
            }
        }

        [Test]
        public void VerifyIsOpen()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.IsOpen(new ChangeRequest { Status = AnnotationStatusKind.OPEN }), Is.True);
                Assert.That(RequirementAnnotationHelper.IsOpen(new ReviewItemDiscrepancy { Status = AnnotationStatusKind.DONE }), Is.False);
                Assert.That(RequirementAnnotationHelper.IsOpen(new RequestForWaiver { Status = AnnotationStatusKind.CLOSED }), Is.False);
                Assert.That(RequirementAnnotationHelper.IsOpen(new EngineeringModelDataNote()), Is.False);
            }
        }

        [Test]
        public void VerifyGetAnnotatedThingIid()
        {
            var annotated = new ChangeRequest { PrimaryAnnotatedThing = new ModellingThingReference(this.requirement) { Iid = Guid.NewGuid() } };
            var emptyReference = new ChangeRequest { PrimaryAnnotatedThing = new ModellingThingReference { Iid = Guid.NewGuid() } };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.GetAnnotatedThingIid(annotated), Is.EqualTo(this.requirement.Iid));
                Assert.That(RequirementAnnotationHelper.GetAnnotatedThingIid(new ChangeRequest()), Is.Null);
                Assert.That(RequirementAnnotationHelper.GetAnnotatedThingIid(emptyReference), Is.Null);
            }
        }

        [Test]
        public void VerifyGetNextShortName()
        {
            List<ModellingAnnotationItem> existing =
            [
                new ReviewItemDiscrepancy { ShortName = "R1-RID-001" },
                new ReviewItemDiscrepancy { ShortName = "R1-RID-004" },
                new ReviewItemDiscrepancy { ShortName = "R1-RID-xyz" },
                new ReviewItemDiscrepancy { ShortName = "R1-RID--7" },
                new ReviewItemDiscrepancy { ShortName = null },
                new ChangeRequest { ShortName = "R1-CR-009" },
                new ChangeRequest { ShortName = "r2-cr-012" },
                new ChangeRequest { ShortName = "R10-CR-050" },
                new ChangeRequest { ShortName = "CR-020" }
            ];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ReviewItemDiscrepancy, "R1", []), Is.EqualTo("R1-RID-001"));
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, "R1", []), Is.EqualTo("R1-CR-001"));
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ReviewItemDiscrepancy, "R1", existing), Is.EqualTo("R1-RID-005"), "the highest number with the prefix plus one, ignoring non numeric suffixes");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, "R1", existing), Is.EqualTo("R1-CR-010"), "another requirement sharing the leading characters does not count");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, "R2", existing), Is.EqualTo("R2-CR-013"), "the prefix is matched case-insensitively");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.RequestForWaiver, "R1", existing), Is.EqualTo("R1-RFW-001"), "another kind does not count");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.RequestForDeviation, "R3", existing), Is.EqualTo("R3-RFD-001"), "another requirement does not count");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeProposal, "R1", existing), Is.EqualTo("R1-CP-001"));
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, "R1", [new ChangeRequest { ShortName = "R1-CR-999" }]), Is.EqualTo("R1-CR-1000"), "the number grows past three digits");

                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, "REQ-1", [new ChangeRequest { ShortName = "REQ-1-CR-003" }]), Is.EqualTo("REQ-1-CR-004"), "the requirement short name is kept as is");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, " R1 ", []), Is.EqualTo("R1-CR-001"), "the requirement short name is trimmed");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, string.Empty, existing), Is.EqualTo("CR-021"), "without a requirement short name only the kind prefixes the number");
                Assert.That(RequirementAnnotationHelper.GetNextShortName(RequirementAnnotationKind.ChangeRequest, null, []), Is.EqualTo("CR-001"));
            }
        }

        [Test]
        public void VerifyCreateAnnotation()
        {
            var (note, noteReference) = RequirementAnnotationHelper.CreateAnnotation(RequirementAnnotationKind.Note, this.requirement, this.author, this.owner, "en", "Note-001", "ignored", "a note", AnnotationClassificationKind.MAJOR);
            var (item, itemReference) = RequirementAnnotationHelper.CreateAnnotation(RequirementAnnotationKind.ReviewItemDiscrepancy, this.requirement, this.author, this.owner, "en", "RID-001", "a title", "an item", AnnotationClassificationKind.MAJOR);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(note, Is.TypeOf<EngineeringModelDataNote>());
                Assert.That(note.Iid, Is.Not.EqualTo(Guid.Empty));
                Assert.That(note.Author, Is.SameAs(this.author));
                Assert.That(note.Content, Is.EqualTo("a note"));
                Assert.That(note.LanguageCode, Is.EqualTo("en"));
                Assert.That(note.CreatedOn, Is.Not.EqualTo(default(DateTime)));
                Assert.That(note.PrimaryAnnotatedThing, Is.SameAs(noteReference));
                Assert.That(note.RelatedThing, Is.EqualTo([noteReference]));
                Assert.That(noteReference.ReferencedThing, Is.SameAs(this.requirement));
                Assert.That(noteReference.Iid, Is.Not.EqualTo(Guid.Empty));

                Assert.That(item, Is.TypeOf<ReviewItemDiscrepancy>());
                Assert.That(item.PrimaryAnnotatedThing, Is.SameAs(itemReference));
                Assert.That(itemReference.ReferencedThing, Is.SameAs(this.requirement));
                var modellingAnnotationItem = (ModellingAnnotationItem)item;
                Assert.That(modellingAnnotationItem.Owner, Is.SameAs(this.owner));
                Assert.That(modellingAnnotationItem.ShortName, Is.EqualTo("RID-001"));
                Assert.That(modellingAnnotationItem.Title, Is.EqualTo("a title"));
                Assert.That(modellingAnnotationItem.Classification, Is.EqualTo(AnnotationClassificationKind.MAJOR));
                Assert.That(modellingAnnotationItem.Status, Is.EqualTo(AnnotationStatusKind.OPEN));
            }
        }

        [Test]
        public void VerifyCreateApproval()
        {
            var withComment = RequirementAnnotationHelper.CreateApproval(this.author, this.owner, "en", AnnotationApprovalKind.ACCEPTED, "looks good");
            var withoutComment = RequirementAnnotationHelper.CreateApproval(this.author, this.owner, "en", AnnotationApprovalKind.REJECTED, "  ");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(withComment.Iid, Is.Not.EqualTo(Guid.Empty));
                Assert.That(withComment.Author, Is.SameAs(this.author));
                Assert.That(withComment.Owner, Is.SameAs(this.owner));
                Assert.That(withComment.Classification, Is.EqualTo(AnnotationApprovalKind.ACCEPTED));
                Assert.That(withComment.Content, Is.EqualTo("looks good"));
                Assert.That(withComment.LanguageCode, Is.EqualTo("en"));
                Assert.That(withoutComment.Content, Is.EqualTo("Rejected"));
            }
        }

        [Test]
        public void VerifyCreateSolution()
        {
            var solution = RequirementAnnotationHelper.CreateSolution(this.author, this.owner, "en", "a fix");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(solution.Iid, Is.Not.EqualTo(Guid.Empty));
                Assert.That(solution.Author, Is.SameAs(this.author));
                Assert.That(solution.Owner, Is.SameAs(this.owner));
                Assert.That(solution.Content, Is.EqualTo("a fix"));
                Assert.That(solution.LanguageCode, Is.EqualTo("en"));
                Assert.That(solution.CreatedOn, Is.Not.EqualTo(default(DateTime)));
            }
        }

        [Test]
        public void VerifyCreateDiscussionItem()
        {
            var item = RequirementAnnotationHelper.CreateDiscussionItem(this.author, "en", "a reply");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.Iid, Is.Not.EqualTo(Guid.Empty));
                Assert.That(item.Author, Is.SameAs(this.author));
                Assert.That(item.Content, Is.EqualTo("a reply"));
                Assert.That(item.LanguageCode, Is.EqualTo("en"));
                Assert.That(item.CreatedOn, Is.Not.EqualTo(default(DateTime)));
            }
        }
    }
}
