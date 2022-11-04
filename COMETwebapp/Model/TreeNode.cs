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
        /// Creates a new instance of the <see cref="TreeNode"/>
        /// </summary>
        /// <param name="name">Name of the <see cref="ElementUsage"/> asociated to this node</param>
        public TreeNode(string name)
        {
            this.Name = name;
        }
    }
}
