// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationHelperTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Annotations
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Annotations;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="AnnotationHelper" />
    /// </summary>
    [TestFixture]
    public class AnnotationHelperTestFixture
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
                Assert.That(AnnotationHelper.GetKind(new EngineeringModelDataNote()), Is.EqualTo(AnnotationKind.Note));
                Assert.That(AnnotationHelper.GetKind(new ChangeRequest()), Is.EqualTo(AnnotationKind.ChangeRequest));
                Assert.That(AnnotationHelper.GetKind(new ReviewItemDiscrepancy()), Is.EqualTo(AnnotationKind.ReviewItemDiscrepancy));
                Assert.That(AnnotationHelper.GetKind(new RequestForDeviation()), Is.EqualTo(AnnotationKind.RequestForDeviation));
                Assert.That(AnnotationHelper.GetKind(new RequestForWaiver()), Is.EqualTo(AnnotationKind.RequestForWaiver));
                Assert.That(AnnotationHelper.GetKind(new ChangeProposal()), Is.EqualTo(AnnotationKind.ChangeProposal));
                Assert.That(AnnotationHelper.GetKind(new ActionItem()), Is.Null);
            }
        }

        [Test]
        public void VerifyKindDescriptors()
        {
            var kinds = Enum.GetValues<AnnotationKind>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(kinds.Select(AnnotationHelper.GetLabel), Is.EqualTo(["Note", "Change Request", "Review Item Discrepancy", "Request for Deviation", "Request for Waiver", "Change Proposal"]));
                Assert.That(kinds.Select(AnnotationHelper.GetAcronym), Is.EqualTo(["Note", "CR", "RID", "RFD", "RFW", "CP"]));
                Assert.That(kinds.Select(AnnotationHelper.GetCssSuffix), Is.EqualTo(["note", "cr", "rid", "rfd", "rfw", "cp"]));
                Assert.That(kinds.Select(AnnotationHelper.GetClassKind), Is.EqualTo([ClassKind.EngineeringModelDataNote, ClassKind.ChangeRequest, ClassKind.ReviewItemDiscrepancy, ClassKind.RequestForDeviation, ClassKind.RequestForWaiver, ClassKind.ChangeProposal]));
                Assert.That(kinds.Select(AnnotationHelper.IsModellingAnnotationItem), Is.EqualTo([false, true, true, true, true, true]));
            }
        }

        [Test]
        public void VerifyGetPluralLabel()
        {
            var kinds = Enum.GetValues<AnnotationKind>();

            Assert.That(kinds.Select(AnnotationHelper.GetPluralLabel), Is.EqualTo(["Notes", "Change Requests", "Review Item Discrepancies", "Requests for Deviation", "Requests for Waiver", "Change Proposals"]));
        }

        [Test]
        public void VerifyGetClassificationLabel()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetClassificationLabel(AnnotationClassificationKind.MAJOR), Is.EqualTo("Major"));
                Assert.That(AnnotationHelper.GetClassificationLabel(AnnotationClassificationKind.MINOR), Is.EqualTo("Minor"));
            }
        }

        [Test]
        public void VerifyGetSummaryTooltip()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.ChangeRequest, 2, 1)), Is.EqualTo("2 Change Requests, 1 needing attention"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.RequestForDeviation, 2, 1)), Is.EqualTo("2 Requests for Deviation, 1 needing attention"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.ReviewItemDiscrepancy, 3, 3)), Is.EqualTo("3 Review Item Discrepancies, 3 needing attention"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.ReviewItemDiscrepancy, 3, 2)), Is.EqualTo("3 Review Item Discrepancies, 2 needing attention"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.ReviewItemDiscrepancy, 1, 0)), Is.EqualTo("1 Review Item Discrepancy, 0 needing attention"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.RequestForWaiver, 1, 1)), Is.EqualTo("1 Request for Waiver, 1 needing attention"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.ChangeRequest, 1, 1)), Is.EqualTo("1 Change Request, 1 needing attention"), "a closed change request with an open change proposal");
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.Note, 1, 0)), Is.EqualTo("1 Note"));
                Assert.That(AnnotationHelper.GetSummaryTooltip(new AnnotationKindSummary(AnnotationKind.Note, 4, 0)), Is.EqualTo("4 Notes"));
            }
        }

        [Test]
        public void VerifyAttachableKindsAndStatuses()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.AttachableKinds, Is.EqualTo([AnnotationKind.Note, AnnotationKind.ChangeRequest, AnnotationKind.ReviewItemDiscrepancy, AnnotationKind.RequestForDeviation, AnnotationKind.RequestForWaiver]));
                Assert.That(AnnotationHelper.AttachableKinds, Does.Not.Contain(AnnotationKind.ChangeProposal));
                Assert.That(AnnotationHelper.Statuses, Is.EquivalentTo(Enum.GetValues<AnnotationStatusKind>()));
                Assert.That(AnnotationHelper.GetStatusLabel(AnnotationStatusKind.WONTFIX), Is.EqualTo("Won't fix"));
                Assert.That(AnnotationHelper.Statuses.Select(AnnotationHelper.GetStatusLabel), Is.EqualTo(["Open", "Done", "Closed", "Invalid", "Won't fix", "Not applicable"]));
                Assert.That(Enum.GetValues<AnnotationApprovalKind>().Select(AnnotationHelper.GetApprovalLabel), Is.EquivalentTo(["Accepted", "Rejected", "Withdrawn"]));
            }
        }

        [Test]
        public void VerifyIsApplicable()
        {
            var element = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var iteration = new Iteration { Iid = Guid.NewGuid() };
            var book = new Book { Iid = Guid.NewGuid(), ShortName = "BOOK" };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.IsApplicable(AnnotationKind.Note, iteration), Is.True, "a note applies to any thing");
                Assert.That(AnnotationHelper.IsApplicable(AnnotationKind.ChangeRequest, iteration), Is.False);
                Assert.That(AnnotationHelper.IsApplicable(AnnotationKind.ChangeRequest, element), Is.True);

                foreach (var kind in Enum.GetValues<AnnotationKind>())
                {
                    Assert.That(AnnotationHelper.IsApplicable(kind, this.requirement), Is.True, $"{kind} applies to a requirement");
                    Assert.That(AnnotationHelper.IsApplicable(kind, element), Is.True, $"{kind} applies to an element definition");
                    Assert.That(AnnotationHelper.IsApplicable(kind, null), Is.False, $"{kind} applies to nothing");
                    Assert.That(AnnotationHelper.IsApplicable(kind, iteration), Is.EqualTo(kind == AnnotationKind.Note), $"only a note applies to an iteration, not {kind}");
                    Assert.That(AnnotationHelper.IsApplicable(kind, book), Is.EqualTo(kind == AnnotationKind.Note), $"only a note applies to a book, not {kind}");
                }

                foreach (var thing in (Thing[])
                         [
                             new ElementUsage(), new Parameter(), new ParameterOverride(), new ParameterValueSet(), new Option(), new BinaryRelationship(),
                             new ActualFiniteStateList(), new PossibleFiniteState(), new RequirementsGroup(), new RequirementsSpecification()
                         ])
                {
                    Assert.That(AnnotationHelper.AttachableKinds.Select(kind => AnnotationHelper.IsApplicable(kind, thing)), Is.All.True, $"every kind applies to a {thing.ClassKind}");
                }
            }
        }

        [Test]
        public void VerifyGetThingShortName()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetThingShortName(this.requirement), Is.EqualTo("REQ-1"));
                Assert.That(AnnotationHelper.GetThingShortName(new ElementDefinition { ShortName = string.Empty, Name = "Satellite" }), Is.EqualTo("Satellite"), "the name when there is no short name");
                Assert.That(AnnotationHelper.GetThingShortName(new ElementDefinition { ShortName = "SAT", Name = "Satellite" }), Is.EqualTo("SAT"));
                Assert.That(AnnotationHelper.GetThingShortName(new Iteration()), Is.EqualTo("Iteration"), "the class when there is neither");
                Assert.That(AnnotationHelper.GetThingShortName(null), Is.Empty);
            }
        }

        [Test]
        public void VerifyGetThingName()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetThingName(new ElementDefinition { ShortName = "SAT", Name = "Satellite" }), Is.EqualTo("Satellite"));
                Assert.That(AnnotationHelper.GetThingName(new ElementDefinition { ShortName = "SAT", Name = null }), Is.Empty);
                Assert.That(AnnotationHelper.GetThingName(new Iteration()), Is.Empty, "an iteration has no name");
                Assert.That(AnnotationHelper.GetThingName(null), Is.Empty);
            }
        }

        [Test]
        public void VerifyGetThingKindLabel()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetThingKindLabel(this.requirement), Is.EqualTo("Requirement"));
                Assert.That(AnnotationHelper.GetThingKindLabel(new RequirementsSpecification()), Is.EqualTo("Requirements Specification"));
                Assert.That(AnnotationHelper.GetThingKindLabel(new ElementDefinition()), Is.EqualTo("Element Definition"));
                Assert.That(AnnotationHelper.GetThingKindLabel(new Iteration()), Is.EqualTo("Iteration"));
                Assert.That(AnnotationHelper.GetThingKindLabel(null), Is.Empty);
            }
        }

        [Test]
        public void VerifyGetLanguageCode()
        {
            var element = new ElementDefinition { Iid = Guid.NewGuid() };

            Assert.That(AnnotationHelper.GetLanguageCode(element), Is.Null, "no definition");

            element.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "fr", Content = "premier" });
            element.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "nl", Content = "tweede" });

            var blank = new ElementDefinition { Iid = Guid.NewGuid() };
            blank.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = " ", Content = "blank" });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetLanguageCode(element), Is.EqualTo("fr"), "the language of the first definition");
                Assert.That(AnnotationHelper.GetLanguageCode(blank), Is.Null, "a blank language is no language");
                Assert.That(AnnotationHelper.GetLanguageCode(new Iteration()), Is.Null, "an iteration is not a defined thing");
                Assert.That(AnnotationHelper.GetLanguageCode(null), Is.Null);
            }
        }

        [Test]
        public void VerifyIsContainedIn()
        {
            var model = new EngineeringModel { Iid = Guid.NewGuid() };
            var iteration = new Iteration { Iid = Guid.NewGuid() };
            var otherIteration = new Iteration { Iid = Guid.NewGuid() };
            model.Iteration.AddRange([iteration, otherIteration]);

            var specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "SPEC" };
            specification.Requirement.Add(this.requirement);
            iteration.RequirementsSpecification.Add(specification);

            var element = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            iteration.Element.Add(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.IsContainedIn(this.requirement, iteration), Is.True);
                Assert.That(AnnotationHelper.IsContainedIn(this.requirement, model), Is.True, "the model contains the iteration that contains the requirement");
                Assert.That(AnnotationHelper.IsContainedIn(element, iteration), Is.True);
                Assert.That(AnnotationHelper.IsContainedIn(iteration, iteration), Is.True, "a thing is part of itself");
                Assert.That(AnnotationHelper.IsContainedIn(this.requirement, otherIteration), Is.False, "another iteration");
                Assert.That(AnnotationHelper.IsContainedIn(new Requirement { Iid = Guid.NewGuid() }, iteration), Is.False, "a thing without container");
                Assert.That(AnnotationHelper.IsContainedIn(null, iteration), Is.False);
                Assert.That(AnnotationHelper.IsContainedIn(this.requirement, null), Is.False);
                Assert.That(AnnotationHelper.IsContainedIn(null, null), Is.False);
            }

            // deleted on the server: dropped from the list of its container, which it still points at
            specification.Requirement.Remove(this.requirement);
            iteration.Element.Remove(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.requirement.Container, Is.SameAs(specification), "the container reference is kept");
                Assert.That(AnnotationHelper.IsContainedIn(this.requirement, iteration), Is.False);
                Assert.That(AnnotationHelper.IsContainedIn(this.requirement, model), Is.False);
                Assert.That(AnnotationHelper.IsContainedIn(element, iteration), Is.False);
            }

            // an intermediate container dropped from its own container takes everything below it along
            specification.Requirement.Add(this.requirement);
            iteration.RequirementsSpecification.Remove(specification);

            Assert.That(AnnotationHelper.IsContainedIn(this.requirement, iteration), Is.False);
        }

        [Test]
        public void VerifyIsOpen()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.IsOpen(new ChangeRequest { Status = AnnotationStatusKind.OPEN }), Is.True);
                Assert.That(AnnotationHelper.IsOpen(new ReviewItemDiscrepancy { Status = AnnotationStatusKind.DONE }), Is.False);
                Assert.That(AnnotationHelper.IsOpen(new RequestForWaiver { Status = AnnotationStatusKind.CLOSED }), Is.False);
                Assert.That(AnnotationHelper.IsOpen(new EngineeringModelDataNote()), Is.False);
            }
        }

        [Test]
        public void VerifyGetAnnotatedThingIid()
        {
            var annotated = new ChangeRequest { PrimaryAnnotatedThing = new ModellingThingReference(this.requirement) { Iid = Guid.NewGuid() } };
            var emptyReference = new ChangeRequest { PrimaryAnnotatedThing = new ModellingThingReference { Iid = Guid.NewGuid() } };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.GetAnnotatedThingIid(annotated), Is.EqualTo(this.requirement.Iid));
                Assert.That(AnnotationHelper.GetAnnotatedThingIid(new ChangeRequest()), Is.Null);
                Assert.That(AnnotationHelper.GetAnnotatedThingIid(emptyReference), Is.Null);
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
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ReviewItemDiscrepancy, "R1", []), Is.EqualTo("R1-RID-001"));
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, "R1", []), Is.EqualTo("R1-CR-001"));
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ReviewItemDiscrepancy, "R1", existing), Is.EqualTo("R1-RID-005"), "the highest number with the prefix plus one, ignoring non numeric suffixes");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, "R1", existing), Is.EqualTo("R1-CR-010"), "another requirement sharing the leading characters does not count");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, "R2", existing), Is.EqualTo("R2-CR-013"), "the prefix is matched case-insensitively");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.RequestForWaiver, "R1", existing), Is.EqualTo("R1-RFW-001"), "another kind does not count");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.RequestForDeviation, "R3", existing), Is.EqualTo("R3-RFD-001"), "another requirement does not count");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeProposal, "R1", existing), Is.EqualTo("R1-CP-001"));
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, "R1", [new ChangeRequest { ShortName = "R1-CR-999" }]), Is.EqualTo("R1-CR-1000"), "the number grows past three digits");

                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, "REQ-1", [new ChangeRequest { ShortName = "REQ-1-CR-003" }]), Is.EqualTo("REQ-1-CR-004"), "the requirement short name is kept as is");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, " R1 ", []), Is.EqualTo("R1-CR-001"), "the requirement short name is trimmed");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, string.Empty, existing), Is.EqualTo("CR-021"), "without a requirement short name only the kind prefixes the number");
                Assert.That(AnnotationHelper.GetNextShortName(AnnotationKind.ChangeRequest, null, []), Is.EqualTo("CR-001"));
            }
        }

        [Test]
        public void VerifyCreateAnnotation()
        {
            var (note, noteReference) = AnnotationHelper.CreateAnnotation(AnnotationKind.Note, this.requirement, this.author, this.owner, "en", "Note-001", "ignored", "a note", AnnotationClassificationKind.MAJOR);
            var (item, itemReference) = AnnotationHelper.CreateAnnotation(AnnotationKind.ReviewItemDiscrepancy, this.requirement, this.author, this.owner, "en", "RID-001", "a title", "an item", AnnotationClassificationKind.MAJOR);

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
            var withComment = AnnotationHelper.CreateApproval(this.author, this.owner, "en", AnnotationApprovalKind.ACCEPTED, "looks good");
            var withoutComment = AnnotationHelper.CreateApproval(this.author, this.owner, "en", AnnotationApprovalKind.REJECTED, "  ");

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
            var solution = AnnotationHelper.CreateSolution(this.author, this.owner, "en", "a fix");

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
            var item = AnnotationHelper.CreateDiscussionItem(this.author, "en", "a reply");

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
