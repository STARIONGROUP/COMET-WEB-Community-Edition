// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionsTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Options
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;

    /// <summary>
    /// View model used to manage <see cref="Option" />
    /// </summary>
    public class OptionsTableViewModel : DeletableDataItemTableViewModel<Option, OptionRowViewModel>, IOptionsTableViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="Iteration"/>
        /// </summary>
        private Iteration CurrentIteration { get; set; }

        /// <summary>
        /// A collection of <see cref="Type" /> used to create <see cref="ObjectChangedEvent" /> subscriptions
        /// </summary>
        private static readonly IEnumerable<Type> ObjectChangedTypesOfInterest = new List<Type>
        {
            typeof(Iteration)
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public OptionsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<OptionsTableViewModel> logger)
            : base(sessionService, messageBus, logger)
        {
            this.CurrentThing = new Option();
            this.InitializeSubscriptions(ObjectChangedTypesOfInterest);
        }

        /// <summary>
        /// Gets or sets the value to check if the option to create is the default option for the <see cref="CurrentIteration"/>
        /// </summary>
        public bool SelectedIsDefaultValue { get; set; }

        /// <summary>
        /// Sets the <see cref="CurrentIteration"/> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        public void SetCurrentIteration(Iteration iteration)
        {
            this.CurrentIteration = iteration;
        }

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.SelectedIsDefaultValue = this.CurrentThing.IsDefault;
        }

        /// <summary>
        /// Creates or edits a <see cref="Option"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Option"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditOption(bool shouldCreate)
        {
            this.IsLoading = true;

            var iterationClone = this.CurrentIteration.Clone(false);
            var thingsToCreate = new List<Thing>();
            var originalOption = (Option)this.CurrentThing.Original;

            iterationClone.DefaultOption = this.SelectedIsDefaultValue switch
            {
                true when !originalOption.IsDefault => this.CurrentThing,
                false when originalOption.IsDefault => null,
                _ => iterationClone.DefaultOption
            };

            if (shouldCreate)
            {
                iterationClone.Option.Add(this.CurrentThing);
            }

            thingsToCreate.Add(iterationClone);
            thingsToCreate.Add(this.CurrentThing);

            try
            {
                await this.SessionService.CreateOrUpdateThingsWithNotification(this.CurrentIteration.Container.Clone(true), thingsToCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing an Option");
            }
            
            this.IsLoading = false;
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<Option> QueryListOfThings()
        {
            return this.CurrentIteration.Option.ToList();
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
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            var wasCurrentIterationUpdated = this.UpdatedThings.OfType<Iteration>().Any(x => x == this.CurrentIteration);

            if (wasCurrentIterationUpdated)
            {
                this.RefreshIsDefault();
            }

            await base.OnSessionRefreshed();
        }

        /// <summary>
        /// Refreshes the IsDefault field for all rows
        /// </summary>
        private void RefreshIsDefault()
        {
            foreach (var row in this.Rows.Items)
            {
                row.IsDefault = row.Thing == this.CurrentIteration.DefaultOption;
            }
        }
    }
}
