﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IProductTreeViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;

    /// <summary>
    /// View Model for building the product tree
    /// </summary>
    public interface IProductTreeViewModel<T>
    {
        /// <summary>
        /// Gets or sets the filter options for the tree
        /// </summary>
        IReadOnlyList<TreeFilter> TreeFilters { get; }

        /// <summary>
        /// Gets or sets the selected filter option
        /// </summary>
        TreeFilter SelectedFilter { get; set; }

        /// <summary>
        /// Gets or sets the search text used for filtering the tree
        /// </summary>
        string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the root of the <see cref="IProductTreeViewModel{T}"/>
        /// </summary>
        T RootViewModel { get; set; }

        /// <summary>
        /// Event for when the filter on the tree changes
        /// </summary>
        void OnFilterChanged();

        /// <summary>
        /// Event for when the text of the search filter is changing
        /// </summary>
        void OnSearchFilterChange();

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="productTreeElements">the product tree elements</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedActualFiniteStates">the selected states</param>
        /// <returns>the root baseNode of the tree or null if the tree can not be created</returns>
        T CreateTree(IEnumerable<ElementBase> productTreeElements, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates);
    }
}
