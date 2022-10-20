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
    using CDP4Common.SiteDirectoryData;
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
        /// Injected property to get acess to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        ISessionAnchor? SessionAnchor { get; set; }

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
                if(JsRuntime != null)
                {
                    JSInterop.JsRuntime = JsRuntime;
                }
                else
                {
                    throw new JSException("JSRuntime can't be null");
                }
                
                Scene.InitCanvas(this.CanvasReference);
                InitializeElements();

                AddWorldAxes();
            }
        }

        /// <summary>
        /// Initialize the elements in the scene based on the elements of the iteration
        /// </summary>
        private void InitializeElements()
        {
            var iteration = this.SessionAnchor?.OpenIteration;

            var elementUsages = this.SessionAnchor?.OpenIteration?.Element.SelectMany(ed => ed.ContainedElement).OrderBy(x=>x.Name).ToList();
            
            if(elementUsages is not null)
            {
                foreach (var elementUsage in elementUsages)
                {
                    var parameter = elementUsage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.Name == "Shape kind"
                                          && x.ParameterType is EnumerationParameterType
                                          || x.ParameterType is TextParameterType);

                    Primitive basicShape = null;
                    if (parameter is not null)
                    {
                        string? shapekind = parameter?.ExtractActualValues(1).First();
                        switch (shapekind?.ToLowerInvariant())
                        {
                            case "box": basicShape = new Cube(1, 1, 1); break;
                        }
                    }
                    parameter = elementUsage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.Name == "Position"
                                                          && x.ParameterType is CompoundParameterType);

                    string[]? translations = parameter?.ExtractActualValues(3);
                    if (basicShape is PositionablePrimitives positionablePrim && translations is not null)
                    {
                        var x = double.Parse(translations[0]);
                        var y = double.Parse(translations[1]);
                        var z = double.Parse(translations[2]);
                        positionablePrim.SetTranslation(x, y, z);
                    }

                    if (basicShape is not null)
                    {
                        Scene.AddPrimitive(basicShape);
                    }
                }
            }
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
