// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileHandlerViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using FluentResults;

    /// <summary>
    /// View model used to manage the files in Filestores
    /// </summary>
    public interface IFileHandlerViewModel : IApplicationBaseViewModel
    {
        /// <summary>
        /// Initializes the current <see cref="FileHandlerViewModel"/>
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> to be set</param>
        /// <param name="iteration">The <see cref="Iteration"/> to load data from</param>
        void InitializeViewModel(FileStore fileStore, Iteration iteration);

        /// <summary>
        /// Gets or sets the file to be created/edited
        /// </summary>
        File CurrentThing { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if the file or folder to be created is locked
        /// </summary>
        bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets a collection of the available <see cref="Folder" />s
        /// </summary>
        IEnumerable<Folder> Folders { get; set; }

        /// <summary>
        /// Gets or sets the selected folder to create a file revision
        /// </summary>
        Folder SelectedFolder { get; set; }

        /// <summary>
        /// Gets or sets a collection of the file revisions to be created/edited
        /// </summary>
        IEnumerable<FileRevision> SelectedFileRevisions { get; set; }

        /// <summary>
        /// Gets a collection of the available <see cref="FileType"/>
        /// </summary>
        IEnumerable<FileType> FileTypes { get; }

        /// <summary>
        /// Gets the <see cref="IFileRevisionHandlerViewModel"/>
        /// </summary>
        IFileRevisionHandlerViewModel FileRevisionHandlerViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Moves a file to a target folder
        /// </summary>
        /// <param name="file">The file to be moved</param>
        /// <param name="targetFolder">The target folder</param>
        /// <returns>A <see cref="Task"/></returns>
        Task MoveFile(File file, Folder targetFolder);

        /// <summary>
        /// Creates a file into a target folder
        /// </summary>
        /// <param name="shouldCreate"></param>
        /// <returns>A <see cref="Task"/> containing a <see cref="Result"/></returns>
        Task<Result> CreateOrEditFile(bool shouldCreate);

        /// <summary>
        /// Deletes the current file
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task DeleteFile();
    }
}
