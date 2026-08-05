// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeSelector.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.Selectors
{
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component used to select a <see cref="ParameterType" />
    /// </summary>
    public partial class ParameterTypeSelector
    {
        /// <summary>
        /// Text to be displayed when the selector is shown
        /// </summary>
        [Parameter]
        public string DisplayText { get; set; } = "Filter on Parameter Type:";

        /// <summary>
        /// The placeholder text shown when nothing is selected. Defaults to a "Select ..." wording since the
        /// selector is a general-purpose selector; pass a "Filter by ..." value when it is used as a page filter.
        /// </summary>
        [Parameter]
        public string NullText { get; set; } = "Select a parameter type...";

        /// <summary>
        /// Condition to check if name and shortname shall be displayed in the selector. If false, only the name is displayed
        /// </summary>
        [Parameter]
        public bool DisplayNameAndShortname { get; set; }
    }
}
