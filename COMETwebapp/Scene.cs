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
        /// Inits the canvas, the asociated resources and the render loop.
        /// </summary>
        public static void Init()
        {
            JSInterop.Invoke("InitCanvas");
        }

        /// <summary>
        /// Adds a primitive to the scene
        /// </summary>
        /// <param name="primitive">The primitive to add</param>
        public static void AddPrimitive(Primitive primitive)
        {
            AddPrimitive(primitive, Color.LightGray);
        }

        /// <summary>
        /// Adds a primitive to the scene with the specified color
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        /// <param name="color">the color of the primitive</param>
        public static void AddPrimitive(Primitive primitive, Color color)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            Vector3 colorVectorized = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            string jsonColor = JsonConvert.SerializeObject(colorVectorized, Formatting.Indented);

            primitivesCollection.Add(primitive.ID, primitive);
            JSInterop.Invoke("AddPrimitive", jsonPrimitive, jsonColor);
        }

        /// <summary>
        /// Adds a temporary primitive to the scene
        /// </summary>
        /// <param name="primitive">The primitive to add</param>
        public static void AddTemporaryPrimitive(Primitive primitive)
        {
            AddTemporaryPrimitive(primitive, Color.LightGray);
        }

        /// <summary>
        /// Adds a temporary primitive to the scene with the specified color
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        /// <param name="color">the color of the primitive</param>
        public static void AddTemporaryPrimitive(Primitive primitive, Color color)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            Vector3 colorVectorized = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            string jsonColor = JsonConvert.SerializeObject(colorVectorized, Formatting.Indented);

            primitivesCollection.Add(primitive.ID, primitive);
            JSInterop.Invoke("AddPrimitive", jsonPrimitive, jsonColor);
        }

        /// <summary>
        /// Clears the scene deleting the primitives that contains
        /// </summary>
        public static void ClearPrimitives()
        {
            foreach(var id in primitivesCollection.Keys)
            {
                JSInterop.Invoke("Dispose", id);
            }
        }

        /// <summary>
        /// Clears the scene deleting the temporary primitives
        /// </summary>
        public static void ClearTemporaryPrimitives()
        {
            foreach (var id in temporaryPrimitivesCollection.Keys)
            {
                JSInterop.Invoke("Dispose", id);
            }
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
            return GetEntityById(id);
        }

        /// <summary>
        /// Gets the primitive asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the entity</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        private static Primitive GetEntityById(string id)
        {
            if (!primitivesCollection.ContainsKey(id))
            {
                throw new ArgumentException("The specified Id dont exist in the scene");
            }

            return primitivesCollection[id];
        }

        /// <summary>
        /// Sets the info panel position in screen coordinates
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public static void SetInfoPanelPosition(int x, int y)
        {
            JSInterop.Invoke("SetPanelPosition", x, y);
        }

        /// <summary>
        /// Sets the info panel visibility
        /// </summary>
        /// <param name="visible">true if the panel must be visible, false otherwise</param>
        public static void SetInfoPanelVisibility(bool visible)
        {
            JSInterop.Invoke("SetPanelVisibility", visible);
        }

        /// <summary>
        /// Sets the info panel with the specified content
        /// </summary>
        /// <param name="info">The info that the panel must display</param>
        public static void SetInfoPanelContent(string info)
        {
            JSInterop.Invoke("SetPanelContent", info);
        }

        /// <summary>
        /// Tries to get the world coordinates of the specified coordinates. A ray is projected from the specified screen coordinates.
        /// If the ray don't collide with a mesh it is not posible to compute the world coordinates. <see cref="https://learnopengl.com/Getting-started/Coordinate-Systems"/>
        /// </summary>
        /// <param name="x">the x coordinate in screen coordinates</param>
        /// <param name="y">the y coordinate in screen coordinates</param>
        /// <param name="worldCoords">the computed world coordinates</param>
        /// <returns>True if the computation is successful, false otherwise</returns>
        public static async Task<Vector3?> TryGetWorldCoordinates(double x, double y)
        {
            var result = await JSInterop.Invoke<float[]>("TryGetWorldCoordinates", x, y);
            return new Vector3(result[0], result[1], result[2]);
        }
    }
}
