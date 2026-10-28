// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationCard.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Annotations
{
    using System.Globalization;

    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Annotations;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders one annotation of the annotations side panel: its kind, short name, classification, status, content,
    /// decisions, proposed solutions or change proposals, its discussion, and the actions its kind allows (reply,
    /// accept / reject / withdraw, propose a solution or a change, open the annotated thing, edit or delete the annotation).
    /// </summary>
    public partial class AnnotationCard
    {
        /// <summary>
        /// The status values offered by the status selector, as a stable list so the combo box keeps its selection.
        /// </summary>
        private static readonly IReadOnlyList<StatusOption> StatusOptions =
            [..AnnotationHelper.Statuses.Select(x => new StatusOption(x, AnnotationHelper.GetStatusLabel(x)))];

        /// <summary>
        /// The inline form currently open on the card.
        /// </summary>
        private AnnotationCardForm openForm;

        /// <summary>
        /// The text typed in the open inline form.
        /// </summary>
        private string formText = string.Empty;

        /// <summary>
        /// The decision the open decision form records.
        /// </summary>
        private AnnotationApprovalKind pendingDecision;

        /// <summary>
        /// Gets or sets the <see cref="IAnnotationsViewModel" /> of the host page.
        /// </summary>
        [Parameter]
        public IAnnotationsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the callback opening the host's own edit form on the annotated thing. Without one, the action is not
        /// shown.
        /// </summary>
        [Parameter]
        public EventCallback<Thing> OnEditAnnotatedThing { get; set; }

        /// <summary>
        /// Gets or sets the text of the action opening the annotated thing for editing, e.g. "Edit requirement".
        /// </summary>
        [Parameter]
        public string EditAnnotatedThingText { get; set; } = "Edit";

        /// <summary>
        /// Gets or sets the <see cref="EngineeringModelDataAnnotation" /> to render.
        /// </summary>
        [Parameter]
        public EngineeringModelDataAnnotation Annotation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the card is rendered inside another card, as a change proposal is
        /// inside its change request.
        /// </summary>
        [Parameter]
        public bool IsNested { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="ISessionService" />, used to check whether the annotated thing may be edited.
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets the display name of the author of an annotation, decision, solution or reply.
        /// </summary>
        /// <param name="author">The authoring <see cref="Participant" /></param>
        /// <returns>The person's name, or "Unknown" when it cannot be resolved</returns>
        public static string GetAuthorName(Participant author)
        {
            return string.IsNullOrWhiteSpace(author?.Person?.Name) ? "Unknown" : author.Person.Name;
        }

        /// <summary>
        /// Formats a creation date. Time zones are not available in this application, so the date is shown in UTC.
        /// </summary>
        /// <param name="date">The UTC date</param>
        /// <returns>The formatted date, e.g. "2026-10-21 14:05 UTC"</returns>
        public static string FormatDate(DateTime date)
        {
            return $"{date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC";
        }

        /// <summary>
        /// Gets the icon of an approval decision.
        /// </summary>
        /// <param name="classification">The <see cref="AnnotationApprovalKind" /></param>
        /// <returns>The <see cref="IconName" /></returns>
        private static IconName GetApprovalIcon(AnnotationApprovalKind classification)
        {
            return classification switch
            {
                AnnotationApprovalKind.ACCEPTED => IconName.Check,
                AnnotationApprovalKind.REJECTED => IconName.Close,
                _ => IconName.Undo
            };
        }

        /// <summary>
        /// Gets a value indicating whether the card offers to open the annotated thing for editing: the kinds that ask for
        /// the thing itself to change, when the host supplied an edit action and the user may edit the thing.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /> of the annotation</param>
        /// <returns>True if the edit action is shown</returns>
        private bool CanEditAnnotatedThing(AnnotationKind kind)
        {
            var thing = this.ViewModel.ActiveThing;

            return kind is AnnotationKind.ChangeRequest or AnnotationKind.ReviewItemDiscrepancy or AnnotationKind.ChangeProposal
                   && this.OnEditAnnotatedThing.HasDelegate
                   && thing != null
                   && !this.SessionService.IsReadOnly
                   && this.SessionService.Session.PermissionService.CanWrite(thing);
        }

        /// <summary>
        /// Opens an inline form with an empty text.
        /// </summary>
        /// <param name="form">The <see cref="AnnotationCardForm" /> to open</param>
        private void OpenForm(AnnotationCardForm form)
        {
            this.formText = string.Empty;
            this.openForm = form;
        }

        /// <summary>
        /// Opens the comment form confirming the given decision.
        /// </summary>
        /// <param name="decision">The <see cref="AnnotationApprovalKind" /> to record</param>
        private void OpenDecision(AnnotationApprovalKind decision)
        {
            this.pendingDecision = decision;
            this.OpenForm(AnnotationCardForm.Decision);
        }

        /// <summary>
        /// Closes the open inline form.
        /// </summary>
        private void CloseForm()
        {
            this.openForm = AnnotationCardForm.None;
            this.formText = string.Empty;
        }

        /// <summary>
        /// Writes the typed reply and closes the form on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SendReplyAsync()
        {
            if (await this.ViewModel.ReplyAsync(this.Annotation, this.formText))
            {
                this.CloseForm();
            }
        }

        /// <summary>
        /// Writes the typed solution and closes the form on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SendSolutionAsync()
        {
            if (this.Annotation is ReviewItemDiscrepancy discrepancy && await this.ViewModel.AddSolutionAsync(discrepancy, this.formText))
            {
                this.CloseForm();
            }
        }

        /// <summary>
        /// Writes the pending decision with the typed comment and closes the form on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SendDecisionAsync()
        {
            if (this.Annotation is ModellingAnnotationItem item && await this.ViewModel.ApproveAsync(item, this.pendingDecision, this.formText))
            {
                this.CloseForm();
            }
        }

        /// <summary>
        /// An entry of the status selector.
        /// </summary>
        /// <param name="Status">The <see cref="AnnotationStatusKind" /></param>
        /// <param name="Label">The text shown for it</param>
        private sealed record StatusOption(AnnotationStatusKind Status, string Label);
    }
}
