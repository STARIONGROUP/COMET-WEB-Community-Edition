// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationTreeViewModel.cs" company="Starion Group S.A.">
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
        /// Backing field for <see cref="ShowName" />.
        /// </summary>
        private bool showName = true;

        /// <summary>
        /// Backing field for <see cref="ShowOwner" />.
        /// </summary>
        private bool showOwner = true;

        /// <summary>
        /// Backing field for <see cref="ShowCategories" />.
        /// </summary>
        private bool showCategories = true;

        /// <summary>
        /// Backing field for <see cref="DraggedNode" />.
        /// </summary>
        private SystemNodeViewModel draggedNode;

        /// <summary>
        /// Backing field for <see cref="DragOverNode" />.
        /// </summary>
        private SystemNodeViewModel dragOverNode;

        /// <summary>
        /// Iids of the element nodes the user has expanded. Persisted on the (per-tab) tree view model so the
        /// product tree keeps its expansion state across tree rebuilds (e.g. an option/domain-change ApplyFilters
        /// or a tab switch); it is discarded only when the tab — and therefore this view model — is disposed.
        /// </summary>
        public HashSet<Guid> ExpandedElementIids { get; } = [];

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
        /// Gets or sets a value indicating whether nodes should display their <see cref="CDP4Common.CommonData.DefinedThing.Name" />
        /// (<c>true</c>) or <see cref="CDP4Common.CommonData.DefinedThing.ShortName" /> (<c>false</c>).
        /// </summary>
        public bool ShowName
        {
            get => this.showName;
            set => this.RaiseAndSetIfChanged(ref this.showName, value);
        }

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
        /// Gets or sets a value indicating whether category pills are shown on each tree node.
        /// </summary>
        public bool ShowCategories
        {
            get => this.showCategories;
            set => this.RaiseAndSetIfChanged(ref this.showCategories, value);
        }

        /// <summary>
        /// The <see cref="EventCallback" /> to call on baseNode selection
        /// </summary>
        public EventCallback<SystemNodeViewModel> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SystemNodeViewModel" /> currently being dragged, or <c>null</c> when
        /// no drag is in progress.
        /// </summary>
        public SystemNodeViewModel DraggedNode
        {
            get => this.draggedNode;
            set => this.RaiseAndSetIfChanged(ref this.draggedNode, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="SystemNodeViewModel" /> that the pointer is currently hovering over
        /// during a drag operation, or <c>null</c> when no valid drop target is highlighted.
        /// </summary>
        public SystemNodeViewModel DragOverNode
        {
            get => this.dragOverNode;
            set => this.RaiseAndSetIfChanged(ref this.dragOverNode, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{T}" /> invoked when the user drops a
        /// <see cref="SystemNodeViewModel" /> onto a valid target node. The tuple carries
        /// (<see cref="SystemNodeViewModel" /> From, <see cref="SystemNodeViewModel" /> To).
        /// </summary>
        public EventCallback<(SystemNodeViewModel From, SystemNodeViewModel To)> OnDrop { get; set; }

        /// <summary>
        /// Determines whether <paramref name="from" /> may be dropped onto <paramref name="to" />.
        /// The drop is rejected when either argument is <see langword="null" />, the two nodes are
        /// the same instance, either node's <see cref="COMETwebapp.ViewModels.Components.Shared.BaseNodeViewModel{T}.Thing" />
        /// is not an <see cref="ElementBase" />, or it would introduce a containment cycle. A drop creates a new
        /// <see cref="ElementUsage" /> of <paramref name="from" />'s <see cref="ElementDefinition" /> inside
        /// <paramref name="to" />'s <see cref="ElementDefinition" />, so the comparison is made by
        /// <see cref="ElementDefinition" /> (not by tree-node instance): the drop is refused when the target's
        /// definition is the dragged definition itself or is already (transitively) contained by it. This also
        /// catches dropping a definition onto a separate node that represents that same definition.
        /// </summary>
        /// <param name="from">The node being dragged.</param>
        /// <param name="to">The node being dropped onto.</param>
        /// <returns><see langword="true" /> if the drop is permitted; otherwise <see langword="false" />.</returns>
        public static bool CanDrop(SystemNodeViewModel from, SystemNodeViewModel to)
        {
            if (from is null || to is null || from == to)
            {
                return false;
            }

            if (from.Thing is not ElementBase || to.Thing is not ElementBase)
            {
                return false;
            }

            var toDefinition = to.Thing as ElementDefinition ?? (to.Thing as ElementUsage)?.ElementDefinition;

            if (toDefinition is null)
            {
                return false;
            }

            return from.GetFlatListOfDescendants(true)
                .Select(node => node.Thing as ElementDefinition ?? (node.Thing as ElementUsage)?.ElementDefinition)
                .All(definition => definition != toDefinition);
        }

        /// <summary>
        /// Sets <paramref name="node" />'s expansion state and records it in <see cref="ExpandedElementIids" />
        /// so a later tree rebuild can restore it.
        /// </summary>
        /// <param name="node">The node whose expansion is toggled.</param>
        /// <param name="expanded">The new expansion state.</param>
        public void SetNodeExpansion(SystemNodeViewModel node, bool expanded)
        {
            node.IsExpanded = expanded;

            if (node.Thing is null)
            {
                return;
            }

            if (expanded)
            {
                this.ExpandedElementIids.Add(node.Thing.Iid);
            }
            else
            {
                this.ExpandedElementIids.Remove(node.Thing.Iid);
            }
        }

        /// <summary>
        /// Restores the persisted expansion state onto a freshly-created <paramref name="node" />.
        /// </summary>
        /// <param name="node">The newly-created node to initialise.</param>
        private void ApplyPersistedExpansion(SystemNodeViewModel node)
        {
            if (node.Thing != null && this.ExpandedElementIids.Contains(node.Thing.Iid))
            {
                node.IsExpanded = true;
            }
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

            if (treeElements.Count == 0 || selectedOption == null || selectedActualFiniteStates == null)
            {
                return null;
            }

            var topElement = treeElements.First();

            this.RootViewModel = new SystemNodeViewModel(topElement)
            {
                OnSelect = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement)
            };

            this.ApplyPersistedExpansion(this.RootViewModel);
            this.CreateTreeRecursively(topElement, this.RootViewModel, null, selectedOption, selectedActualFiniteStates);
            this.RootViewModel.OrderAllDescendantsByShortName();
            return this.RootViewModel;
        }

        /// <summary>
        /// Adds a sequence of elements to the tree
        /// </summary>
        /// <param name="elementBases">A collection of element bases</param>
        /// <param name="option">The selected option</param>
        /// <param name="finiteState">The selected finite state</param>
        public void AddElementsToTree(IEnumerable<ElementBase> elementBases, Option option, List<ActualFiniteState> finiteState)
        {
            foreach (var elementBase in elementBases)
            {
                // Recomputed per element because the tree mutates as elements are attached. The container of
                // an ElementUsage is its ElementDefinition, which can be represented in the tree either as the
                // root (top element) node or as one-or-more usage nodes that reference it — so the new usage
                // must be attached under every matching node.
                var parentNodes = this.RootViewModel.GetFlatListOfDescendants(true)
                    .Where(x => x.Thing.Iid == elementBase.Container.Iid
                                || (x.Thing is ElementUsage usage && usage.ElementDefinition != null && usage.ElementDefinition.Iid == elementBase.Container.Iid))
                    .ToList();

                foreach (var parentNode in parentNodes)
                {
                    if (parentNode.GetChildren().Any(c => c.Thing != null && c.Thing.Iid == elementBase.Iid))
                    {
                        continue;
                    }

                    var nodeToAdd = new SystemNodeViewModel(elementBase)
                    {
                        OnSelect = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement)
                    };

                    this.ApplyPersistedExpansion(nodeToAdd);
                    this.CreateTreeRecursively(elementBase, nodeToAdd, parentNode, option, finiteState);
                }
            }
        }

        /// <summary>
        /// Removes a collection of elements from the tree
        /// </summary>
        /// <param name="elementBases">A collection of element bases</param>
        public void RemoveElementsFromTree(IEnumerable<ElementBase> elementBases)
        {
            foreach (var elementBase in elementBases)
            {
                // Recompute per element (removals mutate the tree) and remove every occurrence — a usage's
                // ElementDefinition can be represented by more than one node. Skip silently when the element
                // is not in the tree (e.g. it was filtered out by the current Option).
                var nodesToRemove = this.RootViewModel.GetFlatListOfDescendants(true)
                    .Where(x => x.Thing != null && x.Thing.Iid == elementBase.Iid)
                    .ToList();

                foreach (var nodeToRemove in nodesToRemove)
                {
                    nodeToRemove.Parent?.RemoveChild(nodeToRemove);
                }
            }
        }

        /// <summary>
        /// Updates a collection of elements from the tree
        /// </summary>
        /// <param name="elementBases">A collection of element bases</param>
        public void UpdateElementsFromTree(IEnumerable<ElementBase> elementBases)
        {
            var nodes = this.RootViewModel.GetFlatListOfDescendants(true);

            foreach (var elementBase in elementBases)
            {
                // Update every occurrence, and skip silently when the element is not currently in the tree
                // (e.g. it is excluded from the selected Option) — a missing node must never throw here, as
                // this runs inside the session End-Update notification and would fail the whole write.
                foreach (var nodeToUpdate in nodes.Where(x => x.Thing != null && x.Thing.Iid == elementBase.Iid))
                {
                    nodeToUpdate.SetThing(elementBase);
                }
            }
        }

        /// <summary>
        /// Applies the current <see cref="COMETwebapp.ViewModels.Components.Shared.ProductTreeViewModel{T}.SearchText" /> as a
        /// visibility filter across the tree. Each node's <see cref="COMETwebapp.ViewModels.Components.Shared.BaseNodeViewModel{T}.IsDrawn" />
        /// is set to <c>true</c> when the node itself — or any of its descendants — matches the search term
        /// (case-insensitive, against both Name and ShortName). Ancestor branches whose descendants match are
        /// auto-expanded so matched nodes are immediately visible.
        /// </summary>
        public override void OnSearchFilterChange()
        {
            if (this.RootViewModel is null)
            {
                return;
            }

            this.ApplySearchFilter(this.RootViewModel, this.SearchText);
        }

        /// <summary>
        /// Recursively applies the search filter to <paramref name="node" /> and all its descendants,
        /// returning <c>true</c> when the node or any child matches.
        /// </summary>
        /// <param name="node">The <see cref="SystemNodeViewModel" /> to evaluate.</param>
        /// <param name="term">The search term from <see cref="COMETwebapp.ViewModels.Components.Shared.ProductTreeViewModel{T}.SearchText" />.</param>
        /// <returns>
        /// <c>true</c> when <paramref name="node" /> itself or at least one of its descendants matches
        /// <paramref name="term" />.
        /// </returns>
        private bool ApplySearchFilter(SystemNodeViewModel node, string term)
        {
            var hasTerm = !string.IsNullOrWhiteSpace(term);
            var self = !hasTerm || NodeMatches(node, term);
            var anyChild = false;

            foreach (var child in node.GetChildren())
            {
                anyChild |= this.ApplySearchFilter(child, term);
            }

            node.IsDrawn = self || anyChild;

            if (hasTerm && anyChild)
            {
                node.IsExpanded = true;
            }

            return self || anyChild;
        }

        /// <summary>
        /// Returns <c>true</c> when the Name or ShortName of <paramref name="node" />'s underlying
        /// <see cref="CDP4Common.CommonData.DefinedThing" /> contains <paramref name="term" />
        /// (case-insensitive). Falls back to <see cref="COMETwebapp.ViewModels.Components.Shared.BaseNodeViewModel{T}.Title" />
        /// for nodes whose <c>Thing</c> is not a <see cref="CDP4Common.CommonData.DefinedThing" />.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="term">The search term.</param>
        /// <returns><c>true</c> when a match is found; otherwise <c>false</c>.</returns>
        private static bool NodeMatches(SystemNodeViewModel node, string term)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;

            if (node.Thing is not ElementBase elementBase)
            {
                return node.Title?.Contains(term, comparison) ?? false;
            }

            if ((elementBase.Name?.Contains(term, comparison) ?? false)
                || (elementBase.ShortName?.Contains(term, comparison) ?? false))
            {
                return true;
            }

            if (elementBase.Owner is not null
                && ((elementBase.Owner.ShortName?.Contains(term, comparison) ?? false)
                    || (elementBase.Owner.Name?.Contains(term, comparison) ?? false)))
            {
                return true;
            }

            return elementBase.GetDisplayCategories().Any(category => (category.ShortName?.Contains(term, comparison) ?? false)
                                              || (category.Name?.Contains(term, comparison) ?? false));
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

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase">the element base used in the baseNode</param>
        /// <param name="current">the current baseNode</param>
        /// <param name="parent">the parent of the current baseNode. Null if the current baseNode is the root baseNode</param>
        /// <param name="selectedOption">the selected <see cref="Option" /></param>
        /// <param name="selectedActualFiniteStates">the selected <see cref="ActualFiniteState" /></param>
        protected override void CreateTreeRecursively(ElementBase elementBase, SystemNodeViewModel current, SystemNodeViewModel parent, Option selectedOption, IEnumerable<ActualFiniteState> selectedActualFiniteStates)
        {
            var childsOfElementBase = elementBase.QueryElementUsageChildrenFromElementBase()
                .Where(child => selectedOption is null
                                || child is not ElementUsage usage
                                || usage.ExcludeOption.All(o => o.Iid != selectedOption.Iid));

            parent?.AddChild(current);

            foreach (var child in childsOfElementBase)
            {
                var nodeViewModel = new SystemNodeViewModel(child)
                {
                    OnSelect = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement)
                };

                this.ApplyPersistedExpansion(nodeViewModel);
                this.CreateTreeRecursively(child, nodeViewModel, current, selectedOption, selectedActualFiniteStates);
            }
        }
    }
}
