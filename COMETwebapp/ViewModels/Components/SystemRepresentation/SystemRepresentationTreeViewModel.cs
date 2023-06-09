// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationTreeViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Extensions;
    using COMETwebapp.ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the SystemRepresentationTree
    /// </summary>
    public class SystemRepresentationTreeViewModel : ProductTreeViewModel<SystemNodeViewModel>
    {
        /// <summary>
        ///     The <see cref="EventCallback" /> to call on baseNode selection
        /// </summary>
        public EventCallback<SystemNodeViewModel> OnClick { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="SystemRepresentationTreeViewModel" />
        /// </summary>
        public SystemRepresentationTreeViewModel()
        {
            var enumValues = Enum.GetValues(typeof(TreeFilter)).Cast<TreeFilter>();
            this.TreeFilters = enumValues.ToList();
            this.SelectedFilter = TreeFilter.ShowFullTree;

            this.Disposables.Add(this.WhenAnyValue(x => x.SearchText).Subscribe(_ => this.OnSearchFilterChange()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedFilter).Subscribe(_ => this.OnFilterChanged()));
        }

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="productTreeElements">the product tree elements</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedActualFiniteStates">the selected states</param>
        /// <returns>the root baseNode of the tree or null if the tree can not be created</returns>
        public override SystemNodeViewModel CreateTree(IEnumerable<ElementBase> productTreeElements, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            var treeElements = productTreeElements.ToList();

            if (treeElements.Any() && selectedOption != null && selectedActualFiniteStates != null)
            {
                var topElement = treeElements.First();

                this.RootViewModel = new SystemNodeViewModel(topElement.Name)
                {
                    OnSelect = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement)
                };

                this.CreateTreeRecursively(topElement, this.RootViewModel, null, selectedOption, selectedActualFiniteStates);
                this.RootViewModel.OrderAllDescendantsByShortName();
                return this.RootViewModel;
            }

            return null;
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase">the element base used in the baseNode</param>
        /// <param name="current">the current baseNode</param>
        /// <param name="parent">the parent of the current baseNode. Null if the current baseNode is the root baseNode</param>
        /// <param name="selectedOption">the selected <see cref="Option" /></param>
        /// <param name="selectedActualFiniteStates">the selected <see cref="ActualFiniteState"/></param>
        protected override void CreateTreeRecursively(ElementBase elementBase, SystemNodeViewModel current, SystemNodeViewModel parent, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            var childsOfElementBase = elementBase.QueryElementUsageChildrenFromElementBase();

            parent?.AddChild(current);

            foreach (var child in childsOfElementBase)
            {
                var nodeViewModel = new SystemNodeViewModel(child.Name)
                {
                    OnSelect = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement)
                };

                this.CreateTreeRecursively(child, nodeViewModel, current, selectedOption, selectedActualFiniteStates);
            }
        }

        /// <summary>
        /// set the selected <see cref="SystemNodeViewModel" />
        /// </summary>
        /// <param name="selectedNode">The selected <see cref="SystemNodeViewModel" /></param>
        /// <returns>A <see cref="Task" /></returns>
        public void SelectElement(SystemNodeViewModel selectedNode)
        {
            this.OnClick.InvokeAsync(selectedNode);
        }
    }
}
