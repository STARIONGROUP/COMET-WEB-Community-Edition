// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BabylonInterop.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using Microsoft.JSInterop;

    using Newtonsoft.Json;

    /// <summary>
    /// Class used for calling the babylon.js methods
    /// </summary>
    public class BabylonInterop : InteroperabilityService, IBabylonInterop
    {
        /// <summary>
        /// Creates a new instance of type <see cref="BabylonInterop"/>
        /// </summary>
        /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
        public BabylonInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        /// <summary>
        /// Initializes the canvas with the Babylon.js engine
        /// </summary>
        /// <param name="canvasReference">the reference to the canvas to initialize</param>
        /// <param name="addAxes">if the world should show axes or not</param>
        /// <returns></returns>
        public async Task InitCanvas(ElementReference canvasReference, bool addAxes)
        {
            await this.JsRuntime.InvokeVoidAsync("InitCanvas", canvasReference, addAxes);
        }

        /// <summary>
        /// Adds a <see cref="SceneObject"/> in to the 3D Scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to add</param>
        /// <returns>an asynchronous task</returns>
        public async Task AddSceneObject(SceneObject sceneObject)
        {
            if (sceneObject is null)
            {
                throw  new ArgumentNullException(nameof(sceneObject));
            }

            var sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            await this.JsRuntime.InvokeVoidAsync("AddSceneObject", sceneObjectJson);
        }

        /// <summary>
        /// Clear the <see cref="SceneObject"/> from scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to clear</param>
        /// <returns>an asynchronous task</returns>
        public async Task ClearSceneObject(SceneObject sceneObject)
        {
            if (sceneObject is null)
            {
                throw new ArgumentNullException(nameof(sceneObject));
            }

            await this.ClearSceneObjects(new List<SceneObject>() { sceneObject });
        }

        /// <summary>
        /// Clear all the <see cref="SceneObject"/> from scene
        /// </summary>
        /// <param name="sceneObjects">the <see cref="SceneObject"/> to clear</param>
        /// <returns>an asynchronous task</returns>
        public async Task ClearSceneObjects(IEnumerable<SceneObject> sceneObjects)
        {
            if (sceneObjects is null)
            {
                throw new ArgumentNullException(nameof(sceneObjects));
            }

            var ids = sceneObjects.Select(x => x.ID).ToList();
            await this.JsRuntime.InvokeVoidAsync("DisposeAll", ids.ToArray());
        }

        /// <summary>
        /// Sets the visibility of a <see cref="SceneObject"/>
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to change the visibility of</param>
        /// <param name="visibility">the new visibility</param>
        /// <returns>an asynchronous task</returns>
        public async Task SetVisibility(SceneObject sceneObject, bool visibility)
        {
            if (sceneObject is null)
            {
                throw new ArgumentNullException(nameof(sceneObject));
            }

            await this.JsRuntime.InvokeVoidAsync("SetMeshVisibility", sceneObject.ID, visibility);
        }

        /// <summary>
        /// Regenerates the <see cref="SceneObject"/> updating the mesh on scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject"/> to regenerate</param>
        public async Task RegenerateMesh(SceneObject sceneObject)
        {
            var sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            await this.JsRuntime.InvokeVoidAsync("RegenMesh", sceneObjectJson);
        }

        /// <summary>
        /// Tries to get the <see cref="SceneObject.ID"/> that's under the mouse cursor.
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> GetPrimitiveIdUnderMouseAsync()
        {
            var id = await this.JsRuntime.InvokeAsync<string>("GetPrimitiveIDUnderMouse");
            
            if (id == null || !Guid.TryParse(id, out var ID))
            {
                return Guid.Empty;
            }

            return ID;
        }
    }
}
