// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementAnnotationHelper.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations
{
    using System.Globalization;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Pure helpers mapping the ECSS-E-TM-10-25 annotation classes to a <see cref="RequirementAnnotationKind" /> and
    /// building the new annotation POCOs (with every property the server requires) written by the
    /// <see cref="RequirementAnnotationsViewModel" />.
    /// </summary>
    public static class RequirementAnnotationHelper
    {
        /// <summary>
        /// The kinds a user can attach directly to a requirement, in the order they are offered. A
        /// <see cref="RequirementAnnotationKind.ChangeProposal" /> is only created in answer to a change request.
        /// </summary>
        public static readonly IReadOnlyList<RequirementAnnotationKind> RequirementKinds =
        [
            RequirementAnnotationKind.Note,
            RequirementAnnotationKind.ChangeRequest,
            RequirementAnnotationKind.ReviewItemDiscrepancy,
            RequirementAnnotationKind.RequestForDeviation,
            RequirementAnnotationKind.RequestForWaiver
        ];

        /// <summary>
        /// The status values a <see cref="ModellingAnnotationItem" /> can be moved to, in the order they are offered.
        /// </summary>
        public static readonly IReadOnlyList<AnnotationStatusKind> Statuses =
        [
            AnnotationStatusKind.OPEN,
            AnnotationStatusKind.DONE,
            AnnotationStatusKind.CLOSED,
            AnnotationStatusKind.INVALID,
            AnnotationStatusKind.WONTFIX,
            AnnotationStatusKind.NOT_APPLICABLE
        ];

        /// <summary>
        /// Gets the <see cref="RequirementAnnotationKind" /> of an annotation.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>The kind, or null for an annotation class the Requirements Editor does not handle (e.g. an action item)</returns>
        public static RequirementAnnotationKind? GetKind(EngineeringModelDataAnnotation annotation)
        {
            return annotation switch
            {
                EngineeringModelDataNote => RequirementAnnotationKind.Note,
                ChangeRequest => RequirementAnnotationKind.ChangeRequest,
                ReviewItemDiscrepancy => RequirementAnnotationKind.ReviewItemDiscrepancy,
                RequestForDeviation => RequirementAnnotationKind.RequestForDeviation,
                RequestForWaiver => RequirementAnnotationKind.RequestForWaiver,
                ChangeProposal => RequirementAnnotationKind.ChangeProposal,
                _ => null
            };
        }

        /// <summary>
        /// Gets the human-readable name of a <see cref="RequirementAnnotationKind" />.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The label, e.g. "Review Item Discrepancy"</returns>
        public static string GetLabel(RequirementAnnotationKind kind)
        {
            return kind switch
            {
                RequirementAnnotationKind.Note => "Note",
                RequirementAnnotationKind.ChangeRequest => "Change Request",
                RequirementAnnotationKind.ReviewItemDiscrepancy => "Review Item Discrepancy",
                RequirementAnnotationKind.RequestForDeviation => "Request for Deviation",
                RequirementAnnotationKind.RequestForWaiver => "Request for Waiver",
                _ => "Change Proposal"
            };
        }

        /// <summary>
        /// Gets the plural name of a <see cref="RequirementAnnotationKind" />.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The plural label, e.g. "Requests for Deviation"</returns>
        public static string GetPluralLabel(RequirementAnnotationKind kind)
        {
            return kind switch
            {
                RequirementAnnotationKind.Note => "Notes",
                RequirementAnnotationKind.ChangeRequest => "Change Requests",
                RequirementAnnotationKind.ReviewItemDiscrepancy => "Review Item Discrepancies",
                RequirementAnnotationKind.RequestForDeviation => "Requests for Deviation",
                RequirementAnnotationKind.RequestForWaiver => "Requests for Waiver",
                _ => "Change Proposals"
            };
        }

        /// <summary>
        /// Gets the short acronym of a <see cref="RequirementAnnotationKind" />, used on the badges and as the prefix of a
        /// generated short name.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The acronym, e.g. "RID"</returns>
        public static string GetAcronym(RequirementAnnotationKind kind)
        {
            return kind switch
            {
                RequirementAnnotationKind.Note => "Note",
                RequirementAnnotationKind.ChangeRequest => "CR",
                RequirementAnnotationKind.ReviewItemDiscrepancy => "RID",
                RequirementAnnotationKind.RequestForDeviation => "RFD",
                RequirementAnnotationKind.RequestForWaiver => "RFW",
                _ => "CP"
            };
        }

        /// <summary>
        /// Gets the CSS suffix identifying a <see cref="RequirementAnnotationKind" />, so each kind has its own colour.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The lower-case suffix, e.g. "rid"</returns>
        public static string GetCssSuffix(RequirementAnnotationKind kind)
        {
            return GetAcronym(kind).ToLowerInvariant();
        }

        /// <summary>
        /// Gets the <see cref="ClassKind" /> written for a <see cref="RequirementAnnotationKind" />, used for the
        /// permission check.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The <see cref="ClassKind" /></returns>
        public static ClassKind GetClassKind(RequirementAnnotationKind kind)
        {
            return kind switch
            {
                RequirementAnnotationKind.Note => ClassKind.EngineeringModelDataNote,
                RequirementAnnotationKind.ChangeRequest => ClassKind.ChangeRequest,
                RequirementAnnotationKind.ReviewItemDiscrepancy => ClassKind.ReviewItemDiscrepancy,
                RequirementAnnotationKind.RequestForDeviation => ClassKind.RequestForDeviation,
                RequirementAnnotationKind.RequestForWaiver => ClassKind.RequestForWaiver,
                _ => ClassKind.ChangeProposal
            };
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="RequirementAnnotationKind" /> is a <see cref="ModellingAnnotationItem" />,
        /// which carries a title, a short name, a status, a classification and approvals. A note carries none of these.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>True for every kind but <see cref="RequirementAnnotationKind.Note" /></returns>
        public static bool IsModellingAnnotationItem(RequirementAnnotationKind kind)
        {
            return kind != RequirementAnnotationKind.Note;
        }

        /// <summary>
        /// Gets a value indicating whether an annotation still needs attention: a <see cref="ModellingAnnotationItem" />
        /// whose status is <see cref="AnnotationStatusKind.OPEN" />. Notes are informational and never count as open.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if the annotation is open</returns>
        public static bool IsOpen(EngineeringModelDataAnnotation annotation)
        {
            return annotation is ModellingAnnotationItem { Status: AnnotationStatusKind.OPEN };
        }

        /// <summary>
        /// Gets the human-readable label of an <see cref="AnnotationStatusKind" />.
        /// </summary>
        /// <param name="status">The <see cref="AnnotationStatusKind" /></param>
        /// <returns>The label, e.g. "Won't fix"</returns>
        public static string GetStatusLabel(AnnotationStatusKind status)
        {
            return status switch
            {
                AnnotationStatusKind.OPEN => "Open",
                AnnotationStatusKind.DONE => "Done",
                AnnotationStatusKind.CLOSED => "Closed",
                AnnotationStatusKind.INVALID => "Invalid",
                AnnotationStatusKind.WONTFIX => "Won't fix",
                _ => "Not applicable"
            };
        }

        /// <summary>
        /// Gets the human-readable label of an <see cref="AnnotationApprovalKind" />.
        /// </summary>
        /// <param name="classification">The <see cref="AnnotationApprovalKind" /></param>
        /// <returns>The label, e.g. "Accepted"</returns>
        public static string GetApprovalLabel(AnnotationApprovalKind classification)
        {
            return classification switch
            {
                AnnotationApprovalKind.ACCEPTED => "Accepted",
                AnnotationApprovalKind.REJECTED => "Rejected",
                _ => "Withdrawn"
            };
        }

        /// <summary>
        /// Gets the human-readable label of an <see cref="AnnotationClassificationKind" />.
        /// </summary>
        /// <param name="classification">The <see cref="AnnotationClassificationKind" /></param>
        /// <returns>"Major" or "Minor"</returns>
        public static string GetClassificationLabel(AnnotationClassificationKind classification)
        {
            return classification == AnnotationClassificationKind.MAJOR ? "Major" : "Minor";
        }

        /// <summary>
        /// Gets the tooltip describing an <see cref="AnnotationKindSummary" />, e.g. "2 of 3 Review Item Discrepancies open".
        /// </summary>
        /// <param name="summary">The <see cref="AnnotationKindSummary" /></param>
        /// <returns>The tooltip</returns>
        public static string GetSummaryTooltip(AnnotationKindSummary summary)
        {
            var label = summary.Count == 1 ? GetLabel(summary.Kind) : GetPluralLabel(summary.Kind);

            return summary.Kind == RequirementAnnotationKind.Note
                ? $"{summary.Count} {label}"
                : $"{summary.OpenCount} of {summary.Count} {label} open";
        }

        /// <summary>
        /// Gets the <see cref="Thing.Iid" /> of the thing an annotation is primarily about.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>The iid, or null when the annotation has no resolvable primary annotated thing</returns>
        /// <remarks>
        /// <see cref="EngineeringModelDataAnnotation.PrimaryAnnotatedThing" /> is a <see cref="ModellingThingReference" />,
        /// not the thing itself: compare its <see cref="ThingReference.ReferencedThing" />.
        /// </remarks>
        public static Guid? GetAnnotatedThingIid(EngineeringModelDataAnnotation annotation)
        {
            var iid = annotation.PrimaryAnnotatedThing?.ReferencedThing?.Iid;
            return iid == Guid.Empty ? null : iid;
        }

        /// <summary>
        /// Computes the next free short name for a new annotation of the given <paramref name="kind" /> about the
        /// requirement known as <paramref name="requirementShortName" />: the requirement's short name, the kind's acronym and
        /// one more than the highest number already used with that prefix anywhere in the model, e.g. "R12-CR-004", so the
        /// result is unique in the model.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <param name="requirementShortName">The short name of the annotated requirement</param>
        /// <param name="existing">The <see cref="ModellingAnnotationItem" />s already in the model</param>
        /// <returns>The short name</returns>
        public static string GetNextShortName(RequirementAnnotationKind kind, string requirementShortName, IEnumerable<ModellingAnnotationItem> existing)
        {
            var prefix = string.IsNullOrWhiteSpace(requirementShortName)
                ? $"{GetAcronym(kind)}-"
                : $"{requirementShortName.Trim()}-{GetAcronym(kind)}-";

            var highest = existing
                .Select(x => x.ShortName)
                .Where(x => x != null && x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(x => int.TryParse(x[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(highest + 1).ToString("D3", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Builds a new annotation of the given <paramref name="kind" /> about the given <paramref name="target" />, with
        /// the <see cref="ModellingThingReference" /> that links it, so both can be written in one transaction.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /> to create</param>
        /// <param name="target">The annotated <see cref="Thing" /></param>
        /// <param name="author">The <see cref="Participant" /> writing the annotation</param>
        /// <param name="owner">The <see cref="DomainOfExpertise" /> owning a <see cref="ModellingAnnotationItem" /></param>
        /// <param name="languageCode">The language of the content</param>
        /// <param name="shortName">The short name of a <see cref="ModellingAnnotationItem" /></param>
        /// <param name="title">The title of a <see cref="ModellingAnnotationItem" /></param>
        /// <param name="content">The text of the annotation</param>
        /// <param name="classification">The <see cref="AnnotationClassificationKind" /> of a <see cref="ModellingAnnotationItem" /></param>
        /// <returns>The new annotation and the reference to add to the same write</returns>
        public static (EngineeringModelDataAnnotation Annotation, ModellingThingReference Reference) CreateAnnotation(RequirementAnnotationKind kind, Thing target, Participant author,
            DomainOfExpertise owner, string languageCode, string shortName, string title, string content, AnnotationClassificationKind classification)
        {
            EngineeringModelDataAnnotation annotation = kind switch
            {
                RequirementAnnotationKind.Note => new EngineeringModelDataNote(),
                RequirementAnnotationKind.ChangeRequest => new ChangeRequest(),
                RequirementAnnotationKind.ReviewItemDiscrepancy => new ReviewItemDiscrepancy(),
                RequirementAnnotationKind.RequestForDeviation => new RequestForDeviation(),
                RequirementAnnotationKind.RequestForWaiver => new RequestForWaiver(),
                _ => new ChangeProposal()
            };

            var reference = new ModellingThingReference(target) { Iid = Guid.NewGuid() };

            annotation.Iid = Guid.NewGuid();
            annotation.Author = author;
            annotation.Content = content;
            annotation.LanguageCode = languageCode;
            annotation.CreatedOn = DateTime.UtcNow;
            annotation.RelatedThing.Add(reference);
            annotation.PrimaryAnnotatedThing = reference;

            if (annotation is ModellingAnnotationItem item)
            {
                item.Owner = owner;
                item.ShortName = shortName;
                item.Title = title;
                item.Classification = classification;
                item.Status = AnnotationStatusKind.OPEN;
            }

            return (annotation, reference);
        }

        /// <summary>
        /// Builds a new reply to add to an annotation's discussion.
        /// </summary>
        /// <param name="author">The <see cref="Participant" /> replying</param>
        /// <param name="languageCode">The language of the content</param>
        /// <param name="content">The text of the reply</param>
        /// <returns>The new <see cref="EngineeringModelDataDiscussionItem" /></returns>
        public static EngineeringModelDataDiscussionItem CreateDiscussionItem(Participant author, string languageCode, string content)
        {
            return new EngineeringModelDataDiscussionItem
            {
                Iid = Guid.NewGuid(),
                Author = author,
                Content = content,
                LanguageCode = languageCode,
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Builds a new approval decision to add to a <see cref="ModellingAnnotationItem" />. The server requires a
        /// content, so an empty comment falls back to the decision label.
        /// </summary>
        /// <param name="author">The <see cref="Participant" /> deciding</param>
        /// <param name="owner">The <see cref="DomainOfExpertise" /> the decision is made for</param>
        /// <param name="languageCode">The language of the content</param>
        /// <param name="classification">The <see cref="AnnotationApprovalKind" /> decision</param>
        /// <param name="comment">An optional comment explaining the decision</param>
        /// <returns>The new <see cref="Approval" /></returns>
        public static Approval CreateApproval(Participant author, DomainOfExpertise owner, string languageCode, AnnotationApprovalKind classification, string comment)
        {
            return new Approval
            {
                Iid = Guid.NewGuid(),
                Author = author,
                Owner = owner,
                Classification = classification,
                Content = string.IsNullOrWhiteSpace(comment) ? GetApprovalLabel(classification) : comment,
                LanguageCode = languageCode,
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Builds a new proposed solution to add to a <see cref="ReviewItemDiscrepancy" />.
        /// </summary>
        /// <param name="author">The <see cref="Participant" /> proposing it</param>
        /// <param name="owner">The <see cref="DomainOfExpertise" /> proposing it</param>
        /// <param name="languageCode">The language of the content</param>
        /// <param name="content">The proposed solution</param>
        /// <returns>The new <see cref="Solution" /></returns>
        public static Solution CreateSolution(Participant author, DomainOfExpertise owner, string languageCode, string content)
        {
            return new Solution
            {
                Iid = Guid.NewGuid(),
                Author = author,
                Owner = owner,
                Content = content,
                LanguageCode = languageCode,
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}
