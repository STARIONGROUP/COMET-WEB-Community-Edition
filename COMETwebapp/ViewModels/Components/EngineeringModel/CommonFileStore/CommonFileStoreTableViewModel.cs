// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CommonFileStoreTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model used to manage <see cref="CommonFileStore" />
    /// </summary>
    public class CommonFileStoreTableViewModel : DeletableDataItemTableViewModel<CommonFileStore, CommonFileStoreRowViewModel>, ICommonFileStoreTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="folderFileStructureViewModel">The <see cref="IFolderFileStructureViewModel" /></param>
        public CommonFileStoreTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<CommonFileStoreTableViewModel> logger, IFolderFileStructureViewModel folderFileStructureViewModel)
            : base(sessionService, messageBus, logger)
        {
            this.FolderFileStructureViewModel = folderFileStructureViewModel;
            this.Thing = new CommonFileStore();

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner => { this.Thing.Owner = selectedOwner; })
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
        /// Gets or sets the value to verify if the <see cref="CommonFileStore" /> to create is private
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
        /// Selects the current <see cref="CommonFileStore" />
        /// </summary>
        /// <param name="commonFileStore">The common file store to be set</param>
        public void SelectCommonFileStore(CommonFileStore commonFileStore)
        {
            this.Thing = commonFileStore.Clone(true);
            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(commonFileStore.Iid == Guid.Empty, commonFileStore.Owner);
        }

        /// <summary>
        /// Loads the file structure handled by the <see cref="FolderFileStructureViewModel" />
        /// </summary>
        public void LoadFileStructure()
        {
            this.FolderFileStructureViewModel.InitializeViewModel(this.Thing, this.CurrentIteration);
        }

        /// <summary>
        /// Creates or edits a <see cref="CommonFileStore" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="CommonFileStore" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditCommonFileStore(bool shouldCreate)
        {
            this.IsLoading = true;

            var engineeringModelClone = ((EngineeringModel)this.CurrentIteration.Container).Clone(true);
            var thingsToCreate = new List<Thing>();

            if (shouldCreate)
            {
                engineeringModelClone.CommonFileStore.Add(this.Thing);
                thingsToCreate.Add(engineeringModelClone);
            }

            thingsToCreate.Add(this.Thing);

            try
            {
                await this.SessionService.CreateOrUpdateThingsWithNotification(engineeringModelClone, thingsToCreate);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing a Common File Store");
            }

            this.IsLoading = false;
        }
    }
}
