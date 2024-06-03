// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileHandlerViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model used to manage the files in Filestores
    /// </summary>
    public class FileHandlerViewModel : SingleThingApplicationBaseViewModel<File>, IFileHandlerViewModel
    {
        /// <summary>
        /// Gets the <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<FileHandlerViewModel> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="fileRevisionHandlerViewModel">The <see cref="IFileRevisionHandlerViewModel" /></param>
        public FileHandlerViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<FileHandlerViewModel> logger, IFileRevisionHandlerViewModel fileRevisionHandlerViewModel)
            : base(sessionService, messageBus)
        {
            this.CurrentThing = new File();
            this.logger = logger;
            this.FileRevisionHandlerViewModel = fileRevisionHandlerViewModel;

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner => { this.CurrentThing.Owner = selectedOwner; })
            };

            this.InitializeSubscriptions([typeof(FileStore)]);
        }

        /// <summary>
        /// Gets or sets the current <see cref="FileStore" />
        /// </summary>
        private FileStore CurrentFileStore { get; set; }

        /// <summary>
        /// Gets the <see cref="IFileRevisionHandlerViewModel" />
        /// </summary>
        public IFileRevisionHandlerViewModel FileRevisionHandlerViewModel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="FileType" />
        /// </summary>
        public IEnumerable<FileType> FileTypes { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="Folder" />s
        /// </summary>
        public IEnumerable<Folder> Folders { get; private set; }

        /// <summary>
        /// Gets or sets a collection of the file revisions to be created/edited
        /// </summary>
        public IEnumerable<FileRevision> SelectedFileRevisions { get; set; }

        /// <summary>
        /// Gets or sets the selected folder to create a file revision
        /// </summary>
        public Folder SelectedFolder { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if the file or folder to be created is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Initializes the current <see cref="FileHandlerViewModel" />
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore" /> to be set</param>
        /// <param name="iteration">The <see cref="Iteration" /> to load data from</param>
        public void InitializeViewModel(FileStore fileStore, Iteration iteration)
        {
            this.CurrentFileStore = fileStore;
            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
            this.FileTypes = this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.FileType);
            this.SetFolders(this.CurrentFileStore.Folder);
        }

        /// <summary>
        /// Moves a file to a target folder
        /// </summary>
        /// <param name="file">The file to be moved</param>
        /// <param name="targetFolder">The target folder</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task MoveFile(File file, Folder targetFolder)
        {
            this.IsLoading = true;

            var fileClone = file.Clone(true);
            var newFileRevision = fileClone.CurrentFileRevision.Clone(true);

            newFileRevision.Iid = Guid.NewGuid();
            newFileRevision.CreatedOn = DateTime.UtcNow;
            newFileRevision.ContainingFolder = targetFolder;

            fileClone.FileRevision.Add(newFileRevision);
            await this.SessionService.CreateOrUpdateThings(this.CurrentFileStore.Clone(true), [fileClone, newFileRevision]);

            this.IsLoading = false;
        }

        /// <summary>
        /// Creates or edits a file
        /// </summary>
        /// <param name="shouldCreate">The condition to check if the file should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditFile(bool shouldCreate)
        {
            this.IsLoading = true;
            this.logger.LogInformation("Creating or editing file");

            var thingsToUpdate = new List<Thing>();
            var fileStoreClone = this.CurrentFileStore.Clone(true);

            this.CurrentThing.LockedBy = this.IsLocked switch
            {
                true when this.CurrentThing.LockedBy == null => this.SessionService.Session.ActivePerson,
                false when this.CurrentThing.LockedBy != null => null,
                _ => this.CurrentThing.LockedBy
            };

            var newFileRevisions = this.SelectedFileRevisions.Where(x => !this.CurrentThing.FileRevision.Contains(x)).ToList();

            foreach (var fileRevision in newFileRevisions)
            {
                if (shouldCreate)
                {
                    fileRevision.ContainingFolder = this.SelectedFolder;
                }

                this.CurrentThing.FileRevision.Add(fileRevision);
                thingsToUpdate.Add(fileRevision);
            }

            var fileRevisionsToRemove = this.CurrentThing.FileRevision.Where(x => !this.SelectedFileRevisions.Contains(x)).ToList();

            foreach (var fileRevisionToRemove in fileRevisionsToRemove)
            {
                this.CurrentThing.FileRevision.Remove(fileRevisionToRemove);
                thingsToUpdate.Add(fileRevisionToRemove);
            }

            if (shouldCreate)
            {
                fileStoreClone.File.Add(this.CurrentThing);
                thingsToUpdate.Add(fileStoreClone);
            }

            thingsToUpdate.Add(this.CurrentThing);

            await this.SessionService.CreateOrUpdateThings(fileStoreClone, thingsToUpdate, newFileRevisions.Select(x => x.LocalPath).ToList());

            this.logger.LogInformation("File with iid {iid} updated successfully", this.CurrentThing.Iid);
            this.IsLoading = false;
        }

        /// <summary>
        /// Deletes the current file
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeleteFile()
        {
            var clonedContainer = this.CurrentThing.Container.Clone(false);
            await this.SessionService.DeleteThings(clonedContainer, [this.CurrentThing]);
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if (this.UpdatedThings.Count == 0)
            {
                return Task.CompletedTask;
            }

            var updatedFileStore = this.UpdatedThings.OfType<FileStore>().FirstOrDefault(x => x.Iid == this.CurrentFileStore?.Iid);

            if (updatedFileStore != null)
            {
                this.SetFolders(updatedFileStore.Folder);
            }

            this.ClearRecordedChanges();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.EndUpdate" /> message received
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEndUpdate()
        {
            await this.OnSessionRefreshed();
        }

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has
        /// changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            this.IsLocked = this.CurrentThing.LockedBy is not null;
            this.SelectedFileRevisions = this.CurrentThing.FileRevision;
            this.SelectedFolder = null;
            this.FileRevisionHandlerViewModel?.InitializeViewModel(this.CurrentThing, this.CurrentFileStore);

            if (this.DomainOfExpertiseSelectorViewModel is not null)
            {
                await this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(this.CurrentThing.Iid == Guid.Empty, this.CurrentThing.Owner);
            }
        }

        /// <summary>
        /// Sets the current <see cref="Folders"/> property
        /// </summary>
        /// <param name="folders">The collection of folders to set</param>
        private void SetFolders(IEnumerable<Folder> folders)
        {
            var resultingFolders = folders.ToList();
            resultingFolders.Add(null);
            this.Folders = resultingFolders;
        }
    }
}
