// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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
    /// <summary>
    /// Represents data of the tree in the <see cref="Pages.Viewer.ViewerPage"/>
    /// </summary>
    public class TreeNode : BaseNode
    {
        /// <summary>
        /// The <see cref="SceneObject"/> that this <see cref="TreeNode"/> represents
        /// </summary>
        public SceneObject SceneObject { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TreeNode"/>
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> asociated to this node</param>
        public TreeNode(SceneObject sceneObject) : base(sceneObject?.ElementBase?.Name)
        {
            this.SceneObject = sceneObject;
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
        /// Gets a flat list of the descendants of this node
        /// </summary>
        /// <returns>the flat list</returns>
        public List<BaseNode> GetFlatListOfDescendants(bool includeSelf = false)
        {
            var descendants = new List<BaseNode>();
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
        private void GetListOfDescendantsRecursively(TreeNode current, ref List<BaseNode> descendants)
        {
            foreach(var child in current.GetChildren())
            {
                if (!descendants.Contains(child))
                {
                    descendants.Add(child);
                }

                this.GetListOfDescendantsRecursively((TreeNode)child, ref descendants);
            }
        }

        /// <summary>
        /// Sort all descendants of this node by the <see cref="TreeNode.Name"/>
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
            current.Children = current.GetChildren().OrderBy(x => x.Title).ToList();

            foreach (var child in current.GetChildren())
            {
                this.OrderChildrenByShortNameHelper((TreeNode)child);
            }
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
