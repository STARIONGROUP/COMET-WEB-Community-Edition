// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the rows asociated to a <see cref="ParameterBase" />
    /// </summary>
    public class ParameterBaseRowViewModel : DisposableObject
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="isReadOnly">Value asserting if the row is readonly or not</param>
        /// <param name="parameterBase">the parameter of this row</param>
        /// <param name="valueSet">the valueSet of the parameter</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public ParameterBaseRowViewModel(ISessionService sessionService, bool isReadOnly, ParameterBase parameterBase, IValueSet valueSet, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;
            this.Parameter = parameterBase ?? throw new ArgumentNullException(nameof(parameterBase));
            this.ParameterType = this.Parameter.ParameterType;
            this.ValueSet = valueSet;

            this.InitializesProperties(isReadOnly);

            this.ParameterSwitchKindSelectorViewModel = new ParameterSwitchKindSelectorViewModel(this.Switch, this.IsReadOnly)
            {
                OnUpdate = new EventCallbackFactory().Create(this, this.UpdateParameterValueSwitch)
            };

            this.ParameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(this.ParameterType, this.ValueSet, this.IsReadOnly, messageBus)
            {
                ParameterValueChanged = new EventCallbackFactory().Create<(IValueSet,int)>(this, this.OnParameterValueChanged)
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterSwitchKindSelectorViewModel.SwitchValue)
                .Subscribe(_ => this.OnParameterValueSwitchChanged()));
        }

        /// <summary>
        /// Initializes this row view model properties
        /// </summary>
        /// <param name="isReadOnly">Value asserting if this row view model should be readonly or not</param>
        private void InitializesProperties(bool isReadOnly)
        {
            this.IsReadOnly = isReadOnly;
            this.ParameterName = this.Parameter.ParameterType is not null ? this.Parameter.ParameterType.Name : string.Empty;

            if (this.ValueSet is ParameterValueSetBase valueSetBase)
            {
                this.PublishedValue = valueSetBase.Published.First();

                if (this.Parameter.Scale != null)
                {
                    this.PublishedValue += $" [{this.Parameter.Scale.ShortName}]";
                }
            }

            this.OwnerName = this.Parameter.Owner is not null ? this.Parameter.Owner.ShortName : string.Empty;
            this.ModelCode = this.Parameter.ModelCode();
            this.ElementBaseName = (this.Parameter.Container as ElementBase)?.Name;
            this.ValueSetId = (this.ValueSet as ParameterValueSetBase)?.Iid ?? Guid.Empty;
            this.Option = this.ValueSet.ActualOption is not null ? this.ValueSet.ActualOption?.Name : string.Empty;
            this.State = this.ValueSet.ActualState is not null ? this.ValueSet.ActualState.Name : string.Empty;
            this.Switch = this.ValueSet.ValueSwitch;
        }

        /// <summary>
        /// Value asserting that the <see cref="ParameterBaseRowViewModel" /> is readonly
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets the published value
        /// </summary>
        public Guid ValueSetId { get; private set; }

        /// <summary>
        /// Gets the published value
        /// </summary>
        public string PublishedValue { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ParameterTypeEditorSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterBase" /> for this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public ParameterBase Parameter { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" /> for this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public ParameterType ParameterType { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ElementBase" /> used for grouping this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public string ElementBaseName { get; private set; }

        /// <summary>
        /// Gets the <see cref="Parameter" /> type name
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// Gets the <see cref="Parameter" /> owner name
        /// </summary>
        public string OwnerName { get; private set; }

        /// <summary>
        /// Gets the switch for the published value
        /// </summary>
        public ParameterSwitchKind Switch { get; private set; }

        /// <summary>
        /// Gets the <see cref="Parameter" /> model code
        /// </summary>
        public string ModelCode { get; private set; }

        /// <summary>
        /// Gets the <see cref="Option" /> name this <see cref="Parameter" /> is dependant on
        /// </summary>
        public string Option { get; private set; }

        /// <summary>
        /// Gets the <see cref="ActualFiniteState" /> name this <see cref="Parameter" /> is dependant on
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IValueSet" /> of this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public IValueSet ValueSet { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterSwitchKindSelectorViewModel ParameterSwitchKindSelectorViewModel { get; }

        /// <summary>
        /// Update this row view model properties 
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        public void UpdateProperties(bool readOnly)
        {
            this.InitializesProperties(readOnly);
            this.ParameterTypeEditorSelectorViewModel.UpdateProperties(this.IsReadOnly);
            this.ParameterSwitchKindSelectorViewModel.UpdateProperties(this.Switch, this.IsReadOnly);
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <param name="value">The updated <see cref="IValueSet"/> with the index</param>
        /// <returns>an asynchronous operation</returns>
        private async Task OnParameterValueChanged((IValueSet valueSet, int _) value)
        {
            if (!this.IsReadOnly && value.valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                await this.sessionService.UpdateThings(this.Parameter.GetContainerOfType<Iteration>(), new List<Thing>{parameterValueSetBase});
            }
        }

        /// <summary>
        /// Handles the change of <see cref="ParameterSwitchKind" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private void OnParameterValueSwitchChanged()
        {
            if (!this.IsReadOnly)
            {
                this.ParameterTypeEditorSelectorViewModel.UpdateSwitchKind(this.ParameterSwitchKindSelectorViewModel.SwitchValue);
            }
        }

        /// <summary>
        /// Update the <see cref="ParameterSwitchKind" />
        /// </summary>
        /// <returns></returns>
        private async Task UpdateParameterValueSwitch()
        {
            if (!this.IsReadOnly && this.ParameterSwitchKindSelectorViewModel.SwitchValue != this.ValueSet.ValueSwitch && this.ValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                clonedParameterValueSet.ValueSwitch = this.ParameterSwitchKindSelectorViewModel.SwitchValue;
                await this.sessionService.UpdateThings(this.Parameter.GetContainerOfType<Iteration>(), new List<Thing> { clonedParameterValueSet });
            }
        }
    }
}
