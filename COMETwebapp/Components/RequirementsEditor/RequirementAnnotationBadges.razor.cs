// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementAnnotationBadges.razor.cs" company="Starion Group S.A.">
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

    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders, on a <see cref="Requirement" /> row, one badge per annotation kind it carries (solid while an
    /// annotation of that kind is still open), or a discreet "annotate" button when it carries none. Clicking opens the
    /// annotations side panel on the requirement.
    /// </summary>
    public partial class RequirementAnnotationBadges
    {
        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> whose annotations are summarised.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// Opens the annotations side panel on the requirement.
        /// </summary>
        /// <param name="kind">The kind to narrow the panel to, or null to show all kinds</param>
        private void Open(RequirementAnnotationKind? kind)
        {
            this.ViewModel.AnnotationsViewModel.OpenPanel(this.Requirement, kind);
        }
    }
}
