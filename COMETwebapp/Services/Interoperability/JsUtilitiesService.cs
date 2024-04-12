// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsUtilitiesService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
    using Microsoft.JSInterop;

    /// <summary>
    /// The service used to provide js utilities
    /// </summary>
    public class JsUtilitiesService : InteroperabilityService, IJsUtilitiesService
    {
        /// <summary>
        /// Creates a new instance of <see cref="JsUtilitiesService"/>
        /// </summary>
        /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
        public JsUtilitiesService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        /// <summary>
        /// Downloads a file from a stream asynchronously
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="fileName">the file name</param>
        /// <returns>an asynchronous operation</returns>
        public async Task DownloadFileFromStreamAsync(Stream stream, string fileName)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(fileName);

            using var streamRef = new DotNetStreamReference(stream: stream);

            await this.JsRuntime.InvokeVoidAsync("DownloadFileFromStream", fileName, streamRef);
        }
    }
}
