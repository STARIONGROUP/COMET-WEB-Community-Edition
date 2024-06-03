// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantsTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="Participant" />
    /// </summary>
    public class ParticipantsTableViewModel : DeletableDataItemTableViewModel<Participant, ParticipantRowViewModel>, IParticipantsTableViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="EngineeringModelSetup" />
        /// </summary>
        private EngineeringModelSetup CurrentModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public ParticipantsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<ParticipantsTableViewModel> logger)
            : base(sessionService, messageBus, logger)
        {
            this.CurrentThing = new Participant();
        }

        /// <summary>
        /// Gets a collection of all the available <see cref="Person" />s
        /// </summary>
        public IEnumerable<Person> Persons { get; private set; }

        /// <summary>
        /// Gets a collection of all the available <see cref="ParticipantRole" />s
        /// </summary>
        public IEnumerable<ParticipantRole> ParticipantRoles { get; private set; }

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise" />s
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets or sets a collection of all the selected <see cref="DomainOfExpertise" />s for the participant creation
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedDomains { get; set; }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup" /> to get its participants</param>
        public void InitializeViewModel(EngineeringModelSetup model)
        {
            var siteDirectory = this.SessionService.GetSiteDirectory();
            this.Persons = siteDirectory.Person;
            this.ParticipantRoles = siteDirectory.ParticipantRole;
            this.CurrentModel = model;
            this.DomainsOfExpertise = this.CurrentModel.ActiveDomain;

            base.InitializeViewModel();
        }

        /// <summary>
        /// Updates the current participant domains with the <see cref="SelectedDomains" />
        /// </summary>
        public void UpdateSelectedDomains()
        {
            this.CurrentThing.Domain = this.SelectedDomains.ToList();
        }

        /// <summary>
        /// Creates or edits the current participant
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Participant" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditParticipant(bool shouldCreate)
        {
            this.IsLoading = true;

            try
            {
                var modelClone = this.CurrentModel.Clone(false);
                var thingsToCreate = new List<Thing>();

                this.UpdateSelectedDomains();
                this.CurrentThing.SelectedDomain = this.SelectedDomains.FirstOrDefault();

                if (shouldCreate)
                {
                    modelClone.Participant.Add(this.CurrentThing);
                    thingsToCreate.Add(modelClone);
                }

                thingsToCreate.Add(this.CurrentThing);
                await this.SessionService.CreateOrUpdateThingsWithNotification(modelClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing the model participants");
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<Participant> QueryListOfThings()
        {
            return this.CurrentModel?.Participant;
        }

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has
        /// changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.SelectedDomains = this.CurrentThing.Domain;
        }

        /// <summary>
        /// Gets the message for the success notification
        /// </summary>
        /// <param name="created">The value to check if the thing was created</param>
        /// <returns>The message</returns>
        protected override NotificationDescription GetNotificationDescription(bool created)
        {
            var notificationDescription = new NotificationDescription
            {
                OnSuccess = $"The {nameof(Participant)} {this.CurrentThing.Person.GetShortNameOrName()} was {(created ? "added" : "updated")}",
                OnError = $"Error while {(created ? "adding" : "updating")} the {nameof(Participant)} {this.CurrentThing.Person.GetShortNameOrName()}"
            };

            return notificationDescription;
        }

        /// <summary>
        /// Gets the message for the success notification
        /// </summary>
        /// <returns>The message</returns>
        protected override NotificationDescription GetDeletionNotificationDescription()
        {
            var notificationDescription = new NotificationDescription
            {
                OnSuccess = $"The {nameof(Participant)} {this.CurrentThing.Person.GetShortNameOrName()} was deleted!",
                OnError = $"Error while deleting The {nameof(Participant)} {this.CurrentThing.Person.GetShortNameOrName()}"
            };

            return notificationDescription;
        }
    }
}
