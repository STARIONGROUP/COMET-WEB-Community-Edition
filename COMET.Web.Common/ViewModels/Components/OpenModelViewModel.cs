// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModelViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables a user to open an <see cref="EngineeringModel" />
    /// </summary>
    public class OpenModelViewModel : DisposableObject, IOpenModelViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Gets the <see cref="IConfigurationService" />
        /// </summary>
        private readonly IConfigurationService configurationService;

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
        /// <param name="configurationService">The <see cref="IConfigurationService"/></param>
        public OpenModelViewModel(ISessionService sessionService, IConfigurationService configurationService)
        {
            this.sessionService = sessionService;
            this.configurationService = configurationService;

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

            var filteredEngineeringModelSetups = availableEngineeringModelSetups;

            var rdlFilter = this.configurationService.ServerConfiguration?.RdlFilter;

            if (rdlFilter != null)
            {
                filteredEngineeringModelSetups = filteredEngineeringModelSetups.Where(x =>
                {
                    return x.RequiredRdl.Exists(c => rdlFilter.RdlShortNames.Contains(c.RequiredRdl.ShortName))
                           && rdlFilter.Kinds.Contains(x.Kind);
                }).ToList();
            }

            this.AvailableEngineeringModelSetups = filteredEngineeringModelSetups;
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
