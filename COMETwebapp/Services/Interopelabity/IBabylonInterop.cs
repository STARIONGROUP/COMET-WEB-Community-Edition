// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBabylonInterop.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Services.Interopelabity
{
    using COMETwebapp.Model;
    
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    /// <summary>
    /// Interface for the <see cref="BabylonInterop"/>
    /// </summary>
    public interface IBabylonInterop
    {
        /// <summary>
        /// Gets or sets the see <see cref="IJSRuntime"/>
        /// </summary>
        IJSRuntime JsRuntime { get; }

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
        /// Tries to get the <see cref="SceneObject.ID"/> that's under the mouse cursor.
        /// </summary>
        /// <returns></returns>
        Task<Guid> GetPrimitiveIdUnderMouseAsync();
    }
}
