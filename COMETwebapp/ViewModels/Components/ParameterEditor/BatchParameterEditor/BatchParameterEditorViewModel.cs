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

    //TODO
    // Need to make the user select the value sets available to update for each parameter/element definition, because it depends on the actual state and the option
    // Use a grid to display the selection => filters with options and 
    // Exclude the parameter types SFPT, Array and Compound

    /// <summary>
    /// ViewModel used to apply batch operations for a parameter
    /// </summary>
    public class BatchParameterEditorViewModel : BelongsToIterationSelectorViewModel, IBatchParameterEditorViewModel
    {
        /// <summary>
        /// Gets the <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<BatchParameterEditorViewModel> logger;

        /// <summary>
        /// The backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Gets or sets the selected <see cref="IValueSet" />
        /// </summary>
        private IValueSet SelectedValueSet { get; set; }

        /// <summary>
        /// Gets the <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Gets the excluded <see cref="ParameterType"/>s for applying the batch updates
        /// </summary>
        private IEnumerable<ClassKind> excludedParameterTypes = [ClassKind.SampledFunctionParameterType, ClassKind.ArrayParameterType, ClassKind.CompoundParameterType];

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

            this.ConfirmCancelPopupViewModel = new ConfirmCancelPopupViewModel
            {
                OnCancel = eventCallbackFactory.Create(this, this.OnCancelPopup),
                OnConfirm = eventCallbackFactory.Create(this, this.OnConfirmPopup),
                ContentText = "This value will be applied to all valuesets of the selected parameter type in this engineering model. Are you sure?",
                HeaderText = "Apply Changes"
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterTypeSelectorViewModel.SelectedParameterType).Subscribe(this.OnSelectedParameterTypeChange));
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
        /// Gets the <see cref="IConfirmCancelPopupViewModel" />
        /// </summary>
        public IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="ParameterValueSetBaseRowViewModel"/>s
        /// </summary>
        public SourceList<ParameterValueSetBaseRowViewModel> Rows { get; private set; } = new();

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.ParameterTypeSelectorViewModel.CurrentIteration = this.CurrentIteration;
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
        private async Task OnConfirmPopup()
        {
            this.IsLoading = true;
            this.ConfirmCancelPopupViewModel.IsVisible = false;
            this.IsVisible = false;

            this.logger.LogInformation("Applying manual value {value} to all parameters types with iid {iid}", this.SelectedValueSet.Manual, this.ParameterTypeSelectorViewModel.SelectedParameterType.Iid);
            var thingsToUpdate = new List<Thing>();

            foreach (var parameterValueSet in this.Rows.Items.Select(x => x.ParameterValueSetBase))
            {
                var valueSetClone = parameterValueSet.Clone(true);
                valueSetClone.Manual = this.SelectedValueSet.Manual;
                thingsToUpdate.Add(valueSetClone);
            }

            await this.SessionService.CreateOrUpdateThings(this.CurrentIteration.Clone(false), thingsToUpdate);
            await this.SessionService.RefreshSession();
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

            this.SelectedValueSet = defaultParameterValueSet;

            var parameterValueSets = this.CurrentIteration?
                .QueryParameterAndOverrideBases()
                .Where(x => x.ParameterType.Iid == selectedParameterType.Iid)
                .Cast<Parameter>()
                .SelectMany(x => x.ValueSet);

            this.Rows.Edit(action =>
            {
                action.Clear();
                action.AddRange(parameterValueSets?.Select(x => new ParameterValueSetBaseRowViewModel(x)) ?? Array.Empty<ParameterValueSetBaseRowViewModel>());
            });

            this.ParameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(selectedParameterType, defaultParameterValueSet, false, this.messageBus)
            {
                ParameterValueChanged = new EventCallbackFactory().Create<(IValueSet, int)>(this, callbackValueSet => { this.SelectedValueSet = callbackValueSet.Item1; })
            };
        }
    }
}
