// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBodyViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using ReactiveUI;

    /// <summary>
    /// View Model that handle the logic for the Subscription Dashboard application
    /// </summary>
    public class SubscriptionDashboardBodyViewModel : SingleIterationApplicationBaseViewModel, ISubscriptionDashboardBodyViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionDashboardBodyViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="subscribedTable">The <see cref="ISubscribedTableViewModel" /></param>
        /// <param name="messageBus">The <see cref="CDPMessageBus"/></param>
        public SubscriptionDashboardBodyViewModel(ISessionService sessionService, ISubscribedTableViewModel subscribedTable, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.SubscribedTable = subscribedTable;

            this.Disposables.Add(this.WhenAnyValue(x => x.OptionSelector.SelectedOption,
                    x => x.ParameterTypeSelector.SelectedParameterType)
                .SubscribeAsync(_ => this.UpdateTables()));
        }

        /// <summary>
        /// Gets the <see cref="ISubscribedTableViewModel" />
        /// </summary>
        public ISubscribedTableViewModel SubscribedTable { get; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSubscriptionTableViewModel" />
        /// </summary>
        public IDomainOfExpertiseSubscriptionTableViewModel DomainOfExpertiseSubscriptionTable { get; } = new DomainOfExpertiseSubscriptionTableViewModel();

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; } = new OptionSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelector { get; } = new ParameterTypeSelectorViewModel();

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            this.OptionSelector.CurrentIteration = this.CurrentThing;
            this.ParameterTypeSelector.CurrentIteration = this.CurrentThing;

            var ownedSubscriptions = this.CurrentThing?.QueryOwnedParameterSubscriptions(this.CurrentDomain).ToList()
                                     ?? new List<ParameterSubscription>();

            var availableOptions = this.CurrentThing?.Option.ToList();

            var subscribedParameters = this.CurrentThing?.QuerySubscribedParameterByOthers(this.CurrentDomain).ToList()
                                       ?? new List<ParameterOrOverrideBase>();

            var availableParameterTypes = ownedSubscriptions.Select(x => x.ParameterType).ToList();
            availableParameterTypes.AddRange(subscribedParameters.Select(x => x.ParameterType));
            this.ParameterTypeSelector.FilterAvailableParameterTypes(availableParameterTypes.Select(x => x.Iid).Distinct());

            this.SubscribedTable.UpdateProperties(ownedSubscriptions, availableOptions, this.CurrentThing);
            this.DomainOfExpertiseSubscriptionTable.UpdateProperties(subscribedParameters);
            await Task.Delay(1);
            this.IsLoading = false;
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            await this.OnThingChanged();
            await this.UpdateTables();
        }

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnDomainChanged()
        {
            base.OnDomainChanged();
            return this.OnThingChanged();
        }

        /// <summary>
        /// Updates the <see cref="ISubscribedTableViewModel" /> and <see cref="IDomainOfExpertiseSubscriptionTableViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task UpdateTables()
        {
            this.IsLoading = true;
            await Task.Delay(1);

            this.SubscribedTable.ApplyFilters(this.OptionSelector.SelectedOption, this.ParameterTypeSelector.SelectedParameterType);
            this.DomainOfExpertiseSubscriptionTable.ApplyFilters(this.OptionSelector.SelectedOption, this.ParameterTypeSelector.SelectedParameterType);
            this.IsLoading = false;
        }
    }
}
