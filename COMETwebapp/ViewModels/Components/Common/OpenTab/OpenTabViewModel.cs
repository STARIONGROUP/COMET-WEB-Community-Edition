// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTabViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.Common.OpenTab
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Pages;

    using FluentResults;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables a user to open an <see cref="EngineeringModel" />
    /// </summary>
    public class OpenTabViewModel : OpenModelViewModel, IOpenTabViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="ITabsViewModel" />
        /// </summary>
        private readonly ITabsViewModel tabsViewModel;

        /// <summary>
        /// Backing field for <see cref="SelectedApplication" />
        /// </summary>
        private TabbedApplication selectedApplication;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTabViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="configurationService">The <see cref="IConfigurationService" /></param>
        /// <param name="tabsViewModel">The <see cref="ITabsViewModel" /></param>
        /// <param name="cacheService">The <see cref="ICacheService"/></param>
        public OpenTabViewModel(ISessionService sessionService, IConfigurationService configurationService, ITabsViewModel tabsViewModel, ICacheService cacheService) : base(sessionService, configurationService, cacheService)
        {
            this.sessionService = sessionService;
            this.tabsViewModel = tabsViewModel;
            this.Disposables.Add(this.WhenAnyValue(x => x.tabsViewModel.SelectedApplication).Subscribe(application => { this.SelectedApplication = application; }));
        }

        /// <summary>
        /// Gets the <see cref="Iteration" /> from the <see cref="OpenModelViewModel.SelectedEngineeringModel" />
        /// </summary>
        private Iteration SelectedEngineeringModelIteration => this.sessionService.OpenIterations.Items.FirstOrDefault(x => x.IterationSetup.Iid == this.SelectedIterationSetup?.IterationSetupId);

        /// <summary>
        /// Gets the collection of participant models
        /// </summary>
        public IEnumerable<EngineeringModelSetup> EngineeringModelSetups => this.sessionService.GetParticipantModels().OrderBy(x => x.Name);

        /// <summary>
        /// Gets the condition to check if the current selected iteration is already opened
        /// </summary>
        public bool IsCurrentIterationOpened => this.SelectedEngineeringModelIteration != null;

        /// <summary>
        /// Gets a value indicating that an already open <see cref="Iteration" /> may be selected. Opening a tab on an iteration
        /// that is already open is the normal way to show that same iteration in another view, for instance the Model Editor and
        /// the System Representation side by side, so the iteration is not opened again, only a new tab is created.
        /// </summary>
        protected override bool CanSelectAlreadyOpenIteration => true;

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> from the <see cref="OpenModelViewModel.SelectedIterationSetup" />
        /// </summary>
        public DomainOfExpertise SelectedIterationDomainOfExpertise => this.sessionService.GetDomainOfExpertise(this.SelectedEngineeringModelIteration);

        /// <summary>
        /// The selected <see cref="TabbedApplication" />
        /// </summary>
        public TabbedApplication SelectedApplication
        {
            get => this.selectedApplication;
            set => this.RaiseAndSetIfChanged(ref this.selectedApplication, value);
        }

        /// <summary>
        /// Opens the <see cref="EngineeringModel" /> based on the selected field
        /// </summary>
        /// <param name="panel">The <see cref="TabPanelInformation" /> for which the new tab will be opened</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OpenTab(TabPanelInformation panel)
        {
            Result<Iteration> result;
            var isIteration = this.SelectedApplication?.ThingTypeOfInterest == typeof(Iteration);

            if (this.SelectedApplication?.ThingTypeOfInterest is null)
            {
                this.tabsViewModel.CreateNewTab(this.SelectedApplication, Guid.Empty, panel);
                return;
            }

            if (!this.IsCurrentIterationOpened)
            {
                result = await base.OpenSession();
            }
            else
            {
                if (this.SelectedDomainOfExpertise != this.SelectedIterationDomainOfExpertise)
                {
                    this.sessionService.SwitchDomain(this.SelectedEngineeringModelIteration, this.SelectedDomainOfExpertise);
                }

                result = Result.Ok(this.SelectedEngineeringModelIteration);
            }

            this.cacheService.AddOrUpdateBrowserSessionSetting(BrowserSessionSettingKey.LastUsedIterationData, this.SelectedIterationSetup);
            
            this.cacheService.AddOrUpdateBrowserSessionSetting(BrowserSessionSettingKey.LastUsedDomainOfExpertise, this.SelectedDomainOfExpertise);

            this.cacheService.AddOrUpdateBrowserSessionSetting(BrowserSessionSettingKey.LastUsedEngineeringModel, this.SelectedEngineeringModel);

            if (result.IsSuccess)
            {
                this.tabsViewModel.CreateNewTab(this.SelectedApplication, isIteration ? this.SelectedEngineeringModelIteration.Iid : this.SelectedEngineeringModel.EngineeringModelIid, panel);
            }
        }
    }
}
