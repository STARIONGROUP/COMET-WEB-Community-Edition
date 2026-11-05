// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorInterop.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Utilities
{
    using COMETwebapp.Services.Interoperability;

    using Microsoft.JSInterop;

    /// <summary>
    /// The cosmetic JS interop of the tabs of the Requirements Editor: none of it may ever kill the page.
    /// </summary>
    public static class RequirementsEditorInterop
    {
        /// <summary>
        /// The minimum width of the table of contents while it is dragged, in pixels.
        /// </summary>
        private const int TocMinimumWidth = 150;

        /// <summary>
        /// Initialises the drag-to-resize handle between a table of contents and its document, tolerating the JS interop
        /// being unavailable. The underlying script guards against double initialisation.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime" />.</param>
        /// <param name="logger">The <see cref="ILogger" /> recording a swallowed failure.</param>
        /// <param name="resizerId">The HTML id of the handle.</param>
        /// <param name="tocId">The HTML id of the table of contents.</param>
        /// <returns>A <see cref="Task" /></returns>
        public static async Task InitialiseTocResizerAsync(IJSRuntime jsRuntime, ILogger logger, string resizerId, string tocId)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("cometResizer.init", resizerId, tocId, TocMinimumWidth);
            }
            catch (Exception exception)
            {
                // JS interop failures during pre-render or tests are non-fatal.
                logger.LogDebug(exception, "Initialising the table-of-contents resizer failed; non-fatal.");
            }
        }

        /// <summary>
        /// Scrolls the element with the given id into view, swallowing the interop errors that are harmless for a purely
        /// cosmetic scroll: a stale cached script or a circuit that disconnected mid-render.
        /// </summary>
        /// <param name="domDataService">The <see cref="IDomDataService" />.</param>
        /// <param name="logger">The <see cref="ILogger" /> recording a swallowed failure.</param>
        /// <param name="anchorId">The HTML id of the element to scroll to.</param>
        /// <returns>A <see cref="Task" /></returns>
        public static async Task ScrollIntoViewAsync(IDomDataService domDataService, ILogger logger, string anchorId)
        {
            try
            {
                await domDataService.ScrollElementIntoView(anchorId);
            }
            catch (Exception exception) when (exception is JSException or JSDisconnectedException)
            {
                logger.LogDebug(exception, "Scrolling the element into view failed; non-fatal.");
            }
        }
    }
}
