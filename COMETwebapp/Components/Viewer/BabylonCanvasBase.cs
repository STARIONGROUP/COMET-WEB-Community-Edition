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

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;

    /// <summary>
    /// Support class for the <see cref="BabylonCanvas.razor"/>
    /// </summary>
    public class BabylonCanvasBase : ComponentBase
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
        /// Property to inject the JSRuntime and allow C#-JS interop
        /// </summary>
        [Inject] 
        IJSRuntime? JsRuntime { get; set; }

        /// <summary>
        /// Invokable method from JS to get a GUID
        /// </summary>
        /// <returns>the GUID in string format</returns>
        [JSInvokable]
        public static string GetGUID() => Guid.NewGuid().ToString();

        /// <summary>
        /// Shape factory for creating <see cref="Primitive"/> from <see cref="ElementUsage"/>
        /// </summary>
        [Inject]
        public IShapeFactory? ShapeFactory { get; set; }

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
                if(this.JsRuntime != null)
                {
                    JSInterop.JsRuntime = this.JsRuntime;
                }
                else
                {
                    throw new JSException("JSRuntime can't be null");
                }
 
                Scene.InitCanvas(this.CanvasReference);
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
        public void OnMouseUp(MouseEventArgs e)
        {
            this.IsMouseDown = false;
            //TODO: when the tools are ready here we are going to manage the different types of actions that a user can make.
        }

        /// <summary>
        /// Create the world axes and adds them to the scene
        /// </summary>
        private async Task AddWorldAxes()
        {
            float size = 700;
            Line xAxis = new Line(-size, 0, 0, size, 0, 0);
            await Scene.AddPrimitive(xAxis, Color.Red);

            Line yAxis = new Line(0, -size, 0, 0, size, 0);
            await Scene.AddPrimitive(yAxis, Color.Green);

            Line zAxis = new Line(0, 0, -size, 0, 0, size);
            await Scene.AddPrimitive(zAxis, Color.Blue);
        }

        /// <summary>
        /// Clears the scene and populates again with the <see cref="ElementUsage"/> 
        /// </summary>
        /// <param name="elementUsages">the <see cref="ElementUsage"/> used for the population</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to position the <see cref="Primitive"/></param>
        public async void RepopulateScene(List<ElementUsage> elementUsages, Option selectedOption, List<ActualFiniteState> states)
        {
            await Scene.ClearPrimitives();

            foreach (var elementUsage in elementUsages)
            {
                var basicShape = await this.ShapeFactory.TryGetPrimitiveFromElementUsageParameter(elementUsage, selectedOption, states);

                if (basicShape is not null)
                {
                    if (basicShape is BasicPrimitive basicPrimitive)
                    {
                        await basicPrimitive.SetPositionFromElementUsageParameters(elementUsage, selectedOption, states);
                        await basicPrimitive.SetDimensionsFromElementUsageParameters(elementUsage, selectedOption, states);
                    }

                    await Scene.AddPrimitive(basicShape);
                }
            }
        }
    }
}
