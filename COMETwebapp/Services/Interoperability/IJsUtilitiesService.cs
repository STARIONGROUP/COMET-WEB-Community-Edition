// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJsUtilitiesService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
    /// <summary>
    /// The service used to provide js utilities
    /// </summary>
    public interface IJsUtilitiesService
    {
        /// <summary>
        /// Downloads a file from a stream asynchronously
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <param name="fileName">the file name</param>
        /// <returns>an asynchronous operation</returns>
        Task DownloadFileFromStreamAsync(Stream stream, string fileName);

        /// <summary>
        /// Gets the dimensions [width, height] of an item
        /// </summary>
        /// <param name="cssSelector">the css selector used to find the element</param>
        /// <returns>The dimensions if the element is found. An empty array otherwise</returns>
        Task<int[]> GetItemDimensions(string cssSelector);
    }
}
