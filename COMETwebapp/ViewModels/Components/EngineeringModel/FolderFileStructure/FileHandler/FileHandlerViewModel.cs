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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Routing.Constraints;

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
        /// Gets the <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<FileHandlerViewModel> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="jsUtilitiesService">The <see cref="JsUtilitiesService"/></param>
        public FileHandlerViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<FileHandlerViewModel> logger, IJsUtilitiesService jsUtilitiesService) 
            : base(sessionService, messageBus)
        {
            this.JsUtilitiesService = jsUtilitiesService;
            this.logger = logger;
            this.InitializeSubscriptions([typeof(File), typeof(Folder)]);
        }

        /// <summary>
        /// Gets or sets the <see cref="IJsUtilitiesService"/>
        /// </summary>
        public IJsUtilitiesService JsUtilitiesService { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="DomainOfExpertise"/>
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="FileType"/>
        /// </summary>
        public IEnumerable<FileType> FileTypes { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="Folder"/>s
        /// </summary>
        public IEnumerable<Folder> Folders { get; private set; }

        /// <summary>
        /// Gets or sets the file to be created/edited
        /// </summary>
        public File File { get; set; } = new();

        /// <summary>
        /// Gets or sets a collection of the file revisions to be created/edited
        /// </summary>
        public IEnumerable<FileRevision> FileRevisions { get; set; }

        /// <summary>
        /// Gets or sets the selected folder to create a file revision
        /// </summary>
        public Folder SelectedFolder { get; set; }

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
            this.FileTypes = this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.FileType);

            var folders = this.CurrentFileStore.Folder.ToList();
            folders.Add(null);
            this.Folders = folders;
        }

        /// <summary>
        /// Selects the current <see cref="File"/>
        /// </summary>
        /// <param name="file">The file to be set</param>
        public void SelectFile(File file)
        {
            this.File = file.Clone(true);
            this.IsLocked = this.File.LockedBy is not null;
            this.FileRevisions = this.File.FileRevision;
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

        public void SetupFileWithNewFileRevisions()
        {

        }

        /// <summary>
        /// Creates or edits a file
        /// </summary>
        /// <param name="shouldCreate">The condition to check if the file should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditFile(bool shouldCreate)
        {
            this.IsLoading = true;

            var thingsToCreate = new List<Thing>();
            var fileStoreClone = this.CurrentFileStore.Clone(false);
            var engineeringModel = this.CurrentFileStore.GetContainerOfType<EngineeringModel>();

            this.File.LockedBy = this.IsLocked switch
            {
               true when this.File.LockedBy == null => this.SessionService.Session.ActivePerson,
               false when this.File.LockedBy != null => null,
               _ => this.File.LockedBy
            };

            var newFileRevisions = this.FileRevisions.Where(x => !this.File.FileRevision.Contains(x));

            foreach (var fileRevision in newFileRevisions)
            {
                var fileExtension = Path.GetExtension(fileRevision.Name);
                var fileType = engineeringModel.RequiredRdls.SelectMany(x => x.FileType).First(x => $".{x.Extension}" == fileExtension);

                fileRevision.FileType.Add(fileType);
                fileRevision.Name = Path.GetFileNameWithoutExtension(fileRevision.Name);
                fileRevision.Creator = engineeringModel.GetActiveParticipant(this.SessionService.Session.ActivePerson);
                fileRevision.CreatedOn = DateTime.UtcNow;
                fileRevision.ContentHash = CalculateContentHash(fileRevision.LocalPath);

                this.File.FileRevision.Add(fileRevision);
                thingsToCreate.Add(fileRevision);
            }

            if (shouldCreate)
            {
                fileStoreClone.File.Add(this.File);
                thingsToCreate.Add(fileStoreClone);
            }

            thingsToCreate.Add(this.File);

            await this.SessionService.UpdateThings(fileStoreClone, thingsToCreate);
            await this.SessionService.RefreshSession();

            // delete all stored files

            this.IsLoading = false;
        }

        /// <summary>
        /// Deletes the current file
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task DeleteFile()
        {
            var clonedContainer = this.File.Container.Clone(false);

            await this.SessionService.DeleteThing(clonedContainer, this.File);
            await this.SessionService.RefreshSession();
        }

        /// <summary>
        /// Downloads a file revision
        /// </summary>
        /// <param name="fileRevision">the file revision</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task DownloadFileRevision(FileRevision fileRevision)
        {
            this.logger.LogInformation("Starting File Revision download...");
            var fileRevisionNameWithExtension = $"{fileRevision.Name}.{fileRevision.FileType.First().Extension}";

            try
            {
               var bytes = await this.SessionService.Session.ReadFile(fileRevision);
               var stream = new MemoryStream(bytes);
               await this.JsUtilitiesService.DownloadFileFromStreamAsync(stream, fileRevisionNameWithExtension);
               this.logger.LogInformation("Downloading PDF...");
            }
            catch (Exception ex)
            {
               this.logger.LogError(ex,"File Revision could not be downloaded") ;
            }
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed() => Task.CompletedTask;

        /// <summary>
        /// Calculates the hash of the file's content
        /// </summary>
        /// <param name="filePath">the path to the file</param>
        /// <returns>the hash of the content</returns>
        private static string CalculateContentHash(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return StreamToHashComputer.CalculateSha1HashFromStream(fileStream);
        }
    }
}
