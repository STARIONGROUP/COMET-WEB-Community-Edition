﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBabylonInterop.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Services.Interoperability
{
    using COMETwebapp.Model;
    
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The <see cref="IBabylonInterop"/> is used to call the babylon.js methods
    /// </summary>
    public interface IBabylonInterop
    {
        /// <summary>
        /// Initializes the canvas with the Babylon.js engine
        /// </summary>
        /// <param name="canvasReference">the reference to the canvas to initialize</param>
        /// <param name="addAxes">if the world should show axes or not</param>
        /// <returns></returns>
        Task InitCanvas(ElementReference canvasReference, bool addAxes);

        /// <summary>
        /// Adds a <see cref="SceneObject"/> in to the 3D Scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to add</param>
        /// <returns>an asynchronous task</returns>
        Task AddSceneObject(SceneObject sceneObject);

        /// <summary>
        /// Clear the <see cref="SceneObject"/> from scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to clear</param>
        /// <returns>an asynchronous task</returns>
        Task ClearSceneObject(SceneObject sceneObject);

        /// <summary>
        /// Clear all the <see cref="SceneObject"/> from scene
        /// </summary>
        /// <param name="sceneObjects">the <see cref="SceneObject"/> to clear</param>
        /// <returns>an asynchronous task</returns>
        Task ClearSceneObjects(IEnumerable<SceneObject> sceneObjects);

        /// <summary>
        /// Sets the visibility of a <see cref="SceneObject"/>
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to change the visibility of</param>
        /// <param name="visibility">the new visibility</param>
        /// <returns>an asynchronous task</returns>
        Task SetVisibility(SceneObject sceneObject, bool visibility);

        /// <summary>
        /// Regenerates the <see cref="SceneObject"/> updating the mesh on scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to regenerate</param>
        Task RegenerateMesh(SceneObject sceneObject);

        /// <summary>
        /// Tries to get the <see cref="SceneObject.ID"/> that's under the mouse cursor.
        /// </summary>
        /// <returns></returns>
        Task<Guid> GetPrimitiveIdUnderMouseAsync();
    }
}
