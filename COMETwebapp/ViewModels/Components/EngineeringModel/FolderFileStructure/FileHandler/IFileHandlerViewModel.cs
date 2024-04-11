// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileHandlerViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FolderFileStructure.FileHandler
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    /// <summary>
    /// View model used to manage the files in Filestores
    /// </summary>
    public interface IFileHandlerViewModel : IApplicationBaseViewModel
    {
        /// <summary>
        /// Initializes the current <see cref="FileHandlerViewModel"/>
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> to be set</param>
        void InitializeViewModel(FileStore fileStore);

        /// <summary>
        /// Gets or sets the file to be created/edited
        /// </summary>
        File File { get; set; }

        /// <summary>
        /// Gets a collection of the available <see cref="DomainOfExpertise"/>
        /// </summary>
        IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; }

        /// <summary>
        /// Gets or sets the condition to check if the file or folder to be created is locked
        /// </summary>
        bool IsLocked { get; set; }

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
        /// <param name="targetFolder">The target folder</param>
        /// <param name="localFilePath">The local file path for the file revision</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateFile(Folder targetFolder, string localFilePath);

        /// <summary>
        /// Selects the current <see cref="FileHandlerViewModel.File"/>
        /// </summary>
        /// <param name="file">The file to be set</param>
        void SelectFile(File file);
    }
}
