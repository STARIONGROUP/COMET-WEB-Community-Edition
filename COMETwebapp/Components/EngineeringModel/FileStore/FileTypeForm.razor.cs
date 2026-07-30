// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileTypeForm.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.EngineeringModel.FileStore
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="FileTypeForm" />
    /// </summary>
    public partial class FileTypeForm : SelectedDataItemForm
    {
        /// <summary>
        /// Gets or sets the <see cref="FileType" /> being edited or selected
        /// </summary>
        [Parameter]
        public FileType FileType { get; set; }

        /// <summary>
        /// Event callback triggered when the selected <see cref="FileType" /> changes
        /// </summary>
        [Parameter]
        public EventCallback<FileType> FileTypeChanged { get; set; }

        /// <summary>
        /// A collection of available file types to pick from
        /// </summary>
        [Parameter]
        public IEnumerable<FileType> AvailableFileTypes { get; set; } = [];

        /// <summary>
        /// Handles changes to the selected <see cref="FileType" /> in the dropdown
        /// </summary>
        /// <param name="fileType">The selected <see cref="FileType" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnFileTypeChanged(FileType fileType)
        {
            this.FileType = fileType;
            await this.FileTypeChanged.InvokeAsync(this.FileType);
        }
    }
}
