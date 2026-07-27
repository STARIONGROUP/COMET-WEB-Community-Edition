// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationsTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="IterationSetup" />
    /// </summary>
    public class IterationsTableViewModel : DeletableDataItemTableViewModel<IterationSetup, IterationSetupRowViewModel>, IIterationsTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public IterationsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<IterationsTableViewModel> logger)
            : base(sessionService, messageBus, logger)
        {
            this.CurrentThing = new IterationSetup();
        }

        /// <summary>
        /// Gets or sets the current <see cref="EngineeringModelSetup" />
        /// </summary>
        private EngineeringModelSetup CurrentModel { get; set; }

        /// <summary>
        /// Gets a value indicating whether a new iteration can be created.
        /// </summary>
        public bool CanCreateIteration => this.CurrentModel?.StudyPhase is StudyPhaseKind.DESIGN_SESSION_PHASE
            or StudyPhaseKind.REPORTING_PHASE
            or StudyPhaseKind.COMPLETED_STUDY;

        /// <summary>
        /// Gets a collection of all the available <see cref="IterationSetup" />s to be used as a source
        /// </summary>
        public IEnumerable<IterationSetup> SourceIterations => this.CurrentModel?.IterationSetup.OrderBy(x => x.IterationNumber);

        /// <summary>
        /// Initializes the view model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup" /> to get its iterations</param>
        public void InitializeViewModel(EngineeringModelSetup model)
        {
            this.CurrentModel = model;
            base.InitializeViewModel();
        }

        /// <summary>
        /// Creates or edits the current iteration
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="IterationSetup" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditIteration(bool shouldCreate)
        {
            this.IsLoading = true;

            try
            {
                var modelClone = this.CurrentModel.Clone(false);
                var thingsToUpdateOrCreate = new List<Thing> { modelClone };

                if (shouldCreate)
                {
                    this.CurrentThing.IterationIid = Guid.NewGuid();
                    modelClone.IterationSetup.Add(this.CurrentThing);
                }

                thingsToUpdateOrCreate.Add(this.CurrentThing);
                await this.SessionService.CreateOrUpdateThingsWithNotification(modelClone, thingsToUpdateOrCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing the model iteration");
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<IterationSetup> QueryListOfThings()
        {
            return this.CurrentModel?.IterationSetup;
        }

        /// <summary>
        /// Gets the message for the success notification
        /// </summary>
        /// <param name="created">The value to check if the thing was created</param>
        /// <returns>The message</returns>
        protected override NotificationDescription GetNotificationDescription(bool created)
        {
            return new NotificationDescription
            {
                OnSuccess = $"The Iteration was {(created ? "created" : "updated")}",
                OnError = $"Error while {(created ? "creating" : "updating")} the Iteration"
            };
        }

        /// <summary>
        /// Gets the message for the success notification
        /// </summary>
        /// <returns>The message</returns>
        protected override NotificationDescription GetDeletionNotificationDescription()
        {
            return new NotificationDescription
            {
                OnSuccess = $"The Iteration {this.CurrentThing.IterationNumber} was deleted!",
                OnError = $"Error while deleting The Iteration {this.CurrentThing.IterationNumber}"
            };
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            // If an iteration from this model was created, update the model
            var createdIterationSetup = this.AddedThings.OfType<IterationSetup>().FirstOrDefault(x => x.Container?.Iid == this.CurrentModel.Iid);

            if (createdIterationSetup is { Container: EngineeringModelSetup updatedModel })
            {
                this.CurrentModel = updatedModel;
            }

            await base.OnSessionRefreshed();
        }
    }
}
