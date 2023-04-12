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
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Services.SubscriptionService;

    using ReactiveUI;

    /// <summary>
    /// View Model that handle the logic for the Parameter Editor application
    /// </summary>
    public class ParameterEditorBodyViewModel : SingleIterationApplicationBaseViewModel, IParameterEditorBodyViewModel
    {
        /// <summary>
        /// A collection of added <see cref="Thing" />s
        /// </summary>
        private readonly List<Thing> addedThings = new();

        /// <summary>
        /// A collection of deleted <see cref="Thing" />s
        /// </summary>
        private readonly List<Thing> deletedThings = new();

        /// <summary>
        /// Backing field for the <see cref="IsOwnedParameters" />
        /// </summary>
        private bool isOwnedParameters;

        /// <summary>
        /// A collection of updated <see cref="Thing" />s
        /// </summary>
        private readonly List<Thing> updatedThings = new();

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

            var observables = new List<IObservable<ObjectChangedEvent>>
            {
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterValueSetBase)),
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementBase)),
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterOrOverrideBase))
            };

            this.Disposables.Add(observables.Merge().Subscribe(this.RecordChange));
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
            if (!this.addedThings.Any() && !this.deletedThings.Any() && !this.updatedThings.Any())
            {
                return;
            }

            this.IsLoading = true;
            await Task.Delay(1);
            this.ParameterTableViewModel.RemoveRows(this.deletedThings.ToList());
            this.ParameterTableViewModel.UpdateRows(this.updatedThings.ToList());
            this.ParameterTableViewModel.AddRows(this.addedThings.ToList());
            this.ClearRecordedChange();
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
        protected override async Task OnIterationChanged()
        {
            await base.OnIterationChanged();
            this.IsOwnedParameters = true;
            this.ElementSelector.CurrentIteration = this.CurrentIteration;
            this.OptionSelector.CurrentIteration = this.CurrentIteration;
            this.ParameterTypeSelector.CurrentIteration = this.CurrentIteration;
            await this.InitializeTable();
        }

        /// <summary>
        /// Clears all recorded changed
        /// </summary>
        private void ClearRecordedChange()
        {
            this.deletedThings.Clear();
            this.updatedThings.Clear();
            this.addedThings.Clear();
        }

        /// <summary>
        /// Records an <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChangedEvent">The <see cref="ObjectChangedEvent" /></param>
        private void RecordChange(ObjectChangedEvent objectChangedEvent)
        {
            if (this.CurrentIteration == null || objectChangedEvent.ChangedThing.GetContainerOfType<Iteration>().Iid != this.CurrentIteration.Iid)
            {
                return;
            }

            switch (objectChangedEvent.EventKind)
            {
                case EventKind.Added:
                    this.addedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                case EventKind.Removed:
                    this.deletedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                case EventKind.Updated:
                    this.updatedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectChangedEvent.EventKind), "Unrecognised value EventKind value");
            }
        }

        /// <summary>
        /// Initialize the <see cref="IParameterTableViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task InitializeTable()
        {
            this.IsLoading = true;
            await Task.Delay(1);
            this.ParameterTableViewModel.InitializeViewModel(this.CurrentIteration, this.CurrentDomain, this.OptionSelector.SelectedOption);
            this.IsLoading = false;
        }

        /// <summary>
        /// Apply all the filters on the <see cref="IParameterTableViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ApplyFilters()
        {
            if (this.CurrentIteration != null)
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
