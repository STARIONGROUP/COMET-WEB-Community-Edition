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
    }
}
