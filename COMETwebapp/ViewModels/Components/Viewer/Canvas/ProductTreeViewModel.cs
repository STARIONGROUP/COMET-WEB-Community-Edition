// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ProductTreeViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.Viewer.Canvas
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.Utilities.DisposableObject;

    using ReactiveUI;

    /// <summary>
    /// View Model for building the product tree
    /// </summary>
    public class ProductTreeViewModel : DisposableObject, IProductTreeViewModel
    {
        /// <summary>
        /// Gets o sets the <see cref="SelectionMediator"/>
        /// </summary>
        public ISelectionMediator SelectionMediator { get; private set; }

        /// <summary>
        /// Gets or sets the filter options for the tree
        /// </summary>
        public IReadOnlyList<TreeFilter> TreeFilters { get; private set; }

        /// <summary>
        /// Backing field for the <see cref="SelectedFilter"/>
        /// </summary>
        private TreeFilter selectedFilter;

        /// <summary>
        /// Gets or sets the selected filter option
        /// </summary>
        public TreeFilter SelectedFilter
        {
            get => this.selectedFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedFilter, value);
        }

        /// <summary>
        /// Backing field for the <see cref="RootViewModel"/>
        /// </summary>
        private INodeComponentViewModel rootViewModel;

        /// <summary>
        /// Gets or sets the root of the <see cref="COMETwebapp.Components.Viewer.Canvas.ProductTree"/>
        /// </summary>
        public INodeComponentViewModel RootViewModel
        {
            get => this.rootViewModel;
            set => this.RaiseAndSetIfChanged(ref this.rootViewModel, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SearchText"/>
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
        /// Creates a new instance of type <see cref="ProductTreeViewModel"/>
        /// </summary>
        public ProductTreeViewModel(ISelectionMediator selectionMediator)
        {
            this.SelectionMediator = selectionMediator;
            var enumValues = Enum.GetValues(typeof(TreeFilter)).Cast<TreeFilter>();
            this.TreeFilters = enumValues.ToList();
            this.SelectedFilter = TreeFilter.ShowFullTree;

            this.SelectionMediator.OnModelSelectionChanged += (sceneObject) =>
            {
                var treeNodes = this.RootViewModel.GetFlatListOfDescendants();
                treeNodes.ForEach(x => x.IsSelected = false);

                if (sceneObject != null)
                {
                    var node = treeNodes.FirstOrDefault(x => x.Node.SceneObject == sceneObject);

                    if (node is not null)
                    {
                        node.IsSelected = true;
                    }
                }
            };
            
            this.Disposables.Add(this.WhenAnyValue(x => x.SearchText).Subscribe(_ => this.OnSearchFilterChange()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedFilter).Subscribe(_ => this.OnFilterChanged()));
        }

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="productTreeElements">the product tree elements</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedActualFiniteStates">the selected states</param>
        /// <returns>the root node of the tree or null if the tree can not be created</returns>
        public INodeComponentViewModel CreateTree(IEnumerable<ElementBase> productTreeElements, Option selectedOption, 
            IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            var treeElements = productTreeElements.ToList();
            
            if (treeElements.Any() && selectedOption != null && selectedActualFiniteStates != null) 
            {
                var topElement = treeElements.First();
                var topSceneObject = SceneObject.Create(topElement, selectedOption, selectedActualFiniteStates.ToList());
                this.RootViewModel = new NodeComponentViewModel(new TreeNode(topSceneObject), this.SelectionMediator);
                this.CreateTreeRecursively(topElement, this.RootViewModel, null, selectedOption, selectedActualFiniteStates);
                this.RootViewModel.OrderAllDescendantsByShortName();
                return this.RootViewModel;
            }

            return null;
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase">the element base used in the node</param>
        /// <param name="current">the current node</param>
        /// <param name="parent">the parent of the current node. Null if the current node is the root node</param>
        /// <param name="selectedOption">the selected <see cref="Option"/></param>
        /// <param name="selectedActualFiniteStates">the selected <see cref="ActualFiniteState"/></param>
        private void CreateTreeRecursively(ElementBase elementBase, INodeComponentViewModel current, INodeComponentViewModel parent, 
            Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            List<ElementUsage> childsOfElementBase = null;

            if (elementBase is ElementDefinition elementDefinition)
            {
                childsOfElementBase = elementDefinition.ContainedElement;
            }
            else if (elementBase is ElementUsage elementUsage)
            {
                childsOfElementBase = elementUsage.ElementDefinition.ContainedElement;
            }

            if (childsOfElementBase is not null)
            {
                if (parent is not null)
                {
                    parent.AddChild(current);
                }

                foreach (var child in childsOfElementBase)
                {
                    var sceneObject = SceneObject.Create(child, selectedOption, selectedActualFiniteStates.ToList());

                    if (sceneObject is not null)
                    {
                        var nodeViewModel = new NodeComponentViewModel(new TreeNode(sceneObject), this.SelectionMediator);
                        this.CreateTreeRecursively(child, nodeViewModel, current, selectedOption, selectedActualFiniteStates);
                    }
                }
            }
        }

        /// <summary>
        /// Event for when the filter on the tree changes
        /// </summary>
        public void OnFilterChanged()
        {
            var fullTree = this.RootViewModel?.GetFlatListOfDescendants(true);

            if(fullTree is not null)
            {
                if(this.SelectedFilter == TreeFilter.ShowNodesWithGeometry)
                {
                    fullTree.ForEach(x => x.IsDrawn = x.Node.SceneObject.Primitive != null);
                }
                else
                {
                    fullTree.ForEach(x => x.IsDrawn = true);
                }
            }
        }

        /// <summary>
        /// Event for when the text of the search filter is changing
        /// </summary>
        public void OnSearchFilterChange()
        {
            var fullTree = this.RootViewModel?.GetFlatListOfDescendants(true);

            if (this.SearchText == string.Empty)
            {
                fullTree?.ForEach(x => x.IsDrawn = true);
            }
            else
            {
                fullTree?.ForEach(x =>
                {
                    x.IsDrawn = x.Node.Title.Contains(this.SearchText, StringComparison.InvariantCultureIgnoreCase);
                });
            }
        }
    }
}
