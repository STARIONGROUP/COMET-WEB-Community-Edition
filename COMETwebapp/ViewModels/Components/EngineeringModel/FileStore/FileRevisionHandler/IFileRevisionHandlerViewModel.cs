// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileRevisionHandlerViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    /// View model used to manage the files revisions in Filestores
    /// </summary>
    public interface IFileRevisionHandlerViewModel : IApplicationBaseViewModel
    {
        /// <summary>
        /// Sets the file for the <see cref="FileRevisionHandlerViewModel"/>
        /// </summary>
        /// <param name="file">The <see cref="File"/> to be set</param>
        /// <param name="fileStore"></param>
        void InitializeViewModel(File file, FileStore fileStore);

        /// <summary>
        /// Gets a collection of the selected <see cref="FileType"/>
        /// </summary>
        IEnumerable<FileType> SelectedFileTypes { get; }

        /// <summary>
        /// The file revision that will be handled for both edit and add forms
        /// </summary>
        FileRevision FileRevision { get; set; }

        /// <summary>
        /// Gets a collection of the available <see cref="FileType"/>s
        /// </summary>
        IEnumerable<FileType> FileTypes { get; }

        /// <summary>
        /// Gets or sets the error message that is displayed in the component
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets or sets the current <see cref="File"/>
        /// </summary>
        File CurrentFile { get; }

        /// <summary>
        /// Downloads a file revision
        /// </summary>
        /// <param name="fileRevision">the file revision</param>
        /// <returns>A <see cref="Task"/></returns>
        Task DownloadFileRevision(FileRevision fileRevision);

        /// <summary>
        /// Uploads a file to server and creates a file revision
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <returns>A <see cref="Task"/></returns>
        Task UploadFile(IBrowserFile file);
    }
}
