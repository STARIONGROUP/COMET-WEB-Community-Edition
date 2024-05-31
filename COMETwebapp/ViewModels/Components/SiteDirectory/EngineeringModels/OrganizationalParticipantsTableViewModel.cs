// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrganizationalParticipantsTableViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// View model used to manage <see cref="OrganizationalParticipant" />
    /// </summary>
    public class OrganizationalParticipantsTableViewModel : DeletableDataItemTableViewModel<OrganizationalParticipant, OrganizationalParticipantRowViewModel>, IOrganizationalParticipantsTableViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="ParticipatingOrganizations" />
        /// </summary>
        private IEnumerable<Organization> participatingOrganizations;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationalParticipantsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public OrganizationalParticipantsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<OrganizationalParticipantsTableViewModel> logger)
            : base(sessionService, messageBus, logger)
        {
            this.Disposables.Add(this.WhenAnyValue(x => x.ParticipatingOrganizations).Subscribe(orgs =>
            {
                if (this.CurrentModel is null)
                {
                    return;
                }

                var organizationsToRemove = this.CurrentModel.OrganizationalParticipant.Where(x => !orgs.Contains(x.Organization));

                var organizationsToAdd = orgs
                    .Where(x => !this.CurrentModel.OrganizationalParticipant.Select(y => y.Organization).Contains(x))
                    .Select(org => new OrganizationalParticipant
                    {
                        Organization = org
                    });

                this.CurrentModel.OrganizationalParticipant.RemoveMany(organizationsToRemove);
                this.CurrentModel.OrganizationalParticipant.AddRange(organizationsToAdd);
            }));
        }

        /// <summary>
        /// Gets the current <see cref="EngineeringModelSetup" />
        /// </summary>
        public EngineeringModelSetup CurrentModel { get; private set; }

        /// <summary>
        /// Gets a collection of all the available <see cref="Organization" />s
        /// </summary>
        public IEnumerable<Organization> Organizations { get; set; }

        /// <summary>
        /// Gets or sets a collection of all the participating <see cref="Organization" />s for the organizational participant
        /// creation
        /// </summary>
        public IEnumerable<Organization> ParticipatingOrganizations
        {
            get => this.participatingOrganizations;
            set => this.RaiseAndSetIfChanged(ref this.participatingOrganizations, value);
        }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup" /> to get its active domains</param>
        public void InitializeViewModel(EngineeringModelSetup model)
        {
            this.CurrentModel = model;
            this.ParticipatingOrganizations = model.OrganizationalParticipant.Select(x => x.Organization);
            this.Organizations = this.SessionService.GetSiteDirectory().Organization;
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<OrganizationalParticipant> QueryListOfThings() => [];
    }
}
