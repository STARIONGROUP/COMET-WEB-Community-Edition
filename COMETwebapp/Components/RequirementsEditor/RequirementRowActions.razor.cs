// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRowActions.razor.cs" company="Starion Group S.A.">
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

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders the deprecated badge (when applicable), the owner/category pills and the edit and deprecate/restore
    /// buttons of a <see cref="Requirement" /> row. When <see cref="Inline" /> is false (the default) it stacks two
    /// rows (owner + edit, then categories + deprecate) for the fixed-width pills column shown beside the value
    /// columns; when true it renders a single inline row (pills then both buttons) for the row with no value columns.
    /// </summary>
    public partial class RequirementRowActions
    {
        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> to render the pills and actions for.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pills and actions are laid out on a single inline row (true) or
        /// stacked as two pill-and-action rows (false, the default).
        /// </summary>
        [Parameter]
        public bool Inline { get; set; }
    }
}
