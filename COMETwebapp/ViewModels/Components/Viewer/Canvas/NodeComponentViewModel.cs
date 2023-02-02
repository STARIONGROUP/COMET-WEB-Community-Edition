// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NodeComponentViewModel.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.Canvas.NodeComponent"/>
    /// </summary>
    public class NodeComponentViewModel : ReactiveObject, INodeComponentViewModel
    {
        /// <summary>
        /// Gets or set the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Level of the tree. Increases by one for each nested element
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Current node that this <see cref="NodeComponentViewModel" /> represents
        /// </summary>
        public TreeNode Node { get; set; }

        /// <summary>
        /// Backing field for the <see cref="IsExpanded"/>
        /// </summary>
        private bool isExpanded = true;

        /// <summary>
        /// Gets or sets if the <see cref="Node"/> is expanded
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isExpanded, value);
            }
        }

        /// <summary>
        /// Backing field for the <see cref="IsDrawn"/>
        /// </summary>
        private bool isDrawn = true;

        /// <summary>
        /// Gets or sets if the <see cref="Node"/> is drawn
        /// </summary>
        public bool IsDrawn
        {
            get => this.isDrawn;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isDrawn, value);
            }
        }

        /// <summary>
        /// Backing field for the <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Gets or sets if the <see cref="Node"/> is selected
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isSelected, value);
            }
        }

        /// <summary>
        /// Backing field for the <see cref="IsSceneObjectVisible"/>
        /// </summary>
        private bool isSceneObjectVisible = true;

        /// <summary>
        /// Gets or sets if the <see cref="SceneObject"/> asociated to the <see cref="Node"/> is visible
        /// </summary>
        public bool IsSceneObjectVisible
        {
            get => this.isSceneObjectVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isSceneObjectVisible, value);
            }
        }

        /// <summary> 
        /// Gets or sets if the propagation of the click event should be stopped  
        /// </summary> 
        private bool StopClickPropagation { get; set; }

        /// <summary>
        /// Gets or sets the parent of this <see cref="NodeComponentViewModel"/>
        /// </summary>
        public INodeComponentViewModel Parent { get; set; }

        /// <summary>
        /// Field for containing the children of this <see cref="INodeComponentViewModel"/>
        /// </summary>
        public List<INodeComponentViewModel> Children { get; set; } = new();

        /// <summary>
        /// Creates a new instance of type <see cref="NodeComponentViewModel"/>
        /// </summary>
        /// <param name="node">the <see cref="TreeNode"/></param>
        /// <param name="selectionMediator">the <see cref="ISelectionMediator"/></param>
        public NodeComponentViewModel(TreeNode node, ISelectionMediator selectionMediator)
        {
            this.Node = node;
            this.SelectionMediator = selectionMediator;
        }

        /// <summary>
        /// Adds a <see cref="INodeComponentViewModel"/> as a child of this 
        /// </summary>
        /// <param name="nodeViewModel">the <see cref="INodeComponentViewModel"/> to add</param>
        /// <returns>this <see cref="INodeComponentViewModel"/></returns>
        public INodeComponentViewModel AddChild(INodeComponentViewModel nodeViewModel)
        {
            if(nodeViewModel is not null)
            {
                nodeViewModel.Parent = this;
                this.Children.Add(nodeViewModel);

                this.Node.AddChild(nodeViewModel.Node);
            }

            return this;
        }

        /// <summary>
        /// Removes a <see cref="INodeComponentViewModel"/> as a child of this 
        /// </summary>
        /// <param name="nodeViewModel">the <see cref="INodeComponentViewModel"/> to remove</param>
        /// <returns>this <see cref="INodeComponentViewModel"/></returns>
        public INodeComponentViewModel RemoveChild(INodeComponentViewModel nodeViewModel)
        {
            if (nodeViewModel is not null)
            {
                nodeViewModel.Parent = null;
                this.Children.Remove(nodeViewModel);

                this.Node.RemoveChild(nodeViewModel.Node);
            }

            return this;
        }

        /// <summary>
        /// Gets the <see cref="TreeNode"/> that is on top of the hierarchy
        /// </summary>
        /// <returns>the <see cref="TreeNode"/> or this node if the RootViewModel can't be computed</returns>
        public INodeComponentViewModel GetRootNode()
        {
            var currentParent = this.Parent;

            while (currentParent != null)
            {
                if (currentParent.Parent is null)
                {
                    return currentParent;
                }

                currentParent = currentParent.Parent;
            }

            return this;
        }

        /// <summary>
        /// Gets a flat list of the descendants of this node
        /// </summary>
        /// <returns>the flat list</returns>
        public List<INodeComponentViewModel> GetFlatListOfDescendants(bool includeSelf = false)
        {
            var descendants = new List<INodeComponentViewModel>();
            this.GetListOfDescendantsRecursively(this, ref descendants);
            
            if (includeSelf && !descendants.Contains(this))
            {
                descendants.Add(this);
            }

            return descendants;
        }

        /// <summary>
        /// Helper method for <see cref="GetFlatListOfDescendants"/>
        /// </summary>
        /// <param name="current">the current evaluated <see cref="TreeNode"/></param>
        /// <param name="descendants">the list of descendants till this moment</param>
        private void GetListOfDescendantsRecursively(INodeComponentViewModel current, ref List<INodeComponentViewModel> descendants)
        {
            foreach (var child in current.GetChildren())
            {
                if (!descendants.Contains(child))
                {
                    descendants.Add(child);
                }

                this.GetListOfDescendantsRecursively(child, ref descendants);
            }
        }

        /// <summary>
        /// Sort all descendants of this node by the <see cref="Name"/>
        /// </summary>
        public void OrderAllDescendantsByShortName()
        {
            this.OrderChildrenByShortNameHelper(this);
        }

        /// <summary>
        /// Helper method for <see cref="OrderAllDescendantsByShortName"/>
        /// </summary>
        /// <param name="current">the current evaluated <see cref="TreeNode"/></param>
        private void OrderChildrenByShortNameHelper(INodeComponentViewModel current)
        {
            current.Children = current.GetChildren().OrderBy(x => x.Node.Title).ToList();
            
            foreach (var child in current.GetChildren())
            {
                this.OrderChildrenByShortNameHelper(child);
            }
        }

        /// <summary>
        /// Gets the parent node of this <see cref="TreeNode"/>
        /// </summary>
        /// <returns>the parent node</returns>
        public INodeComponentViewModel GetParentNode()
        {
            return this.Parent;
        }

        /// <summary>
        /// Gets the children of this <see cref="TreeNode"/>
        /// </summary>
        /// <returns>the children of the node</returns>
        public IReadOnlyList<INodeComponentViewModel> GetChildren()
        {
            return this.Children.AsReadOnly();
        }

        /// <summary>
        /// Method for when a node is selected
        /// </summary>
        /// <param name="node">the selected <see cref="TreeNode"/></param>
        public void TreeSelectionChanged(INodeComponentViewModel nodeViewModel)
        {
            this.GetRootNode().GetFlatListOfDescendants(true).ForEach(x => x.IsSelected = false);

            if (!this.StopClickPropagation)
            {
                this.SelectionMediator.RaiseOnTreeSelectionChanged(nodeViewModel);
                this.IsSelected = true;
            }

            this.StopClickPropagation = false;
        }

        /// <summary>
        /// Method for when a node visibility changed
        /// </summary>
        /// <param name="node">the selected <see cref="TreeNode"/></param>
        public void TreeNodeVisibilityChanged(INodeComponentViewModel nodeViewModel)
        {
            this.StopClickPropagation = true;
            this.SelectionMediator.RaiseOnTreeVisibilityChanged(nodeViewModel);
        }
    }
}
