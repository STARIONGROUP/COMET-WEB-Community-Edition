// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementAnnotationsPanel.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RequirementsEditor
{
    using CDP4Common.ReportingData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    using ReactiveUI;

    /// <summary>
    /// The side panel listing the annotations of the active requirement of the Requirements Editor, with kind filter
    /// chips, an inline composer for a new or edited annotation, and one <see cref="AnnotationCard" /> per annotation.
    /// </summary>
    public partial class RequirementAnnotationsPanel
    {
        /// <summary>
        /// The classifications offered for a modelling annotation item, in display order.
        /// </summary>
        private static readonly AnnotationClassificationKind[] Classifications = [AnnotationClassificationKind.MINOR, AnnotationClassificationKind.MAJOR];

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Add annotation" dropdown, offered once the requirement already has
        /// annotations, is open.
        /// </summary>
        public bool IsAddMenuOpen { get; set; }

        /// <summary>
        /// Gets the one-line explanation of what a <see cref="RequirementAnnotationKind" /> is for, shown in the composer and
        /// as the tooltip of its add button.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The explanation</returns>
        public static string GetKindHint(RequirementAnnotationKind kind)
        {
            return kind switch
            {
                RequirementAnnotationKind.Note => "A comment or a clarification question. Others can reply to it.",
                RequirementAnnotationKind.ChangeRequest => "Ask for the requirement to be changed. It can be accepted or rejected and answered with a change proposal.",
                RequirementAnnotationKind.ReviewItemDiscrepancy => "Flag a problem found while reviewing the requirement. Solutions can be proposed and the item accepted or rejected.",
                RequirementAnnotationKind.RequestForDeviation => "Ask, before implementation, to be allowed to depart from the requirement.",
                RequirementAnnotationKind.RequestForWaiver => "Ask, after implementation, to accept a product that does not conform to the requirement.",
                _ => "Describe the concrete change that answers the change request."
            };
        }

        /// <summary>
        /// Gets the placeholder of the composer's text area.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /> being written</param>
        /// <returns>The placeholder</returns>
        public static string GetContentPlaceholder(RequirementAnnotationKind kind)
        {
            return kind switch
            {
                RequirementAnnotationKind.Note => "Write a comment or a question...",
                RequirementAnnotationKind.ChangeRequest => "What should change, and why?",
                RequirementAnnotationKind.ReviewItemDiscrepancy => "What is wrong with the requirement?",
                RequirementAnnotationKind.ChangeProposal => "The proposed new wording or change...",
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
            var label = RequirementAnnotationHelper.GetLabel(draft.Kind);

            if (draft.EditedAnnotation != null)
            {
                return $"Edit {label}";
            }

            return draft.ChangeRequest == null ? $"New {label}" : $"{label} for {draft.ChangeRequest.ShortName}";
        }

        /// <summary>
        /// Opens the composer on a new annotation of the given <paramref name="kind" /> and closes the add dropdown.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /> to add</param>
        public void StartNew(RequirementAnnotationKind kind)
        {
            this.IsAddMenuOpen = false;
            this.ViewModel.AnnotationsViewModel.StartNew(kind);
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
                    x => x.ViewModel.AnnotationsViewModel.ActiveRequirement,
                    x => x.ViewModel.AnnotationsViewModel.KindFilter,
                    x => x.ViewModel.AnnotationsViewModel.ShowResolved,
                    x => x.ViewModel.AnnotationsViewModel.Draft,
                    x => x.ViewModel.AnnotationsViewModel.IsBusy)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
