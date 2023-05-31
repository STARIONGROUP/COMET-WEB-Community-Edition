// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseNode.cs" company="RHEA System S.A.">
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
    /// Represents a node in a tree structure
    /// </summary>
    public abstract class BaseNode
    {
        /// <summary>
        /// Gets or sets the title of this node
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The children of this node
        /// </summary>
        protected List<BaseNode> Children { get; set; }

        /// <summary>
        /// The parent of this node
        /// </summary>
        protected BaseNode Parent { get; set; }

        /// <summary>
        /// Creates a new instance of the BaseNode
        /// </summary>
        /// <param name="title">the title of this node</param>
        protected BaseNode(string title)
        {
            Title = title;
            Children = new List<BaseNode>();
        }

        /// <summary>
        /// Adds a child to this node
        /// </summary>
        /// <param name="node">the node to add</param>
        /// <returns>this node</returns>
        public BaseNode AddChild(BaseNode node)
        {
            if (node is not null)
            {
                node.Parent = this;
                Children.Add(node);
            }

            return this;
        }

        /// <summary>
        /// Removes a child from this node
        /// </summary>
        /// <param name="node">the node to remove</param>
        /// <returns>this node</returns>
        public BaseNode RemoveChild(BaseNode node)
        {
            if (node is not null)
            {
                node.Parent = null;
                this.Children.Remove(node);
            }

            return this;
        }

        /// <summary>
        /// Gets the parent node of this node
        /// </summary>
        /// <returns>the parent node</returns>
        public BaseNode GetParentNode()
        {
            return Parent;
        }

        /// <summary>
        /// Gets the <see cref="BaseNode"/> that is on top of the hierarchy
        /// </summary>
        /// <returns>the <see cref="BaseNode"/> or this node if the RootViewModel can't be computed</returns>
        public BaseNode GetRootNode()
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
        /// Gets the children of this node
        /// </summary>
        /// <returns>the children of the node</returns>
        public IReadOnlyList<BaseNode> GetChildren()
        {
            return Children.AsReadOnly();
        }
    }
}