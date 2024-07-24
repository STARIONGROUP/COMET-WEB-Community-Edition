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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

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
        /// Backing field for the property <see cref="SelectedSiteRdl"/>
        /// </summary>
        private SiteReferenceDataLibrary selectedSiteRdl;

        /// <summary>
        /// Backing field for the property <see cref="SelectedSourceModel"/>
        /// </summary>
        private EngineeringModelSetup selectedSourceModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        /// <param name="organizationalParticipantsTableViewModel">The <see cref="IOrganizationalParticipantsTableViewModel"/></param>
        /// <param name="participantsTableViewModel">The <see cref="IParticipantsTableViewModel"/></param>
        public EngineeringModelsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<EngineeringModelsTableViewModel> logger, IOrganizationalParticipantsTableViewModel organizationalParticipantsTableViewModel,
            IParticipantsTableViewModel participantsTableViewModel) : base(sessionService, messageBus, logger)
        {
            this.OrganizationalParticipantsTableViewModel = organizationalParticipantsTableViewModel;
            this.ParticipantsTableViewModel = participantsTableViewModel;
            this.CurrentThing = new EngineeringModelSetup();

            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedSiteRdl).Subscribe(this.OnSelectedSiteRdlChanged));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedSourceModel).Subscribe(this.OnSelectedSourceModelChanged));
        }

        /// <summary>
        /// Gets the <see cref="IOrganizationalParticipantsTableViewModel"/>
        /// </summary>
        public IOrganizationalParticipantsTableViewModel OrganizationalParticipantsTableViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IOrganizationalParticipantsTableViewModel"/>
        /// </summary>
        public IParticipantsTableViewModel ParticipantsTableViewModel { get; }

        /// <summary>
        /// Gets a collection of the available engineering models
        /// </summary>
        public IEnumerable<EngineeringModelSetup> EngineeringModels { get; private set; }

        /// <summary>
        /// Gets a collection of the available <see cref="IterationRowViewModel"/>s
        /// </summary>
        public IEnumerable<IterationRowViewModel> IterationRows { get; private set; }

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
        public SiteReferenceDataLibrary SelectedSiteRdl
        {
            get => this.selectedSiteRdl;
            set => this.RaiseAndSetIfChanged(ref this.selectedSiteRdl, value);
        }

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
        public EngineeringModelSetup SelectedSourceModel
        {
            get => this.selectedSourceModel;
            set => this.RaiseAndSetIfChanged(ref this.selectedSourceModel, value);
        }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();

            var siteDirectory = this.SessionService.GetSiteDirectory();
            this.EngineeringModels = siteDirectory.Model.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.SiteRdls = siteDirectory.SiteReferenceDataLibrary.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.DomainsOfExpertise = siteDirectory.Domain.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.Organizations = siteDirectory.Organization.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

            this.IterationRows = this.SessionService.OpenIterations.Items
                .Where(x => ((EngineeringModel)x.Container).EngineeringModelSetup.Iid == this.CurrentThing?.Iid)
                .Select(x => new IterationRowViewModel(x));
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<EngineeringModelSetup> QueryListOfThings()
        {
            return this.SessionService.GetSiteDirectory().Model;
        }

        /// <summary>
        /// Creates a new <see cref="EngineeringModelSetup"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="EngineeringModelSetup"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditEngineeringModel(bool shouldCreate)
        {
            try
            {
                this.IsLoading = true;

                var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
                var thingsToCreate = new List<Thing>();

                if (shouldCreate)
                {
                    this.CurrentThing.EngineeringModelIid = Guid.NewGuid();
                    siteDirectoryClone.Model.Add(this.CurrentThing);
                    thingsToCreate.Add(siteDirectoryClone);

                    if (this.CurrentThing.SourceEngineeringModelSetupIid != null)
                    {
                        this.CurrentThing.RequiredRdl.Clear();
                    }
                    else
                    {
                        thingsToCreate.AddRange(this.CurrentThing.RequiredRdl);
                    }
                }

                if (this.CurrentThing.OrganizationalParticipant.Count > 0)
                {
                    thingsToCreate.AddRange(this.CurrentThing.OrganizationalParticipant);
                }

                thingsToCreate.Add(this.CurrentThing);
                await this.SessionService.CreateOrUpdateThingsWithNotification(siteDirectoryClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));

                if (this.CurrentThing.Original is EngineeringModelSetup originalModel)
                {
                    this.CurrentThing = originalModel.Clone(true);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Create or Update EngineeringModelSetup failed");
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
        protected override Task OnThingChanged()
        {
            this.SelectedSiteRdl = this.CurrentThing.RequiredRdl.FirstOrDefault()?.RequiredRdl;
            this.SelectedSourceModel = this.Rows.Items.FirstOrDefault(x => x.Thing.Iid == this.CurrentThing.SourceEngineeringModelSetupIid)?.Thing;
            this.OrganizationalParticipantsTableViewModel.InitializeViewModel(this.CurrentThing);
            this.ParticipantsTableViewModel.InitializeViewModel(this.CurrentThing);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Method invoked every time the <see cref="SelectedSiteRdl"/> property has changed, synchronizing with the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing"/>
        /// </summary>
        /// <param name="siteRdl">The updated <see cref="SiteReferenceDataLibrary"/></param>
        private void OnSelectedSiteRdlChanged(SiteReferenceDataLibrary siteRdl)
        {
            if (this.CurrentThing.Iid != Guid.Empty)
            {
                return;
            }

            this.CurrentThing.RequiredRdl.Clear();

            if (siteRdl != null)
            {
                this.CurrentThing.RequiredRdl.Add(new ModelReferenceDataLibrary()
                {
                    RequiredRdl = siteRdl,
                    Name = $"{this.CurrentThing.Name} Model RDL",
                    ShortName = $"{this.CurrentThing.ShortName}MRDL"
                });
            }
        }

        /// <summary>
        /// Method invoked every time the <see cref="SelectedSourceModel"/> property has changed, synchronizing with the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing"/>
        /// </summary>
        /// <param name="sourceModel">The updated <see cref="EngineeringModelSetup"/></param>
        private void OnSelectedSourceModelChanged(EngineeringModelSetup sourceModel)
        {
            this.CurrentThing.SourceEngineeringModelSetupIid = sourceModel?.Iid;
        }
    }
}
