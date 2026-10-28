// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IAnnotationsViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Interface for the <see cref="AnnotationsViewModel" />, which indexes the annotations of an engineering model by the
    /// thing they are about and drives the annotations side panel of the thing a host page opens it on.
    /// </summary>
    public interface IAnnotationsViewModel
    {
        /// <summary>
        /// Gets the options offered by an annotation filter: open items, then each kind that can be attached.
        /// </summary>
        IReadOnlyList<AnnotationFilterOption> FilterOptions { get; }

        /// <summary>
        /// Gets the <see cref="Thing" /> whose annotations are shown in the side panel, or null when it is closed.
        /// </summary>
        Thing ActiveThing { get; }

        /// <summary>
        /// Gets a value indicating whether the annotations side panel is open.
        /// </summary>
        bool IsPanelOpen { get; }

        /// <summary>
        /// Gets or sets the <see cref="AnnotationKind" /> the side panel is narrowed to, or null to show all kinds.
        /// </summary>
        AnnotationKind? KindFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the side panel also lists annotations that are no longer open.
        /// </summary>
        bool ShowResolved { get; set; }

        /// <summary>
        /// Gets the <see cref="AnnotationDraft" /> being composed, or null when the composer is closed.
        /// </summary>
        AnnotationDraft Draft { get; }

        /// <summary>
        /// Gets a value indicating whether a write is in progress, during which the write buttons are disabled so a slow
        /// server round-trip cannot be submitted twice.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets the annotations of the <see cref="ActiveThing" /> that pass <see cref="KindFilter" /> and
        /// <see cref="ShowResolved" />.
        /// </summary>
        IReadOnlyList<EngineeringModelDataAnnotation> VisibleAnnotations { get; }

        /// <summary>
        /// Gets the kinds the active user is allowed to attach to the <see cref="ActiveThing" />: the kinds ECSS-E-TM-10-25
        /// permits on its class (<see cref="AnnotationHelper.IsApplicable" />) that the user's role may create.
        /// </summary>
        IReadOnlyList<AnnotationKind> CreatableKinds { get; }

        /// <summary>
        /// Sets the <see cref="Iteration" /> whose things are annotated and rebuilds the annotation indexes. The panel closes
        /// when its thing is gone, and the composer when the annotation it edits, or the change request it answers, is gone.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open</param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /> the active user works for</param>
        void SetIteration(Iteration iteration, DomainOfExpertise domain);

        /// <summary>
        /// Gets the kinds the active user is allowed to attach to the given <paramref name="thing" />: the kinds
        /// ECSS-E-TM-10-25 permits on its class (<see cref="AnnotationHelper.IsApplicable" />) that the user's role may create.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to annotate</param>
        /// <returns>The creatable <see cref="AnnotationKind" />s, none for a null thing</returns>
        IReadOnlyList<AnnotationKind> GetCreatableKinds(Thing thing);

        /// <summary>
        /// Gets the top-level annotations about the given <paramref name="thing" />, open ones first, newest first. A
        /// change proposal answering a change request about the same thing is nested under it instead.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>The annotations</returns>
        IReadOnlyList<EngineeringModelDataAnnotation> GetAnnotations(Thing thing);

        /// <summary>
        /// Gets, per <see cref="AnnotationKind" />, how many annotations the given <paramref name="thing" /> carries.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>One <see cref="AnnotationKindSummary" /> per kind present, in the <see cref="AnnotationKind" /> order</returns>
        IReadOnlyList<AnnotationKindSummary> GetSummary(Thing thing);

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="thing" /> has at least one open annotation, counting a
        /// change request whose nested change proposal is still open, exactly as its badges do.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>True if an annotation about it is still open</returns>
        bool HasOpenAnnotations(Thing thing);

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="thing" /> passes any of the selected annotation filters.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <param name="filters">The selected <see cref="AnnotationFilterOption" />s; none selected lets everything pass</param>
        /// <returns>True if it should be shown</returns>
        bool MatchesFilter(Thing thing, IEnumerable<AnnotationFilterOption> filters);

        /// <summary>
        /// Gets the change proposals answering the given <paramref name="changeRequest" />, newest first.
        /// </summary>
        /// <param name="changeRequest">The <see cref="ChangeRequest" /></param>
        /// <returns>The <see cref="ChangeProposal" />s</returns>
        IReadOnlyList<ChangeProposal> GetChangeProposals(ChangeRequest changeRequest);

        /// <summary>
        /// Gets why the short name of an edited annotation cannot be saved: it is empty, or another annotation of the model
        /// already uses it. A new annotation's short name is generated when it is written, so it is never in error.
        /// </summary>
        /// <param name="draft">The <see cref="AnnotationDraft" /></param>
        /// <returns>The error message, or null when the short name is fine</returns>
        string GetShortNameError(AnnotationDraft draft);

        /// <summary>
        /// Gets a value indicating whether the active user may attach a new annotation of the given <paramref name="kind" />.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>True if allowed</returns>
        bool CanCreate(AnnotationKind kind);

        /// <summary>
        /// Gets a value indicating whether the active user may edit, re-status or delete the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if allowed</returns>
        bool CanEdit(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Gets a value indicating whether the active user may reply to the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if allowed</returns>
        bool CanReply(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Gets a value indicating whether the active user may accept, reject or withdraw the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it is a <see cref="ModellingAnnotationItem" /> and approvals may be added to it</returns>
        bool CanApprove(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Gets a value indicating whether the active user may propose a solution to the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it is a <see cref="ReviewItemDiscrepancy" /> and solutions may be added to it</returns>
        bool CanAddSolution(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Gets a value indicating whether the active user may answer the given <paramref name="annotation" /> with a
        /// change proposal.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it is a <see cref="ChangeRequest" /> and change proposals may be created</returns>
        bool CanProposeChange(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Opens the side panel on the annotations of the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <param name="kind">The kind to narrow the panel to, or null to show all kinds</param>
        void OpenPanel(Thing thing, AnnotationKind? kind = null);

        /// <summary>
        /// Closes the side panel and discards any draft.
        /// </summary>
        void ClosePanel();

        /// <summary>
        /// Opens the composer on a new annotation of the given <paramref name="kind" /> about the <see cref="ActiveThing" />.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        void StartNew(AnnotationKind kind);

        /// <summary>
        /// Opens the composer on the values of an existing <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /> to edit</param>
        void StartEdit(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Opens the composer on a new change proposal answering the given <paramref name="changeRequest" />.
        /// </summary>
        /// <param name="changeRequest">The <see cref="ChangeRequest" /></param>
        void StartChangeProposal(ChangeRequest changeRequest);

        /// <summary>
        /// Closes the composer without writing.
        /// </summary>
        void CancelDraft();

        /// <summary>
        /// Writes the <see cref="Draft" /> and closes the composer on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task SaveDraftAsync();

        /// <summary>
        /// Adds a reply to the discussion of the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <param name="content">The text of the reply</param>
        /// <returns>True if the reply was written</returns>
        Task<bool> ReplyAsync(EngineeringModelDataAnnotation annotation, string content);

        /// <summary>
        /// Moves the given <paramref name="annotation" /> to another status.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem" /></param>
        /// <param name="status">The new <see cref="AnnotationStatusKind" /></param>
        /// <returns>A <see cref="Task" /></returns>
        Task SetStatusAsync(ModellingAnnotationItem annotation, AnnotationStatusKind status);

        /// <summary>
        /// Records an approval decision on the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem" /></param>
        /// <param name="classification">The <see cref="AnnotationApprovalKind" /> decision</param>
        /// <param name="comment">An optional comment explaining the decision</param>
        /// <returns>True if the decision was written</returns>
        Task<bool> ApproveAsync(ModellingAnnotationItem annotation, AnnotationApprovalKind classification, string comment);

        /// <summary>
        /// Adds a proposed solution to the given <paramref name="reviewItemDiscrepancy" />.
        /// </summary>
        /// <param name="reviewItemDiscrepancy">The <see cref="ReviewItemDiscrepancy" /></param>
        /// <param name="content">The proposed solution</param>
        /// <returns>True if the solution was written</returns>
        Task<bool> AddSolutionAsync(ReviewItemDiscrepancy reviewItemDiscrepancy, string content);

        /// <summary>
        /// Asks for confirmation, then deletes the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        void RequestDelete(EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Deletes the given <paramref name="annotation" />, together with the change proposals answering it when it is a
        /// change request (their reference to it is mandatory).
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>A <see cref="Task" /></returns>
        Task DeleteAsync(EngineeringModelDataAnnotation annotation);
    }
}
