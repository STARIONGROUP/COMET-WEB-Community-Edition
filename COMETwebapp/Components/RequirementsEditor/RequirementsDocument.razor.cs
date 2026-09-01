// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsDocument.razor.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders a <see cref="RequirementsSpecification" /> as a document: the specification header, its requirements,
    /// and its <see cref="RequirementsGroup" /> hierarchy (recursively) with the requirements filed under each group.
    /// </summary>
    public partial class RequirementsDocument
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a
        /// session opened from an ECSS-E-TM-10-25 Annex C3 archive. The add requirement, add group and delete
        /// group controls are disabled when this is <see langword="true" />, so the document can still be
        /// inspected but never modified.
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// The number of simple-parameter-value columns that still fit inline beside the definition and pills; above
        /// this the value grid drops to its own full-width line below the row. A count heuristic — it does not measure
        /// the actual panel width — tuned to 5 so the common case stays inline even with the table of contents open.
        /// The grid also wraps at 5 columns per line (see .req-row-values max-width) so it never runs off the row.
        /// </summary>
        private const int MaxInlineValueColumns = 5;

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsContainer" /> rendered by this instance. When <c>null</c>, the
        /// selected specification (the document root) is rendered.
        /// </summary>
        [Parameter]
        public RequirementsContainer Container { get; set; }

        /// <summary>
        /// Gets or sets the nesting level of the current group, used to size its header (0 for the specification root).
        /// </summary>
        [Parameter]
        public int Level { get; set; }

        /// <summary>
        /// Gets the HTML anchor id used to scroll to the given <paramref name="group" /> from the table of contents.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <returns>The anchor id</returns>
        public static string GroupAnchorId(RequirementsGroup group)
        {
            return $"req-group-{group.Iid}";
        }

        /// <summary>
        /// Gets the HTML anchor id used to scroll to the given <paramref name="requirement" /> from a traceability link.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The anchor id</returns>
        public static string RequirementAnchorId(Requirement requirement)
        {
            return $"req-{requirement.Iid}";
        }
    }
}
