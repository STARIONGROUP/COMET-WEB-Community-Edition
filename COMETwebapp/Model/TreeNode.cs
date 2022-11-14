// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

    /// <summary>
    /// Represents data of the tree in the <see cref="Pages.Viewer.Viewer"/>
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// If the node is expanded or not
        /// </summary>
        public bool IsExpanded { get; set; } = true;

        /// <summary>
        /// If the node is the current selected node
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// If the node is visible 
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// The name of the <see cref="ElementUsage"/> represented by the node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The parent of this <see cref="TreeNode"/>
        /// </summary>
        public TreeNode? Parent { get; set; }

        /// <summary>
        /// The children of this <see cref="TreeNode"/>
        /// </summary>
        public List<TreeNode> Children { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TreeNode"/>
        /// </summary>
        /// <param name="name">Name of the <see cref="ElementUsage"/> asociated to this node</param>
        public TreeNode(string name)
        {
            this.Name = name;
            this.Children = new List<TreeNode>();
        }

        /// <summary>
        /// Gets a flat list of the descendants of this node
        /// </summary>
        /// <returns>the flat list</returns>
        public List<TreeNode> GetFlatListOfDescendants()
        {
            var descendants = new List<TreeNode>();
            this.GetListOfDescendantsRecursively(this, ref descendants);
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
    }
}
