// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTabViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.OpenTab
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

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
        public OpenTabViewModel(ISessionService sessionService, IConfigurationService configurationService, ITabsViewModel tabsViewModel) : base(sessionService, configurationService)
        {
            this.sessionService = sessionService;
            this.tabsViewModel = tabsViewModel;
        }

        /// <summary>
        /// Gets the <see cref="Iteration" /> from the <see cref="OpenModelViewModel.SelectedEngineeringModel" />
        /// </summary>
        private Iteration SelectedEngineeringModelIteration => this.sessionService.OpenIterations.Items.FirstOrDefault(x => ((EngineeringModel)x.Container).EngineeringModelSetup == this.SelectedEngineeringModel);

        /// <summary>
        /// Gets the collection of participant models
        /// </summary>
        public IEnumerable<EngineeringModelSetup> EngineeringModelSetups => this.sessionService.GetParticipantModels().OrderBy(x => x.Name);

        /// <summary>
        /// Gets the condition to check if the current selected model is already opened
        /// </summary>
        public bool IsCurrentModelOpened => this.sessionService.OpenEngineeringModels.Any(x => x.EngineeringModelSetup == this.SelectedEngineeringModel);

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
        /// <returns>A <see cref="Task" /> containing the operation <see cref="Result" /></returns>
        public override async Task<Result<Iteration>> OpenSession()
        {
            var result = new Result<Iteration>();

            if (!this.IsCurrentModelOpened)
            {
                result = await base.OpenSession();
            }
            else
            {
                if (this.SelectedDomainOfExpertise != this.SelectedIterationDomainOfExpertise)
                {
                    this.sessionService.SwitchDomain(this.SelectedEngineeringModelIteration, this.SelectedDomainOfExpertise);
                }
            }

            if (result.IsSuccess)
            {
                this.tabsViewModel.SelectedApplication = this.SelectedApplication;
            }

            return result;
        }
    }
}
