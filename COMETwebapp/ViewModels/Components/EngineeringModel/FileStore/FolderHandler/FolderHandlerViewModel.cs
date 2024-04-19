// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderHandlerViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    /// <summary>
    /// View model used to manage the folders in Filestores
    /// </summary>
    public class FolderHandlerViewModel : ApplicationBaseViewModel, IFolderHandlerViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="FileStore"/>
        /// </summary>
        private FileStore CurrentFileStore { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderHandlerViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public FolderHandlerViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
        }

        /// <summary>
        /// Gets a collection of the available <see cref="DomainOfExpertise"/>
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="Folder"/>s
        /// </summary>
        public IEnumerable<Folder> Folders { get; private set; }

        /// <summary>
        /// Gets or sets the folder to be created/edited
        /// </summary>
        public Folder Folder { get; set; } = new();

        /// <summary>
        /// Initializes the current <see cref="FolderHandlerViewModel"/>
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> to be set</param>
        public void InitializeViewModel(FileStore fileStore)
        {
            this.CurrentFileStore = fileStore;
            this.DomainsOfExpertise = this.SessionService.GetSiteDirectory().Domain;

            var folders = this.CurrentFileStore.Folder.ToList();
            folders.Add(null);
            this.Folders = folders;
        }

        /// <summary>
        /// Selects the current <see cref="Folder"/>
        /// </summary>
        /// <param name="folder">The folder to be set</param>
        public void SelectFolder(Folder folder)
        {
            this.Folder = folder.Clone(true);
        }

        /// <summary>
        /// Moves a folder to a target folder
        /// </summary>
        /// <param name="folder">The folder to be moved</param>
        /// <param name="targetFolder">the target folders</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task MoveFolder(Folder folder, Folder targetFolder)
        {
            this.IsLoading = true;

            var folderClone = folder.Clone(true);
            folderClone.ContainingFolder = targetFolder;

            await this.SessionService.CreateOrUpdateThings(this.CurrentFileStore.Clone(true), [folderClone]);
            await this.SessionService.RefreshSession();

            this.IsLoading = false;
        }

        /// <summary>
        /// Creates or edits a folder
        /// </summary>
        /// <param name="shouldCreate">the value to check if the <see cref="Folder"/> should be created or edited</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditFolder(bool shouldCreate)
        {
            this.IsLoading = true;

            var thingsToCreate = new List<Thing>();
            var fileStoreClone = this.CurrentFileStore.Clone(false);

            if (shouldCreate)
            {
                var engineeringModel = this.CurrentFileStore.GetContainerOfType<EngineeringModel>();
                this.Folder.Creator = engineeringModel.GetActiveParticipant(this.SessionService.Session.ActivePerson);

                fileStoreClone.Folder.Add(this.Folder);
                thingsToCreate.Add(fileStoreClone);
            }

            thingsToCreate.Add(this.Folder);

            await this.SessionService.UpdateThings(fileStoreClone, thingsToCreate);
            await this.SessionService.RefreshSession();

            this.IsLoading = false;
        }

        /// <summary>
        /// Deletes the current folder
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task DeleteFolder()
        {
            var clonedContainer = this.Folder.Container.Clone(false);

            await this.SessionService.DeleteThing(clonedContainer, this.Folder);
            await this.SessionService.RefreshSession();
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed() => Task.CompletedTask;
    }
}
