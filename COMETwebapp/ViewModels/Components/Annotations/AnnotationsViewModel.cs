// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationsViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;

    using FluentResults;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model indexing the ECSS-E-TM-10-25 annotations (notes, change requests, review item discrepancies, requests
    /// for deviation and waiver, change proposals) of an engineering model by the thing they are about, and driving the
    /// annotations side panel of the thing a host page opens it on: composing, replying, approving, proposing solutions
    /// and changes, re-statusing and deleting.
    /// </summary>
    /// <remarks>
    /// Annotations are contained by the <see cref="EngineeringModel" />, not by the iteration, and point at the thing they
    /// are about through <see cref="EngineeringModelDataAnnotation.PrimaryAnnotatedThing" />. Nothing here is specific to
    /// a kind of thing: <see cref="AnnotationHelper.IsApplicable" /> tells which kinds a thing may carry. To annotate
    /// another kind of thing (an element definition, a parameter, an option, ...) a host page only has to
    /// <list type="number">
    ///     <item>own one <see cref="AnnotationsViewModel" /> and feed it through <see cref="SetIteration" /> whenever its iteration (re)loads,</item>
    ///     <item>render <c>AnnotationBadges</c> next to each thing and <c>AnnotationsPanel</c> beside its content,</item>
    ///     <item>optionally pass <c>OnEditAnnotatedThing</c> to the panel so a change request or review item can open the host's own edit form.</item>
    /// </list>
    /// The Requirements Editor (<c>RequirementsEditorBodyViewModel</c>) is the reference host.
    /// </remarks>
    public class AnnotationsViewModel : DisposableObject, IAnnotationsViewModel
    {
        /// <summary>
        /// The language used when the annotated thing has no definition to borrow a language from.
        /// </summary>
        private const string DefaultLanguageCode = "en";

        /// <summary>
        /// The longest excerpt of a note's text used to refer to it in messages.
        /// </summary>
        private const int NoteExcerptLength = 30;

        /// <summary>
        /// The <see cref="ISessionService" /> used to check permissions and write.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The shared <see cref="IConfirmCancelPopupViewModel" /> used to confirm a deletion.
        /// </summary>
        private readonly IConfirmCancelPopupViewModel confirmCancelPopupViewModel;

        /// <summary>
        /// The <see cref="ILogger" /> used to record a write failure.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The handled annotations of the model, grouped by the iid of the thing they are primarily about.
        /// </summary>
        private Dictionary<Guid, List<EngineeringModelDataAnnotation>> annotationsByThing = [];

        /// <summary>
        /// The change proposals of the model grouped by the iid of the change request they answer, newest first, built once
        /// per <see cref="SetIteration" /> so badges, cards and prompts do not scan the whole model on every render.
        /// </summary>
        private Dictionary<Guid, List<ChangeProposal>> proposalsByChangeRequest = [];

        /// <summary>
        /// The <see cref="EngineeringModel" /> containing the annotations, set by <see cref="SetIteration" />.
        /// </summary>
        private EngineeringModel currentModel;

        /// <summary>
        /// The <see cref="DomainOfExpertise" /> owning the annotations, approvals and solutions written by the active user.
        /// </summary>
        private DomainOfExpertise currentDomain;

        /// <summary>
        /// The <see cref="Participant" /> of the active user in the model, the author of everything written.
        /// </summary>
        private Participant currentParticipant;

        /// <summary>
        /// Backing field for <see cref="ActiveThing" />
        /// </summary>
        private Thing activeThing;

        /// <summary>
        /// Backing field for <see cref="KindFilter" />
        /// </summary>
        private AnnotationKind? kindFilter;

        /// <summary>
        /// Backing field for <see cref="ShowResolved" />
        /// </summary>
        private bool showResolved = true;

        /// <summary>
        /// Backing field for <see cref="Draft" />
        /// </summary>
        private AnnotationDraft draft;

        /// <summary>
        /// Backing field for <see cref="IsBusy" />
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Creates a new instance of <see cref="AnnotationsViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="confirmCancelPopupViewModel">The shared <see cref="IConfirmCancelPopupViewModel" /> used to confirm a deletion</param>
        /// <param name="logger">The <see cref="ILogger" /> used to record a write failure</param>
        public AnnotationsViewModel(ISessionService sessionService, IConfirmCancelPopupViewModel confirmCancelPopupViewModel, ILogger logger)
        {
            this.sessionService = sessionService;
            this.confirmCancelPopupViewModel = confirmCancelPopupViewModel;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the options offered by an annotation filter: open items, then each kind that can be attached.
        /// </summary>
        public IReadOnlyList<AnnotationFilterOption> FilterOptions { get; } =
        [
            new("Open items", null),
            ..AnnotationHelper.AttachableKinds.Select(x => new AnnotationFilterOption(AnnotationHelper.GetLabel(x), x))
        ];

        /// <summary>
        /// Gets the <see cref="Thing" /> whose annotations are shown in the side panel, or null when it is closed.
        /// </summary>
        public Thing ActiveThing
        {
            get => this.activeThing;
            private set => this.RaiseAndSetIfChanged(ref this.activeThing, value);
        }

        /// <summary>
        /// Gets a value indicating whether the annotations side panel is open.
        /// </summary>
        public bool IsPanelOpen => this.ActiveThing != null;

        /// <summary>
        /// Gets or sets the <see cref="AnnotationKind" /> the side panel is narrowed to, or null to show all kinds.
        /// </summary>
        public AnnotationKind? KindFilter
        {
            get => this.kindFilter;
            set => this.RaiseAndSetIfChanged(ref this.kindFilter, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the side panel also lists annotations that are no longer open.
        /// </summary>
        public bool ShowResolved
        {
            get => this.showResolved;
            set => this.RaiseAndSetIfChanged(ref this.showResolved, value);
        }

        /// <summary>
        /// Gets the <see cref="AnnotationDraft" /> being composed, or null when the composer is closed.
        /// </summary>
        public AnnotationDraft Draft
        {
            get => this.draft;
            private set => this.RaiseAndSetIfChanged(ref this.draft, value);
        }

        /// <summary>
        /// Gets a value indicating whether a write is in progress, during which the write buttons are disabled so a slow
        /// server round-trip cannot be submitted twice.
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            private set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Gets the annotations of the <see cref="ActiveThing" /> that pass <see cref="KindFilter" /> and
        /// <see cref="ShowResolved" />.
        /// </summary>
        public IReadOnlyList<EngineeringModelDataAnnotation> VisibleAnnotations =>
            this.ActiveThing == null
                ? []
                : this.GetAnnotations(this.ActiveThing)
                    .Where(x => this.KindFilter == null || AnnotationHelper.GetKind(x) == this.KindFilter)
                    .Where(x => this.ShowResolved || x is not ModellingAnnotationItem || this.NeedsAttention(x))
                    .ToList();

        /// <summary>
        /// Gets the kinds the active user is allowed to attach to the <see cref="ActiveThing" />: the kinds ECSS-E-TM-10-25
        /// permits on its class (<see cref="AnnotationHelper.IsApplicable" />) that the user's role may create.
        /// </summary>
        public IReadOnlyList<AnnotationKind> CreatableKinds => this.GetCreatableKinds(this.ActiveThing);

        /// <summary>
        /// Sets the <see cref="Iteration" /> whose things are annotated and rebuilds the annotation indexes. The panel closes
        /// when its thing is gone, and the composer when the annotation it edits, or the change request it answers, is gone.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open</param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /> the active user works for</param>
        public void SetIteration(Iteration iteration, DomainOfExpertise domain)
        {
            this.currentModel = iteration?.Container as EngineeringModel;
            this.currentDomain = domain;

            var activePerson = this.sessionService.Session?.ActivePerson;
            this.currentParticipant = this.currentModel?.EngineeringModelSetup?.Participant.FirstOrDefault(x => x.Person == activePerson);

            this.annotationsByThing = this.currentModel == null
                ? []
                : this.currentModel.ModellingAnnotation
                    .Cast<EngineeringModelDataAnnotation>()
                    .Concat(this.currentModel.GenericNote)
                    .Where(x => AnnotationHelper.GetKind(x) != null)
                    .Select(x => (Annotation: x, ThingIid: AnnotationHelper.GetAnnotatedThingIid(x)))
                    .Where(x => x.ThingIid != null)
                    .GroupBy(x => x.ThingIid.Value)
                    .ToDictionary(x => x.Key, x => x.Select(entry => entry.Annotation).ToList());

            this.proposalsByChangeRequest = this.currentModel == null
                ? []
                : this.currentModel.ModellingAnnotation
                    .OfType<ChangeProposal>()
                    .Where(x => x.ChangeRequest != null)
                    .GroupBy(x => x.ChangeRequest.Iid)
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(proposal => proposal.CreatedOn).ToList());

            if (this.ActiveThing != null && !IsStillPresent(this.ActiveThing, iteration, this.currentModel))
            {
                this.ClosePanel();
            }
            else if (this.Draft != null && !this.IsDraftSubjectPresent(this.Draft))
            {
                this.Draft = null;
            }
            else if (this.Draft is { EditedAnnotation: null } newDraft)
            {
                // the model may have changed under an open composer: keep the previewed short name the one that will be written
                newDraft.ShortName = this.GetNextShortName(newDraft.Kind);
            }
        }

        /// <summary>
        /// Gets the kinds the active user is allowed to attach to the given <paramref name="thing" />: the kinds
        /// ECSS-E-TM-10-25 permits on its class (<see cref="AnnotationHelper.IsApplicable" />) that the user's role may create.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to annotate</param>
        /// <returns>The creatable <see cref="AnnotationKind" />s, none for a null thing</returns>
        public IReadOnlyList<AnnotationKind> GetCreatableKinds(Thing thing)
        {
            return [..AnnotationHelper.AttachableKinds.Where(x => AnnotationHelper.IsApplicable(x, thing) && this.CanCreate(x))];
        }

        /// <summary>
        /// Gets the top-level annotations about the given <paramref name="thing" />, open ones first, newest first. A
        /// change proposal answering a change request about the same thing is nested under it instead.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>The annotations</returns>
        public IReadOnlyList<EngineeringModelDataAnnotation> GetAnnotations(Thing thing)
        {
            if (thing == null || !this.annotationsByThing.TryGetValue(thing.Iid, out var annotations))
            {
                return [];
            }

            return annotations
                .Where(x => x is not ChangeProposal proposal || !annotations.Contains(proposal.ChangeRequest))
                .OrderByDescending(this.NeedsAttention)
                .ThenByDescending(x => x.CreatedOn)
                .ToList();
        }

        /// <summary>
        /// Gets, per <see cref="AnnotationKind" />, how many annotations the given <paramref name="thing" /> carries.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>One <see cref="AnnotationKindSummary" /> per kind present, in the <see cref="AnnotationKind" /> order</returns>
        public IReadOnlyList<AnnotationKindSummary> GetSummary(Thing thing)
        {
            return this.GetAnnotations(thing)
                .GroupBy(x => AnnotationHelper.GetKind(x).Value)
                .OrderBy(x => x.Key)
                .Select(x => new AnnotationKindSummary(x.Key, x.Count(), x.Count(this.NeedsAttention)))
                .ToList();
        }

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="thing" /> has at least one open annotation, counting a
        /// change request whose nested change proposal is still open, exactly as its badges do.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <returns>True if an annotation about it is still open</returns>
        public bool HasOpenAnnotations(Thing thing)
        {
            return this.GetAnnotations(thing).Any(this.NeedsAttention);
        }

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="thing" /> passes any of the selected annotation filters.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <param name="filters">The selected <see cref="AnnotationFilterOption" />s; none selected lets everything pass</param>
        /// <returns>True if it should be shown</returns>
        public bool MatchesFilter(Thing thing, IEnumerable<AnnotationFilterOption> filters)
        {
            var selected = filters?.ToList() ?? [];

            if (selected.Count == 0)
            {
                return true;
            }

            if (thing == null || !this.annotationsByThing.TryGetValue(thing.Iid, out var annotations))
            {
                return false;
            }

            return selected.Exists(filter => filter.Kind == null
                ? this.HasOpenAnnotations(thing)
                : annotations.Exists(x => AnnotationHelper.GetKind(x) == filter.Kind));
        }

        /// <summary>
        /// Gets the change proposals answering the given <paramref name="changeRequest" />, newest first.
        /// </summary>
        /// <param name="changeRequest">The <see cref="ChangeRequest" /></param>
        /// <returns>The <see cref="ChangeProposal" />s</returns>
        public IReadOnlyList<ChangeProposal> GetChangeProposals(ChangeRequest changeRequest)
        {
            return changeRequest != null && this.proposalsByChangeRequest.TryGetValue(changeRequest.Iid, out var proposals) ? proposals : [];
        }

        /// <summary>
        /// Gets why the short name of an edited annotation cannot be saved: it is empty, or another annotation of the model
        /// already uses it. A new annotation's short name is generated when it is written, so it is never in error.
        /// </summary>
        /// <param name="draft">The <see cref="AnnotationDraft" /></param>
        /// <returns>The error message, or null when the short name is fine</returns>
        public string GetShortNameError(AnnotationDraft draft)
        {
            if (draft?.EditedAnnotation is not ModellingAnnotationItem)
            {
                return null;
            }

            var shortName = draft.ShortName?.Trim();

            if (string.IsNullOrEmpty(shortName))
            {
                return "A short name is required.";
            }

            var isTaken = this.currentModel?.ModellingAnnotation
                .Any(x => x.Iid != draft.EditedAnnotation.Iid && string.Equals(x.ShortName, shortName, StringComparison.OrdinalIgnoreCase)) == true;

            return isTaken ? $"The short name '{shortName}' is already used by another annotation." : null;
        }

        /// <summary>
        /// Gets a value indicating whether the active user may attach a new annotation of the given <paramref name="kind" />.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>True if allowed</returns>
        /// <remarks>
        /// <see cref="SessionServiceExtensions.IsAllowedToCreateIn" /> cannot be used for the model itself: an
        /// <see cref="EngineeringModel" /> has no container, which that helper treats as "still being created" and allows.
        /// </remarks>
        public bool CanCreate(AnnotationKind kind)
        {
            return !this.sessionService.IsReadOnly
                   && this.currentModel != null
                   && this.currentParticipant != null
                   && this.sessionService.Session.PermissionService.CanWrite(AnnotationHelper.GetClassKind(kind), this.currentModel);
        }

        /// <summary>
        /// Gets a value indicating whether the active user may edit, re-status or delete the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if allowed</returns>
        public bool CanEdit(EngineeringModelDataAnnotation annotation)
        {
            return annotation != null && !this.sessionService.IsReadOnly && this.sessionService.Session.PermissionService.CanWrite(annotation);
        }

        /// <summary>
        /// Gets a value indicating whether the active user may reply to the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if allowed</returns>
        public bool CanReply(EngineeringModelDataAnnotation annotation)
        {
            return this.CanAddTo(annotation, ClassKind.EngineeringModelDataDiscussionItem);
        }

        /// <summary>
        /// Gets a value indicating whether the active user may accept, reject or withdraw the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it is a <see cref="ModellingAnnotationItem" /> and approvals may be added to it</returns>
        public bool CanApprove(EngineeringModelDataAnnotation annotation)
        {
            return annotation is ModellingAnnotationItem && this.currentDomain != null && this.CanAddTo(annotation, ClassKind.Approval);
        }

        /// <summary>
        /// Gets a value indicating whether the active user may propose a solution to the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it is a <see cref="ReviewItemDiscrepancy" /> and solutions may be added to it</returns>
        public bool CanAddSolution(EngineeringModelDataAnnotation annotation)
        {
            return annotation is ReviewItemDiscrepancy && this.currentDomain != null && this.CanAddTo(annotation, ClassKind.Solution);
        }

        /// <summary>
        /// Gets a value indicating whether the active user may answer the given <paramref name="annotation" /> with a
        /// change proposal.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it is a <see cref="ChangeRequest" /> and change proposals may be created</returns>
        public bool CanProposeChange(EngineeringModelDataAnnotation annotation)
        {
            return annotation is ChangeRequest && this.CanCreate(AnnotationKind.ChangeProposal);
        }

        /// <summary>
        /// Opens the side panel on the annotations of the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <param name="kind">The kind to narrow the panel to, or null to show all kinds</param>
        public void OpenPanel(Thing thing, AnnotationKind? kind = null)
        {
            if (!ReferenceEquals(this.ActiveThing, thing))
            {
                this.Draft = null;
            }

            this.KindFilter = kind;
            this.ActiveThing = thing;
        }

        /// <summary>
        /// Closes the side panel and discards any draft.
        /// </summary>
        public void ClosePanel()
        {
            this.Draft = null;
            this.ActiveThing = null;
        }

        /// <summary>
        /// Opens the composer on a new annotation of the given <paramref name="kind" /> about the <see cref="ActiveThing" />,
        /// clearing a <see cref="KindFilter" /> that would hide it once written.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        public void StartNew(AnnotationKind kind)
        {
            // a panel narrowed to another kind would hide the annotation once written
            if (this.KindFilter != null && this.KindFilter != kind)
            {
                this.KindFilter = null;
            }

            this.Draft = new AnnotationDraft
            {
                Kind = kind,
                ShortName = this.GetNextShortName(kind),
                Owner = this.currentDomain
            };
        }

        /// <summary>
        /// Opens the composer on the values of an existing <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /> to edit</param>
        public void StartEdit(EngineeringModelDataAnnotation annotation)
        {
            var kind = AnnotationHelper.GetKind(annotation);

            if (kind == null)
            {
                return;
            }

            var item = annotation as ModellingAnnotationItem;

            this.Draft = new AnnotationDraft
            {
                Kind = kind.Value,
                Content = annotation.Content,
                Title = item?.Title ?? string.Empty,
                ShortName = item?.ShortName ?? string.Empty,
                Classification = item?.Classification ?? AnnotationClassificationKind.MINOR,
                Owner = item?.Owner,
                Categories = item == null ? [] : [..item.Category],
                AvailableOwners = this.GetAvailableOwners(item?.Owner),
                AvailableCategories = this.GetAvailableCategories(annotation.ClassKind),
                EditedAnnotation = annotation
            };
        }

        /// <summary>
        /// Opens the composer on a new change proposal answering the given <paramref name="changeRequest" />.
        /// </summary>
        /// <param name="changeRequest">The <see cref="ChangeRequest" /></param>
        public void StartChangeProposal(ChangeRequest changeRequest)
        {
            this.Draft = new AnnotationDraft
            {
                Kind = AnnotationKind.ChangeProposal,
                Title = $"Proposal for {changeRequest.ShortName}",
                Classification = changeRequest.Classification,
                ShortName = this.GetNextShortName(AnnotationKind.ChangeProposal),
                Owner = this.currentDomain,
                ChangeRequest = changeRequest
            };
        }

        /// <summary>
        /// Closes the composer without writing.
        /// </summary>
        public void CancelDraft()
        {
            this.Draft = null;
        }

        /// <summary>
        /// Writes the <see cref="Draft" /> and closes the composer on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task SaveDraftAsync()
        {
            var toSave = this.Draft;

            if (toSave?.IsValid != true || this.GetShortNameError(toSave) != null || this.currentModel == null || (toSave.EditedAnnotation == null && this.ActiveThing == null))
            {
                return;
            }

            var written = toSave.EditedAnnotation == null
                ? await this.WriteAsync(() => this.CreateAnnotation(toSave), "creating an annotation")
                : await this.WriteAsync(() => this.UpdateAnnotation(toSave), "editing an annotation");

            if (written && ReferenceEquals(this.Draft, toSave))
            {
                this.Draft = null;
            }
        }

        /// <summary>
        /// Adds a reply to the discussion of the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <param name="content">The text of the reply</param>
        /// <returns>True if the reply was written</returns>
        public Task<bool> ReplyAsync(EngineeringModelDataAnnotation annotation, string content)
        {
            if (annotation == null || string.IsNullOrWhiteSpace(content))
            {
                return Task.FromResult(false);
            }

            return this.WriteAsync(() =>
            {
                var clone = annotation.Clone(false);
                var reply = AnnotationHelper.CreateDiscussionItem(this.currentParticipant, annotation.LanguageCode, content.Trim());
                clone.Discussion.Add(reply);

                return this.sessionService.CreateOrUpdateThingsWithNotification(clone, [reply, clone], BuildNotification(annotation, "received a reply", "reply to"));
            }, "replying to an annotation");
        }

        /// <summary>
        /// Moves the given <paramref name="annotation" /> to another status.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem" /></param>
        /// <param name="status">The new <see cref="AnnotationStatusKind" /></param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task SetStatusAsync(ModellingAnnotationItem annotation, AnnotationStatusKind status)
        {
            if (annotation == null || annotation.Status == status)
            {
                return;
            }

            await this.WriteAsync(() =>
            {
                var clone = annotation.Clone(false);
                clone.Status = status;

                return this.sessionService.CreateOrUpdateThingsWithNotification(this.currentModel.Clone(false), [clone],
                    BuildNotification(annotation, $"set to {AnnotationHelper.GetStatusLabel(status)}", "change the status of"));
            }, "changing the status of an annotation");
        }

        /// <summary>
        /// Records an approval decision on the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem" /></param>
        /// <param name="classification">The <see cref="AnnotationApprovalKind" /> decision</param>
        /// <param name="comment">An optional comment explaining the decision</param>
        /// <returns>True if the decision was written</returns>
        public Task<bool> ApproveAsync(ModellingAnnotationItem annotation, AnnotationApprovalKind classification, string comment)
        {
            if (annotation == null)
            {
                return Task.FromResult(false);
            }

            return this.WriteAsync(() =>
            {
                var clone = annotation.Clone(false);
                var approval = AnnotationHelper.CreateApproval(this.currentParticipant, this.currentDomain, annotation.LanguageCode, classification, comment?.Trim());
                clone.ApprovedBy.Add(approval);

                return this.sessionService.CreateOrUpdateThingsWithNotification(clone, [approval, clone],
                    BuildNotification(annotation, AnnotationHelper.GetApprovalLabel(classification).ToLowerInvariant(), "record a decision on"));
            }, "recording an approval");
        }

        /// <summary>
        /// Adds a proposed solution to the given <paramref name="reviewItemDiscrepancy" />.
        /// </summary>
        /// <param name="reviewItemDiscrepancy">The <see cref="ReviewItemDiscrepancy" /></param>
        /// <param name="content">The proposed solution</param>
        /// <returns>True if the solution was written</returns>
        public Task<bool> AddSolutionAsync(ReviewItemDiscrepancy reviewItemDiscrepancy, string content)
        {
            if (reviewItemDiscrepancy == null || string.IsNullOrWhiteSpace(content))
            {
                return Task.FromResult(false);
            }

            return this.WriteAsync(() =>
            {
                var clone = reviewItemDiscrepancy.Clone(false);
                var solution = AnnotationHelper.CreateSolution(this.currentParticipant, this.currentDomain, reviewItemDiscrepancy.LanguageCode, content.Trim());
                clone.Solution.Add(solution);

                return this.sessionService.CreateOrUpdateThingsWithNotification(clone, [solution, clone], BuildNotification(reviewItemDiscrepancy, "received a proposed solution", "propose a solution for"));
            }, "proposing a solution");
        }

        /// <summary>
        /// Asks for confirmation, then deletes the given <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        public void RequestDelete(EngineeringModelDataAnnotation annotation)
        {
            if (annotation == null)
            {
                return;
            }

            var proposals = annotation is ChangeRequest changeRequest ? this.GetChangeProposals(changeRequest).Count : 0;

            this.confirmCancelPopupViewModel.HeaderText = "Confirm deletion";
            this.confirmCancelPopupViewModel.ContentText = proposals == 0
                ? $"Are you sure you want to permanently delete {GetDisplayName(annotation)}?"
                : $"Are you sure you want to permanently delete {GetDisplayName(annotation)} and its {proposals} change proposal(s)?";
            this.confirmCancelPopupViewModel.OnConfirm = new EventCallbackFactory().Create(this, () => this.DeleteAsync(annotation));
            this.confirmCancelPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Deletes the given <paramref name="annotation" />, together with the change proposals answering it when it is a
        /// change request (their reference to it is mandatory).
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeleteAsync(EngineeringModelDataAnnotation annotation)
        {
            this.confirmCancelPopupViewModel.IsVisible = false;

            if (annotation == null || this.currentModel == null)
            {
                return;
            }

            var proposals = annotation is ChangeRequest changeRequest ? this.GetChangeProposals(changeRequest) : [];
            HashSet<Guid> deleted = [annotation.Iid, ..proposals.Select(x => x.Iid)];

            var written = await this.WriteAsync(() =>
            {
                Thing[] toDelete = [annotation.Clone(false), ..proposals.Select(x => x.Clone(false))];

                return this.sessionService.DeleteThingsWithNotification(this.currentModel.Clone(false), toDelete, BuildNotification(annotation, "deleted", "delete"));
            }, "deleting an annotation");

            // a composer editing a deleted annotation, or answering a deleted change request, can no longer be saved
            if (written && this.Draft != null && (deleted.Contains(this.Draft.EditedAnnotation?.Iid ?? Guid.Empty) || deleted.Contains(this.Draft.ChangeRequest?.Iid ?? Guid.Empty)))
            {
                this.Draft = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an annotation still needs attention: it is open itself, or it is a change request
        /// with a change proposal that is still open (the proposal is nested under it, so it carries no badge of its own).
        /// Uses the proposal lookup built by <see cref="SetIteration" />.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>True if it needs attention</returns>
        private bool NeedsAttention(EngineeringModelDataAnnotation annotation)
        {
            return AnnotationHelper.IsOpen(annotation)
                   || (annotation is ChangeRequest changeRequest && this.GetChangeProposals(changeRequest).Any(AnnotationHelper.IsOpen));
        }

        /// <summary>
        /// Gets the name an annotation is referred to by in messages: its short name, or the start of its text for a note.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /></param>
        /// <returns>The display name</returns>
        private static string GetDisplayName(EngineeringModelDataAnnotation annotation)
        {
            if (annotation is ModellingAnnotationItem item)
            {
                return item.ShortName;
            }

            var content = annotation.Content?.Trim() ?? string.Empty;
            return content.Length <= NoteExcerptLength ? content : $"{content[..NoteExcerptLength]}...";
        }

        /// <summary>
        /// Builds the notification shown after a write about the given <paramref name="annotation" />, worded like the
        /// Requirements Editor's (<see cref="NotificationDescriptionBuilder" />).
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /> operated on</param>
        /// <param name="pastVerb">The past-tense phrase for the success message (e.g. "deleted")</param>
        /// <param name="actionVerb">The infinitive phrase for the failure message (e.g. "delete")</param>
        /// <returns>The <see cref="NotificationDescription" /></returns>
        private static NotificationDescription BuildNotification(EngineeringModelDataAnnotation annotation, string pastVerb, string actionVerb)
        {
            var kind = AnnotationHelper.GetKind(annotation);
            var kindLabel = kind == null ? "Annotation" : AnnotationHelper.GetLabel(kind.Value);

            return NotificationDescriptionBuilder.Build(kindLabel, GetDisplayName(annotation), pastVerb, actionVerb);
        }

        /// <summary>
        /// Gets a value indicating whether an annotated thing is still part of what is open: a thing inside an iteration (or an
        /// iteration itself) must belong to the open <paramref name="iteration" />, so switching iterations closes the panel;
        /// a thing contained by the engineering model itself (a book, a file store) must still be in the
        /// <paramref name="model" />.
        /// </summary>
        /// <param name="thing">The annotated <see cref="Thing" /></param>
        /// <param name="iteration">The open <see cref="Iteration" />, or null</param>
        /// <param name="model">The <see cref="EngineeringModel" /> of the open iteration, or null</param>
        /// <returns>True if the thing is still there</returns>
        private static bool IsStillPresent(Thing thing, Iteration iteration, EngineeringModel model)
        {
            var isIterationData = thing is Iteration || thing.GetContainerOfType<Iteration>() != null;

            return isIterationData ? AnnotationHelper.IsContainedIn(thing, iteration) : AnnotationHelper.IsContainedIn(thing, model);
        }

        /// <summary>
        /// Gets a value indicating whether what a <paramref name="draft" /> is about still exists: the annotation it edits,
        /// and the change request a new change proposal answers.
        /// </summary>
        /// <param name="draft">The <see cref="AnnotationDraft" /></param>
        /// <returns>True if the draft can still be saved</returns>
        private bool IsDraftSubjectPresent(AnnotationDraft draft)
        {
            var annotations = this.currentModel?.ModellingAnnotation.Cast<EngineeringModelDataAnnotation>().Concat(this.currentModel.GenericNote).ToList() ?? [];

            return (draft.EditedAnnotation == null || annotations.Exists(x => x.Iid == draft.EditedAnnotation.Iid))
                   && (draft.ChangeRequest == null || annotations.Exists(x => x.Iid == draft.ChangeRequest.Iid));
        }

        /// <summary>
        /// Writes an edited annotation from the given <paramref name="toSave" /> draft.
        /// </summary>
        /// <param name="toSave">The <see cref="AnnotationDraft" /> whose <see cref="AnnotationDraft.EditedAnnotation" /> is set</param>
        /// <returns>The <see cref="Result" /> of the write</returns>
        private Task<Result> UpdateAnnotation(AnnotationDraft toSave)
        {
            var clone = toSave.EditedAnnotation.Clone(false);
            clone.Content = toSave.Content.Trim();

            if (clone is ModellingAnnotationItem item)
            {
                item.Title = toSave.Title.Trim();
                item.Classification = toSave.Classification;
                item.ShortName = toSave.ShortName.Trim();
                item.Owner = toSave.Owner ?? item.Owner;
                item.Category.Clear();
                item.Category.AddRange(toSave.Categories);
            }

            return this.sessionService.CreateOrUpdateThingsWithNotification(this.currentModel.Clone(false), [clone], BuildNotification(toSave.EditedAnnotation, "updated", "update"));
        }

        /// <summary>
        /// Writes a new annotation about the <see cref="ActiveThing" /> from the given <paramref name="toSave" /> draft.
        /// </summary>
        /// <param name="toSave">The <see cref="AnnotationDraft" /> describing a new annotation</param>
        /// <returns>The <see cref="Result" /> of the write</returns>
        private Task<Result> CreateAnnotation(AnnotationDraft toSave)
        {
            // the previewed short name is kept current by SetIteration, so what the user saw is what is written
            string shortName = null;

            if (AnnotationHelper.IsModellingAnnotationItem(toSave.Kind))
            {
                shortName = string.IsNullOrWhiteSpace(toSave.ShortName) ? this.GetNextShortName(toSave.Kind) : toSave.ShortName.Trim();
            }

            var (annotation, reference) = AnnotationHelper.CreateAnnotation(toSave.Kind, this.ActiveThing, this.currentParticipant, toSave.Owner ?? this.currentDomain,
                AnnotationHelper.GetLanguageCode(this.ActiveThing) ?? DefaultLanguageCode, shortName, toSave.Title.Trim(), toSave.Content.Trim(), toSave.Classification);

            var modelClone = this.currentModel.Clone(false);

            switch (annotation)
            {
                case EngineeringModelDataNote note:
                    modelClone.GenericNote.Add(note);
                    break;
                case ModellingAnnotationItem item:
                    if (item is ChangeProposal proposal)
                    {
                        proposal.ChangeRequest = toSave.ChangeRequest;
                    }

                    modelClone.ModellingAnnotation.Add(item);
                    break;
            }

            var notification = NotificationDescriptionBuilder.Build(AnnotationHelper.GetThingKindLabel(this.ActiveThing), AnnotationHelper.GetThingShortName(this.ActiveThing),
                $"annotated with a {AnnotationHelper.GetLabel(toSave.Kind)}", $"add the {AnnotationHelper.GetLabel(toSave.Kind)} to");

            return this.sessionService.CreateOrUpdateThingsWithNotification(modelClone, [reference, annotation, modelClone], notification);
        }

        /// <summary>
        /// Computes the next free short name for a new annotation of the given <paramref name="kind" /> in the current model.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The short name, or an empty string for a note</returns>
        private string GetNextShortName(AnnotationKind kind)
        {
            return AnnotationHelper.IsModellingAnnotationItem(kind) && this.currentModel != null && this.ActiveThing != null
                ? AnnotationHelper.GetNextShortName(kind, AnnotationHelper.GetThingShortName(this.ActiveThing), this.currentModel.ModellingAnnotation)
                : string.Empty;
        }

        /// <summary>
        /// Gets the domains an edited annotation can be handed over to: the active domains of the model, plus its current
        /// owner should that no longer be one of them.
        /// </summary>
        /// <param name="currentOwner">The current owner of the annotation, which may be null</param>
        /// <returns>The <see cref="DomainOfExpertise" />s, ordered by name</returns>
        private IReadOnlyList<DomainOfExpertise> GetAvailableOwners(DomainOfExpertise currentOwner)
        {
            IEnumerable<DomainOfExpertise> domains = this.currentModel?.EngineeringModelSetup?.ActiveDomain ?? [];

            return [..domains.Append(currentOwner).Where(x => x != null).DistinctBy(x => x.Iid).OrderBy(x => x.Name)];
        }

        /// <summary>
        /// Gets the non-deprecated categories of the model's reference data libraries that may be applied to the given
        /// <paramref name="classKind" />.
        /// </summary>
        /// <param name="classKind">The <see cref="ClassKind" /> of the annotation</param>
        /// <returns>The <see cref="Category" />s, ordered by name</returns>
        private IReadOnlyList<Category> GetAvailableCategories(ClassKind classKind)
        {
            if (this.currentModel?.EngineeringModelSetup == null)
            {
                return [];
            }

            return
            [
                ..this.currentModel.RequiredRdls
                    .SelectMany(x => x.DefinedCategory)
                    .Where(x => !x.IsDeprecated && x.PermissibleClass.Contains(classKind))
                    .DistinctBy(x => x.Iid)
                    .OrderBy(x => x.Name)
            ];
        }

        /// <summary>
        /// Gets a value indicating whether the active user may add a <paramref name="classKind" /> to the given
        /// <paramref name="annotation" />.
        /// </summary>
        /// <param name="annotation">The containing <see cref="EngineeringModelDataAnnotation" /></param>
        /// <param name="classKind">The <see cref="ClassKind" /> that would be added</param>
        /// <returns>True if allowed</returns>
        private bool CanAddTo(EngineeringModelDataAnnotation annotation, ClassKind classKind)
        {
            return annotation != null && this.currentParticipant != null && this.sessionService.IsAllowedToCreateIn(classKind, annotation);
        }

        /// <summary>
        /// Runs a write, logging an exception instead of letting it escape to the UI.
        /// </summary>
        /// <param name="write">The write to run; the session service reports its outcome to the user</param>
        /// <param name="description">What is being written, for the log</param>
        /// <returns>True if the write succeeded</returns>
        private async Task<bool> WriteAsync(Func<Task<Result>> write, string description)
        {
            this.IsBusy = true;

            try
            {
                var result = await write();
                return result.IsSuccess;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while {Description}", description);
                return false;
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}
