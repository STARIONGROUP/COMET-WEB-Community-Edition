// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelDashboardBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ModelDashboard
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.ModelDashboard.Elements;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    using ReactiveUI;

    /// <summary>
    /// View Model that handle the logic for the Model Dashboard application
    /// </summary>
    public class ModelDashboardBodyViewModel : SingleIterationApplicationBaseViewModel, IModelDashboardBodyViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDashboardBodyViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="parameterDashboard">The <see cref="IParameterDashboardViewModel" /></param>
        public ModelDashboardBodyViewModel(ISessionService sessionService, IParameterDashboardViewModel parameterDashboard) : base(sessionService)
        {
            this.ParameterDashboard = parameterDashboard;

            this.Disposables.Add(this.WhenAnyValue(x => x.FiniteStateSelector.SelectedActualFiniteState,
                    x => x.OptionSelector.SelectedOption,
                    x => x.ParameterTypeSelector.SelectedParameterType)
                .SubscribeAsync(_ => this.UpdateDashboards()));
        }

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementDashboardViewModel" />
        /// </summary>
        public IElementDashboardViewModel ElementDashboard { get; private set; } = new ElementDashboardViewModel();

        /// <summary>
        /// The <see cref="IParameterDashboardViewModel" />
        /// </summary>
        public IParameterDashboardViewModel ParameterDashboard { get; }

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; private set; } = new OptionSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IFiniteStateSelectorViewModel" />
        /// </summary>
        public IFiniteStateSelectorViewModel FiniteStateSelector { get; private set; } = new FiniteStateSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelector { get; private set; } = new ParameterTypeSelectorViewModel();

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            return this.OnIterationChanged();
        }

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnDomainChanged()
        {
            await base.OnDomainChanged();
            await this.UpdateDashboards();
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnIterationChanged()
        {
            await base.OnIterationChanged();
            this.OptionSelector.CurrentIteration = this.CurrentIteration;
            this.FiniteStateSelector.CurrentIteration = this.CurrentIteration;
            this.ParameterTypeSelector.CurrentIteration = this.CurrentIteration;

            this.AvailableDomains = this.CurrentIteration == null
                ? Enumerable.Empty<DomainOfExpertise>()
                : this.SessionService.GetModelDomains((EngineeringModelSetup)this.CurrentIteration.IterationSetup.Container);

            this.CurrentDomain = this.CurrentIteration == null ? null : this.SessionService.GetDomainOfExpertise(this.CurrentIteration);
            await this.UpdateDashboards();
        }

        /// <summary>
        /// Update the dashboard view models properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task UpdateDashboards()
        {
            this.IsLoading = true;
            await Task.Delay(1);

            this.ParameterDashboard.UpdateProperties(this.CurrentIteration, this.OptionSelector.SelectedOption,
                this.FiniteStateSelector.SelectedActualFiniteState, this.ParameterTypeSelector.SelectedParameterType,
                this.CurrentDomain, this.AvailableDomains);

            this.ElementDashboard.UpdateProperties(this.CurrentIteration, this.CurrentDomain);
            this.IsLoading = false;
        }
    }
}
