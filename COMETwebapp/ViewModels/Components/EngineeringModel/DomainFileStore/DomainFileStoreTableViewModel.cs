// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainFileStoreTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.DomainFileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model used to manage <see cref="DomainFileStore" />
    /// </summary>
    public class DomainFileStoreTableViewModel : DeletableDataItemTableViewModel<DomainFileStore, DomainFileStoreRowViewModel>, IDomainFileStoreTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="folderFileStructureViewModel">The <see cref="IFolderFileStructureViewModel" /></param>
        public DomainFileStoreTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<DomainFileStoreTableViewModel> logger, IFolderFileStructureViewModel folderFileStructureViewModel)
            : base(sessionService, messageBus, logger)
        {
            this.FolderFileStructureViewModel = folderFileStructureViewModel;
            this.CurrentThing = new DomainFileStore();

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner => { this.CurrentThing.Owner = selectedOwner; })
            };
        }

        /// <summary>
        /// Gets or sets the current <see cref="Iteration" />
        /// </summary>
        private Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Gets the <see cref="IFolderFileStructureViewModel" />
        /// </summary>
        public IFolderFileStructureViewModel FolderFileStructureViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the value to verify if the <see cref="DomainFileStore" /> to create is private
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

        /// <summary>
        /// Sets the <see cref="CurrentIteration" /> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        public void SetCurrentIteration(Iteration iteration)
        {
            this.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
        }

        /// <summary>
        /// Creates or edits a <see cref="DomainFileStore" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="DomainFileStore" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditDomainFileStore(bool shouldCreate)
        {
            try
            {
                this.IsLoading = true;

                var iterationClone = this.CurrentIteration.Clone(true);
                var thingsToCreate = new List<Thing>();

                if (shouldCreate)
                {
                    this.CurrentThing.CreatedOn = DateTime.UtcNow;
                    iterationClone.DomainFileStore.Add(this.CurrentThing);
                    thingsToCreate.Add(iterationClone);
                }

                thingsToCreate.Add(this.CurrentThing);
                await this.SessionService.CreateOrUpdateThingsWithNotification(iterationClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing a Domain File Store");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has
        /// changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            if (this.DomainOfExpertiseSelectorViewModel is not null)
            {
                await this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(this.CurrentThing.Iid == Guid.Empty, this.CurrentThing.Owner);
                this.FolderFileStructureViewModel.InitializeViewModel(this.CurrentThing, this.CurrentIteration);
            }
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<DomainFileStore> QueryListOfThings()
        {
            return this.CurrentIteration.DomainFileStore;
        }
    }
}
