// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ProductTreeViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Enumerations;

    using ReactiveUI;

    /// <summary>
    /// View Model for building the product tree
    /// </summary>
    public abstract class ProductTreeViewModel<T> : DisposableObject, IProductTreeViewModel<T>, IProductTreeDisplayOptions where T : IBaseNodeViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="RootViewModel" />s
        /// </summary>
        private T rootViewModel;

        /// <summary>
        /// Gets or sets the root of the <see cref="ProductTreeViewModel{T}" />
        /// </summary>
        public T RootViewModel
        {
            get => this.rootViewModel;
            set => this.RaiseAndSetIfChanged(ref this.rootViewModel, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SelectedFilter" />
        /// </summary>
        private TreeFilter selectedFilter;

        /// <summary>
        /// Gets or sets the filter options for the tree
        /// </summary>
        public IReadOnlyList<TreeFilter> TreeFilters { get; protected set; }

        /// <summary>
        /// Gets or sets the selected filter option
        /// </summary>
        public TreeFilter SelectedFilter
        {
            get => this.selectedFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedFilter, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SearchText" />
        /// </summary>
        private string searchText = string.Empty;

        /// <summary>
        /// Gets or sets the search text used for filtering the tree
        /// </summary>
        public string SearchText
        {
            get => this.searchText;
            set => this.RaiseAndSetIfChanged(ref this.searchText, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowName" />.
        /// </summary>
        private bool showName = true;

        /// <summary>
        /// Gets or sets a value indicating whether nodes should display their <see cref="CDP4Common.CommonData.DefinedThing.Name" />
        /// (<c>true</c>) or <see cref="CDP4Common.CommonData.DefinedThing.ShortName" /> (<c>false</c>).
        /// </summary>
        public bool ShowName
        {
            get => this.showName;
            set => this.RaiseAndSetIfChanged(ref this.showName, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowOwner" />.
        /// </summary>
        private bool showOwner = true;

        /// <summary>
        /// Gets or sets a value indicating whether the owning <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />
        /// pill is shown on each tree node.
        /// </summary>
        public bool ShowOwner
        {
            get => this.showOwner;
            set => this.RaiseAndSetIfChanged(ref this.showOwner, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowCategories" />.
        /// </summary>
        private bool showCategories = true;

        /// <summary>
        /// Gets or sets a value indicating whether category pills are shown on each tree node.
        /// </summary>
        public bool ShowCategories
        {
            get => this.showCategories;
            set => this.RaiseAndSetIfChanged(ref this.showCategories, value);
        }

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="productTreeElements">the product tree elements</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedActualFiniteStates">the selected states</param>
        /// <returns>the root baseNode of the tree or null if the tree can not be created</returns>
        public abstract T CreateTree(IEnumerable<ElementBase> productTreeElements, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates);
        
        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase">the element base used in the baseNode</param>
        /// <param name="current">the current baseNode</param>
        /// <param name="parent">the parent of the current baseNode. Null if the current baseNode is the root baseNode</param>
        /// <param name="selectedOption">the selected <see cref="Option" /></param>
        /// <param name="selectedActualFiniteStates">the selected <see cref="ActualFiniteState" /></param>
        protected abstract void CreateTreeRecursively(ElementBase elementBase, T current, T parent, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates);
        
        /// <summary>
        /// Event for when the filter on the tree changes
        /// </summary>
        public virtual void OnFilterChanged()
        {
        }

        /// <summary>
        /// Event for when the text of the search filter is changing
        /// </summary>
        public virtual void OnSearchFilterChange()
        {
        }
    }
}
