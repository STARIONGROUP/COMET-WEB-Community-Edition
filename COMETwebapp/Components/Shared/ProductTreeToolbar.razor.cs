// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ProductTreeToolbar.razor.cs" company="Starion Group S.A.">
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
    using COMETwebapp.ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Shared toolbar rendered above a product tree (System Representation / 3D Viewer): a search box and a
    /// "View" cog that opens a dropdown with the display-option toggles (<see cref="IProductTreeDisplayOptions.ShowName" />,
    /// <see cref="IProductTreeDisplayOptions.ShowOwner" />, <see cref="IProductTreeDisplayOptions.ShowCategories" />).
    /// Callers can append feature-specific toggles through <see cref="ChildContent" />.
    /// </summary>
    public partial class ProductTreeToolbar
    {
        /// <summary>
        /// Gets or sets the <see cref="IProductTreeDisplayOptions" /> the toolbar reads and toggles.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IProductTreeDisplayOptions ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the id given to the "View" cog <c>DxButton</c>, also used as the <c>DxDropDown</c>
        /// <c>PositionTarget</c>. Must be unique per page so two toolbars can coexist.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string ButtonId { get; set; }

        /// <summary>
        /// Gets or sets additional display-option toggles rendered after the shared ones.
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Whether the "View" display-options dropdown is open.
        /// </summary>
        private bool viewMenuOpen;
    }
}
