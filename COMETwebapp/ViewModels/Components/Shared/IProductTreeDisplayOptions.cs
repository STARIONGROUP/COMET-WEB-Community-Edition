// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IProductTreeDisplayOptions.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Shared
{
    /// <summary>
    /// Non-generic contract exposing the search text and node display-option toggles shared by every
    /// product tree view model, so a single non-generic component (<see cref="COMETwebapp.Components.Shared.ProductTreeToolbar" />)
    /// can bind to them without depending on the generic <see cref="ProductTreeViewModel{T}" />.
    /// </summary>
    public interface IProductTreeDisplayOptions
    {
        /// <summary>
        /// Gets or sets the search text used for filtering the tree
        /// </summary>
        string SearchText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether nodes should display their <see cref="CDP4Common.CommonData.DefinedThing.Name" />
        /// (<c>true</c>) or <see cref="CDP4Common.CommonData.DefinedThing.ShortName" /> (<c>false</c>).
        /// </summary>
        bool ShowName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owning <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />
        /// pill is shown on each tree node.
        /// </summary>
        bool ShowOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether category pills are shown on each tree node.
        /// </summary>
        bool ShowCategories { get; set; }
    }
}
