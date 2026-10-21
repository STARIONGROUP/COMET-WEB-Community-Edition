// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileDropZone.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    /// A file input presented as a drop zone: a file can be dropped onto it or selected by clicking it to open the
    /// browser's file picker. Both work because a native file input is laid transparently over the whole zone, which
    /// accepts a dropped file without any JavaScript interop
    /// </summary>
    public partial class FileDropZone
    {
        /// <summary>
        /// Gets or sets the id of the drop zone. The nested file input and the selected-file label derive their own ids
        /// from it, as <c>{Id}-input</c> and <c>{Id}-selected-file</c>
        /// </summary>
        [Parameter]
        public string Id { get; set; } = "file-drop-zone";

        /// <summary>
        /// Gets or sets the value of the file input's <c>accept</c> attribute, which filters what the browser's file
        /// picker offers, for example <c>.zip</c>
        /// </summary>
        [Parameter]
        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the text shown while no file has been selected
        /// </summary>
        [Parameter]
        public string Placeholder { get; set; } = "Drop a file here, or click to browse";

        /// <summary>
        /// Gets or sets the currently selected <see cref="IBrowserFile" />, whose name is shown in place of
        /// <see cref="Placeholder" />. Null when no file has been selected
        /// </summary>
        [Parameter]
        public IBrowserFile SelectedFile { get; set; }

        /// <summary>
        /// Gets or sets the callback raised with the <see cref="IBrowserFile" /> that the user dropped or picked
        /// </summary>
        [Parameter]
        public EventCallback<IBrowserFile> SelectedFileChanged { get; set; }

        /// <summary>
        /// Handles the selection of a file by the user, whether dropped onto the zone or picked through the browser's
        /// file picker
        /// </summary>
        /// <param name="args">The <see cref="InputFileChangeEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private Task OnFileSelected(InputFileChangeEventArgs args)
        {
            return this.SelectedFileChanged.InvokeAsync(args.File);
        }
    }
}
