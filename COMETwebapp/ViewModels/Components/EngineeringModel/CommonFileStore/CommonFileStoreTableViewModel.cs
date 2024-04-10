// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore.FolderFileStructure;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    /// <summary>
    /// View model used to manage <see cref="CommonFileStore" />
    /// </summary>
    public class CommonFileStoreTableViewModel : DeletableDataItemTableViewModel<CommonFileStore, CommonFileStoreRowViewModel>, ICommonFileStoreTableViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="Iteration"/>
        /// </summary>
        private Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        /// <param name="folderFileStructureViewModel">The <see cref="IFolderFileStructureViewModel"/></param>
        public CommonFileStoreTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<CommonFileStoreTableViewModel> logger, IFolderFileStructureViewModel folderFileStructureViewModel)
            : base(sessionService, messageBus, logger)
        {
            this.FolderFileStructureViewModel = folderFileStructureViewModel;
            this.Thing = new CommonFileStore();
        }

        /// <summary>
        /// Gets the <see cref="IFolderFileStructureViewModel"/>
        /// </summary>
        public IFolderFileStructureViewModel FolderFileStructureViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the value to verify if the <see cref="CommonFileStore"/> to create is private
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise"/>
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Initializes the <see cref="CommonFileStoreTableViewModel" />
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            this.DomainsOfExpertise = this.SessionService.GetSiteDirectory().Domain;
        }

        /// <summary>
        /// Sets the <see cref="CurrentIteration"/> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        public void SetCurrentIteration(Iteration iteration)
        {
            this.CurrentIteration = iteration;
        }

        /// <summary>
        /// Loads the file structure handled by the <see cref="FolderFileStructureViewModel"/>
        /// </summary>
        public void LoadFileStructure()
        {
            this.FolderFileStructureViewModel.InitializeViewModel(this.Thing);
        }

        /// <summary>
        /// Creates or edits a <see cref="CommonFileStore"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="CommonFileStore"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
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
                await this.SessionService.UpdateThings(engineeringModelClone, thingsToCreate);
                await this.SessionService.RefreshSession();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing a Common File Store");
            }

            this.IsLoading = false;
        }
    }
}
