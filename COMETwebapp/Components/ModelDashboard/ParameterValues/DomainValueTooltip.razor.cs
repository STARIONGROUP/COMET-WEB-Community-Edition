// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainValueTooltip.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelDashboard.ParameterValues
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Tooltip body shown when hovering a parameter-value bar. It spells out the hovered domain and, for a domain that
    /// owns value sets, offers a "drill in" button carrying the count and its share; otherwise it shows a dash.
    /// </summary>
    public partial class DomainValueTooltip
    {
        /// <summary>
        /// The full name of the hovered domain of expertise.
        /// </summary>
        [Parameter]
        public string DomainName { get; set; }

        /// <summary>
        /// The number of value sets the hovered series point represents.
        /// </summary>
        [Parameter]
        public int Count { get; set; }

        /// <summary>
        /// The share, in percent, that <see cref="Count" /> represents of the domain's value sets, or
        /// <see cref="double.NaN" /> when the domain owns none.
        /// </summary>
        [Parameter]
        public double Percentage { get; set; }

        /// <summary>
        /// Invoked when the drill-in button is clicked, to open the underlying value sets.
        /// </summary>
        [Parameter]
        public EventCallback OnMore { get; set; }
    }
}
