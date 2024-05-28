// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryHierarchyDiagram.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.ReferenceData.Categories
{
    using COMETwebapp.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.ReferenceData.Categories;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParameterTypeTable" />
    /// </summary>
    public partial class CategoryHierarchyDiagram
    {
        /// <summary>
        /// The <see cref="ICategoryHierarchyDiagramViewModel" /> for the component
        /// </summary>
        [Parameter]
        public ICategoryHierarchyDiagramViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IJsUtilitiesService"/>
        /// </summary>
        [Inject]
        public IJsUtilitiesService JsUtilitiesService { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if the diagram is on details mode
        /// </summary>
        private bool IsOnDetailsMode { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered interactively and the UI has finished
        /// updating (for example, after elements have been added to the browser DOM). Any <see cref="T:Microsoft.AspNetCore.Components.ElementReference" />
        /// fields will be populated by the time this runs.
        /// This method is not invoked during prerendering or server-side rendering, because those processes
        /// are not attached to any live browser DOM and are already complete before the DOM is updated.
        /// Note that the component does not automatically re-render after the completion of any returned <see cref="T:System.Threading.Tasks.Task" />,
        /// because that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                var dimensions = await this.JsUtilitiesService.GetItemDimensions(".diagram-canvas");

                if (dimensions.Length == 2)
                {
                    this.ViewModel.DiagramDimensions = [.. dimensions];
                }

                this.ViewModel.SetupDiagram();
            }
        }
    }
}
