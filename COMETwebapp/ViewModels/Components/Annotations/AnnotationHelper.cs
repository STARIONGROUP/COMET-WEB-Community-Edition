// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationHelper.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Annotations
{
    using System.Globalization;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Pure helpers mapping the ECSS-E-TM-10-25 annotation classes to a <see cref="AnnotationKind" />, telling which kinds
    /// a given <see cref="Thing" /> may carry, and building the new annotation POCOs (with every property the server
    /// requires) written by the <see cref="AnnotationsViewModel" />.
    /// </summary>
    public static class AnnotationHelper
    {
        /// <summary>
        /// The kinds a user can attach directly to a thing, in the order they are offered. A
        /// <see cref="AnnotationKind.ChangeProposal" /> is only created in answer to a change request.
        /// </summary>
        public static readonly IReadOnlyList<AnnotationKind> AttachableKinds =
        [
            AnnotationKind.Note,
            AnnotationKind.ChangeRequest,
            AnnotationKind.ReviewItemDiscrepancy,
            AnnotationKind.RequestForDeviation,
            AnnotationKind.RequestForWaiver
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
        /// Gets the <see cref="AnnotationKind" /> of an annotation.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>The kind, or null for an annotation class not handled here (e.g. an action item)</returns>
        public static AnnotationKind? GetKind(EngineeringModelDataAnnotation annotation)
        {
            return annotation switch
            {
                EngineeringModelDataNote => AnnotationKind.Note,
                ChangeRequest => AnnotationKind.ChangeRequest,
                ReviewItemDiscrepancy => AnnotationKind.ReviewItemDiscrepancy,
                RequestForDeviation => AnnotationKind.RequestForDeviation,
                RequestForWaiver => AnnotationKind.RequestForWaiver,
                ChangeProposal => AnnotationKind.ChangeProposal,
                _ => null
            };
        }

        /// <summary>
        /// Gets the human-readable name of a <see cref="AnnotationKind" />.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The label, e.g. "Review Item Discrepancy"</returns>
        public static string GetLabel(AnnotationKind kind)
        {
            return kind switch
            {
                AnnotationKind.Note => "Note",
                AnnotationKind.ChangeRequest => "Change Request",
                AnnotationKind.ReviewItemDiscrepancy => "Review Item Discrepancy",
                AnnotationKind.RequestForDeviation => "Request for Deviation",
                AnnotationKind.RequestForWaiver => "Request for Waiver",
                _ => "Change Proposal"
            };
        }

        /// <summary>
        /// Gets the plural name of a <see cref="AnnotationKind" />.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The plural label, e.g. "Requests for Deviation"</returns>
        public static string GetPluralLabel(AnnotationKind kind)
        {
            return kind switch
            {
                AnnotationKind.Note => "Notes",
                AnnotationKind.ChangeRequest => "Change Requests",
                AnnotationKind.ReviewItemDiscrepancy => "Review Item Discrepancies",
                AnnotationKind.RequestForDeviation => "Requests for Deviation",
                AnnotationKind.RequestForWaiver => "Requests for Waiver",
                _ => "Change Proposals"
            };
        }

        /// <summary>
        /// Gets the short acronym of a <see cref="AnnotationKind" />, used on the badges and as the prefix of a
        /// generated short name.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The acronym, e.g. "RID"</returns>
        public static string GetAcronym(AnnotationKind kind)
        {
            return kind switch
            {
                AnnotationKind.Note => "Note",
                AnnotationKind.ChangeRequest => "CR",
                AnnotationKind.ReviewItemDiscrepancy => "RID",
                AnnotationKind.RequestForDeviation => "RFD",
                AnnotationKind.RequestForWaiver => "RFW",
                _ => "CP"
            };
        }

        /// <summary>
        /// Gets the CSS suffix identifying a <see cref="AnnotationKind" />, so each kind has its own colour.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The lower-case suffix, e.g. "rid"</returns>
        public static string GetCssSuffix(AnnotationKind kind)
        {
            return GetAcronym(kind).ToLowerInvariant();
        }

        /// <summary>
        /// Gets the <see cref="ClassKind" /> written for a <see cref="AnnotationKind" />, used for the
        /// permission check.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The <see cref="ClassKind" /></returns>
        public static ClassKind GetClassKind(AnnotationKind kind)
        {
            return kind switch
            {
                AnnotationKind.Note => ClassKind.EngineeringModelDataNote,
                AnnotationKind.ChangeRequest => ClassKind.ChangeRequest,
                AnnotationKind.ReviewItemDiscrepancy => ClassKind.ReviewItemDiscrepancy,
                AnnotationKind.RequestForDeviation => ClassKind.RequestForDeviation,
                AnnotationKind.RequestForWaiver => ClassKind.RequestForWaiver,
                _ => ClassKind.ChangeProposal
            };
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="AnnotationKind" /> is a <see cref="ModellingAnnotationItem" />,
        /// which carries a title, a short name, a status, a classification and approvals. A note carries none of these.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>True for every kind but <see cref="AnnotationKind.Note" /></returns>
        public static bool IsModellingAnnotationItem(AnnotationKind kind)
        {
            return kind != AnnotationKind.Note;
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
        /// Gets a value indicating whether an annotation of the given <paramref name="kind" /> may be attached to the given
        /// <paramref name="thing" />, following the ECSS-E-TM-10-25 rules the SDK ships in
        /// <see cref="StaticDefaultPermissibleAnnotationProvider" />: a note may be attached to any engineering model thing,
        /// a change request, review item discrepancy, request for deviation or waiver only to the classes listed there
        /// (element definitions and usages, parameters and their value sets, options, finite states, relationships and the
        /// requirements, groups and specifications). A change proposal answers a change request, so it follows the change
        /// request list.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <param name="thing">The <see cref="Thing" /> to annotate</param>
        /// <returns>True if the kind may be attached to the thing</returns>
        public static bool IsApplicable(AnnotationKind kind, Thing thing)
        {
            if (thing == null)
            {
                return false;
            }

            return kind switch
            {
                AnnotationKind.Note => true,
                AnnotationKind.ReviewItemDiscrepancy => StaticDefaultPermissibleAnnotationProvider.ReviewItemDiscrepancyApplicableClassKind.Contains(thing.ClassKind),
                AnnotationKind.RequestForDeviation => StaticDefaultPermissibleAnnotationProvider.RequestForDeviationApplicableClassKind.Contains(thing.ClassKind),
                AnnotationKind.RequestForWaiver => StaticDefaultPermissibleAnnotationProvider.RequestForWaiverApplicableClassKind.Contains(thing.ClassKind),
                _ => StaticDefaultPermissibleAnnotationProvider.ChangeRequestApplicableClassKind.Contains(thing.ClassKind)
            };
        }

        /// <summary>
        /// Gets the short name an annotated thing is known by: its short name, or its name, or its class kind for a thing
        /// that has neither.
        /// </summary>
        /// <remarks>
        /// Unlike <c>ThingExtensions.GetShortNameOrName</c>, an EMPTY short name falls through to the name, so the panel
        /// header and the generated annotation short names never start with nothing.
        /// </remarks>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>The short name, or an empty string when <paramref name="thing" /> is null</returns>
        public static string GetThingShortName(Thing thing)
        {
            return thing switch
            {
                null => string.Empty,
                IShortNamedThing { ShortName: { Length: > 0 } shortName } => shortName,
                INamedThing { Name: { Length: > 0 } name } => name,
                _ => thing.ClassKind.ToString()
            };
        }

        /// <summary>
        /// Gets the human-readable class of an annotated thing, its <see cref="ClassKind" /> split into words, e.g.
        /// "Requirements Specification" or "Element Definition".
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>The class label, or an empty string when <paramref name="thing" /> is null</returns>
        public static string GetThingKindLabel(Thing thing)
        {
            if (thing == null)
            {
                return string.Empty;
            }

            var name = thing.ClassKind.ToString();
            return string.Concat(name.Select((x, index) => index > 0 && char.IsUpper(x) ? $" {x}" : x.ToString()));
        }

        /// <summary>
        /// Gets the full name of an annotated thing, shown next to its short name.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>The name, or an empty string for a thing without one</returns>
        public static string GetThingName(Thing thing)
        {
            return thing is INamedThing { Name: not null } named ? named.Name : string.Empty;
        }

        /// <summary>
        /// Gets the language of a thing's text, which a new annotation about it is written in: the language of its first
        /// definition.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>The language code, or null when the thing has no definition</returns>
        public static string GetLanguageCode(Thing thing)
        {
            var languageCode = (thing as DefinedThing)?.Definition.FirstOrDefault()?.LanguageCode;
            return string.IsNullOrWhiteSpace(languageCode) ? null : languageCode;
        }

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="thing" /> is still part of <paramref name="root" />:
        /// every container up to the root still lists it. A thing deleted on the server is dropped from its container's list
        /// but keeps its <see cref="Thing.Container" />, so following the container alone is not enough.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to look for</param>
        /// <param name="root">The <see cref="Thing" /> it should sit under, e.g. the open iteration or its engineering model</param>
        /// <returns>True if the thing is the root or still contained in it</returns>
        public static bool IsContainedIn(Thing thing, Thing root)
        {
            if (thing == null || root == null)
            {
                return false;
            }

            var current = thing;

            while (!ReferenceEquals(current, root))
            {
                var container = current.Container;

                if (container == null || !container.ContainerLists.Any(list => list.Cast<object>().Contains(current)))
                {
                    return false;
                }

                current = container;
            }

            return true;
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
        /// Gets the tooltip describing an <see cref="AnnotationKindSummary" />, e.g. "3 Review Item Discrepancies, 2 needing
        /// attention". "Attention" rather than "open", because a closed change request whose change proposal is still open
        /// is counted too.
        /// </summary>
        /// <param name="summary">The <see cref="AnnotationKindSummary" /></param>
        /// <returns>The tooltip</returns>
        public static string GetSummaryTooltip(AnnotationKindSummary summary)
        {
            var label = summary.Count == 1 ? GetLabel(summary.Kind) : GetPluralLabel(summary.Kind);

            if (summary.Kind == AnnotationKind.Note)
            {
                return $"{summary.Count} {label}";
            }

            return $"{summary.Count} {label}, {summary.AttentionCount} needing attention";
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
        /// thing known as <paramref name="thingShortName" />: the thing's short name, the kind's acronym and
        /// one more than the highest number already used with that prefix anywhere in the model, e.g. "R12-CR-004", so the
        /// result is unique in the model.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <param name="thingShortName">The short name of the annotated thing</param>
        /// <param name="existing">The <see cref="ModellingAnnotationItem" />s already in the model</param>
        /// <returns>The short name</returns>
        public static string GetNextShortName(AnnotationKind kind, string thingShortName, IEnumerable<ModellingAnnotationItem> existing)
        {
            var prefix = string.IsNullOrWhiteSpace(thingShortName)
                ? $"{GetAcronym(kind)}-"
                : $"{thingShortName.Trim()}-{GetAcronym(kind)}-";

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
        /// <param name="kind">The <see cref="AnnotationKind" /> to create</param>
        /// <param name="target">The annotated <see cref="Thing" /></param>
        /// <param name="author">The <see cref="Participant" /> writing the annotation</param>
        /// <param name="owner">The <see cref="DomainOfExpertise" /> owning a <see cref="ModellingAnnotationItem" /></param>
        /// <param name="languageCode">The language of the content</param>
        /// <param name="shortName">The short name of a <see cref="ModellingAnnotationItem" /></param>
        /// <param name="title">The title of a <see cref="ModellingAnnotationItem" /></param>
        /// <param name="content">The text of the annotation</param>
        /// <param name="classification">The <see cref="AnnotationClassificationKind" /> of a <see cref="ModellingAnnotationItem" /></param>
        /// <returns>The new annotation and the reference to add to the same write</returns>
        public static (EngineeringModelDataAnnotation Annotation, ModellingThingReference Reference) CreateAnnotation(AnnotationKind kind, Thing target, Participant author,
            DomainOfExpertise owner, string languageCode, string shortName, string title, string content, AnnotationClassificationKind classification)
        {
            EngineeringModelDataAnnotation annotation = kind switch
            {
                AnnotationKind.Note => new EngineeringModelDataNote(),
                AnnotationKind.ChangeRequest => new ChangeRequest(),
                AnnotationKind.ReviewItemDiscrepancy => new ReviewItemDiscrepancy(),
                AnnotationKind.RequestForDeviation => new RequestForDeviation(),
                AnnotationKind.RequestForWaiver => new RequestForWaiver(),
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
