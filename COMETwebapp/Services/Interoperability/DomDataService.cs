﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomDataService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using COMETwebapp.Components.BookEditor;
    
    using Microsoft.JSInterop;

    /// <summary>
    /// The service used to retrieve several data from the DOM
    /// </summary>
    public class DomDataService : InteroperabilityService, IDomDataService
    {
        /// <summary>
        /// Creates a new instance of <see cref="DomDataService"/>
        /// </summary>
        /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
        public DomDataService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        /// <summary>
        /// Set the dotnet helper
        /// </summary>
        /// <param name="dotNetHelper">the dotnet helper</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task LoadDotNetHelper<TItem>(DotNetObjectReference<BookEditorColumn<TItem>> dotNetHelper)
        {
            await this.JsRuntime.InvokeVoidAsync("setDotNetHelper", dotNetHelper);
        }

        /// <summary>
        /// Gets an element size and position
        /// </summary>
        /// <param name="elementIndex">the index of the element to search</param>
        /// <param name="cssSelector">the selector to use to select the items</param>
        /// <param name="useScroll">If the scroll must be taken into account for the calculations</param>
        /// <returns>the size and position</returns>
        public async Task<float[]> GetElementSizeAndPosition(int elementIndex, string cssSelector, bool useScroll)
        {
             return await this.JsRuntime.InvokeAsync<float[]>("GetElementSizeAndPosition", elementIndex, cssSelector, useScroll);
        }

        /// <summary>
        /// Subscribes for the resize event with a callback method name
        /// </summary>
        /// <param name="callbackMethodName">the callback method name</param>
        /// <returns>an asynchronous operation</returns>
        public async Task SubscribeToResizeEvent(string callbackMethodName)
        {
            await this.JsRuntime.InvokeVoidAsync("SubscribeToResizeEvent", callbackMethodName);
        }
    }
}
