// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscribedTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// View Model that provide content related to <see cref="ElementBase" /> that contains
    /// <see cref="ParameterOrOverrideBase" /> with <see cref="ParameterSubscription" /> owned by the current
    /// <see cref="DomainOfExpertise" />
    /// </summary>
    public class SubscribedTableViewModel : DisposableObject, ISubscribedTableViewModel
    {
        /// <summary>
        /// Gets the <see cref="ISubscriptionService" />
        /// </summary>
        private readonly ISubscriptionService subscriptionService;

        /// <summary>
        /// A collection of all <see cref="ParameterSubscriptionRowViewModel" />
        /// </summary>
        private IEnumerable<ParameterSubscriptionRowViewModel> allRows = new List<ParameterSubscriptionRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="DidSubscriptionsChanged" />
        /// </summary>
        private bool didSubscriptionsChanged;

        /// <summary>
        /// A collection of filtered <see cref="ParameterSubscriptionRowViewModel" />
        /// </summary>
        private List<ParameterSubscriptionRowViewModel> filteredRows = new();

        /// <summary>
        /// The current <see cref="Iteration" />
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Backing field for <see cref="ShowOnlyChangedSubscription" />
        /// </summary>
        private bool showOnlyChangedSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribedTableViewModel" /> class.
        /// </summary>
        /// <param name="subscriptionService">The <see cref="ISubscriptionService" /></param>
        public SubscribedTableViewModel(ISubscriptionService subscriptionService)
        {
            this.subscriptionService = subscriptionService;

            this.Disposables.Add(this.WhenAnyValue(x => x.subscriptionService.SubscriptionUpdateCount)
                .Subscribe(_ => this.UpdateSubscriptionsChangedProperty()));

            this.Disposables.Add(this.WhenAnyValue(x => x.ShowOnlyChangedSubscription)
                .Subscribe(_ => this.ApplyRowsVisibility()));
        }

        /// <summary>
        /// Value asserting that any <see cref="ParameterSubscription" /> had changed with an <see cref="Iteration" /> update
        /// </summary>
        public bool DidSubscriptionsChanged
        {
            get => this.didSubscriptionsChanged;
            private set => this.RaiseAndSetIfChanged(ref this.didSubscriptionsChanged, value);
        }

        /// <summary>
        /// Value indicating if the rows should only display <see cref="ParameterSubscription" /> that has changed
        /// </summary>
        public bool ShowOnlyChangedSubscription
        {
            get => this.showOnlyChangedSubscription;
            set => this.RaiseAndSetIfChanged(ref this.showOnlyChangedSubscription, value);
        }

        /// <summary>
        /// A reactive collection of <see cref="ParameterSubscriptionRowViewModel" /> to display
        /// </summary>
        public SourceList<ParameterSubscriptionRowViewModel> Rows { get; private set; } = new();

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="subscriptions">A collection of <see cref="ParameterSubscription" /></param>
        /// <param name="availableOptions">A collection of available <see cref="Option" /></param>
        /// <param name="newIteration">The current <see cref="Iteration" /></param>
        public void UpdateProperties(IEnumerable<ParameterSubscription> subscriptions, IEnumerable<Option> availableOptions, Iteration newIteration)
        {
            this.iteration = newIteration;
            var rows = new List<ParameterSubscriptionRowViewModel>();
            availableOptions = availableOptions.ToList();

            foreach (var parameterSubscription in subscriptions)
            {
                if (!parameterSubscription.IsOptionDependent)
                {
                    rows.AddRange(InitializeParameterSubscriptionRowViewModels(parameterSubscription, null));
                }
                else
                {
                    foreach (var option in availableOptions)
                    {
                        rows.AddRange(InitializeParameterSubscriptionRowViewModels(parameterSubscription, option));
                    }
                }
            }

            this.allRows = rows;
            this.filteredRows = rows;
            this.Rows.Clear();
            this.Rows.AddRange(this.allRows);
            this.UpdateSubscriptionsChangedProperty();
        }

        /// <summary>
        /// Apply filters on <see cref="ParameterSubscriptionRowViewModel" /> based on the <see cref="Option" /> and
        /// <see cref="ParameterType" />
        /// </summary>
        /// <param name="selectedOption">The selected <see cref="Option" /></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType" /></param>
        public void ApplyFilters(Option selectedOption, ParameterType selectedParameterType)
        {
            this.filteredRows = new List<ParameterSubscriptionRowViewModel>(this.allRows);

            if (selectedOption != null)
            {
                this.filteredRows.RemoveAll(x => x.Element is ElementUsage usage && usage.ExcludeOption.Any(o => o.Iid == selectedOption.Iid));
                this.filteredRows.RemoveAll(x => x.ValueSet.ActualOption == null || x.ValueSet.ActualOption.Iid != selectedOption.Iid);
            }

            if (selectedParameterType != null)
            {
                this.filteredRows.RemoveAll(x => x.Parameter.ParameterType.Iid != selectedParameterType.Iid);
            }

            this.ApplyRowsVisibility();
        }

        /// <summary>
        /// Validates all changes of <see cref="ParameterValueSetBase" /> related to the current view model
        /// </summary>
        public void ValidateAllChanges()
        {
            this.subscriptionService.UpdateTrackedSubscriptions(this.iteration);
            this.subscriptionService.ComputeUpdateSinceLastTracking();
            this.ShowOnlyChangedSubscription = false;
        }

        /// <summary>
        /// Updates the <see cref="DidSubscriptionsChanged" /> property based on new values of the
        /// <see cref="ISubscriptionService" />
        /// </summary>
        private void UpdateSubscriptionsChangedProperty()
        {
            if (this.iteration != null && this.subscriptionService.SubscriptionsWithUpdate.TryGetValue(this.iteration.Iid, out var subscriptionsWithUpdate))
            {
                this.DidSubscriptionsChanged = subscriptionsWithUpdate.Any();
            }
            else
            {
                this.DidSubscriptionsChanged = false;
            }
        }

        /// <summary>
        /// Add into the <see cref="Rows" /> collection filtered <see cref="ParameterSubscriptionRowViewModel" /> based on the
        /// <see cref="ShowOnlyChangedSubscription" /> value
        /// </summary>
        private void ApplyRowsVisibility()
        {
            this.Rows.Clear();

            this.Rows.AddRange(this.ShowOnlyChangedSubscription
                ? this.filteredRows.Where(x => this.subscriptionService.SubscriptionsWithUpdate[this.iteration.Iid]
                    .Any(s => s == x.SubscriptionValueSet.Iid))
                : this.filteredRows);
        }

        /// <summary>
        /// Initializes a collection of <see cref="ParameterSubscriptionRowViewModel" /> based on an <see cref="Option" /> and an
        /// <see cref="ActualFiniteStateList" />
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> to represent</param>
        /// <param name="currentOption">The current <see cref="Option" /></param>
        /// <param name="state">The current <see cref="ActualFiniteStateList" /></param>
        /// <returns>A collection of <see cref="ParameterSubscriptionRowViewModel" /></returns>
        private static IEnumerable<ParameterSubscriptionRowViewModel> InitializeParameterSubscriptionRowViewModels(ParameterSubscription subscription, Option currentOption, ActualFiniteStateList state)
        {
            return state.ActualState.OrderBy(x => x.Name)
                .Select(actualState => new ParameterSubscriptionRowViewModel(subscription, currentOption, actualState));
        }

        /// <summary>
        /// Initializes a collection of <see cref="ParameterSubscriptionRowViewModel" /> based on an <see cref="Option" />
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> to represent</param>
        /// <param name="option">The current <see cref="Option" /></param>
        /// <returns>A collection of <see cref="ParameterSubscriptionRowViewModel" /></returns>
        private static IEnumerable<ParameterSubscriptionRowViewModel> InitializeParameterSubscriptionRowViewModels(ParameterSubscription subscription, Option option)
        {
            var rows = new List<ParameterSubscriptionRowViewModel>();

            if (subscription.StateDependence == null)
            {
                rows.Add(new ParameterSubscriptionRowViewModel(subscription, option, null));
            }
            else
            {
                rows.AddRange(InitializeParameterSubscriptionRowViewModels(subscription, option, subscription.StateDependence));
            }

            return rows;
        }
    }
}
