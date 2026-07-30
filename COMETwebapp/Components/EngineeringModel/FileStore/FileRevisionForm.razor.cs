// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileRevisionForm.razor.cs" company="Starion Group S.A.">
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
    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    /// Support class for the <see cref="FileRevisionForm" />
    /// </summary>
    public partial class FileRevisionForm : SelectedDataItemForm
    {
        /// <summary>
        /// Gets or sets the <see cref="IFileRevisionHandlerViewModel" />
        /// </summary>
        [Parameter]
        public IFileRevisionHandlerViewModel ViewModel { get; set; }

        /// <summary>
        /// Method that is invoked when a file is uploaded to server
        /// </summary>
        /// <param name="e">The <see cref="InputFileChangeEventArgs" /></param>
        private async Task OnFileUpload(InputFileChangeEventArgs e)
        {
            await this.ViewModel.UploadFile(e.File);
        }
    }
}
