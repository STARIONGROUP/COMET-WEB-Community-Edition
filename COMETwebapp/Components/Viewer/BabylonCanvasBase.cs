// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BabylonCanvasBase.cs" company="RHEA System S.A.">
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
namespace COMETwebapp.Componentes.Viewer
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;

    /// <summary>
    /// Support class for the <see cref="BabylonCanvas"/>
    /// </summary>
    public class BabylonCanvasBase : ComponentBase
    {
        /// <summary>
        /// Tells if the mouse if pressed or not in the canvas component
        /// </summary>
        public bool IsMouseDown { get; private set; } = false;

        /// <summary>
        /// Property to inject the JSRuntime and allow C#-JS interop
        /// </summary>
        [Inject] 
        IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Injected property to get acess to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        ISessionAnchor SessionAnchor { get; set; }

        /// <summary>
        /// Invokable method from JS to get a GUID
        /// </summary>
        /// <returns>the GUID in string format</returns>
        [JSInvokable]
        public static string GetGUID() => Guid.NewGuid().ToString();

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRender(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {                
                JSInterop.JsRuntime = JsRuntime;
                Scene.Init();
                InitializeElements();

                AddWorldAxes();

                Scene.AddPrimitive(new Cube(5, 10, 15), Color.Yellow);
                Scene.AddPrimitive(new Torus(70, 50, 0, 20, 10), Color.Blue);

                Cube cube = new Cube(-50, 70, 10, 20, 20, 20);
                cube.SetRotation(1.0, 0.2, 0.1);
                Scene.AddPrimitive(cube, Color.Red);

                CustomPrimitive cp = new CustomPrimitive("./Assets/obj/", "RX2_CUSTOM_BODYKIT.obj");
                Scene.AddPrimitive(cp);
            }
        }

        /// <summary>
        /// Initialize the elements in the scene based on the elements of the iteration
        /// </summary>
        private void InitializeElements()
        {
            var iteration = this.SessionAnchor.OpenIteration;
            var elementUsages = iteration?.Element.SelectMany(x => x.ContainedElement).ToList();

            if(elementUsages != null)
            {
                foreach (var elementUsage in elementUsages)
                {
                    this.CreateShapeBasedOnElementUsage(elementUsage);
                }
            }
        }

        /// <summary>
        /// Creates a shape for the scene based on the element usage
        /// </summary>
        /// <param name="elementUsage">The element usage used for creating the shape</param>
        private void CreateShapeBasedOnElementUsage(ElementUsage elementUsage)
        {
            //TODO: based on element usage information create a basic shape type and add it to scene.
        }

        /// <summary>
        /// Canvas on mouse down event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseDown(MouseEventArgs e)
        {
            this.IsMouseDown = true;
            //TODO: when the tools are ready here we are going to manager the different types of actions that a user can make.
        }

        /// <summary>
        /// Canvas on mouse up event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseUp(MouseEventArgs e)
        {
            this.IsMouseDown = false;
            //TODO: when the tools are ready here we are going to manager the different types of actions that a user can make.
        }

        /// <summary>
        /// Create the world axes and adds them to the scene
        /// </summary>
        private void AddWorldAxes()
        {
            float size = 700;
            Line xAxis = new Line(-size, 0, 0, size, 0, 0);
            Scene.AddPrimitive(xAxis, Color.Red);

            Line yAxis = new Line(0, -size, 0, 0, size, 0);
            Scene.AddPrimitive(yAxis, Color.Green);

            Line zAxis = new Line(0, 0, -size, 0, 0, size);
            Scene.AddPrimitive(zAxis, Color.Blue);
        }
    }
}
