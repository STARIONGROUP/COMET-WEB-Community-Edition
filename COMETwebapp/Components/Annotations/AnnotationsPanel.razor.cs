// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationsPanel.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;

    using COMETwebapp.ViewModels.Components.Annotations;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    using ReactiveUI;

    /// <summary>
    /// The side panel listing the annotations of the thing a host page opened it on, with kind filter
    /// chips, an inline composer for a new or edited annotation, and one <see cref="AnnotationCard" /> per annotation.
    /// </summary>
    public partial class AnnotationsPanel
    {
        /// <summary>
        /// The classifications offered for a modelling annotation item, in display order.
        /// </summary>
        private static readonly AnnotationClassificationKind[] Classifications = [AnnotationClassificationKind.MINOR, AnnotationClassificationKind.MAJOR];

        /// <summary>
        /// Gets or sets the <see cref="IAnnotationsViewModel" /> of the host page.
        /// </summary>
        [Parameter]
        public IAnnotationsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the callback opening the host's own edit form on the annotated thing, offered on change requests,
        /// review item discrepancies and change proposals. Without one, that action is not shown.
        /// </summary>
        [Parameter]
        public EventCallback<Thing> OnEditAnnotatedThing { get; set; }

        /// <summary>
        /// Gets or sets the text of the action opening the annotated thing for editing, e.g. "Edit requirement".
        /// </summary>
        [Parameter]
        public string EditAnnotatedThingText { get; set; } = "Edit";

        /// <summary>
        /// Gets or sets a value indicating whether the "+ Annotation" dropdown listing the kinds that can be added is open.
        /// </summary>
        public bool IsAddMenuOpen { get; set; }

        /// <summary>
        /// Gets the one-line explanation of what a <see cref="AnnotationKind" /> is for, shown in the composer and
        /// as the tooltip of its add button.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <returns>The explanation</returns>
        public static string GetKindHint(AnnotationKind kind)
        {
            return kind switch
            {
                AnnotationKind.Note => "A comment or a clarification question. Others can reply to it.",
                AnnotationKind.ChangeRequest => "Ask for this item to be changed. It can be accepted or rejected and answered with a change proposal.",
                AnnotationKind.ReviewItemDiscrepancy => "Flag a problem found while reviewing this item. Solutions can be proposed and the item accepted or rejected.",
                AnnotationKind.RequestForDeviation => "Ask, before implementation, to be allowed to depart from this item.",
                AnnotationKind.RequestForWaiver => "Ask, after implementation, to accept a product that does not conform to this item.",
                _ => "Describe the concrete change that answers the change request."
            };
        }

        /// <summary>
        /// Gets the placeholder of the composer's text area.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /> being written</param>
        /// <returns>The placeholder</returns>
        public static string GetContentPlaceholder(AnnotationKind kind)
        {
            return kind switch
            {
                AnnotationKind.Note => "Write a comment or a question...",
                AnnotationKind.ChangeRequest => "What should change, and why?",
                AnnotationKind.ReviewItemDiscrepancy => "What is wrong with this item?",
                AnnotationKind.ChangeProposal => "The proposed new wording or change...",
                _ => "What is requested, and the justification..."
            };
        }

        /// <summary>
        /// Gets the heading of the composer.
        /// </summary>
        /// <param name="draft">The <see cref="AnnotationDraft" /> being composed</param>
        /// <returns>The heading, e.g. "New Change Request"</returns>
        public static string GetComposerTitle(AnnotationDraft draft)
        {
            var label = AnnotationHelper.GetLabel(draft.Kind);

            if (draft.EditedAnnotation != null)
            {
                return $"Edit {label}";
            }

            return draft.ChangeRequest == null ? $"New {label}" : $"{label} for {draft.ChangeRequest.ShortName}";
        }

        /// <summary>
        /// Opens the composer on a new annotation of the given <paramref name="kind" /> and closes the add dropdown.
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /> to add</param>
        public void StartNew(AnnotationKind kind)
        {
            this.IsAddMenuOpen = false;
            this.ViewModel.StartNew(kind);
        }

        /// <summary>
        /// Closes the add dropdown when Escape is pressed inside it.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs" /></param>
        public void OnAddMenuKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Escape")
            {
                this.IsAddMenuOpen = false;
            }
        }

        /// <summary>
        /// Re-renders the panel whenever what it shows changes.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.ActiveThing,
                    x => x.ViewModel.KindFilter,
                    x => x.ViewModel.ShowResolved,
                    x => x.ViewModel.Draft,
                    x => x.ViewModel.IsBusy)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
