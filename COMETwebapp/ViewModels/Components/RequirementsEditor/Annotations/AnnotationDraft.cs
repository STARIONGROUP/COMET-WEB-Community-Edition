// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationDraft.cs" company="Starion Group S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The values typed in the annotation composer before they are written: a new annotation, an edit of an existing
    /// one, or a change proposal answering a change request.
    /// </summary>
    public class AnnotationDraft
    {
        /// <summary>
        /// Gets or sets the <see cref="RequirementAnnotationKind" /> being written.
        /// </summary>
        public RequirementAnnotationKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the title of a <see cref="ModellingAnnotationItem" />.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text of the annotation.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="AnnotationClassificationKind" /> of a <see cref="ModellingAnnotationItem" />.
        /// </summary>
        public AnnotationClassificationKind Classification { get; set; } = AnnotationClassificationKind.MINOR;

        /// <summary>
        /// Gets or sets the short name the annotation will be (or is) known by: a generated preview for a new annotation,
        /// editable when an existing one is edited.
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise" /> owning a <see cref="ModellingAnnotationItem" />.
        /// </summary>
        public DomainOfExpertise Owner { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Category" />s applied to a <see cref="ModellingAnnotationItem" />.
        /// </summary>
        public IEnumerable<Category> Categories { get; set; } = [];

        /// <summary>
        /// Gets the domains offered as owner when an existing annotation is edited, kept as one stable list for the
        /// lifetime of the draft so the selector keeps its selection.
        /// </summary>
        public IReadOnlyList<DomainOfExpertise> AvailableOwners { get; init; } = [];

        /// <summary>
        /// Gets the categories offered when an existing annotation is edited, kept as one stable list for the lifetime of
        /// the draft so the selector keeps its selection.
        /// </summary>
        public IReadOnlyList<Category> AvailableCategories { get; init; } = [];

        /// <summary>
        /// Gets a value indicating whether the draft edits an existing <see cref="ModellingAnnotationItem" />, which also
        /// offers its short name, owner and categories.
        /// </summary>
        public bool ShowsDetails => this.EditedAnnotation is ModellingAnnotationItem;

        /// <summary>
        /// Gets or sets the annotation being edited, or null when the draft creates a new one.
        /// </summary>
        public EngineeringModelDataAnnotation EditedAnnotation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChangeRequest" /> a new <see cref="RequirementAnnotationKind.ChangeProposal" /> answers.
        /// </summary>
        public ChangeRequest ChangeRequest { get; set; }

        /// <summary>
        /// Gets a value indicating whether the draft writes a <see cref="ModellingAnnotationItem" />, which needs a title.
        /// </summary>
        public bool HasTitle => RequirementAnnotationHelper.IsModellingAnnotationItem(this.Kind);

        /// <summary>
        /// Gets a value indicating whether every value the server requires has been filled in.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(this.Content) && (!this.HasTitle || !string.IsNullOrWhiteSpace(this.Title));
    }
}
