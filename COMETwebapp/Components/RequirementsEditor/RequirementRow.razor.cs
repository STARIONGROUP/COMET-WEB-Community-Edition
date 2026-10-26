// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRow.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders one <see cref="Requirement" /> as a document row: its identity and definition, the simple parameter
    /// value columns with the pills/actions, and (when enabled) its parametric constraints and traceability.
    /// </summary>
    public partial class RequirementRow
    {
        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> to render.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// Gets or sets the simple-parameter-value columns to render for the requirement, in display order.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ParameterType> VisibleParameterTypes { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the value columns are shown on their own full-width line below the
        /// definition (true) or inline beside it (false).
        /// </summary>
        [Parameter]
        public bool ValuesBelow { get; set; }

        /// <summary>
        /// Gets the CSS classes flagging the row's annotation state: a marker when it carries open annotations (while
        /// annotations are shown), and a highlight when its annotations are shown in the side panel.
        /// </summary>
        /// <returns>The space-separated CSS classes, possibly empty</returns>
        private string GetAnnotationCssClass()
        {
            var annotations = this.ViewModel.AnnotationsViewModel;
            var open = this.ViewModel.ShowAnnotations && annotations.HasOpenAnnotations(this.Requirement) ? "req-row-annotated" : string.Empty;
            var active = ReferenceEquals(annotations.ActiveRequirement, this.Requirement) ? "req-row-annotation-active" : string.Empty;

            return $"{open} {active}".Trim();
        }

        /// <summary>
        /// Gets the annotation kinds of the requirement that still have open annotations, which colour the bar in front of
        /// the row, or none while annotations are hidden.
        /// </summary>
        /// <returns>The <see cref="AnnotationKindSummary" />s with at least one open annotation</returns>
        private IReadOnlyList<AnnotationKindSummary> GetOpenAnnotationKinds()
        {
            return this.ViewModel.ShowAnnotations
                ? [..this.ViewModel.AnnotationsViewModel.GetSummary(this.Requirement).Where(x => x.OpenCount > 0)]
                : [];
        }
    }
}
