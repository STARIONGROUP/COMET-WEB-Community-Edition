﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomDataService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Services.Interoperability
{
    using Microsoft.AspNetCore.Components;
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
        /// Gets an element size and position
        /// </summary>
        /// <param name="elementIndex">the index of the element to search</param>
        /// <param name="cssSelector">the selector to use to select the items</param>
        /// <returns>the size and position</returns>
        public async Task<float[]> GetElementSizeAndPosition(int elementIndex, string cssSelector)
        {
             return await this.JsRuntime.InvokeAsync<float[]>("GetElementSizeAndPosition", elementIndex, cssSelector);
        }
    }
}