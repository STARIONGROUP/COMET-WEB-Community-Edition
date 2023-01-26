// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Pages.Viewer
{
    using CDP4Common.EngineeringModelData;
    using COMETwebapp.Model;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    using Microsoft.AspNetCore.Components;
    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="COMETwebapp.Pages.Viewer.ViewerPage"/> component
    /// </summary>
    public class ViewerViewModel : ReactiveObject, IViewerViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        public ISessionAnchor SessionAnchor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Backing field for the <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Gets or sets the selected <see cref="Option"/>
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Backing field for the <see cref="RootNode"/>
        /// </summary>
        private TreeNode rootNode;

        /// <summary>
        /// Gets or sets the root of the <see cref="COMETwebapp.Components.Viewer.Canvas.ProductTree"/>
        /// </summary>
        public TreeNode RootNode
        {
            get => this.rootNode;
            set => this.RaiseAndSetIfChanged(ref this.rootNode, value);
        }

        /// <summary>
        /// Gets or sets the list of the available <see cref="Option"/>
        /// </summary>
        public List<Option> TotalOptions { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// List of the of <see cref="ActualFiniteStateList"/> 
        /// </summary>        
        public List<ActualFiniteStateList> ListActualFiniteStateLists { get; set; }

        /// <summary>
        /// Gets or sets the Selected <see cref="ActualFiniteState"/>
        /// </summary>
        public List<ActualFiniteState> SelectedActualFiniteStates { get; private set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ViewerViewModel"/>
        /// </summary>
        /// <param name="sessionAnchor">the <see cref="ISessionAnchor"/></param>
        public ViewerViewModel(ISessionAnchor sessionAnchor, ISelectionMediator selectionMediator)
        {
            if(sessionAnchor == null)
            {
                throw new ArgumentNullException("sessionAnchor");
            }

            if(selectionMediator == null)
            {
                throw new ArgumentNullException("selectionMediator");
            }

            this.SessionAnchor = sessionAnchor;
            this.SelectionMediator = selectionMediator;

            this.Elements = this.InitializeElements();

            var iteration = this.SessionAnchor?.OpenIteration;
            this.TotalOptions = iteration?.Option.OrderBy(o => o.Name).ToList();
            var defaultOption = this.SessionAnchor?.OpenIteration?.DefaultOption;
            this.SelectedOption = defaultOption != null ? defaultOption : this.TotalOptions?.First();
            this.ListActualFiniteStateLists = iteration?.ActualFiniteStateList?.ToList();
            this.SelectedActualFiniteStates = this.ListActualFiniteStateLists?.SelectMany(x => x.ActualState).Where(x => x.IsDefault).ToList();

            this.CreateTree(this.Elements);

            this.SessionAnchor.OnSessionRefreshed += (sender, args) =>
            {
                this.Elements = this.InitializeElements();
                this.CreateTree(this.Elements);
            };

            this.SelectionMediator.OnModelSelectionChanged += (sender, sceneObject) =>
            {
                var treeNodes = this.RootNode.GetFlatListOfDescendants();
                treeNodes.ForEach(x => x.IsSelected = false);
                if (sceneObject != null)
                {
                    var node = treeNodes.FirstOrDefault(x => x.SceneObject == sceneObject);
                    if (node is not null)
                    {
                        node.IsSelected = true;
                    }
                }
            };

            this.WhenAnyValue(x => x.SelectedOption).Subscribe(o => this.OnOptionChange(o));
        }

        /// <summary>
        /// Initialize <see cref="ElementBase"> list
        /// </summary>
        private List<ElementBase> InitializeElements()
        {
            var elements = new List<ElementBase>();
            var iteration = this.SessionAnchor?.OpenIteration;

            if (iteration != null)
            {
                if (iteration.TopElement != null)
                {
                    elements.Add(iteration.TopElement);
                }

                iteration.Element.ForEach(e => elements.AddRange(e.ContainedElement));
            }

            return elements;
        }

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="productTreeElements">the product tree elements</param>
        private void CreateTree(List<ElementBase> productTreeElements)
        {
            var topElement = productTreeElements.First();
            var topSceneObject = SceneObject.Create(topElement, this.SelectedOption, this.SelectedActualFiniteStates);
            this.RootNode = new TreeNode(topSceneObject);
            this.CreateTreeRecursively(topElement, this.RootNode, null);
            this.RootNode.OrderAllDescendantsByShortName();
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase"></param>
        /// <param name="current"></param>
        /// <param name="parent"></param>
        private void CreateTreeRecursively(ElementBase elementBase, TreeNode current, TreeNode parent)
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
                    var sceneObject = SceneObject.Create(child, this.SelectedOption, this.SelectedActualFiniteStates);
                    if (sceneObject is not null)
                    {
                        this.CreateTreeRecursively(child, new TreeNode(sceneObject), current);
                    }
                }
            }
        }

        /// <summary>
        /// Event for when the selected <see cref="Option"/> has changed
        /// </summary>
        /// <param name="option">the new selected option</param>
        public void OnOptionChange(Option option)
        {
            var defaultOption = this.SessionAnchor?.OpenIteration?.DefaultOption;
            this.SelectedOption = this.TotalOptions?.FirstOrDefault(x => x == option, defaultOption);
            this.Elements = this.InitializeElements();
            this.CreateTree(this.Elements);
        }

        /// <summary>
        /// Event raised when an actual finite state has changed
        /// </summary>
        /// <param name="selectedActiveFiniteStates"></param>
        public void ActualFiniteStateChanged(List<ActualFiniteState> selectedActiveFiniteStates)
        {
            this.SelectedActualFiniteStates = selectedActiveFiniteStates;
            this.Elements = this.InitializeElements();
            this.CreateTree(this.Elements);
        }
    }
}
