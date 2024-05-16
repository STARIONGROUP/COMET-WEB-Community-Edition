// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModelViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.Utilities.DisposableObject;

    using DynamicData.Binding;

    using Microsoft.Extensions.Configuration;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables a user to open an <see cref="EngineeringModel" />
    /// </summary>
    public class OpenModelViewModel : DisposableObject, IOpenModelViewModel
    {
        /// <summary>
        /// Gets the <see cref="IConfiguration" />
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Backing field for <see cref="IsOpeningSession" />
        /// </summary>
        private bool isOpeningSession;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainOfExpertise" />
        /// </summary>
        private DomainOfExpertise selectedDomainOfExpertise;

        /// <summary>
        /// </summary>
        private EngineeringModelSetup selectedEngineeringModel;

        /// <summary>
        /// Backing field for <see cref="SelectedIterationSetup" />
        /// </summary>
        private IterationData selectedIterationSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenModelViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="configuration">The <see cref="IConfiguration" /></param>
        public OpenModelViewModel(ISessionService sessionService, IConfiguration configuration)
        {
            this.sessionService = sessionService;
            this.configuration = configuration;

            this.Disposables.Add(this.WhenAnyPropertyChanged(nameof(this.SelectedEngineeringModel))
                .Subscribe(_ => this.ComputeAvailableCollections()));
        }

        /// <summary>
        /// The selected <see cref="EngineeringModelSetup" />
        /// </summary>
        public EngineeringModelSetup SelectedEngineeringModel
        {
            get => this.selectedEngineeringModel;
            set => this.RaiseAndSetIfChanged(ref this.selectedEngineeringModel, value);
        }

        /// <summary>
        /// The selected <see cref="IterationData" />
        /// </summary>
        public IterationData SelectedIterationSetup
        {
            get => this.selectedIterationSetup;
            set => this.RaiseAndSetIfChanged(ref this.selectedIterationSetup, value);
        }

        /// <summary>
        /// The selected <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise SelectedDomainOfExpertise
        {
            get => this.selectedDomainOfExpertise;
            set => this.RaiseAndSetIfChanged(ref this.selectedDomainOfExpertise, value);
        }

        /// <summary>
        /// A collection of available <see cref="EngineeringModelSetup" />
        /// </summary>
        public IEnumerable<EngineeringModelSetup> AvailableEngineeringModelSetups { get; set; }

        /// <summary>
        /// A collection of available <see cref="IterationData" />
        /// </summary>
        public IEnumerable<IterationData> AvailableIterationSetups { get; set; }

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailablesDomainOfExpertises { get; set; }

        /// <summary>
        /// Value asserting that the session is on way to open
        /// </summary>
        public bool IsOpeningSession
        {
            get => this.isOpeningSession;
            set => this.RaiseAndSetIfChanged(ref this.isOpeningSession, value);
        }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        public void InitializesProperties()
        {
            this.SelectedDomainOfExpertise = null;
            this.SelectedEngineeringModel = null;
            this.SelectedIterationSetup = null;
            this.IsOpeningSession = false;

            var availableEngineeringModelSetups = this.sessionService.GetParticipantModels()
                .Where(x => x.IterationSetup.Exists(setup => this.sessionService.OpenIterations.Items.All(i => i.Iid != setup.IterationIid)))
                .OrderBy(x => x.Name).ToList();

            var rdlFilter = this.configuration.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>().RdlFilter;

            if (rdlFilter != null)
            {
                var filteredEngineeringModelSetups = availableEngineeringModelSetups;

                if (rdlFilter.Kinds.Any())
                {
                    filteredEngineeringModelSetups = filteredEngineeringModelSetups
                        .Where(x => rdlFilter.Kinds.Contains(x.Kind))
                        .ToList();
                }

                if (rdlFilter.RdlShortNames.Any())
                {
                    filteredEngineeringModelSetups = filteredEngineeringModelSetups
                        .Where(x => x.RequiredRdl.Any(c => rdlFilter.RdlShortNames.Contains(c.RequiredRdl.ShortName)))
                        .ToList();
                }

                if (filteredEngineeringModelSetups.Any())
                {
                    this.AvailableEngineeringModelSetups = filteredEngineeringModelSetups;
                    return;
                }
            }

            this.AvailableEngineeringModelSetups = availableEngineeringModelSetups;
        }

        /// <summary>
        /// Opens the <see cref="EngineeringModel" /> based on the selected field
        /// </summary>
        /// <returns></returns>
        public async Task OpenSession()
        {
            if (this.SelectedIterationSetup != null && this.SelectedDomainOfExpertise != null)
            {
                this.IsOpeningSession = true;

                await this.sessionService.ReadIteration(this.SelectedEngineeringModel.IterationSetup
                    .First(x => x.Iid == this.SelectedIterationSetup.IterationSetupId), this.SelectedDomainOfExpertise);

                this.IsOpeningSession = false;
            }
        }

        /// <summary>
        /// Preselects the <see cref="Iteration" /> to open
        /// </summary>
        /// <param name="modelId">The <see cref="Guid" /> of the <see cref="EngineeringModel" /></param>
        /// <param name="iterationId">The <see cref="Guid" /> of the <see cref="Iteration" /> to open</param>
        /// <param name="domainId">The <see cref="Guid" /> of the <see cref="DomainOfExpertise" /> to select</param>
        public void PreSelectIteration(Guid modelId, Guid iterationId, Guid domainId)
        {
            this.selectedEngineeringModel = this.AvailableEngineeringModelSetups.FirstOrDefault(x => x.Iid == modelId);
            var iterationSetup = this.SelectedEngineeringModel?.IterationSetup.Find(x => x.IterationIid == iterationId);

            if (iterationSetup != null)
            {
                this.SelectedIterationSetup = new IterationData(iterationSetup);
            }

            this.AvailablesDomainOfExpertises = this.sessionService.GetModelDomains(this.SelectedEngineeringModel);
            this.SelectedDomainOfExpertise = this.AvailablesDomainOfExpertises.FirstOrDefault(x => x.Iid == domainId);
        }

        /// <summary>
        /// Compute the available collection based on the selected <see cref="EngineeringModelSetup" />
        /// </summary>
        private void ComputeAvailableCollections()
        {
            if (this.SelectedEngineeringModel == null)
            {
                this.AvailablesDomainOfExpertises = new List<DomainOfExpertise>();
                this.AvailableIterationSetups = new List<IterationData>();
                this.SelectedDomainOfExpertise = null;
                this.SelectedIterationSetup = null;
            }
            else
            {
                this.SelectedDomainOfExpertise = this.SelectedEngineeringModel.ActiveDomain.Find(x => x == this.sessionService.Session.ActivePerson.DefaultDomain);

                this.AvailablesDomainOfExpertises = this.sessionService.GetModelDomains(this.SelectedEngineeringModel);

                this.AvailableIterationSetups = this.SelectedEngineeringModel.IterationSetup
                    .Where(x => this.sessionService.OpenIterations.Items.All(i => i.Iid != x.IterationIid))
                    .OrderBy(x => x.IterationNumber)
                    .Select(x => new IterationData(x));

                this.SelectedIterationSetup = this.AvailableIterationSetups.Last();
            }
        }
    }
}
