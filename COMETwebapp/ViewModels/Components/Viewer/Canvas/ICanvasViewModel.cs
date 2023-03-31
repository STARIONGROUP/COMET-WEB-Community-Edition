// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ICanvasViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer.Canvas
{
    using COMETwebapp.Components.Viewer.PopUps;
    using COMETwebapp.Model;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View Model for the <see cref="COMETwebapp.Components.Viewer.Canvas.CanvasComponent"/>
    /// </summary>
    public interface ICanvasViewModel
    {
        /// <summary>
        /// Reference to the HTML5 canvas
        /// </summary>
        ElementReference CanvasReference { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IBabylonInterop"/>
        /// </summary>
        public IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        ISelectionMediator SelectionMediator { get; set; }

        /// <summary> 
        /// Gets or sets the PopUp that ask the user if he wants to change the selected primitive before submiting changes 
        /// </summary> 
        ConfirmChangeSelectionPopUp ConfirmChangeSelectionPopUp { get; set; }

        /// <summary>
        /// Initiliazes this <see cref="CanvasViewModel"/>
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Inits the scene, the asociated resources and the render loop.
        /// </summary>
        Task InitCanvas(bool addAxes);

        /// <summary>
        /// Adds a selectable scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        Task AddSceneObject(SceneObject sceneObject);

        /// <summary>
        /// Adds a selectable temporary scene object into scene that contains a primitive
        /// </summary>
        /// <param name="sceneObject"></param>
        Task AddTemporarySceneObject(SceneObject sceneObject);

        /// <summary> 
        /// Clears the scene deleting the all the <see cref="SceneObject"/>
        /// </summary> 
        /// <returns>an asynchronous task</returns> 
        Task ClearScene();

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        Task ClearSceneObjects();

        /// <summary>
        /// Clears the scene deleting the scene objects that contains
        /// </summary>
        Task ClearTemporarySceneObjects();

        /// <summary>
        /// Sets the visibility for the scene object
        /// </summary>
        /// <param name="sceneObject">the scene object the visibility is set for</param>
        /// <param name="visible">the visibility</param>
        Task SetSceneObjectVisibility(SceneObject sceneObject, bool visible);

        /// <summary>
        /// Gets the primitive under the mouse cursor asyncronously
        /// </summary>
        /// <returns>The primitive under the mouse cursor</returns>
        Task<SceneObject> GetSceneObjectUnderMouseAsync();

        /// <summary>
        /// Gets the scene object asociated to an specific Id
        /// </summary>
        /// <param name="id">The Id of the primitive asociated to the scene object</param>
        /// <returns>The primitive</returns>
        /// <exception cref="ArgumentException">If the Id don't exist in the current scene.</exception>
        SceneObject GetSceneObjectById(Guid id);

        /// <summary>
        /// Gets all the <see cref="SceneObject"/> in the scene
        /// </summary>
        /// <returns>the <see cref="SceneObject"/></returns>
        IReadOnlyList<SceneObject> GetAllSceneObjects();

        /// <summary>
        /// Gets all the temporary <see cref="SceneObject"/> in the scene
        /// </summary>
        /// <returns>the temporary <see cref="SceneObject"/></returns>
        IReadOnlyList<SceneObject> GetAllTemporarySceneObjects();

        /// <summary>
        /// Handles the mouse up in the <see cref="COMETwebapp.Components.Viewer.Canvas.CanvasComponent"/>
        /// </summary>
        /// <returns></returns>
        Task HandleMouseUp();
    }
}
