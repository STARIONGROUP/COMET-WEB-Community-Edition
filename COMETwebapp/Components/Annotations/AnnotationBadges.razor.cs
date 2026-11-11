// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationBadges.razor.cs" company="Starion Group S.A.">
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

    using COMETwebapp.ViewModels.Components.Annotations;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders, next to an annotated <see cref="Thing" />, one badge per annotation kind it carries (solid while an
    /// annotation of that kind still needs attention), or a discreet "annotate" button when it carries none and the user
    /// may annotate it. Clicking opens the annotations side panel on the thing.
    /// </summary>
    public partial class AnnotationBadges
    {
        /// <summary>
        /// Gets or sets the <see cref="IAnnotationsViewModel" /> indexing the annotations of the host page.
        /// </summary>
        [Parameter]
        public IAnnotationsViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Thing" /> whose annotations are summarised.
        /// </summary>
        [Parameter]
        public Thing Thing { get; set; }

        /// <summary>
        /// Opens the annotations side panel on the thing.
        /// </summary>
        /// <param name="kind">The kind to narrow the panel to, or null to show all kinds</param>
        private void Open(AnnotationKind? kind)
        {
            this.ViewModel.OpenPanel(this.Thing, kind);
        }
    }
}
