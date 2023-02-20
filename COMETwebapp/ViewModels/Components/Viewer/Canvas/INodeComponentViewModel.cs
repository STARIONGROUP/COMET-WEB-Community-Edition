// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="INodeComponentViewModel.cs" company="RHEA System S.A.">
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

using COMETwebapp.Model;

namespace COMETwebapp.ViewModels.Components.Viewer.Canvas
{
    /// <summary>
    /// ViewModel for that handle information related to <see cref="TreeNode"/> inside a tree
    /// </summary>
    public interface INodeComponentViewModel
    {
        /// <summary>
        /// Level of the tree. Increases by one for each nested element
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// Gets or sets the parent of this <see cref="NodeComponentViewModel"/>
        /// </summary>
        INodeComponentViewModel Parent { get; set; }

        /// <summary>
        /// Field for containing the children of this <see cref="INodeComponentViewModel"/>
        /// </summary>
        List<INodeComponentViewModel> Children { get; set; }

        /// <summary>
        /// Current node that this <see cref="INodeComponentViewModel" /> represents
        /// </summary>
        TreeNode Node { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="Node"/> is expanded
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="Node"/> is drawn
        /// </summary>
        bool IsDrawn { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="Node"/> is selected
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="SceneObject"/> asociated to the <see cref="Node"/> is visible
        /// </summary>
        bool IsSceneObjectVisible { get; set; }

        /// <summary>
        /// Adds a <see cref="INodeComponentViewModel"/> as a child of this 
        /// </summary>
        /// <param name="nodeViewModel">the <see cref="INodeComponentViewModel"/> to add</param>
        /// <returns>this <see cref="INodeComponentViewModel"/></returns>
        INodeComponentViewModel AddChild(INodeComponentViewModel nodeViewModel);

        /// <summary>
        /// Removes a <see cref="INodeComponentViewModel"/> as a child of this 
        /// </summary>
        /// <param name="nodeViewModel">the <see cref="INodeComponentViewModel"/> to remove</param>
        /// <returns>this <see cref="INodeComponentViewModel"/></returns>
        INodeComponentViewModel RemoveChild(INodeComponentViewModel nodeViewModel);

        /// <summary>
        /// Gets the <see cref="INodeComponentViewModel"/> that is on top of the hierarchy
        /// </summary>
        /// <returns>the <see cref="INodeComponentViewModel"/> or this node if the RootViewModel can't be computed</returns>
        INodeComponentViewModel GetRootNode();

        /// <summary>
        /// Gets a flat list of the descendants of this node
        /// </summary>
        /// <returns>the flat list</returns>
        List<INodeComponentViewModel> GetFlatListOfDescendants(bool includeSelf = false);

        /// <summary>
        /// Sort all descendants of this node by the <see cref="Name"/>
        /// </summary>
        void OrderAllDescendantsByShortName();

        /// <summary>
        /// Gets the parent node of this <see cref="INodeComponentViewModel"/>
        /// </summary>
        /// <returns>the parent node</returns>
        INodeComponentViewModel GetParentNode();

        /// <summary>
        /// Gets the children of this <see cref="INodeComponentViewModel"/>
        /// </summary>
        /// <returns>the children of the node</returns>
        IReadOnlyList<INodeComponentViewModel> GetChildren();

        /// <summary>
        /// Method for when a node is selected
        /// </summary>
        /// <param name="node">the selected <see cref="INodeComponentViewModel"/></param>
        void TreeSelectionChanged(INodeComponentViewModel node);

        /// <summary>
        /// Method for when a node visibility changed
        /// </summary>
        /// <param name="node">the selected <see cref="INodeComponentViewModel"/></param>
        void TreeNodeVisibilityChanged(INodeComponentViewModel node);
    }
}
