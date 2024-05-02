// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelsTableViewModel.cs" company="Starion Group S.A.">
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
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using ReactiveUI;

    /// <summary>
    /// View model used to manage <see cref="DomainOfExpertise" />
    /// </summary>
    public class EngineeringModelsTableViewModel : DeletableDataItemTableViewModel<EngineeringModelSetup, EngineeringModelRowViewModel>, IEngineeringModelsTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public EngineeringModelsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<EngineeringModelsTableViewModel> logger) 
            : base(sessionService, messageBus, logger)
        {
            this.Thing = new EngineeringModelSetup();
        }

        /// <summary>
        /// Gets a collection of the available engineering models
        /// </summary>
        public IEnumerable<EngineeringModelSetup> EngineeringModels { get; private set; }

        /// <summary>
        /// Gets a collection of all the possible model kinds
        /// </summary>
        public IEnumerable<EngineeringModelKind> ModelKinds { get; private set; } = Enum.GetValues<EngineeringModelKind>();

        /// <summary>
        /// Gets a collection of all the possible study phase kinds
        /// </summary>
        public IEnumerable<StudyPhaseKind> StudyPhases { get; private set; } = Enum.GetValues<StudyPhaseKind>();

        /// <summary>
        /// Gets a collection of the available site reference data libraries
        /// </summary>
        public IEnumerable<SiteReferenceDataLibrary> SiteRdls { get; private set; }

        /// <summary>
        /// Gets a collection of the available domains of expertise
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets a collection of the available organizations
        /// </summary>
        public IEnumerable<Organization> Organizations { get; private set; }

        /// <summary>
        /// Gets or sets the selected site reference data library
        /// </summary>
        public SiteReferenceDataLibrary SelectedSiteRdl { get; set; }

        /// <summary>
        /// Gets or sets the collection of selected active domains
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedActiveDomains { get; set; } = Enumerable.Empty<DomainOfExpertise>();

        /// <summary>
        /// Gets or sets the collection of selected organizations
        /// </summary>
        public IEnumerable<Organization> SelectedOrganizations { get; set; } = Enumerable.Empty<Organization>();

        /// <summary>
        /// Gets or sets the selected model admin organization
        /// </summary>
        public Organization SelectedModelAdminOrganization { get; set; }

        /// <summary>
        /// Gets or sets the selected source <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup SelectedSourceModel { get; set; }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();

            var siteDirectory = this.SessionService.GetSiteDirectory();
            this.EngineeringModels = siteDirectory.Model.OrderBy(x => x.Name);
            this.SiteRdls = siteDirectory.SiteReferenceDataLibrary.OrderBy(x => x.Name);
            this.DomainsOfExpertise = siteDirectory.Domain.OrderBy(x => x.Name);
            this.Organizations = siteDirectory.Organization.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Resets the selected values
        /// </summary>
        public void ResetSelectedValues()
        {
            this.SelectedActiveDomains = Enumerable.Empty<DomainOfExpertise>();
            this.SelectedOrganizations = Enumerable.Empty<Organization>();
            this.SelectedModelAdminOrganization = null;
            this.SelectedSiteRdl = null;
            this.SelectedSourceModel = null;
        }

        /// <summary>
        /// Updates the current thing with the selected properties
        /// </summary>
        public void SetupEngineeringModelWithSelectedValues()
        {
            this.Thing.ActiveDomain = this.SelectedActiveDomains?.ToList();
            this.Thing.SourceEngineeringModelSetupIid = this.SelectedSourceModel?.Iid;
            this.Thing.OrganizationalParticipant.Clear();
            this.Thing.RequiredRdl.Clear();

            if (this.SelectedOrganizations != null)
            {
                this.Thing.OrganizationalParticipant.AddRange(this.SelectedOrganizations.Select(org => new OrganizationalParticipant()
                {
                    Organization = org
                }));

                this.Thing.DefaultOrganizationalParticipant = this.Thing.OrganizationalParticipant.FirstOrDefault(x => x.Organization == this.SelectedModelAdminOrganization);
            }

            if (this.SelectedSiteRdl != null)
            {
                this.Thing.RequiredRdl.Add(new ModelReferenceDataLibrary()
                {
                    RequiredRdl = this.SelectedSiteRdl,
                    Name = $"{this.Thing.Name} Model RDL",
                    ShortName = $"{this.Thing.ShortName}MRDL"
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="EngineeringModelSetup"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateEngineeringModel()
        {
            this.IsLoading = true;

            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
            var thingsToCreate = new List<Thing>();
            this.Thing.EngineeringModelIid = Guid.NewGuid();

            if (this.Thing.OrganizationalParticipant.Count > 0)
            {
                thingsToCreate.AddRange(this.Thing.OrganizationalParticipant);
            }

            if (this.Thing.RequiredRdl.Count > 0)
            {
                thingsToCreate.AddRange(this.Thing.RequiredRdl);
            }
            
            siteDirectoryClone.Model.Add(this.Thing);
            thingsToCreate.Add(siteDirectoryClone);
            thingsToCreate.Add(this.Thing);

            await this.SessionService.CreateOrUpdateThings(siteDirectoryClone, thingsToCreate);
            await this.SessionService.RefreshSession();

            this.IsLoading = false;
        }
    }
}
