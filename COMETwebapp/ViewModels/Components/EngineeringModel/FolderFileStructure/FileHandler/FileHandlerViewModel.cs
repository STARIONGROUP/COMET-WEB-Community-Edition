// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileHandlerViewModel.cs" company="RHEA System S.A.">
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

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    /// <summary>
    /// View model used to manage the files in Filestores
    /// </summary>
    public class FileHandlerViewModel : ApplicationBaseViewModel, IFileHandlerViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="FileStore"/>
        /// </summary>
        private FileStore CurrentFileStore { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public FileHandlerViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.InitializeSubscriptions([typeof(File), typeof(Folder)]);
        }

        /// <summary>
        /// Gets a collection of the available <see cref="DomainOfExpertise"/>
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets or sets the file to be created/edited
        /// </summary>
        public File File { get; set; } = new();

        /// <summary>
        /// Gets or sets the condition to check if the file or folder to be created is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Initializes the current <see cref="FileHandlerViewModel"/>
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> to be set</param>
        public void InitializeViewModel(FileStore fileStore)
        {
            this.CurrentFileStore = fileStore;
            this.DomainsOfExpertise = this.SessionService.GetSiteDirectory().Domain;
        }

        /// <summary>
        /// Selects the current <see cref="File"/>
        /// </summary>
        /// <param name="file">The file to be set</param>
        public void SelectFile(File file)
        {
            this.File = file.Clone(true);
            this.IsLocked = this.File.LockedBy is not null;
        }

        /// <summary>
        /// Moves a file to a target folder
        /// </summary>
        /// <param name="file">The file to be moved</param>
        /// <param name="targetFolder">The target folder</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task MoveFile(File file, Folder targetFolder)
        {
            this.IsLoading = true;

            var fileClone = file.Clone(true);
            var newFileRevision = fileClone.CurrentFileRevision.Clone(true);

            newFileRevision.Iid = Guid.NewGuid();
            newFileRevision.CreatedOn = DateTime.UtcNow;
            newFileRevision.ContainingFolder = targetFolder;

            fileClone.FileRevision.Add(newFileRevision);

            await this.SessionService.UpdateThings(this.CurrentFileStore.Clone(true), fileClone, newFileRevision);
            await this.SessionService.RefreshSession();

            this.IsLoading = false;
        }

        /// <summary>
        /// Creates a file into a target folder
        /// </summary>
        /// <param name="targetFolder">The target folder</param>
        /// <param name="localFilePath">The local file path for the file revision</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateFile(Folder targetFolder, string localFilePath)
        {
            this.IsLoading = true;

            var fileRevision = new FileRevision()
            {
                LocalPath = localFilePath,
                ContainingFolder = targetFolder
            };

            this.File.FileRevision.Add(fileRevision);

            await this.SessionService.UpdateThings(this.CurrentFileStore.Clone(true), this.File, fileRevision);
            await this.SessionService.RefreshSession();

            this.IsLoading = false;
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed() => Task.CompletedTask;
    }
}
