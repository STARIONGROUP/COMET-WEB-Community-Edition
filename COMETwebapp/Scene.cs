// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Scene.cs" company="RHEA System S.A.">
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

namespace COMETwebapp
{
    using System;
    using System.Drawing;
    using System.Numerics;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using COMETwebapp.Primitives;

    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Static class to access the resources of a Scene 
    /// </summary>
    public static class Scene
    {
        /// <summary>
        /// Collection of temporary <see cref="Primitive"/> in the Scene. 
        /// </summary>
        private static Dictionary<string, Primitive> temporaryPrimitivesCollection = new Dictionary<string, Primitive>();

        /// <summary>
        /// Collection of the <see cref="Primitive"/> in the Scene
        /// </summary>
        private static Dictionary<string, Primitive> primitivesCollection = new Dictionary<string, Primitive>();

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        public static async void InitCanvas(ElementReference canvas)
        {
            await JSInterop.Invoke("InitCanvas", canvas);
        }

        /// <summary>
        /// Get the canvas that contains the scene size
        /// </summary>
        /// <returns>The canvas size</returns>
        public static async Task<Vector2> GetCanvasSize()
        {
            var result = await JSInterop.Invoke <float[]>("GetCanvasSize");
            if(result != null && result.Length == 2)
            {
                return new Vector2(result[0], result[1]);
            }            
            return new Vector2 ();
        }

        /// <summary>
        /// Gets a copy of the current primitives on the scene.
        /// </summary>
        /// <returns>The list of primitives</returns>
        public static List<Primitive> GetPrimitives()
        {
            return primitivesCollection.Values.ToList();
        }

        /// <summary>
        /// Adds a primitive to the scene
        /// </summary>
        /// <param name="primitive">The primitive to add</param>
        public static async Task AddPrimitive(Primitive primitive)
        {
            await AddPrimitive(primitive, Color.LightGray);
        }

        /// <summary>
        /// Adds a primitive to the scene with the specified color
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        /// <param name="color">the color of the primitive</param>
        public static async Task AddPrimitive(Primitive primitive, Color color)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            Vector3 colorVectorized = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            string jsonColor = JsonConvert.SerializeObject(colorVectorized, Formatting.Indented);

            primitivesCollection.Add(primitive.ID, primitive);
            await JSInterop.Invoke("AddPrimitive", jsonPrimitive, jsonColor);
        }

        /// <summary>
        /// Adds a temporary primitive to the scene
        /// </summary>
        /// <param name="primitive">The primitive to add</param>
        public static async Task AddTemporaryPrimitive(Primitive primitive)
        {
            await AddTemporaryPrimitive(primitive, Color.LightGray);
        }

        /// <summary>
        /// Adds a temporary primitive to the scene with the specified color
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        /// <param name="color">the color of the primitive</param>
        public static async Task AddTemporaryPrimitive(Primitive primitive, Color color)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            Vector3 colorVectorized = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            string jsonColor = JsonConvert.SerializeObject(colorVectorized, Formatting.Indented);

            primitivesCollection.Add(primitive.ID, primitive);
            await JSInterop.Invoke("AddPrimitive", jsonPrimitive, jsonColor);
        }

        /// <summary>
        /// Clears the scene deleting the primitives that contains
        /// </summary>
        public static async Task ClearPrimitives()
        {
            var keys = primitivesCollection.Keys.ToList();
            foreach (var id in keys)
            {
                await JSInterop.Invoke("Dispose", id);
            }
            primitivesCollection.Clear();
        }

        /// <summary>
        /// Clears the scene deleting the temporary primitives
        /// </summary>
        public static async Task ClearTemporaryPrimitives()
        {
            var keys = primitivesCollection.Keys.ToList();
            foreach (var id in keys)
            {
                await JSInterop.Invoke("Dispose", id);
            }
            temporaryPrimitivesCollection.Clear();
        }

        /// <summary>
        /// Gets the primitive under the mouse cursor asyncronously
        /// </summary>
        /// <returns>The primitive under the mouse cursor</returns>
        public static async Task<Primitive> GetPrimitiveUnderMouseAsync()
        {
            var id = await JSInterop.Invoke<string>("GetPrimitiveIDUnderMouse");
            if(id == null)
            {
                return null;
            }
            return GetPrimitiveById(id);
        }

        /// <summary>
        /// Gets the primitive asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the entity</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        public static Primitive GetPrimitiveById(string id)
        {
            if (!primitivesCollection.ContainsKey(id))
            {
                throw new ArgumentException("The specified Id dont exist in the scene");
            }

            return primitivesCollection[id];
        }

        /// <summary>
        /// Sets the position of the primitive with the specified ID
        /// </summary>
        /// <param name="Id">the id of the primitive</param>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        public static async void SetPrimitivePosition(string Id, double x, double y, double z)
        {
            await JSInterop.Invoke("SetPrimitivePosition",Id, x, y, z);
        }

        /// <summary>
        /// Sets the position of the primitive 
        /// </summary>
        /// <param name="primitive">the primitive to sets the position to</param>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        public static void SetPrimitivePosition(Primitive primitive, double x, double y, double z)
        {
            SetPrimitivePosition(primitive.ID, x, y, z);
        }

        /// <summary>
        /// Sets the rotation of the primitive with the specified ID
        /// </summary>
        /// <param name="Id">the id of the primitive</param>
        /// <param name="rx">rotation around X axis</param>
        /// <param name="ry">rotation around Y axis</param>
        /// <param name="rz">rotation around Z axis</param>
        public static async void SetPrimitiveRotation(string Id, double rx, double ry, double rz)
        {
            await JSInterop.Invoke("SetPrimitiveRotation", Id, rx, ry, rz);
        }

        /// <summary>
        /// Sets the rotation of the primitive
        /// </summary>
        /// <param name="primitive">the primitive to sets the rotation to</param>
        /// <param name="rx">rotation around X axis</param>
        /// <param name="ry">rotation around Y axis</param>
        /// <param name="rz">rotation around Z axis</param>
        public static void SetPrimitiveRotation(Primitive primitive, double rx, double ry, double rz)
        {
            SetPrimitiveRotation(primitive.ID, rx, ry, rz);
        }

        /// <summary>
        /// Sets the info panel position in screen coordinates
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public static async void SetInfoPanelPosition(int x, int y)
        {
            await JSInterop.Invoke("SetPanelPosition", x, y);
        }

        /// <summary>
        /// Sets the info panel visibility
        /// </summary>
        /// <param name="visible">true if the panel must be visible, false otherwise</param>
        public static async void SetInfoPanelVisibility(bool visible)
        {
            await JSInterop.Invoke("SetPanelVisibility", visible);
        }

        /// <summary>
        /// Sets the info panel with the specified content
        /// </summary>
        /// <param name="info">The info that the panel must display</param>
        public static async void SetInfoPanelContent(string info)
        {
            await JSInterop.Invoke("SetPanelContent", info);
        }

        /// <summary>
        /// Tries to get the world coordinates from the specified screen coordinates. A ray is projected from the specified screen coordinates.
        /// If the ray don't collide with a mesh it is not posible to compute the world coordinates. <see cref="https://learnopengl.com/Getting-started/Coordinate-Systems"/>
        /// </summary>
        /// <param name="x">the x coordinate in screen coordinates</param>
        /// <param name="y">the y coordinate in screen coordinates</param>
        public static async Task<Vector3?> GetWorldCoordinates(double x, double y)
        {
            var result = await JSInterop.Invoke<float[]>("GetWorldCoordinates", x, y);
            if(result != null && result.Length == 3)
            {
                return new Vector3(result[0], result[1], result[2]);
            }

            return null;
        }

        /// <summary>
        /// Tries to get the screen coordinates from the specified world coordinates.
        /// </summary>
        /// <param name="x">the x coordinate in world coordinates</param>
        /// <param name="y">the y coordinate in world coordinates</param>
        /// <param name="z">the z coordinate in world coordinates</param>
        /// <returns></returns>
        public static async Task<Vector2?> GetScreenCoordinates(double x, double y, double z)
        {
            var result = await JSInterop.Invoke<float[]>("GetScreenCoordinates", x, y, z);
            if (result != null && result.Length == 2)
            {
                return new Vector2(result[0], result[1]);
            }

            return null;
        }
    }
}
