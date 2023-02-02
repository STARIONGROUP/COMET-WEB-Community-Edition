// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Model
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Viewer.Canvas;

    /// <summary>
    /// Represents data of the tree in the <see cref="Pages.Viewer.Viewer"/>
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// The <see cref="SceneObject"/> that this <see cref="TreeNode"/> represents
        /// </summary>
        public SceneObject SceneObject { get; private set; }

        /// <summary>
        /// The parent of this <see cref="TreeNode"/>
        /// </summary>
        private TreeNode Parent { get; set; }

        /// <summary>
        /// The children of this <see cref="TreeNode"/>
        /// </summary>
        private List<TreeNode> Children { get; set; }

        /// <summary>
        /// Gets or sets the title of this <see cref="TreeNode"/>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TreeNode"/>
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> asociated to this node</param>
        public TreeNode(SceneObject sceneObject)
        {
            this.SceneObject = sceneObject;
            this.Children = new List<TreeNode>();
            this.Title = this.SceneObject?.ElementBase?.Name;
        }

        /// <summary>
        /// Adds a child to this node
        /// </summary>
        /// <param name="node">the node to add</param>
        /// <returns>this node</returns>
        public TreeNode AddChild(TreeNode node)
        {
            if(node is not null)
            {
                node.Parent = this;
                this.Children.Add(node);
            }

            return this;
        }

        /// <summary>
        /// Removes a child from this node
        /// </summary>
        /// <param name="node">the node to remove</param>
        /// <returns>this node</returns>
        public TreeNode RemoveChild(TreeNode node)
        {
            if(node is not null)
            {
                node.Parent = null;
                this.Children.Remove(node);
            }
            return this;
        }

        /// <summary>
        /// Gets the <see cref="TreeNode"/> that is on top of the hierarchy
        /// </summary>
        /// <returns>the <see cref="TreeNode"/> or this node if the RootViewModel can't be computed</returns>
        public TreeNode GetRootNode()
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
        public List<TreeNode> GetFlatListOfDescendants(bool includeSelf = false)
        {
            var descendants = new List<TreeNode>();
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
        private void GetListOfDescendantsRecursively(TreeNode current, ref List<TreeNode> descendants)
        {
            if (!descendants.Contains(current))
            {
                descendants.Add(current);
            }

            foreach(var child in current.Children)
            {
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
        private void OrderChildrenByShortNameHelper(TreeNode current)
        {
            current.Children = current.Children.OrderBy(x => x.Title).ToList();
            foreach (var child in current.Children)
            {
                this.OrderChildrenByShortNameHelper(child);
            }
        }

        /// <summary>
        /// Gets the parent node of this <see cref="TreeNode"/>
        /// </summary>
        /// <returns>the parent node</returns>
        public TreeNode GetParentNode()
        {
            return this.Parent;
        }

        /// <summary>
        /// Gets the children of this <see cref="TreeNode"/>
        /// </summary>
        /// <returns>the children of the node</returns>
        public IReadOnlyList<TreeNode> GetChildren()
        {
            return this.Children.AsReadOnly();
        }

        /// <summary>
        /// Overrides the equals method for equality checking
        /// </summary>
        /// <param name="obj">the object to check for equality</param>
        /// <returns>true if the objects are the same, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if(obj is not TreeNode treeNode)
            {
                return false;
            }

            if(this.SceneObject.ID == treeNode.SceneObject.ID)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the hashcode of this <see cref="TreeNode"/>
        /// </summary>
        /// <returns>the hashcode</returns>
        public override int GetHashCode()
        {
            return this.SceneObject.ID.GetHashCode();
        }
    }
}
