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
    using DevExpress.Utils.Zip;
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
        private List<SceneObject> SceneObjects = new List<SceneObject>();

        /// <summary>
        /// Collection of temporary scene objects in scene.
        /// </summary>
        private List<SceneObject> TemporarySceneObjects = new List<SceneObject>();

        /// <summary>
        /// ReadOnly collection of scene objects in the scene.
        /// </summary>
        public List<SceneObject> AllSceneObjects => SceneObjects.AsReadOnly().ToList();

        /// <summary>
        /// ReadOnly collection of temporary scene objects in scene.
        /// </summary>
        public List<SceneObject> AllTemporarySceneObjects => TemporarySceneObjects.AsReadOnly().ToList();

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
                this.InitCanvas(this.CanvasReference, true);
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
            await this.ClearTemporarySceneObjects();

            this.SceneObjects.ForEach(async x => { x.Primitive.IsSelected = false; await JSInterop.Invoke("SetSelection", x.Primitive.ID, false); });
            this.SelectedPrimitive = null;

            var primitive = await this.GetPrimitiveUnderMouseAsync();

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
            await this.ClearSceneObjects();

            foreach (var elementUsage in elementUsages)
            {
                var basicShape = this.ShapeFactory?.CreatePrimitiveFromElementUsage(elementUsage, selectedOption, states);               
                this.AddSceneObject(new SceneObject(basicShape));
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
        public async void InitCanvas(ElementReference canvas, bool addAxes)
        {
            await JSInterop.Invoke("InitCanvas", canvas, addAxes);
        }

        /// <summary>
        /// Adds a selectable scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        public void AddSceneObject(SceneObject sceneObject)
        {
            string sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            this.SceneObjects.Add(sceneObject);
            JSInterop.Invoke("AddSceneObject", sceneObjectJson);
        }

        /// <summary>
        /// Adds a selectable temporary scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        public void AddTemporarySceneObject(SceneObject sceneObject)
        {
            string sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            this.TemporarySceneObjects.Add(sceneObject);
            JSInterop.Invoke("AddSceneObject", sceneObjectJson);
        }

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        public async Task ClearSceneObjects()
        {
            foreach(var sceneObj in this.SceneObjects)
            {
                await JSInterop.Invoke("Dispose", sceneObj.Primitive.ID);
            }

            this.SceneObjects.Clear();
        }

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        public async Task ClearTemporarySceneObjects()
        {
            foreach (var sceneObj in this.TemporarySceneObjects)
            {
                await JSInterop.Invoke("Dispose", sceneObj.Primitive.ID);
            }

            this.TemporarySceneObjects.Clear();
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
            if (!this.SceneObjects.Exists(x => x.Primitive.ID == id))
            {
                throw new ArgumentException("The specified Id dont exist in the scene");
            }

            return this.SceneObjects.First(x => x.Primitive.ID == id).Primitive;
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
    }
}
