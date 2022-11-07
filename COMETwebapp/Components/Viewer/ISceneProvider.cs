// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISceneProvider.cs" company="RHEA System S.A.">
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
    using System.Drawing;
    using System.Numerics;

    using COMETwebapp.Model;
    using COMETwebapp.Primitives;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Scene provider
    /// </summary>
    public interface ISceneProvider
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
        /// Event for when the selection has changed
        /// </summary>
        event EventHandler<OnSelectionChangedEventArgs> OnSelectionChanged;

        /// <summary>
        /// The primitive that is currently selected
        /// </summary>
        Primitive SelectedPrimitive { get; set; }

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        void InitCanvas(ElementReference canvas);

        /// <summary>
        /// Create the world axes and adds them to the scene
        /// </summary>
        Task AddWorldAxes();

        /// <summary>
        /// Get the canvas that contains the scene size
        /// </summary>
        /// <returns>The canvas size</returns>
        Task<Vector2> GetCanvasSize();

        /// <summary>
        /// Gets a copy of the current primitives on the scene.
        /// </summary>
        /// <returns>The list of primitives</returns>
        List<Primitive> GetPrimitives();

        /// <summary>
        /// Adds a primitive to the scene
        /// </summary>
        /// <param name="primitive">The primitive to add</param>
        Task AddPrimitive(Primitive primitive);

        /// <summary>
        /// Adds a primitive to the scene with the specified color
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        /// <param name="color">the color of the primitive</param>
        Task AddPrimitive(Primitive primitive, Color color);

        /// <summary>
        /// Clears the scene deleting the primitives that contains
        /// </summary>
        Task ClearPrimitives();

        /// <summary>
        /// Gets the primitive under the mouse cursor asyncronously
        /// </summary>
        /// <returns>The primitive under the mouse cursor</returns>
        Task<Primitive> GetPrimitiveUnderMouseAsync();

        /// <summary>
        /// Gets the primitive asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the entity</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        Primitive GetPrimitiveById(Guid id);

        /// <summary>
        /// Sets the position of the primitive with the specified ID
        /// </summary>
        /// <param name="Id">the id of the primitive</param>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        void SetPrimitivePosition(Guid Id, double x, double y, double z);

        /// <summary>
        /// Sets the position of the primitive 
        /// </summary>
        /// <param name="primitive">the primitive to sets the position to</param>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        void SetPrimitivePosition(Primitive primitive, double x, double y, double z);

        /// <summary>
        /// Sets the rotation of the primitive with the specified ID
        /// </summary>
        /// <param name="Id">the id of the primitive</param>
        /// <param name="rx">rotation around X axis</param>
        /// <param name="ry">rotation around Y axis</param>
        /// <param name="rz">rotation around Z axis</param>
        void SetPrimitiveRotation(Guid Id, double rx, double ry, double rz);

        /// <summary>
        /// Sets the rotation of the primitive
        /// </summary>
        /// <param name="primitive">the primitive to sets the rotation to</param>
        /// <param name="rx">rotation around X axis</param>
        /// <param name="ry">rotation around Y axis</param>
        /// <param name="rz">rotation around Z axis</param>
        void SetPrimitiveRotation(Primitive primitive, double rx, double ry, double rz);

        /// <summary>
        /// Sets the info panel position in screen coordinates
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        void SetInfoPanelPosition(int x, int y);

        /// <summary>
        /// Sets the info panel visibility
        /// </summary>
        /// <param name="visible">true if the panel must be visible, false otherwise</param>
        void SetInfoPanelVisibility(bool visible);

        /// <summary>
        /// Sets the info panel with the specified content
        /// </summary>
        /// <param name="info">The info that the panel must display</param>
        void SetInfoPanelContent(string info);

        /// <summary>
        /// Tries to get the world coordinates from the specified screen coordinates. A ray is projected from the specified screen coordinates.
        /// If the ray don't collide with a mesh it is not posible to compute the world coordinates. <see cref="https://learnopengl.com/Getting-started/Coordinate-Systems"/>
        /// </summary>
        /// <param name="x">the x coordinate in screen coordinates</param>
        /// <param name="y">the y coordinate in screen coordinates</param>
        Task<Vector3?> GetWorldCoordinates(double x, double y);

        /// <summary>
        /// Tries to get the screen coordinates from the specified world coordinates.
        /// </summary>
        /// <param name="x">the x coordinate in world coordinates</param>
        /// <param name="y">the y coordinate in world coordinates</param>
        /// <param name="z">the z coordinate in world coordinates</param>
        /// <returns></returns>
        Task<Vector2?> GetScreenCoordinates(double x, double y, double z);

        /// <summary>
        /// Raise the <see cref="OnSelectionChanged"/> event 
        /// </summary>
        /// <param name="primitive">The <see cref="Primitive"/> that triggers the event</param>
        void RaiseSelectionChanged(Primitive primitive);
    }
}
