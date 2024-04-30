// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// ViewModel used to handle creation of <see cref="IParameterEditorBaseViewModel{T}" />
    /// </summary>
    public class ParameterTypeEditorSelectorViewModel : IParameterTypeEditorSelectorViewModel
    {
        /// <summary>
        /// Gets the <see cref="ICDPMessageBus" />
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The <see cref="IHaveValueSetViewModel" />
        /// </summary>
        private IHaveValueSetViewModel haveValueSetViewModel;

        /// <summary>
        /// Gets if the Editor is readonly.
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// A preset <see cref="ParameterSwitchKind" />
        /// </summary>
        private ParameterSwitchKind? preSetSwitchKind;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorSelectorViewModel" />
        /// </summary>
        /// <param name="parameterType">the <see cref="ParameterType" /> used for this view model</param>
        /// <param name="valueSet">the value set asociated to the ParameterTypeEditor</param>
        /// <param name="isReadOnly">Value asserting that the <see cref="IParameterEditorBaseViewModel{T}" /> should be readonly</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        public ParameterTypeEditorSelectorViewModel(ParameterType parameterType, IValueSet valueSet, bool isReadOnly, ICDPMessageBus messageBus, int valueArrayIndex = 0)
        {
            this.InitializesProperties(isReadOnly);
            this.ValueSet = valueSet;
            this.ParameterType = parameterType;
            this.ValueArrayIndex = valueArrayIndex;
            this.messageBus = messageBus;
        }

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorSelectorViewModel" />
        /// </summary>
        /// <param name="parameterType">the <see cref="ParameterType" /> used for this view model</param>
        /// <param name="valueSet">the value set asociated to the ParameterTypeEditor</param>
        /// <param name="isReadOnly">Value asserting that the <see cref="IParameterEditorBaseViewModel{T}" /> should be readonly</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        /// <param name="switchKind">The <see cref="ParameterSwitchKind" /></param>
        public ParameterTypeEditorSelectorViewModel(ParameterType parameterType, IValueSet valueSet, bool isReadOnly, ICDPMessageBus messageBus, int valueArrayIndex, ParameterSwitchKind switchKind)
            : this(parameterType, valueSet, isReadOnly, messageBus, valueArrayIndex)
        {
            this.preSetSwitchKind = switchKind;
        }

        /// <summary>
        /// Gets the index of the value changed in the value sets
        /// </summary>
        public int ValueArrayIndex { get; private set; }

        /// <summary>
        /// Gets or sets the value set of this <see cref="ParameterType" />
        /// </summary>
        public IValueSet ValueSet { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" />
        /// </summary>
        public ParameterType ParameterType { get; private set; }

        /// <summary>
        /// Event Callback for when a value has changed on the parameter
        /// </summary>
        public EventCallback<(IValueSet, int)> ParameterValueChanged { get; set; }

        /// <summary>
        /// Gets the <see cref="MeasurementScale" /> to use
        /// </summary>
        public MeasurementScale Scale { get; set; }

        /// <summary>
        /// Creates a view model for the corresponding editor
        /// </summary>
        /// <typeparam name="T">the parameter type</typeparam>
        /// <returns>the view model</returns>
        public IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T : ParameterType
        {
            this.haveValueSetViewModel = this.ParameterType switch
            {
                BooleanParameterType booleanParameterType => new BooleanParameterTypeEditorViewModel(booleanParameterType, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                CompoundParameterType compoundParameterType => new CompoundParameterTypeEditorViewModel(compoundParameterType, this.ValueSet, this.isReadOnly, this.messageBus, this.ValueArrayIndex),
                DateParameterType dateParameterType => new DateParameterTypeEditorViewModel(dateParameterType, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                DateTimeParameterType dateTimeParameterType => new DateTimeParameterTypeEditorViewModel(dateTimeParameterType, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                EnumerationParameterType enumerationParameterType => new EnumerationParameterTypeEditorViewModel(enumerationParameterType, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                QuantityKind quantityKind => new QuantityKindParameterTypeEditorViewModel(quantityKind, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                TextParameterType textParameterType => new TextParameterTypeEditorViewModel(textParameterType, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                TimeOfDayParameterType timeOfDayParameterType => new TimeOfDayParameterTypeEditorViewModel(timeOfDayParameterType, this.ValueSet, this.isReadOnly, this.ValueArrayIndex),
                SampledFunctionParameterType sampledFunctionParameterType => new SampledFunctionParameterTypeEditorViewModel(sampledFunctionParameterType, this.ValueSet, this.isReadOnly, this.messageBus, this.ValueArrayIndex),
                _ => throw new NotImplementedException($"The ViewModel for the {this.ParameterType} has not been implemented")
            };

            var parameterViewModel = (this.haveValueSetViewModel as IParameterEditorBaseViewModel<T>)!;
            parameterViewModel.ParameterValueChanged = new EventCallbackFactory().Create<(IValueSet, int)>(this, this.OnParameterValueChange);

            if (this.preSetSwitchKind.HasValue)
            {
                this.UpdateSwitchKind(this.preSetSwitchKind.Value);
            }

            return parameterViewModel;
        }

        /// <summary>
        /// Updates the <see cref="ParameterSwitchKind" /> value
        /// </summary>
        /// <param name="switchValue">The <see cref="ParameterSwitchKind" /></param>
        public void UpdateSwitchKind(ParameterSwitchKind switchValue)
        {
            this.preSetSwitchKind = switchValue;
            this.haveValueSetViewModel?.UpdateParameterSwitchKind(switchValue);
        }

        /// <summary>
        /// Updates the associated <see cref="IParameterEditorBaseViewModel{T}" /> properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        public void UpdateProperties(bool readOnly)
        {
            this.InitializesProperties(readOnly);
            this.preSetSwitchKind = null;
            this.haveValueSetViewModel?.UpdateProperties(this.isReadOnly);
        }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        private void InitializesProperties(bool readOnly)
        {
            this.isReadOnly = readOnly;
        }

        /// <summary>
        /// Method executed when the parameter value has changed
        /// </summary>
        /// <param name="callbackValues">The callback event values</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnParameterValueChange((IValueSet, int) callbackValues)
        {
            this.ValueSet = callbackValues.Item1;
            this.ValueArrayIndex = callbackValues.Item2;

            await this.ParameterValueChanged.InvokeAsync(callbackValues);
        }
    }
}
