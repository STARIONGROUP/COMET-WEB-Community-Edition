// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DraggableElementService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Services.Interoperability
{
    using COMETwebapp.Components.ModelEditor;

    using Microsoft.JSInterop;

    /// <summary>
    /// Class used for calling the draggableElementGrid.js methods
    /// </summary>
    public class DraggableElementService : InteroperabilityService, IDraggableElementService
    {
        /// <summary>
        /// Creates a new instance of type <see cref="DraggableElementService"/>
        /// </summary>
        /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
        public DraggableElementService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        /// <summary>
        /// Set the dotnet helper
        /// </summary>
        /// <param name="dotNetHelper">the dotnet helper</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task LoadDotNetHelper(DotNetObjectReference<ElementDefinitionTable> dotNetHelper)
        {
            await this.JsRuntime.InvokeVoidAsync("setDotNetHelper", dotNetHelper);
        }

        /// <summary>
        ///  Method used to initialize the draggable grids
        /// </summary>
        /// <param name="firstGrid">the first grid</param>
        /// <param name="secondGrid">the second grid</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task InitDraggableGrids(string firstGrid, string secondGrid)
        {
            await this.JsRuntime.InvokeVoidAsync("initialize", firstGrid, secondGrid);
        }
    }
}
