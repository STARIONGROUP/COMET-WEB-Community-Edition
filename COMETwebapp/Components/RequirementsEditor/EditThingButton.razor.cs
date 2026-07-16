// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditThingButton.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A compact edit button that opens the create/edit form for a <see cref="Requirement" />,
    /// <see cref="RequirementsSpecification" /> or <see cref="RequirementsGroup" />.
    /// </summary>
    public partial class EditThingButton
    {
        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Thing" /> to edit.
        /// </summary>
        [Parameter]
        public Thing Thing { get; set; }

        /// <summary>
        /// Gets the human-readable kind of the <see cref="Thing" /> used in the button tooltip.
        /// </summary>
        private string KindLabel => this.Thing switch
        {
            Requirement => "requirement",
            RequirementsSpecification => "specification",
            RequirementsGroup => "group",
            _ => "item"
        };
    }
}
