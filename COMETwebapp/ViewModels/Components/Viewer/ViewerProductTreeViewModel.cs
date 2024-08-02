// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerProductTreeViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.ViewModels.Components.Viewer
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Extensions;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Common;
    using COMETwebapp.ViewModels.Components.Shared;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the ViewerProductTree
    /// </summary>
    public class ViewerProductTreeViewModel : ProductTreeViewModel<ViewerNodeViewModel>, IHasLoading
    {
        /// <summary>
        /// Backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Creates a new instance of type <see cref="ViewerProductTreeViewModel" />
        /// </summary>
        public ViewerProductTreeViewModel(ISelectionMediator selectionMediator)
        {
            this.SelectionMediator = selectionMediator;
            var enumValues = Enum.GetValues(typeof(TreeFilter)).Cast<TreeFilter>();
            this.TreeFilters = enumValues.ToList();
            this.SelectedFilter = TreeFilter.ShowFullTree;

            this.SelectionMediator.OnModelSelectionChanged += this.OnModelSelectionChanged;
            this.Disposables.Add(this.WhenAnyValue(x => x.SearchText).Subscribe(_ => this.OnSearchFilterChange()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedFilter).Subscribe(_ => this.OnFilterChanged()));
        }

        /// <summary>
        /// Gets o sets the <see cref="SelectionMediator" />
        /// </summary>
        public ISelectionMediator SelectionMediator { get; private set; }

        /// <summary>
        /// Value asserting that the current object is loading
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="productTreeElements">the product tree elements</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedActualFiniteStates">the selected states</param>
        /// <returns>the root baseNode of the tree or null if the tree can not be created</returns>
        public override ViewerNodeViewModel CreateTree(IEnumerable<ElementBase> productTreeElements, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            this.IsLoading = true;
            var treeElements = productTreeElements.ToList();

            if (treeElements.Count == 0 || selectedOption == null || selectedActualFiniteStates == null)
            {
                this.IsLoading = false;
                return this.RootViewModel;
            }

            var topElement = treeElements.First();
            var states = selectedActualFiniteStates.ToList();

            var topSceneObject = SceneObject.Create(topElement, selectedOption, states);

            this.RootViewModel = new ViewerNodeViewModel(topSceneObject)
            {
                SelectionMediator = this.SelectionMediator
            };

            this.CreateTreeRecursively(topElement, this.RootViewModel, null, selectedOption, states);
            this.RootViewModel.OrderAllDescendantsByShortName();

            this.IsLoading = false;
            return this.RootViewModel;
        }

        /// <summary>
        /// Event for when the filter on the tree changes
        /// </summary>
        public override void OnFilterChanged()
        {
            var fullTree = this.RootViewModel?.GetFlatListOfDescendants(true);

            if (fullTree is null)
            {
                return;
            }

            if (this.SelectedFilter == TreeFilter.ShowNodesWithGeometry)
            {
                fullTree.ForEach(x => { x.IsDrawn = x.SceneObject.Primitive != null; });
            }
            else
            {
                fullTree.ForEach(x => x.IsDrawn = true);
            }
        }

        /// <summary>
        /// Event for when the text of the search filter is changing
        /// </summary>
        public override void OnSearchFilterChange()
        {
            var fullTree = this.RootViewModel?.GetFlatListOfDescendants(true);

            if (this.SearchText == string.Empty)
            {
                fullTree?.ForEach(x => x.IsDrawn = true);
            }
            else
            {
                fullTree?.ForEach(x => { x.IsDrawn = x.Title.Contains(this.SearchText, StringComparison.InvariantCultureIgnoreCase); });
            }
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase">the element base used in the baseNode</param>
        /// <param name="current">the current baseNode</param>
        /// <param name="parent">the parent of the current baseNode. Null if the current baseNode is the root baseNode</param>
        /// <param name="selectedOption">the selected <see cref="Option" /></param>
        /// <param name="selectedActualFiniteStates">the selected <see cref="ActualFiniteState" /></param>
        protected override void CreateTreeRecursively(ElementBase elementBase, ViewerNodeViewModel current, ViewerNodeViewModel parent, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            var childsOfElementBase = elementBase.QueryElementUsageChildrenFromElementBase();

            parent?.AddChild(current);

            var states = selectedActualFiniteStates.ToList();

            foreach (var child in childsOfElementBase)
            {
                var sceneObject = SceneObject.Create(child, selectedOption, states);

                if (sceneObject is null)
                {
                    continue;
                }

                var nodeViewModel = new ViewerNodeViewModel(sceneObject)
                {
                    SelectionMediator = this.SelectionMediator
                };

                this.CreateTreeRecursively(child, nodeViewModel, current, selectedOption, states);
            }
        }

        /// <summary>
        /// Callback for when a model has been selected
        /// </summary>
        /// <param name="sceneObject">the selected <see cref="SceneObject" /></param>
        private void OnModelSelectionChanged(SceneObject sceneObject)
        {
            if (this.RootViewModel is null)
            {
                return;
            }

            var treeNodes = this.RootViewModel.GetFlatListOfDescendants();
            treeNodes.ForEach(x => x.IsSelected = false);

            if (sceneObject == null)
            {
                return;
            }

            var node = treeNodes.FirstOrDefault(x => x.SceneObject == sceneObject);

            if (node is not null)
            {
                node.IsSelected = true;
            }
        }
    }
}
