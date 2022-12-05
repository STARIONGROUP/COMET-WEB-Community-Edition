// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SceneProvider.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.Viewer
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading.Tasks;

    using COMETwebapp;
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using Newtonsoft.Json;

    /// <summary>
    /// Class to access the resources of a Scene 
    /// </summary>
    public class SceneProvider : ISceneProvider
    {
        /// <summary>
        /// Shape Kind parameter short name
        /// </summary>
        public const string ShapeKindShortName = "kind";

        /// <summary>
        /// Orientation parameter short name
        /// </summary>
        public const string OrientationShortName = "orientation";

        /// <summary>
        /// Position parameter short name
        /// </summary>
        public const string PositionShortName = "coord";

        /// <summary>
        /// Width parameter short name
        /// </summary>
        public const string WidthShortName = "wid_diameter";

        /// <summary>
        /// Diameter parameter short name
        /// </summary>
        public const string DiameterShortName = WidthShortName;

        /// <summary>
        /// Height parameter short name
        /// </summary>
        public const string HeightShortName = "h";

        /// <summary>
        /// Length parameter short name
        /// </summary>
        public const string LengthShortName = "l";

        /// <summary>
        /// Thickness parameter short name
        /// </summary>
        public const string ThicknessShortName = "thickn";

        /// <summary>
        /// Color parameter short name
        /// </summary>
        public const string ColorShortName = "color";

        /// <summary>
        /// Color parameter short name
        /// </summary>
        public const string TransparencyShortName = "alpha";

        /// <summary>
        /// Collection of the <see cref="Primitive"/> in the Scene
        /// </summary>
        private static Dictionary<Guid, Primitive> primitivesCollection = new Dictionary<Guid, Primitive>();           

        /// <summary>
        /// Collection of temporary <see cref="Primitive"/> in the Scene
        /// </summary>
        private static Dictionary<Guid, Primitive> TemporaryPrimitivesCollection = new Dictionary<Guid, Primitive>();

        /// <summary>
        /// Event for when selection has changed;
        /// </summary>
        public event EventHandler<OnSelectionChangedEventArgs> OnSelectionChanged;

        /// <summary>
        /// The primitive that is currently selected
        /// </summary>
        public Primitive? SelectedPrimitive { get; set; }

        /// <summary>
        /// Creates a new instance of class <see cref="SceneProvider"/>
        /// </summary>
        public SceneProvider(IJSRuntime JsRuntime)
        {
            JSInterop.JsRuntime = JsRuntime;
            this.SelectedPrimitive = null;
        }

        /// <summary>
        /// Raise the <see cref="OnSelectionChanged"/> event 
        /// </summary>
        /// <param name="primitive">The <see cref="Primitive"/> that triggers the event</param>
        public void RaiseSelectionChanged(Primitive? primitive)
        {
            OnSelectionChanged?.Invoke(this, new OnSelectionChangedEventArgs(primitive));
        }

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        public void InitCanvas(ElementReference canvas)
        {
            JSInterop.Invoke("InitCanvas", canvas);
        }

        /// <summary>
        /// Create the world axes and adds them to the scene
        /// </summary>
        public void AddWorldAxes()
        {
            float size = 700;
            Line xAxis = new Line(-size, 0, 0, size, 0, 0);
            xAxis.SetColor(255, 0, 0);
            this.AddPrimitive(xAxis);

            Line yAxis = new Line(0, -size, 0, 0, size, 0);
            yAxis.SetColor(0, 255, 0);
            this.AddPrimitive(yAxis);

            Line zAxis = new Line(0, 0, -size, 0, 0, size);
            zAxis.SetColor(0, 0, 255);
            this.AddPrimitive(zAxis);
        }

        /// <summary>
        /// Get the canvas that contains the scene size
        /// </summary>
        /// <returns>The canvas size</returns>
        public async Task<Vector2> GetCanvasSize()
        {
            var result = await JSInterop.Invoke<float[]>("GetCanvasSize");
            if (result != null && result.Length == 2)
            {
                return new Vector2(result[0], result[1]);
            }
            return new Vector2();
        }

        /// <summary>
        /// Gets a copy of the current primitives on the scene.
        /// </summary>
        /// <returns>The list of primitives</returns>
        public List<Primitive> GetPrimitives()
        {
            return primitivesCollection.Values.ToList();
        }

        /// <summary>
        /// Adds a primitive to the scene
        /// </summary>
        /// <param name="primitive">The primitive to add</param>
        public void AddPrimitive(Primitive primitive)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            primitivesCollection.Add(primitive.ID, primitive);
            JSInterop.Invoke("AddPrimitive", jsonPrimitive);
        }

        /// <summary>
        /// Adds a temporary primitive to the scene
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        public void AddTemporaryPrimitive(Primitive primitive)
        {
            primitive.HasHalo = true;
            primitive.Alpha = 0.7;

            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            TemporaryPrimitivesCollection.Add(primitive.ID, primitive);
            JSInterop.Invoke("AddPrimitive", jsonPrimitive);
        }

        /// <summary>
        /// Clears the scene deleting the primitives that contains
        /// </summary>
        public void ClearPrimitives()
        {
            var keys = primitivesCollection.Keys.ToList();
            foreach (var id in keys)
            {
                JSInterop.Invoke("Dispose", id);
            }
            primitivesCollection.Clear();
        }

        /// <summary>
        /// Clears the scene deleting the temporary primitives that contains
        /// </summary>
        public void ClearTemporaryPrimitives()
        {
            var keys = TemporaryPrimitivesCollection.Keys.ToList();
            foreach (var id in keys)
            {
                JSInterop.Invoke("Dispose", id);
            }
            TemporaryPrimitivesCollection.Clear();
        }

        /// <summary>
        /// Gets the primitive under the mouse cursor asyncronously
        /// </summary>
        /// <returns>The primitive under the mouse cursor</returns>
        public async Task<Primitive> GetPrimitiveUnderMouseAsync()
        {
            var id = await JSInterop.Invoke<string>("GetPrimitiveIDUnderMouse");
            if (id == null || !Guid.TryParse(id, out Guid ID))
            {
                return null;
            }
            return GetPrimitiveById(ID);
        }

        /// <summary>
        /// Gets the primitive asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the entity</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        public Primitive GetPrimitiveById(Guid id)
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
        public void SetPrimitivePosition(Guid Id, double x, double y, double z)
        {
            JSInterop.Invoke("SetPrimitivePosition", Id.ToString(), x, y, z);
        }

        /// <summary>
        /// Sets the position of the primitive 
        /// </summary>
        /// <param name="primitive">the primitive to sets the position to</param>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        public void SetPrimitivePosition(Primitive primitive, double x, double y, double z)
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
        public void SetPrimitiveRotation(Guid Id, double rx, double ry, double rz)
        {
            JSInterop.Invoke("SetPrimitiveRotation", Id.ToString(), rx, ry, rz);
        }

        /// <summary>
        /// Sets the rotation of the primitive
        /// </summary>
        /// <param name="primitive">the primitive to sets the rotation to</param>
        /// <param name="rx">rotation around X axis</param>
        /// <param name="ry">rotation around Y axis</param>
        /// <param name="rz">rotation around Z axis</param>
        public void SetPrimitiveRotation(Primitive primitive, double rx, double ry, double rz)
        {
            SetPrimitiveRotation(primitive.ID, rx, ry, rz);
        }

        /// <summary>
        /// Sets the info panel position in screen coordinates
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public void SetInfoPanelPosition(int x, int y)
        {
            JSInterop.Invoke("SetPanelPosition", x, y);
        }

        /// <summary>
        /// Sets the info panel visibility
        /// </summary>
        /// <param name="visible">true if the panel must be visible, false otherwise</param>
        public void SetInfoPanelVisibility(bool visible)
        {
            JSInterop.Invoke("SetPanelVisibility", visible);
        }

        /// <summary>
        /// Sets the info panel with the specified content
        /// </summary>
        /// <param name="info">The info that the panel must display</param>
        public void SetInfoPanelContent(string info)
        {
            JSInterop.Invoke("SetPanelContent", info);
        }

        /// <summary>
        /// Tries to get the world coordinates from the specified screen coordinates. A ray is projected from the specified screen coordinates.
        /// If the ray don't collide with a mesh it is not posible to compute the world coordinates. <see cref="https://learnopengl.com/Getting-started/Coordinate-Systems"/>
        /// </summary>
        /// <param name="x">the x coordinate in screen coordinates</param>
        /// <param name="y">the y coordinate in screen coordinates</param>
        public async Task<Vector3?> GetWorldCoordinates(double x, double y)
        {
            var result = await JSInterop.Invoke<float[]>("GetWorldCoordinates", x, y);
            if (result != null && result.Length == 3)
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
        public async Task<Vector2?> GetScreenCoordinates(double x, double y, double z)
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
