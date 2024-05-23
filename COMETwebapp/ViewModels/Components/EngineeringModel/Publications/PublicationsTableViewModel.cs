// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationsTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Publications
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    /// <summary>
    /// View model used to manage <see cref="Publication" />
    /// </summary>
    public class PublicationsTableViewModel : BaseDataItemTableViewModel<Publication, PublicationRowViewModel>, IPublicationsTableViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="Iteration"/>
        /// </summary>
        private Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public PublicationsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<PublicationsTableViewModel> logger)
            : base(sessionService, messageBus, logger)
        {
        }

        /// <summary>
        /// Gets or sets the collection of selected rows
        /// </summary>
        public IReadOnlyList<object> SelectedParameterRowsToPublish { get; set; } = [];

        /// <summary>
        /// Sets the <see cref="CurrentIteration"/> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        public void SetCurrentIteration(Iteration iteration)
        {
            this.CurrentIteration = iteration;
        }

        /// <summary>
        /// Gets the existing parameters that can be published
        /// </summary>
        /// <returns>A collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel"/> - the parameters that can be published</returns>
        public IEnumerable<OwnedParameterOrOverrideBaseRowViewModel> GetParametersThatCanBePublished()
        {
            return this.CurrentIteration.QueryParameterAndOverrideBases()
                .Where(x => x.CanBePublished)
                .Select(x => new OwnedParameterOrOverrideBaseRowViewModel(x));
        }

        /// <summary>
        /// Gets the published parameters rows for a given publication
        /// </summary>
        /// <param name="publication">The publication that contains the parameters to be retrieved</param>
        /// <returns>A collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel"/> - the published parameters</returns>
        public IEnumerable<OwnedParameterOrOverrideBaseRowViewModel> GetPublishedParametersRows(Publication publication)
        {
            return publication.PublishedParameter.Select(x => new OwnedParameterOrOverrideBaseRowViewModel(x));
        }

        /// <summary>
        /// Creates a new <see cref="Publication"/> with the parameters from <see cref="SelectedParameterRowsToPublish"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreatePublication()
        {
            if (this.SelectedParameterRowsToPublish.Count == 0)
            {
                return;
            }

            this.IsLoading = true;

            var thingsToCreate = new List<Thing>();
            var publication = new Publication(Guid.NewGuid(), null, null);
            var iterationClone = this.CurrentIteration.Clone(false);

            publication.PublishedParameter = this.SelectedParameterRowsToPublish
                .Cast<OwnedParameterOrOverrideBaseRowViewModel>()
                .Select(x => x.Parameter)
                .ToList();

            iterationClone.Publication.Add(publication);
            thingsToCreate.Add(iterationClone);
            thingsToCreate.Add(publication);

            try
            {
                await this.SessionService.CreateOrUpdateThingsWithNotification(iterationClone, thingsToCreate);
                this.SelectedParameterRowsToPublish = [];
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating a Publication");
            }
            
            this.IsLoading = false;
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<Publication> QueryListOfThings()
        {
            return this.CurrentIteration.Publication;
        }
    }
}
