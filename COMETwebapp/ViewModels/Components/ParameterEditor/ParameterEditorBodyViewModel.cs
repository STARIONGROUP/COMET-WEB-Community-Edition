// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Services.SubscriptionService;

    using ReactiveUI;

    /// <summary>
    /// View Model that handle the logic for the Parameter Editor application
    /// </summary>
    public class ParameterEditorBodyViewModel : SingleIterationApplicationBaseViewModel, IParameterEditorBodyViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="IsOwnedParameters" />
        /// </summary>
        private bool isOwnedParameters;

        /// <summary>
        /// A collection of <see cref="Type" /> used to create <see cref="ObjectChangedEvent" /> subscriptions
        /// </summary>
        private static readonly IEnumerable<Type> ObjectChangedTypesOfInterest = new List<Type>
        {
            typeof(ElementBase),
            typeof(ParameterOrOverrideBase),
            typeof(ParameterValueSetBase),
        };
        
        /// <summary>
        /// Creates a new instance of <see cref="ParameterEditorBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="subscriptionService">the <see cref="ISubscriptionService" /></param>
        /// <param name="parameterTableView">The <see cref="IParameterTableViewModel" /></param>
        public ParameterEditorBodyViewModel(ISessionService sessionService, ISubscriptionService subscriptionService,
            IParameterTableViewModel parameterTableView) : base(sessionService)
        {
            this.SubscriptionService = subscriptionService;
            this.ParameterTableViewModel = parameterTableView;

            this.Disposables.Add(this.WhenAnyValue(x => x.ElementSelector.SelectedElementBase,
                x => x.OptionSelector.SelectedOption,
                x => x.ParameterTypeSelector.SelectedParameterType,
                x => x.IsOwnedParameters).SubscribeAsync(_ => this.ApplyFilters()));

            this.InitializeSubscriptions(ObjectChangedTypesOfInterest);
        }

        /// <summary>
        /// Gets or sets the <see cref="ISubscriptionService" />
        /// </summary>
        public ISubscriptionService SubscriptionService { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementBaseSelectorViewModel" />
        /// </summary>
        public IElementBaseSelectorViewModel ElementSelector { get; private set; } = new ElementBaseSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; private set; } = new OptionSelectorViewModel(false);

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelector { get; private set; } = new ParameterTypeSelectorViewModel();

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        public bool IsOwnedParameters
        {
            get => this.isOwnedParameters;
            set => this.RaiseAndSetIfChanged(ref this.isOwnedParameters, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel" />
        /// </summary>
        public IParameterTableViewModel ParameterTableViewModel { get; set; }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            if (!this.AddedThings.Any() && !this.DeletedThings.Any() && !this.UpdatedThings.Any())
            {
                return;
            }

            this.IsLoading = true;
            await Task.Delay(1);
            this.ParameterTableViewModel.RemoveRows(this.DeletedThings.ToList());
            this.ParameterTableViewModel.UpdateRows(this.UpdatedThings.ToList());
            this.ParameterTableViewModel.AddRows(this.AddedThings.ToList());
            this.ClearRecordedChanges();
            this.IsLoading = false;
        }

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnDomainChanged()
        {
            await base.OnDomainChanged();

            if (this.CurrentDomain != null)
            {
                this.IsLoading = true;
                await Task.Delay(1);
                this.ParameterTableViewModel.UpdateDomain(this.CurrentDomain);

                if (this.IsOwnedParameters)
                {
                    await this.ApplyFilters();
                }

                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.IsOwnedParameters = true;
            this.ElementSelector.CurrentIteration = this.CurrentThing;
            this.OptionSelector.CurrentIteration = this.CurrentThing;
            this.ParameterTypeSelector.CurrentIteration = this.CurrentThing;
            await this.InitializeTable();
        }

        /// <summary>
        /// Initialize the <see cref="IParameterTableViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task InitializeTable()
        {
            this.IsLoading = true;
            await Task.Delay(1);
            this.ParameterTableViewModel.InitializeViewModel(this.CurrentThing, this.CurrentDomain, this.OptionSelector.SelectedOption);
            this.IsLoading = false;
        }

        /// <summary>
        /// Apply all the filters on the <see cref="IParameterTableViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ApplyFilters()
        {
            if (this.CurrentThing != null)
            {
                this.IsLoading = true;
                await Task.Delay(1);

                this.ParameterTableViewModel.ApplyFilters(this.OptionSelector.SelectedOption, this.ElementSelector.SelectedElementBase,
                    this.ParameterTypeSelector.SelectedParameterType, this.IsOwnedParameters);

                this.IsLoading = false;
            }
        }
    }
}
