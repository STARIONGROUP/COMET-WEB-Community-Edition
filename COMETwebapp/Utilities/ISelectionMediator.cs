// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISelectionMediator.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Utilities
{
    using COMETwebapp.Model;

    /// <summary>
    /// Interface for controlling the selecetion of <see cref="SceneObject"/> in the Scene
    /// </summary>
    public interface ISelectionMediator
    {
        /// <summary> 
        /// Gets or sets if the current <see cref="SelectedSceneObject"/> has changes 
        /// </summary> 
        public bool SceneObjectHasChanges { get; set; }

        /// <summary> 
        /// Gets the current selected scene object 
        /// </summary> 
        public SceneObject SelectedSceneObject { get; }

        /// <summary> 
        /// Gets the current selected scene object clone 
        /// </summary> 
        public SceneObject SelectedSceneObjectClone { get; }

        /// <summary>
        /// Event for when the tree selection has changed
        /// </summary>
        event EventHandler<TreeNode> OnTreeSelectionChanged;

        /// <summary>
        /// Event for when a node in the tree has changed his visibility
        /// </summary>
        event EventHandler<TreeNode> OnTreeVisibilityChanged;

        /// <summary>
        /// Event for when the model selection has changed
        /// </summary>
        event EventHandler<SceneObject> OnModelSelectionChanged;

        /// <summary>
        /// Raises the <see cref="OnTreeSelectionChanged"/> event
        /// </summary>
        /// <param name="node">the node that raised the event</param>
        void RaiseOnTreeSelectionChanged(TreeNode node);

        /// <summary>
        /// Raises the <see cref="OnTreeVisibilityChanged"/> event
        /// </summary>
        /// <param name="node"></param>
        void RaiseOnTreeVisibilityChanged(TreeNode node);

        /// <summary>
        /// Raises the <see cref="OnModelSelectionChanged"/> event
        /// </summary>
        /// <param name="sceneObject"></param>
        void RaiseOnModelSelectionChanged(SceneObject sceneObject);
    }
}
