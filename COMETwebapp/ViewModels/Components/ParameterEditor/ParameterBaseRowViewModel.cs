// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;
    using COMETwebapp.ViewModels.Components.Shared.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the rows asociated to a <see cref="ParameterBase" />
    /// </summary>
    public class ParameterBaseRowViewModel : DisposableObject, IParameterBaseRowViewModel
    {
        /// <summary>
        /// Value asserting that the <see cref="ParameterBaseRowViewModel" /> is readonly
        /// </summary>
        private readonly bool isReadOnly;

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
        public ParameterBaseRowViewModel(ISessionService sessionService, bool isReadOnly, ParameterBase parameterBase, IValueSet valueSet)
        {
            this.isReadOnly = isReadOnly;
            this.sessionService = sessionService;
            this.Parameter = parameterBase ?? throw new ArgumentNullException(nameof(parameterBase));
            this.ParameterType = this.Parameter.ParameterType;
            this.ParameterName = this.Parameter.ParameterType is not null ? this.Parameter.ParameterType.Name : string.Empty;
            this.OwnerName = this.Parameter.Owner is not null ? this.Parameter.Owner.ShortName : string.Empty;
            this.ModelCode = this.Parameter.ModelCode();
            this.ElementBaseName = (parameterBase.Container as ElementBase)?.Name;
            this.ValueSet = valueSet;
            this.Option = valueSet.ActualOption is not null ? valueSet.ActualOption?.Name : string.Empty;
            this.State = valueSet.ActualState is not null ? valueSet.ActualState.Name : string.Empty;
            this.Switch = valueSet.ValueSwitch;

            this.ParameterSwitchKindSelectorViewModel = new ParameterSwitchKindSelectorViewModel(this.Switch, isReadOnly)
            {
                OnUpdate = new EventCallbackFactory().Create(this, this.UpdateParameterValueSwitch)
            };

            this.ParameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(this.ParameterType, this.ValueSet, isReadOnly)
            {
                ParameterValueChanged = new EventCallbackFactory().Create<IValueSet>(this, this.OnParameterValueChanged)
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterSwitchKindSelectorViewModel.SwitchValue)
                .Subscribe(_ => this.OnParameterValueSwitchChanged()));
        }

        /// <summary>
        /// Gets the <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ParameterTypeEditorSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterBase" /> for this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public ParameterBase Parameter { get; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" /> for this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ElementBase" /> used for grouping this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public string ElementBaseName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter" /> type name
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter" /> owner name
        /// </summary>
        public string OwnerName { get; }

        /// <summary>
        /// Gets the switch for the published value
        /// </summary>
        public ParameterSwitchKind Switch { get; private set; }

        /// <summary>
        /// Gets the <see cref="Parameter" /> model code
        /// </summary>
        public string ModelCode { get; }

        /// <summary>
        /// Gets the <see cref="Option" /> name this <see cref="Parameter" /> is dependant on
        /// </summary>
        public string Option { get; }

        /// <summary>
        /// Gets the <see cref="ActualFiniteState" /> name this <see cref="Parameter" /> is dependant on
        /// </summary>
        public string State { get; }

        /// <summary>
        /// Gets or sets the <see cref="IValueSet" /> of this <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public IValueSet ValueSet { get; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterSwitchKindSelectorViewModel ParameterSwitchKindSelectorViewModel { get; }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        private async Task OnParameterValueChanged(IValueSet value)
        {
            if (!this.isReadOnly && value is ParameterValueSetBase parameterValueSetBase)
            {
                await this.sessionService.UpdateThings(this.Parameter.GetContainerOfType<Iteration>(), new List<Thing> { parameterValueSetBase });
            }
        }

        /// <summary>
        /// Handles the change of <see cref="ParameterSwitchKind" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private void OnParameterValueSwitchChanged()
        {
            if (!this.isReadOnly)
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
            if (!this.isReadOnly && this.ParameterSwitchKindSelectorViewModel.SwitchValue != this.ValueSet.ValueSwitch && this.ValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                clonedParameterValueSet.ValueSwitch = this.ParameterSwitchKindSelectorViewModel.SwitchValue;
                await this.sessionService.UpdateThings(this.Parameter.GetContainerOfType<Iteration>(), new List<Thing> { clonedParameterValueSet });
            }
        }
    }
}
