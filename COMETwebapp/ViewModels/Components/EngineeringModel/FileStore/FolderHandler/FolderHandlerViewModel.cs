// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FolderHandlerViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model used to manage the folders in Filestores
    /// </summary>
    public class FolderHandlerViewModel : SingleThingApplicationBaseViewModel<Folder>, IFolderHandlerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderHandlerViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public FolderHandlerViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.CurrentThing = new Folder();

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
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets or sets a collection of the available <see cref="Folder" />s
        /// </summary>
        public IEnumerable<Folder> Folders { get; set; }

        /// <summary>
        /// Initializes the current <see cref="FolderHandlerViewModel" />
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore" /> to be set</param>
        /// <param name="iteration">The current <see cref="Iteration" /></param>
        public void InitializeViewModel(FileStore fileStore, Iteration iteration)
        {
            this.CurrentFileStore = fileStore;
            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
        }

        /// <summary>
        /// Moves a folder to a target folder
        /// </summary>
        /// <param name="folder">The folder to be moved</param>
        /// <param name="targetFolder">the target folders</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task MoveFolder(Folder folder, Folder targetFolder)
        {
            this.IsLoading = true;

            var folderClone = folder.Clone(true);
            folderClone.ContainingFolder = targetFolder;

            await this.SessionService.CreateOrUpdateThings(this.CurrentFileStore.Clone(true), [folderClone]);
            this.IsLoading = false;
        }

        /// <summary>
        /// Creates or edits a folder
        /// </summary>
        /// <param name="shouldCreate">the value to check if the <see cref="Folder" /> should be created or edited</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditFolder(bool shouldCreate)
        {
            this.IsLoading = true;

            var thingsToCreate = new List<Thing>();
            var fileStoreClone = this.CurrentFileStore.Clone(false);

            if (shouldCreate)
            {
                var engineeringModel = this.CurrentFileStore.GetContainerOfType<EngineeringModel>();
                this.CurrentThing.CreatedOn = DateTime.UtcNow;
                this.CurrentThing.Creator = engineeringModel.GetActiveParticipant(this.SessionService.Session.ActivePerson);

                fileStoreClone.Folder.Add(this.CurrentThing);
                thingsToCreate.Add(fileStoreClone);
            }

            thingsToCreate.Add(this.CurrentThing);

            await this.SessionService.CreateOrUpdateThings(fileStoreClone, thingsToCreate);
            this.IsLoading = false;
        }

        /// <summary>
        /// Deletes the current folder
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeleteFolder()
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
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has
        /// changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            if (this.DomainOfExpertiseSelectorViewModel is not null)
            {
                await this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(this.CurrentThing.Iid == Guid.Empty, this.CurrentThing.Owner);
            }
        }
    }
}
