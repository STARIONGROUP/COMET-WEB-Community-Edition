// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CanvasViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer
{
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Model;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="COMETwebapp.Components.Viewer.Canvas3D"/>
    /// </summary>
    public class CanvasViewModel : DisposableObject, ICanvasViewModel
    {
        /// <summary>
        /// Reference to the HTML5 canvas
        /// </summary>
        public ElementReference CanvasReference { get; set; }

        /// <summary> 
        /// Gets or sets the PopUp that ask the user if he wants to change the selected primitive before submiting changes 
        /// </summary> 
        public IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IBabylonInterop"/>
        /// </summary>
        [Inject]
        public IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Collection of scene objects in the scene.
        /// </summary>
        private readonly List<SceneObject> sceneObjects = new();

        /// <summary>
        /// Collection of temporary scene objects in scene.
        /// </summary>
        private readonly List<SceneObject> temporarySceneObjects = new();

        /// <summary>
        /// Backing field for the <see cref="IsOnChangePrimitiveMode"/> property
        /// </summary>
        private bool isOnChangePrimitiveMode;

        /// <summary>
        /// Gets or sets if the user is about to change the selected primitive
        /// </summary>
        public bool IsOnChangePrimitiveMode
        {
            get => this.isOnChangePrimitiveMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnChangePrimitiveMode, value);
        }

        /// <summary>
        /// Creates a new instance of type <see cref="CanvasViewModel"/>
        /// </summary>
        public CanvasViewModel(IBabylonInterop babylonInterop, ISelectionMediator selectionMediator)
        {
            this.BabylonInterop = babylonInterop;
            this.SelectionMediator = selectionMediator;

            this.ConfirmCancelPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Alert!",
                ContentText = "You are about to select another primitive. The changes on the previous one haven't been saved. Do you want to continue?",
                CancelRenderStyle = ButtonRenderStyle.Danger,
                ConfirmRenderStyle = ButtonRenderStyle.Success,
                OnConfirm = new EventCallback(null, async () =>
                {
                    await this.SelectSceneObjectUnderMouse();
                    this.IsOnChangePrimitiveMode = false;
                }),
                OnCancel =  new EventCallback(null, () => { this.IsOnChangePrimitiveMode = false; })
            };
        }

        /// <summary>
        /// Initiliazes this <see cref="CanvasViewModel"/>
        /// </summary>
        public void InitializeViewModel()
        {
            this.SelectionMediator.SceneObjectHasChanges = false;
            this.SelectionMediator.OnTreeSelectionChanged += async (nodeViewModel) => await this.OnTreeSelectionChanged(nodeViewModel);
            this.SelectionMediator.OnTreeVisibilityChanged += async (nodeViewModel) => await this.OnTreeVisibilityChanged(nodeViewModel);
        }

        /// <summary>
        /// Callback method for when the tree selection changed
        /// </summary>
        /// <param name="nodeViewModel">the <see cref="ViewerNodeViewModel"/> that raised the event</param>
        /// <returns>an asynchronous operation</returns>
        private async Task OnTreeSelectionChanged(ViewerNodeViewModel nodeViewModel)
        {
            await this.ClearTemporarySceneObjects();

            if (nodeViewModel.SceneObject?.Primitive != null)
            {
                await this.AddTemporarySceneObject(this.SelectionMediator.SelectedSceneObjectClone);
            }
        }

        /// <summary>
        /// Callback method for when the tree visibility changed
        /// </summary>
        /// <param name="nodeViewModel">the <see cref="ViewerNodeViewModel"/> that raised the event</param>
        /// <returns>an asynchronous operation</returns>
        private async Task OnTreeVisibilityChanged(ViewerNodeViewModel nodeViewModel)
        {
            await this.ClearTemporarySceneObjects();

            var nodesAffected = nodeViewModel.GetFlatListOfDescendants(true)
                .Where(x => x.SceneObject.Primitive is not null).ToList();

            foreach (var sceneObject in nodesAffected.Select(x => x.SceneObject))
            {
                await this.SetSceneObjectVisibility(sceneObject, nodeViewModel.IsSceneObjectVisible);
            }
        }

        /// <summary> 
        /// Tries to select a <see cref="SceneObject"/> under the mouse 
        /// </summary> 
        /// <returns>an asynchronous operation</returns> 
        private async Task SelectSceneObjectUnderMouse()
        {
            this.SelectionMediator.SceneObjectHasChanges = false;
            var sceneObject = await this.GetSceneObjectUnderMouseAsync();
            this.SelectionMediator.RaiseOnModelSelectionChanged(sceneObject);

            await this.ClearTemporarySceneObjects();
            
            if (this.SelectionMediator.SelectedSceneObjectClone is not null && this.SelectionMediator.SelectedSceneObjectClone.Primitive is not null)
            {
                await this.AddTemporarySceneObject(this.SelectionMediator.SelectedSceneObjectClone);
            }
        }

        /// <summary>
        /// Handles the mouse up in the <see cref="COMETwebapp.Components.Viewer.Canvas3D"/>
        /// </summary>
        /// <returns></returns>
        public async Task HandleMouseUp()
        {
            if (this.SelectionMediator.SelectedSceneObject is not null && this.SelectionMediator.SceneObjectHasChanges)
            {
                this.IsOnChangePrimitiveMode = true;
            }
            else
            {
                await this.SelectSceneObjectUnderMouse();
            }
        }

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        public async Task InitCanvas(bool addAxes)
        {
            await this.BabylonInterop.InitCanvas(this.CanvasReference,addAxes);
        }

        /// <summary>
        /// Adds a selectable scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        public async Task AddSceneObject(SceneObject sceneObject)
        {
            this.sceneObjects.Add(sceneObject);
            await this.BabylonInterop.AddSceneObject(sceneObject);
        }

        /// <summary>
        /// Adds a selectable temporary scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        public async Task AddTemporarySceneObject(SceneObject sceneObject)
        {
            if (sceneObject.Primitive is not null)
            {
                sceneObject.Primitive.HasHalo = true;
                this.temporarySceneObjects.Add(sceneObject);
                await this.BabylonInterop.AddSceneObject(sceneObject);
            }
        }

        /// <summary> 
        /// Clears the scene deleting the <see cref="sceneObjects"/> and <see cref="temporarySceneObjects"/> lists 
        /// </summary> 
        /// <returns>an asynchronous task</returns> 
        public async Task ClearScene()
        {
            await this.ClearSceneObjects();
            await this.ClearTemporarySceneObjects();
        }

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        public async Task ClearSceneObjects()
        {
            await this.BabylonInterop.ClearSceneObjects(this.sceneObjects);
            this.sceneObjects.Clear();
        }

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        public async Task ClearTemporarySceneObjects()
        {
            await this.BabylonInterop.ClearSceneObjects(this.temporarySceneObjects);
            this.temporarySceneObjects.Clear();
        }

        /// <summary>
        /// Sets the visibility for the scene object
        /// </summary>
        /// <param name="sceneObject">the scene object the visibility is set for</param>
        /// <param name="visible">the visibility</param>
        public async Task SetSceneObjectVisibility(SceneObject sceneObject, bool visible)
        {
            await this.BabylonInterop.SetVisibility(sceneObject, visible);
        }

        /// <summary>
        /// Gets the primitive under the mouse cursor asyncronously
        /// </summary>
        /// <returns>The primitive under the mouse cursor</returns>
        public async Task<SceneObject> GetSceneObjectUnderMouseAsync()
        {
            var id = await this.BabylonInterop.GetPrimitiveIdUnderMouseAsync();
            return this.GetSceneObjectById(id);
        }

        /// <summary>
        /// Gets the scene object asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the primitive asociated to the scene object</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        public SceneObject GetSceneObjectById(Guid id)
        {
            return this.sceneObjects.FirstOrDefault(x => x.ID == id, null);
        }

        /// <summary>
        /// Gets all the <see cref="SceneObject"/> in the scene
        /// </summary>
        /// <returns>the <see cref="SceneObject"/></returns>
        public IReadOnlyList<SceneObject> GetAllSceneObjects()
        {
            return this.sceneObjects.AsReadOnly();
        }

        /// <summary>
        /// Gets all the temporary <see cref="SceneObject"/> in the scene
        /// </summary>
        /// <returns>the temporary <see cref="SceneObject"/></returns>
        public IReadOnlyList<SceneObject> GetAllTemporarySceneObjects()
        {
            return this.temporarySceneObjects.AsReadOnly();
        }
    }
}
