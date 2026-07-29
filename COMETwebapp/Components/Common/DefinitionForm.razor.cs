// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefinitionForm.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Common
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="DefinitionForm" /> component.
    /// </summary>
    public partial class DefinitionForm : SelectedDataItemForm
    {
        /// <summary>
        /// The <see cref="Definition" /> being created or edited.
        /// </summary>
        [Parameter]
        public Definition Item { get; set; }

        /// <summary>
        /// The <see cref="NaturalLanguage" />s available for selection.
        /// </summary>
        [Parameter]
        public IEnumerable<NaturalLanguage> AvailableLanguages { get; set; } = [];
    }
}
