// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NodePills.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Shared
{
    using CDP4Common.EngineeringModelData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders the owner and category pills of an <see cref="ElementBase" /> tree node, shared by the
    /// System Representation and 3D Viewer product trees so the pill markup and styling stay in one place.
    /// </summary>
    public partial class NodePills
    {
        /// <summary>
        /// Gets or sets the <see cref="ElementBase" /> whose owner and categories are rendered as pills.
        /// </summary>
        [Parameter]
        public ElementBase ElementBase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner pill is shown.
        /// </summary>
        [Parameter]
        public bool ShowOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown.
        /// </summary>
        [Parameter]
        public bool ShowCategories { get; set; }
    }
}
