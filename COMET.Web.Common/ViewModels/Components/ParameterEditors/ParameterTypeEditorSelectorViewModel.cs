// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
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
        public int ValueArrayIndex { get; }

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
            parameterViewModel.ParameterValueChanged = this.ParameterValueChanged;

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
    }
}
