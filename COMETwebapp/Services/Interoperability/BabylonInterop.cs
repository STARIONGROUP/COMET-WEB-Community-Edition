// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BabylonInterop.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
    using Microsoft.JSInterop;

    using Newtonsoft.Json;

    /// <summary>
    /// Class used for calling the babylon.js methods
    /// </summary>
    public class BabylonInterop : InteroperabilityService, IBabylonInterop
    {
        /// <summary>
        /// JavaScript function name for loading Babylon.js scripts on demand
        /// </summary>
        public const string LoadBabylonScriptsFunction = "loadBabylonScripts";

        /// <summary>
        /// JavaScript function name for initializing the 3D canvas
        /// </summary>
        public const string InitCanvasFunction = "InitCanvas";

        /// <summary>
        /// JavaScript function name for adding a scene object to the 3D scene
        /// </summary>
        public const string AddSceneObjectFunction = "AddSceneObject";

        /// <summary>
        /// JavaScript function name for disposing scene objects from the 3D scene
        /// </summary>
        public const string DisposeAllFunction = "DisposeAll";

        /// <summary>
        /// JavaScript function name for setting mesh visibility
        /// </summary>
        public const string SetMeshVisibilityFunction = "SetMeshVisibility";

        /// <summary>
        /// JavaScript function name for regenerating a mesh
        /// </summary>
        public const string RegenMeshFunction = "RegenMesh";

        /// <summary>
        /// JavaScript function name for retrieving the primitive ID under the mouse cursor
        /// </summary>
        public const string GetPrimitiveIdUnderMouseFunction = "GetPrimitiveIDUnderMouse";

        /// <summary>
        /// Creates a new instance of type <see cref="BabylonInterop" />
        /// </summary>
        /// <param name="jsRuntime">the <see cref="IJSRuntime" /></param>
        public BabylonInterop(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        /// <summary>
        /// Initializes the canvas with the Babylon.js engine
        /// </summary>
        /// <param name="canvasReference">the reference to the canvas to initialize</param>
        /// <param name="addAxes">if the world should show axes or not</param>
        /// <returns>An asynchronous task representing the operation</returns>
        public async Task InitCanvas(ElementReference canvasReference, bool addAxes)
        {
            await this.EnsureScriptsLoadedAsync();
            await this.JsRuntime.InvokeVoidAsync(InitCanvasFunction, canvasReference, addAxes);
        }

        /// <summary>
        /// Adds a <see cref="SceneObject" /> in to the 3D Scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject" /> to add</param>
        /// <returns>an asynchronous task</returns>
        public async Task AddSceneObject(SceneObject sceneObject)
        {
            ArgumentNullException.ThrowIfNull(sceneObject);

            var sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            await this.JsRuntime.InvokeVoidAsync(AddSceneObjectFunction, sceneObjectJson);
        }

        /// <summary>
        /// Clear the <see cref="SceneObject" /> from scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject" /> to clear</param>
        /// <returns>an asynchronous task</returns>
        public async Task ClearSceneObject(SceneObject sceneObject)
        {
            ArgumentNullException.ThrowIfNull(sceneObject);

            await this.ClearSceneObjects(new List<SceneObject> { sceneObject });
        }

        /// <summary>
        /// Clear all the <see cref="SceneObject" /> from scene
        /// </summary>
        /// <param name="sceneObjects">the <see cref="SceneObject" /> to clear</param>
        /// <returns>an asynchronous task</returns>
        public async Task ClearSceneObjects(IEnumerable<SceneObject> sceneObjects)
        {
            ArgumentNullException.ThrowIfNull(sceneObjects);

            var ids = sceneObjects.Select(x => x.ID).ToList();

            if (ids.Count == 0)
            {
                return;
            }

            await this.JsRuntime.InvokeVoidAsync(DisposeAllFunction, ids);
        }

        /// <summary>
        /// Sets the visibility of a <see cref="SceneObject" />
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject" /> to change the visibility of</param>
        /// <param name="visibility">the new visibility</param>
        /// <returns>an asynchronous task</returns>
        public async Task SetVisibility(SceneObject sceneObject, bool visibility)
        {
            ArgumentNullException.ThrowIfNull(sceneObject);

            await this.JsRuntime.InvokeVoidAsync(SetMeshVisibilityFunction, sceneObject.ID, visibility);
        }

        /// <summary>
        /// Regenerates the <see cref="SceneObject" /> updating the mesh on scene
        /// </summary>
        /// <param name="sceneObject">the <see cref="SceneObject" /> to regenerate</param>
        public async Task RegenerateMesh(SceneObject sceneObject)
        {
            var sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            await this.JsRuntime.InvokeVoidAsync(RegenMeshFunction, sceneObjectJson);
        }

        /// <summary>
        /// Tries to get the <see cref="SceneObject.ID" /> that's under the mouse cursor.
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> GetPrimitiveIdUnderMouseAsync()
        {
            var id = await this.JsRuntime.InvokeAsync<string>(GetPrimitiveIdUnderMouseFunction);

            if (id == null || !Guid.TryParse(id, out var parsedId))
            {
                return Guid.Empty;
            }

            return parsedId;
        }

        /// <summary>
        /// Ensures that Babylon.js and associated scripts are dynamically loaded.
        /// </summary>
        /// <returns>An asynchronous task representing the operation</returns>
        private async Task EnsureScriptsLoadedAsync()
        {
            await this.JsRuntime.InvokeVoidAsync(LoadBabylonScriptsFunction);
        }
    }
}
