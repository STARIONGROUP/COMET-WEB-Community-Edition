// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InteroperabilityService.cs" company="Starion Group S.A.">
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
    using Microsoft.JSInterop;

    /// <summary>
    /// Base service for services that need to invoke JS
    /// </summary>
    public abstract class InteroperabilityService 
    {
        /// <summary>
        /// Gets or sets the see <see cref="IJSRuntime"/>
        /// </summary>
        protected IJSRuntime JsRuntime { get; }

        /// <summary>
        /// Creates a new instance of type <see cref="InteroperabilityService"/>
        /// </summary>
        /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
        protected InteroperabilityService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
        }
    }
}
