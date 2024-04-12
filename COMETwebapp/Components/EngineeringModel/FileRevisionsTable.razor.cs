// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionsTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.EngineeringModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Extensions;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    ///  Support class for the <see cref="FileRevisionsTable"/>
    /// </summary>
    public partial class FileRevisionsTable : DisposableComponent
    {
        /// <summary>
        /// The file to be handled
        /// </summary>
        [Parameter]
        public File File { get; set; }

        /// <summary>
        /// A collection of file revisions to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<FileRevision> FileRevisions { get; set; }

        /// <summary>
        /// A collection of the acceptable file extensions
        /// </summary>
        [Parameter]
        public IEnumerable<FileType> AcceptableFileExtensions { get; set; }

        /// <summary>
        /// Method used to download a file revision file
        /// </summary>
        [Parameter]
        public Action<FileRevision> DownloadFileRevision { get; set; }

        /// <summary>
        /// The method that is executed when the file revisions change
        /// </summary>
        [Parameter]
        public EventCallback<IEnumerable<FileRevision>> FileRevisionsChanged { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// The file revision that will be handled for both edit and add forms
        /// </summary>
        private FileRevision FileRevision { get; set; } = new();

        /// <summary>
        /// The directory where uploaded files are stored
        /// </summary>
        private const string UploadsDirectory = "wwwroot/uploads/";

        /// <summary>
        /// The maximum file size to upload in megabytes
        /// </summary>
        private const double MaxUploadFileSizeInMb = 512;

        /// <summary>
        /// Gets or sets the uploaded file path
        /// </summary>
        private string UploadedFilePath { get; set; }

        /// <summary>
        /// Gets or sets the error message that is displayed in the component
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Method that is invoked when the edit/add file revision form is being saved
        /// </summary>
        private async Task OnEditFileRevisionSaving()
        {
            var listOfFileRevisions = this.FileRevisions.ToList();
            listOfFileRevisions.Add(this.FileRevision);

            this.FileRevisions = listOfFileRevisions;
            await this.FileRevisionsChanged.InvokeAsync(this.FileRevisions);
        }

        /// <summary>
        /// Method that is invoked when a file revision row is being removed
        /// </summary>
        private void RemoveFileRevision(FileRevisionRowViewModel row)
        {
            this.File.FileRevision.Remove(row.Thing);
        }

        /// <summary>
        /// Method invoked when creating a new file revision
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditFileRevision(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (FileRevisionRowViewModel)e.DataItem;
            this.FileRevision = dataItem == null ? new FileRevision() : dataItem.Thing;
            this.FileRevision.ContainingFolder = this.File.CurrentContainingFolder;
            e.EditModel = this.FileRevision;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="FileRevision"/> from <see cref="File"/>
        /// </summary>
        /// <returns>A collection of <see cref="FileRevisionRowViewModel"/>s to display</returns>
        private List<FileRevisionRowViewModel> GetRows()
        {
            return this.FileRevisions.Select(x => new FileRevisionRowViewModel(x)).ToList();
        }

        /// <summary>
        /// Method that is invoked when a file is uploaded to server
        /// </summary>
        private async Task OnFileUpload(InputFileChangeEventArgs e)
        {
            var maxUploadFileSizeInBytes = (long)(MaxUploadFileSizeInMb * 1024 * 1024);

            if (e.File.Size > maxUploadFileSizeInBytes)
            {
                this.ErrorMessage = $"The max file size is {MaxUploadFileSizeInMb} MB";
                return;
            }

            this.UploadedFilePath = Path.Combine(UploadsDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(UploadsDirectory);

            await using (var fileStream = new FileStream(this.UploadedFilePath, FileMode.Create))
            {
                await e.File.OpenReadStream(maxUploadFileSizeInBytes).CopyToAsync(fileStream);
            }

            this.FileRevision.Name = e.File.Name;
            this.FileRevision.LocalPath = this.UploadedFilePath;

            await this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            FileExtensions.TryDelete(this.UploadedFilePath);
        }
    }
}
