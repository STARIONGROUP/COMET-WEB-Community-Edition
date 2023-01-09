// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BabylonCanvas.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.CanvasComponent
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Interoperability;
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;
    using DevExpress.Data.Mask.Internal;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;
    
    using Newtonsoft.Json;

    /// <summary>
    /// Support class for the <see cref="BabylonCanvas.razor"/>
    /// </summary>
    public partial class BabylonCanvas 
    {
        /// <summary>
        /// Reference to the HTML5 canvas
        /// </summary>
        public ElementReference CanvasReference { get; set; }

        /// <summary>
        /// Tells if the mouse if pressed or not in the canvas component
        /// </summary>
        public bool IsMouseDown { get; private set; } = false;

        /// <summary>
        /// Shape factory for creating <see cref="Primitive"/> from a <see cref="ElementUsage"/>
        /// </summary>
        [Inject]
        public IShapeFactory? ShapeFactory { get; set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        [Inject]
        public IJSInterop JSInterop { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Primitive"/> that is currently selected
        /// </summary>
        public Primitive SelectedPrimitive { get; set; }

        /// <summary>
        /// Collection of scene objects in the scene.
        /// </summary>
        private Dictionary<Guid, SceneObject> SceneObjects = new Dictionary<Guid, SceneObject>();

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
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {               
                this.InitCanvas(this.CanvasReference);
                await this.AddWorldAxes();
            }
        }
               
        /// <summary>
        /// Canvas on mouse down event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseDown(MouseEventArgs e)
        {
            this.IsMouseDown = true;
            //TODO: when the tools are ready here we are going to manage the different types of actions that a user can make.
        }

        /// <summary>
        /// Canvas on mouse up event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public async void OnMouseUp(MouseEventArgs e)
        {
            this.IsMouseDown = false;
            //TODO: when the tools are ready here we are going to manage the different types of actions that a user can make.
            await this.ClearTemporaryPrimitives();
            var primitive = await this.GetPrimitiveUnderMouseAsync();
            this.GetPrimitives().ForEach(async x => { x.IsSelected = false; await JSInterop.Invoke("SetSelection", x.ID, false); });
            this.SelectedPrimitive = null;

            if (primitive is not null)
            {
                primitive.IsSelected = true;
                await JSInterop.Invoke("SetSelection", primitive.ID, true);
                this.SelectedPrimitive = primitive;
            }
            this.RaiseSelectionChanged(primitive);
        }

        /// <summary>
        /// Clears the scene and populates again with the <see cref="ElementUsage"/> 
        /// </summary>
        /// <param name="elementUsages">the <see cref="ElementUsage"/> used for the population</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to position the <see cref="Primitive"/></param>
        public async void RepopulateScene(List<ElementUsage> elementUsages, Option selectedOption, List<ActualFiniteState> states)
        {
            await this.ClearPrimitives();

            foreach (var elementUsage in elementUsages)
            {
                var sceneObject = new SceneObject(this.ShapeFactory);
                sceneObject.CreatePrimitive(elementUsage, selectedOption, states);
                                

                var basicShape = this.ShapeFactory.CreatePrimitiveFromElementUsage(elementUsage, selectedOption, states);

                if (basicShape is not null)
                {
                    await this.AddPrimitive(basicShape);
                }
            }
        }

        /// <summary>
        /// Raise the <see cref="OnSelectionChanged"/> event 
        /// </summary>
        /// <param name="primitive">The <see cref="Primitive"/> that triggers the event</param>
        public void RaiseSelectionChanged(Primitive primitive)
        {
            OnSelectionChanged?.Invoke(this, new OnSelectionChangedEventArgs(primitive));
        }

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        public async void InitCanvas(ElementReference canvas)
        {
            await JSInterop.Invoke("InitCanvas", canvas);
        }

        /// <summary>
        /// Create the world axes and adds them to the scene
        /// </summary>
        public async Task AddWorldAxes()
        {
            float size = 700;
            Line xAxis = new Line(-size, 0, 0, size, 0, 0);
            xAxis.SetColor(255, 0, 0);
            await this.AddPrimitive(xAxis);

            Line yAxis = new Line(0, -size, 0, 0, size, 0);
            yAxis.SetColor(0, 255, 0);
            await this.AddPrimitive(yAxis);

            Line zAxis = new Line(0, 0, -size, 0, 0, size);
            zAxis.SetColor(0, 0, 255);
            await this.AddPrimitive(zAxis);
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
        public async Task AddPrimitive(Primitive primitive)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            primitivesCollection.Add(primitive.ID, primitive);
            await JSInterop.Invoke("AddPrimitive", jsonPrimitive);
        }

        /// <summary>
        /// Adds a temporary primitive to the scene
        /// </summary>
        /// <param name="primitive">the primitive to add</param>
        public async Task AddTemporaryPrimitive(Primitive primitive)
        {
            string jsonPrimitive = JsonConvert.SerializeObject(primitive, Formatting.Indented);
            TemporaryPrimitivesCollection.Add(primitive.ID, primitive);
            await JSInterop.Invoke("AddPrimitive", jsonPrimitive);
        }

        /// <summary>
        /// Clears the scene deleting the primitives that contains
        /// </summary>
        public async Task ClearPrimitives()
        {
            var keys = primitivesCollection.Keys.ToList();
            foreach (var id in keys)
            {
                await JSInterop.Invoke("Dispose", id);
            }
            primitivesCollection.Clear();
        }

        /// <summary>
        /// Clears the scene deleting the temporary primitives that contains
        /// </summary>
        public async Task ClearTemporaryPrimitives()
        {
            var keys = TemporaryPrimitivesCollection.Keys.ToList();
            foreach (var id in keys)
            {
                await JSInterop.Invoke("Dispose", id);
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
        public async void SetPrimitivePosition(Guid Id, double x, double y, double z)
        {
            await JSInterop.Invoke("SetPrimitivePosition", Id.ToString(), x, y, z);
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
        public async void SetPrimitiveRotation(Guid Id, double rx, double ry, double rz)
        {
            await JSInterop.Invoke("SetPrimitiveRotation", Id.ToString(), rx, ry, rz);
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
        public async void SetInfoPanelPosition(int x, int y)
        {
            await JSInterop.Invoke("SetPanelPosition", x, y);
        }

        /// <summary>
        /// Sets the info panel visibility
        /// </summary>
        /// <param name="visible">true if the panel must be visible, false otherwise</param>
        public async void SetInfoPanelVisibility(bool visible)
        {
            await JSInterop.Invoke("SetPanelVisibility", visible);
        }

        /// <summary>
        /// Sets the info panel with the specified content
        /// </summary>
        /// <param name="info">The info that the panel must display</param>
        public async void SetInfoPanelContent(string info)
        {
            await JSInterop.Invoke("SetPanelContent", info);
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
