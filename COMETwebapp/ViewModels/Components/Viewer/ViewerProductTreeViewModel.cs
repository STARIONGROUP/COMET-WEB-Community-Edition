// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerProductTreeViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Viewer
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Extensions;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Shared;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the ViewerProductTree
    /// </summary>
    public class ViewerProductTreeViewModel : ProductTreeViewModel<ViewerNodeViewModel>
    {
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
            this.SelectionMediator.OnParameterSubmitted += this.OnParameterSubmitted;
            this.Disposables.Add(this.WhenAnyValue(x => x.SearchText).Subscribe(_ => this.OnSearchFilterChange()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedFilter).Subscribe(_ => this.OnFilterChanged()));
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ISelectionMediator" /> events and releases the resources used by this view model
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.SelectionMediator.OnModelSelectionChanged -= this.OnModelSelectionChanged;
                this.SelectionMediator.OnParameterSubmitted -= this.OnParameterSubmitted;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets o sets the <see cref="SelectionMediator" />
        /// </summary>
        public ISelectionMediator SelectionMediator { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tree only shows nodes that have a 3D geometry
        /// primitive.
        /// </summary>
        // One-way convenience wrapper over SelectedFilter for the "View" cog checkbox; the Viewer never mutates SelectedFilter elsewhere.
        public bool ShowOnlyNodesWithGeometry
        {
            get => this.SelectedFilter == TreeFilter.ShowNodesWithGeometry;
            set => this.SelectedFilter = value ? TreeFilter.ShowNodesWithGeometry : TreeFilter.ShowFullTree;
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
            var treeElements = productTreeElements.ToList();

            if (treeElements.Count == 0 || selectedOption == null || selectedActualFiniteStates == null)
            {
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

            this.OnFilterChanged();
            this.OnSearchFilterChange();

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
        /// Adds elements to the tree incrementally
        /// </summary>
        /// <param name="elementBases">The elements to add</param>
        /// <param name="option">The selected option</param>
        /// <param name="finiteStates">The finite states</param>
        /// <returns>A list of added <see cref="ViewerNodeViewModel" /></returns>
        public List<ViewerNodeViewModel> AddElementsToTree(IEnumerable<ElementBase> elementBases, Option option, List<ActualFiniteState> finiteStates)
        {
            List<ViewerNodeViewModel> addedNodes = [];

            if (this.RootViewModel == null)
            {
                return addedNodes;
            }

            foreach (var elementBase in elementBases)
            {
                var parentNodes = this.RootViewModel.GetFlatListOfDescendants(true)
                    .Where(x => x.SceneObject?.ElementBase != null &&
                                (x.SceneObject.ElementBase.Iid == elementBase.Container?.Iid
                                 || (x.SceneObject.ElementBase is ElementUsage { ElementDefinition: not null } usage && usage.ElementDefinition.Iid == elementBase.Container?.Iid)))
                    .Where(x => !x.GetChildren().Any(c => c.SceneObject?.ElementBase != null && c.SceneObject.ElementBase.Iid == elementBase.Iid))
                    .ToList();

                foreach (var parentNode in parentNodes)
                {
                    if (option != null && elementBase is ElementUsage usage && usage.ExcludeOption.Any(o => o.Iid == option.Iid))
                    {
                        continue;
                    }

                    var sceneObject = SceneObject.Create(elementBase, option, finiteStates);

                    if (sceneObject == null)
                    {
                        continue;
                    }

                    var nodeToAdd = new ViewerNodeViewModel(sceneObject)
                    {
                        SelectionMediator = this.SelectionMediator
                    };

                    this.CreateTreeRecursively(elementBase, nodeToAdd, parentNode, option, finiteStates);
                    addedNodes.Add(nodeToAdd);
                    addedNodes.AddRange(nodeToAdd.GetFlatListOfDescendants());
                }
            }

            return addedNodes;
        }

        /// <summary>
        /// Removes elements from the tree incrementally
        /// </summary>
        /// <param name="elementBases">The elements to remove</param>
        /// <returns>A list of removed <see cref="ViewerNodeViewModel" /></returns>
        public List<ViewerNodeViewModel> RemoveElementsFromTree(IEnumerable<ElementBase> elementBases)
        {
            List<ViewerNodeViewModel> removedNodes = [];

            if (this.RootViewModel == null)
            {
                return removedNodes;
            }

            foreach (var elementBase in elementBases)
            {
                var nodesToRemove = this.RootViewModel.GetFlatListOfDescendants(true)
                    .Where(x => x.SceneObject?.ElementBase != null && x.SceneObject.ElementBase.Iid == elementBase.Iid)
                    .ToList();

                foreach (var nodeToRemove in nodesToRemove)
                {
                    removedNodes.Add(nodeToRemove);
                    removedNodes.AddRange(nodeToRemove.GetFlatListOfDescendants());
                    nodeToRemove.Parent?.RemoveChild(nodeToRemove);
                }
            }

            return removedNodes;
        }

        /// <summary>
        /// Updates elements from the tree incrementally
        /// </summary>
        /// <param name="elementBases">The elements to update</param>
        /// <param name="option">The selected option</param>
        /// <param name="finiteStates">The finite states</param>
        /// <returns>A list of tuples containing the old and new <see cref="SceneObject" /></returns>
        public List<(SceneObject oldSceneObject, SceneObject newSceneObject)> UpdateElementsFromTree(IEnumerable<ElementBase> elementBases, Option option, List<ActualFiniteState> finiteStates)
        {
            List<(SceneObject, SceneObject)> updated = [];

            if (this.RootViewModel == null)
            {
                return updated;
            }

            var nodes = this.RootViewModel.GetFlatListOfDescendants(true);

            foreach (var elementBase in elementBases)
            {
                var matchingNodes = nodes.Where(x => x.SceneObject?.ElementBase != null && x.SceneObject.ElementBase.Iid == elementBase.Iid).ToList();

                foreach (var nodeToUpdate in matchingNodes)
                {
                    var parent = nodeToUpdate.Parent;

                    if (parent == null)
                    {
                        continue;
                    }

                    var oldSceneObject = nodeToUpdate.SceneObject;
                    
                    if (option != null && elementBase is ElementUsage usage && usage.ExcludeOption.Any(o => o.Iid == option.Iid))
                    {
                        continue;
                    }

                    var newSceneObject = SceneObject.Create(elementBase, option, finiteStates);

                    if (newSceneObject == null)
                    {
                        continue;
                    }

                    var newNode = new ViewerNodeViewModel(newSceneObject)
                    {
                        SelectionMediator = this.SelectionMediator,
                        IsExpanded = nodeToUpdate.IsExpanded,
                        IsSceneObjectVisible = nodeToUpdate.IsSceneObjectVisible,
                        IsSelected = nodeToUpdate.IsSelected
                    };

                    foreach (var child in nodeToUpdate.GetChildren().ToList())
                    {
                        nodeToUpdate.RemoveChild(child);
                        newNode.AddChild(child);
                    }

                    parent.RemoveChild(nodeToUpdate);
                    parent.AddChild(newNode);
                    updated.Add((oldSceneObject, newSceneObject));
                }
            }

            return updated;
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
                if (selectedOption != null && child.ExcludeOption.Any(o => o.Iid == selectedOption.Iid))
                {
                    continue;
                }

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

        /// <summary>
        /// Callback for when a parameter has been submitted
        /// </summary>
        private void OnParameterSubmitted()
        {
            if (this.RootViewModel is null || this.SelectionMediator.SelectedSceneObject is null)
            {
                return;
            }

            var node = this.RootViewModel.GetFlatListOfDescendants(true).FirstOrDefault(x => x.SceneObject == this.SelectionMediator.SelectedSceneObject);
            node?.UpdateSceneObjectProperty();
        }
    }
}
