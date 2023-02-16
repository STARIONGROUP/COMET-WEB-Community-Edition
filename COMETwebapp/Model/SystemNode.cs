// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemNode.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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
    using COMETwebapp.Components.SystemRepresentation;

    /// <summary>
    /// Represents the node of the tree in the <see cref="SystemTree"/>
    /// </summary>
    public class SystemNode
    {
        /// <summary>
        ///     The children of this <see cref="SystemNode"/>
        /// </summary>
        private List<SystemNode> Children { get; set; }

        /// <summary>
        ///     Gets or sets the title of this <see cref="SystemNode"/>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The parent of this <see cref="SystemNode"/>
        /// </summary>
        private SystemNode Parent { get; set; }

        /// <summary>
        ///     Creates a new instance of the <see cref="SystemNode"/>
        /// </summary>
        /// <param name="title">the <see cref="string"/> title of this node</param>
        public SystemNode(string title)
        {
            this.Children = new List<SystemNode>();
            this.Title = title;
        }

        /// <summary>
        /// Adds a child to this node
        /// </summary>
        /// <param name="node">the node to add</param>
        /// <returns>this node</returns>
        public SystemNode AddChild(SystemNode node)
        {
            if (node is not null)
            {
                node.Parent = this;
                this.Children.Add(node);
            }

            return this;
        }

        /// <summary>
        /// Gets the parent node of this <see cref="SystemNode"/>
        /// </summary>
        /// <returns>the parent node</returns>
        public SystemNode GetParentNode()
        {
            return this.Parent;
        }

        /// <summary>
        /// Gets the childrens of this <see cref="SystemNode"/>
        /// </summary>
        /// <returns>the childrens of the node</returns>
        public IReadOnlyList<SystemNode> GetChildren()
        {
            return this.Children.AsReadOnly();
        }
    }
}