// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasComponent.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Components.Canvas
{
    using System;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Interoperability;
    using COMETwebapp.Model;
    using COMETwebapp.Pages.Viewer;
    using COMETwebapp.Primitives;
    using COMETwebapp.Utilities;
    
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    
    using Newtonsoft.Json;
    
    /// <summary>
    /// Support class for the <see cref="CanvasComponent.razor"/>
    /// </summary>
    public partial class CanvasComponent 
    {        
        /// <summary>
        /// Reference to the HTML5 canvas
        /// </summary>
        public ElementReference CanvasReference { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewerPage"/> that contains this <see cref="CanvasComponent"/>
        /// </summary>
        [Parameter]
        public ViewerPage ViewerPage { get; set; }

        /// <summary>
        /// Tells if the mouse if pressed or not in the canvas component
        /// </summary>
        public bool IsMouseDown { get; private set; } = false;

        /// <summary>
        /// Tells is the scene is being moved, rotated...
        /// </summary>
        public bool IsMovingScene { get; private set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        [Inject]
        public IJSInterop? JSInterop { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Primitive"/> that is currently selected
        /// </summary>
        public SceneObject? SelectedSceneObject{ get; set; }

        /// <summary>
        /// Collection of scene objects in the scene.
        /// </summary>
        private List<SceneObject> SceneObjects = new List<SceneObject>();

        /// <summary>
        /// Collection of temporary scene objects in scene.
        /// </summary>
        private List<SceneObject> TemporarySceneObjects = new List<SceneObject>();

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

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
                await this.InitCanvas(this.CanvasReference, true);
                this.SelectionMediator.OnTreeSelectionChanged += async (sender, node) =>
                {
                    await this.ClearTemporarySceneObjects();
                    this.SelectedSceneObject = node.SceneObject;
                    if (this.SelectedSceneObject.Primitive != null)
                    {
                        await this.AddTemporarySceneObject(this.SelectedSceneObject);
                    }
                    this.ViewerPage?.Refresh();
                };

                this.SelectionMediator.OnTreeVisibilityChanged += async (sender, node) =>
                {
                    var nodesAffected = node.GetFlatListOfDescendants();

                    foreach (var sceneObject in nodesAffected.Select(x => x.SceneObject))
                    {
                        await this.SetSceneObjectVisibility(sceneObject, node.SceneObjectIsVisible);
                    }
                };
            }
        }
               
        /// <summary>
        /// Canvas on mouse down event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseDown(MouseEventArgs e)
        {
            this.IsMouseDown = true;
        }

        /// <summary>
        /// Canvas on mouse up event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public async void OnMouseUp(MouseEventArgs e)
        {
            if (!this.IsMovingScene)
            {
                this.SelectedSceneObject = await this.GetSceneObjectUnderMouseAsync();
                await this.ClearTemporarySceneObjects();
                if (this.SelectedSceneObject != null)
                { 
                    await this.AddTemporarySceneObject(this.SelectedSceneObject);
                }
                this.SelectionMediator.RaiseOnModelSelectionChanged(this.SelectedSceneObject);
            }

            this.IsMouseDown = false;
        }

        /// <summary>
        /// Canvas on mouse move event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseMove(MouseEventArgs e)
        {
            this.IsMovingScene = this.IsMouseDown;
        }


        /// <summary>
        /// Clears the scene and populates again with the <see cref="ElementUsage"/> 
        /// </summary>
        /// <param name="elementUsages">the <see cref="ElementUsage"/> used for the population</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to position the <see cref="Primitive"/></param>
        public async Task<List<SceneObject>> RepopulateScene(List<ElementUsage> elementUsages, Option selectedOption, List<ActualFiniteState> states)
        {
            await this.ClearSceneObjects();

            foreach (var elementUsage in elementUsages)
            {
                var sceneObject = SceneObject.Create(elementUsage, selectedOption, states);
                await this.AddSceneObject(sceneObject);
            }

            return this.SceneObjects;
        }

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        public async Task InitCanvas(ElementReference canvas, bool addAxes)
        {
            await this.JSInterop.Invoke("InitCanvas", canvas, addAxes);
        }

        /// <summary>
        /// Adds a selectable scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        public async Task AddSceneObject(SceneObject sceneObject)
        {
            string sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
            this.SceneObjects.Add(sceneObject);
            await this.JSInterop.Invoke("AddSceneObject", sceneObjectJson);
        }

        /// <summary>
        /// Adds a selectable temporary scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        public async Task AddTemporarySceneObject(SceneObject sceneObject)
        {
            if(sceneObject.Primitive is not null)
            {
                sceneObject.Primitive.HasHalo = true;
                string sceneObjectJson = JsonConvert.SerializeObject(sceneObject);
                this.TemporarySceneObjects.Add(sceneObject);
                await this.JSInterop.Invoke("AddSceneObject", sceneObjectJson);
            }
        }

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        public async Task ClearSceneObjects()
        {
            var ids = this.SceneObjects.Select(x => x.ID).ToList();
            this.SceneObjects.Clear();
            foreach (var id in ids)
            {
                await this.JSInterop.Invoke("Dispose", id);
            }
        }

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        public async Task ClearTemporarySceneObjects()
        {
            var ids = this.TemporarySceneObjects.Select(x => x.ID).ToList();
            this.TemporarySceneObjects.Clear();
            foreach (var id in ids)
            {
                await this.JSInterop.Invoke("Dispose", id);
            }
        }

        /// <summary>
        /// Sets the visibility for the scene object
        /// </summary>
        /// <param name="sceneObject">the scene object the visibility is set for</param>
        /// <param name="visible">the visibility</param>
        public async Task SetSceneObjectVisibility(SceneObject sceneObject, bool visible)
        {
            await this.JSInterop.Invoke("SetMeshVisibility", sceneObject.ID, visible);
        }

        /// <summary>
        /// Gets the primitive under the mouse cursor asyncronously
        /// </summary>
        /// <returns>The primitive under the mouse cursor</returns>
        public async Task<SceneObject> GetSceneObjectUnderMouseAsync()
        {
            var id = await this.JSInterop.Invoke<string>("GetPrimitiveIDUnderMouse");
            if (id == null || !Guid.TryParse(id, out Guid ID))
            {
                return null;
            }
            return GetSceneObjectById(ID);
        }

        /// <summary>
        /// Gets the scene object asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the primitive asociated to the scene object</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        public SceneObject? GetSceneObjectById(Guid id)
        {
            return this.SceneObjects.FirstOrDefault(x => x.ID == id, null);
        }

        /// <summary>
        /// Gets all the <see cref="SceneObject"/> in the scene
        /// </summary>
        /// <returns>the <see cref="SceneObject"/></returns>
        public IReadOnlyList<SceneObject> GetAllSceneObjects()
        {
            return this.SceneObjects.AsReadOnly();
        }

        /// <summary>
        /// Gets all the temporary <see cref="SceneObject"/> in the scene
        /// </summary>
        /// <returns>the temporary <see cref="SceneObject"/></returns>
        public IReadOnlyList<SceneObject> GetAllTemporarySceneObjects()
        {
            return this.TemporarySceneObjects.AsReadOnly();
        }
    }
}
