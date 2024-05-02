// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISelectionMediator.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using COMETwebapp.ViewModels.Components.Viewer;

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
        event Action<ViewerNodeViewModel> OnTreeSelectionChanged;

        /// <summary>
        /// Event for when a baseNode in the tree has changed his visibility
        /// </summary>
        event Action<ViewerNodeViewModel> OnTreeVisibilityChanged;

        /// <summary>
        /// Event for when the model selection has changed
        /// </summary>
        event Action<SceneObject> OnModelSelectionChanged;

        /// <summary>
        /// Raises the <see cref="OnTreeSelectionChanged"/> event
        /// </summary>
        /// <param name="baseNodeViewModel">the node that raised the event</param>
        void RaiseOnTreeSelectionChanged(ViewerNodeViewModel baseNodeViewModel);

        /// <summary>
        /// Raises the <see cref="OnTreeVisibilityChanged"/> event
        /// </summary>
        /// <param name="baseNodeViewModel">the node that raised the event</param>
        void RaiseOnTreeVisibilityChanged(ViewerNodeViewModel baseNodeViewModel);

        /// <summary>
        /// Raises the <see cref="OnModelSelectionChanged"/> event
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> that raised the event</param>
        void RaiseOnModelSelectionChanged(SceneObject sceneObject);
    }
}
