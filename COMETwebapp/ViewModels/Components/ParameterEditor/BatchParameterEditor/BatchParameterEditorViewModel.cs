// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BatchParameterEditorViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel used to apply batch operations for a parameter
    /// </summary>
    public class BatchParameterEditorViewModel : BelongsToIterationSelectorViewModel, IBatchParameterEditorViewModel
    {
        /// <summary>
        /// Gets the excluded <see cref="ParameterType" />s for applying the batch updates
        /// </summary>
        private readonly IEnumerable<ClassKind> excludedParameterTypes = [ClassKind.SampledFunctionParameterType, ClassKind.ArrayParameterType, ClassKind.CompoundParameterType];

        /// <summary>
        /// Gets the <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<BatchParameterEditorViewModel> logger;

        /// <summary>
        /// Gets the <see cref="ICDPMessageBus" />
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// The backing field for <see cref="SelectedCategory" />
        /// </summary>
        private Category selectedCategory;

        /// <summary>
        /// Creates a new instance of type <see cref="BatchParameterEditorViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public BatchParameterEditorViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<BatchParameterEditorViewModel> logger)
        {
            this.SessionService = sessionService;
            this.messageBus = messageBus;
            this.logger = logger;
            var eventCallbackFactory = new EventCallbackFactory();

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                AllowNullSelection = true,
                CurrentIteration = this.CurrentIteration
            };

            this.ConfirmCancelPopupViewModel = new ConfirmCancelPopupViewModel
            {
                OnCancel = eventCallbackFactory.Create(this, this.OnCancelPopup),
                OnConfirm = eventCallbackFactory.Create(this, this.OnConfirmPopup),
                ContentText = "This value will be applied to all valuesets of the selected parameter type in this engineering model. Are you sure?",
                HeaderText = "Apply Changes"
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterTypeSelectorViewModel.SelectedParameterType).Subscribe(this.OnSelectedParameterTypeChange));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.OptionSelectorViewModel.SelectedOption,
                    x => x.FiniteStateSelectorViewModel.SelectedActualFiniteState,
                    x => x.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise,
                    x => x.SelectedCategory)
                .Subscribe(_ => this.ApplyFilters()));
        }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; private set; }

        /// <summary>
        /// Gets or sets the visibility of the current component
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the loading value
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets a collection of all available categories
        /// </summary>
        public IEnumerable<Category> AvailableCategories => this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.DefinedCategory).Distinct();

        /// <summary>
        /// Gets or sets the selected category
        /// </summary>
        public Category SelectedCategory
        {
            get => this.selectedCategory;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategory, value);
        }

        /// <summary>
        /// Gets the <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ParameterTypeEditorSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelectorViewModel { get; private set; } = new ParameterTypeSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelectorViewModel { get; private set; } = new OptionSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IFiniteStateSelectorViewModel" />
        /// </summary>
        public IFiniteStateSelectorViewModel FiniteStateSelectorViewModel { get; private set; } = new FiniteStateSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" />
        /// </summary>
        public IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="ParameterValueSetBaseRowViewModel" />s
        /// </summary>
        public SourceList<ParameterValueSetBaseRowViewModel> Rows { get; private set; } = new();

        /// <summary>
        /// Gets or sets the collection of selected rows
        /// </summary>
        public IReadOnlyList<object> SelectedValueSetsRowsToUpdate { get; set; } = [];

        /// <summary>
        /// Method invoked for opening the batch update popup
        /// </summary>
        public void OpenPopup()
        {
            this.ParameterTypeSelectorViewModel.SelectedParameterType = null;
            this.OptionSelectorViewModel.SelectedOption = null;
            this.FiniteStateSelectorViewModel.SelectedActualFiniteState = null;
            this.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = null;
            this.SelectedCategory = null;
            this.SelectedValueSetsRowsToUpdate = [];
            this.IsVisible = true;
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.ParameterTypeSelectorViewModel.CurrentIteration = this.CurrentIteration;
            this.OptionSelectorViewModel.CurrentIteration = this.CurrentIteration;
            this.FiniteStateSelectorViewModel.CurrentIteration = this.CurrentIteration;
            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = this.CurrentIteration;

            var availableParameterTypeIids = this.CurrentIteration?
                .QueryParameterAndOverrideBases()
                .Select(x => x.ParameterType)
                .Where(x => !this.excludedParameterTypes.Contains(x.ClassKind))
                .Select(x => x.Iid);

            this.ParameterTypeSelectorViewModel.FilterAvailableParameterTypes(availableParameterTypeIids);
        }

        /// <summary>
        /// Method executed everytime the <see cref="ConfirmCancelPopupViewModel" /> is canceled
        /// </summary>
        private void OnCancelPopup()
        {
            this.ConfirmCancelPopupViewModel.IsVisible = false;
        }

        /// <summary>
        /// Method executed everytime the <see cref="ConfirmCancelPopupViewModel" /> is confirmed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnConfirmPopup()
        {
            this.IsLoading = true;
            this.ConfirmCancelPopupViewModel.IsVisible = false;
            this.IsVisible = false;

            this.logger.LogInformation("Applying manual value {value} to all selected parameters types with iid {iid}", this.ParameterTypeEditorSelectorViewModel.ValueSet.Manual, this.ParameterTypeSelectorViewModel.SelectedParameterType.Iid);
            var thingsToUpdate = new List<Thing>();

            foreach (var parameterValueSetRow in this.SelectedValueSetsRowsToUpdate.Cast<ParameterValueSetBaseRowViewModel>())
            {
                var valueSetClone = parameterValueSetRow.ParameterValueSetBase.Clone(true);
                valueSetClone.Manual = this.ParameterTypeEditorSelectorViewModel.ValueSet.Manual;
                thingsToUpdate.Add(valueSetClone);
            }

            await this.SessionService.CreateOrUpdateThings(this.CurrentIteration.Clone(false), thingsToUpdate);
            this.SelectedValueSetsRowsToUpdate = [];
            this.IsLoading = false;
        }

        /// <summary>
        /// Method executed everytime the selected parameter type has changed
        /// </summary>
        /// <param name="selectedParameterType">The newly selected parameter type</param>
        private void OnSelectedParameterTypeChange(ParameterType selectedParameterType)
        {
            var defaultParameterValueSet = new ParameterValueSet
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["-"])
            };

            this.ApplyFilters();
            this.ParameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(selectedParameterType, defaultParameterValueSet, false, this.messageBus);
        }

        /// <summary>
        /// Applies the selected filters for the <see cref="Rows" />
        /// </summary>
        private void ApplyFilters()
        {
            var parameterAndOverrideBases = this.CurrentIteration?.QueryParameterAndOverrideBases();

            if (parameterAndOverrideBases == null)
            {
                return;
            }

            if (this.ParameterTypeSelectorViewModel.SelectedParameterType == null)
            {
                return;
            }

            parameterAndOverrideBases = parameterAndOverrideBases.Where(x => x.ParameterType.Iid == this.ParameterTypeSelectorViewModel.SelectedParameterType.Iid);

            var parameterValueSets = parameterAndOverrideBases
                .OfType<Parameter>()
                .SelectMany(x => x.ValueSet);

            if (this.OptionSelectorViewModel.SelectedOption != null)
            {
                parameterValueSets = parameterValueSets.Where(x => x.ActualOption?.Iid == this.OptionSelectorViewModel.SelectedOption.Iid);
            }

            if (this.FiniteStateSelectorViewModel.SelectedActualFiniteState != null)
            {
                parameterValueSets = parameterValueSets.Where(x => x.ActualState?.Iid == this.FiniteStateSelectorViewModel.SelectedActualFiniteState.Iid);
            }

            if (this.SelectedCategory != null)
            {
                var parameterTypeCategories = this.ParameterTypeSelectorViewModel.SelectedParameterType.Category;

                parameterValueSets = parameterTypeCategories.Count > 0
                    ? parameterValueSets.Where(x => x.GetContainerOfType<Parameter>().ParameterType.Category.Contains(this.SelectedCategory))
                    : parameterValueSets.Where(x => x.GetContainerOfType<ElementDefinition>().Category.Contains(this.SelectedCategory));
            }

            if (this.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise != null)
            {
                parameterValueSets = parameterValueSets.Where(x => x.GetContainerOfType<Parameter>().Owner.Iid == this.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise.Iid);
            }

            this.Rows.Edit(action =>
            {
                action.Clear();
                action.AddRange(parameterValueSets.Select(x => new ParameterValueSetBaseRowViewModel(x)));
            });
        }
    }
}
