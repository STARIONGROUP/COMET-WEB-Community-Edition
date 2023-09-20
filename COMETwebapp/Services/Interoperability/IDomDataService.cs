// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDomDataService.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Components.BookEditor;

    using Microsoft.JSInterop;

    /// <summary>
    /// The service used to retrieve several data from the DOM
    /// </summary>
    public interface IDomDataService
    {
        /// <summary>
        /// Gets an element size and position
        /// </summary>
        /// <param name="elementIndex">the index of the element to search</param>
        /// <param name="cssSelector">the selector to use to select the items</param>
        /// <param name="useScroll">If the scroll must be taken into account for the calculations</param>
        /// <returns>the size and position</returns>
        Task<float[]> GetElementSizeAndPosition(int elementIndex, string cssSelector, bool useScroll);
        
        /// <summary>
        /// Subscribes for the resize event with a callback method name
        /// </summary>
        /// <param name="callbackMethodName">the callback method name</param>
        /// <returns>an asynchronous operation</returns>
        Task SubscribeToResizeEvent(string callbackMethodName);

        /// <summary>
        /// Set the dotnet helper
        /// </summary>
        /// <param name="dotNetHelper">the dotnet helper</param>
        /// <returns>A <see cref="Task" /></returns>
        Task LoadDotNetHelper<TItem>(DotNetObjectReference<BookEditorColumn<TItem>> dotNetHelper);
    }
}
