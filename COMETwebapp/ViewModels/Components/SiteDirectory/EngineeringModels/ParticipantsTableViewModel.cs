// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantsTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="Participant" />
    /// </summary>
    public class ParticipantsTableViewModel : DeletableDataItemTableViewModel<Participant, ParticipantRowViewModel>, IParticipantsTableViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="EngineeringModelSetup"/>
        /// </summary>
        private EngineeringModelSetup CurrentModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public ParticipantsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<ParticipantsTableViewModel> logger) 
            : base(sessionService, messageBus, logger)
        {
            this.Thing = new Participant();
        }

        /// <summary>
        /// Gets a collection of all the available <see cref="Person"/>s
        /// </summary>
        public IEnumerable<Person> Persons { get; private set; }

        /// <summary>
        /// Gets a collection of all the available <see cref="ParticipantRole"/>s
        /// </summary>
        public IEnumerable<ParticipantRole> ParticipantRoles { get; private set; }

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise"/>s
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets or sets a collection of all the selected <see cref="DomainOfExpertise"/>s for the participant creation
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedDomains { get; set; }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();

            var siteDirectory = this.SessionService.GetSiteDirectory();
            this.Persons = siteDirectory.Person;
            this.ParticipantRoles = siteDirectory.ParticipantRole;
        }

        /// <summary>
        /// Sets the model and filters the current Rows, keeping only the participants associated with the given engineering model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its participants</param>
        public void SetEngineeringModel(EngineeringModelSetup model)
        {
            this.CurrentModel = model;
            var participantsAssociatedWithModel = this.DataSource.Items.Where(x => x.Container.Iid == this.CurrentModel.Iid);

            this.Rows.Edit(action =>
            {
                action.Clear();
                action.AddRange(participantsAssociatedWithModel.Select(x => new ParticipantRowViewModel(x)));
            });

            this.DomainsOfExpertise = this.CurrentModel.ActiveDomain;
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Selects the current participant
        /// </summary>
        /// <param name="participant">The <see cref="Participant"/> to select</param>
        public void SelectThing(Participant participant)
        {
            this.Thing = participant.Clone(false);
            this.SelectedDomains = this.Thing.Domain;
        }

        /// <summary>
        /// Updates the current participant domains with the <see cref="SelectedDomains"/>
        /// </summary>
        public void UpdateSelectedDomains()
        {
            this.Thing.Domain = this.SelectedDomains.ToList();
        }

        /// <summary>
        /// Creates or edits the current participant
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Participant"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditParticipant(bool shouldCreate)
        {
            this.IsLoading = true;

            try
            {
                var modelClone = this.CurrentModel.Clone(false);
                var thingsToCreate = new List<Thing>();

                this.UpdateSelectedDomains();
                this.Thing.SelectedDomain = this.SelectedDomains.FirstOrDefault();

                if (shouldCreate)
                {
                    modelClone.Participant.Add(this.Thing);
                    thingsToCreate.Add(modelClone);
                }

                thingsToCreate.Add(this.Thing);
                await this.SessionService.CreateOrUpdateThings(modelClone, thingsToCreate);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while creating or editing the model participants");
            }

            this.IsLoading = false;
        }
    }
}
