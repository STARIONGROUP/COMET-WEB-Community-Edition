// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorBaseViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Utilities.DisposableObject;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Base ViewModel for the <see cref="CDP4Common.SiteDirectoryData.ParameterType" /> editors
    /// </summary>
    /// <typeparam name="T">the type of the <see cref="Parameter" /></typeparam>
    public abstract class ParameterTypeEditorBaseViewModel<T> : DisposableObject, IParameterEditorBaseViewModel<T> where T : ParameterType
    {
        /// <summary>
        /// Reference to the initial <see cref="IsReadOnly" /> value
        /// </summary>
        private bool initialReadOnlyValue;

        /// <summary>
        /// Backing field for the <see cref="IsReadOnly" /> property
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Backing field for <see cref="ValueArray" />
        /// </summary>
        private ValueArray<string> valueArray;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorBaseViewModel{T}" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        protected ParameterTypeEditorBaseViewModel(T parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0)
        {
            this.ValueSet = valueSet;
            this.ValueArrayIndex = valueArrayIndex;
            this.ParameterType = parameterType;

            if (this.ValueSet is ParameterValueSetBase valueSetBase)
            {
                this.Parameter = valueSetBase.Container as ParameterOrOverrideBase;
            }

            this.InitializesProperties(isReadOnly);
        }

        /// <summary>
        /// The validation messages to display
        /// </summary>
        public string ValidationMessage { get; private set; }

        /// <summary>
        /// Gets the associated <see cref="ParameterOrOverrideBase" />
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; }

        /// <summary>
        /// The <see cref="ValueArray{T}" /> to work with
        /// </summary>
        public ValueArray<string> ValueArray
        {
            get => this.valueArray;
            set => this.RaiseAndSetIfChanged(ref this.valueArray, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.SiteDirectoryData.ParameterType" />
        /// </summary>
        public T ParameterType { get; private set; }

        /// <summary>
        /// Event Callback for when a value has changed on the parameter
        /// </summary>
        public EventCallback<IValueSet> ParameterValueChanged { get; set; }

        /// <summary>
        /// Gets or sets if the Editor is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get => this.isReadOnly;
            set => this.RaiseAndSetIfChanged(ref this.isReadOnly, value);
        }

        /// <summary>
        /// Gets or sets the value set of this <see cref="T" />
        /// </summary>
        public IValueSet ValueSet { get; private set; }

        /// <summary>
        /// Gets the index of the value changed in the value sets
        /// </summary>
        public int ValueArrayIndex { get; set; }

        /// <summary>
        /// The current <see cref="ParameterSwitchKind" />
        /// </summary>
        public ParameterSwitchKind CurrentParameterSwitchKind { get; private set; }

        /// <summary>
        /// Sets the <see cref="ParameterSwitchKind" />
        /// </summary>
        /// <param name="parameterSwitchKind">The <see cref="ParameterSwitchKind" /></param>
        public void UpdateParameterSwitchKind(ParameterSwitchKind parameterSwitchKind)
        {
            this.CurrentParameterSwitchKind = parameterSwitchKind;
            this.IsReadOnly = this.initialReadOnlyValue || this.CurrentParameterSwitchKind == ParameterSwitchKind.COMPUTED;

            this.ValueArray = this.CurrentParameterSwitchKind switch
            {
                ParameterSwitchKind.MANUAL => this.ValueSet.Manual,
                ParameterSwitchKind.COMPUTED => this.ValueSet.Computed,
                ParameterSwitchKind.REFERENCE => this.ValueSet.Reference,
                _ => throw new ArgumentOutOfRangeException(nameof(parameterSwitchKind), parameterSwitchKind, "Unknowned ParameterSwitchKind")
            };
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        public void UpdateProperties( bool readOnly)
        {
            this.InitializesProperties(readOnly);
        }

        /// <summary>
        /// Verifies if the changing value is valid
        /// </summary>
        /// <param name="value">The value to validate </param>
        public bool AreChangesValid(object value)
        {
            if (this.ParameterType is QuantityKind quantityKind)
            {
                this.ValidationMessage = string.Empty + ParameterValueValidator.Validate(value, quantityKind, quantityKind.DefaultScale);
            }
            else
            {
                this.ValidationMessage = string.Empty + ParameterValueValidator.Validate(value, this.ParameterType);
            }

            return this.ValidationMessage == string.Empty;
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public abstract Task OnParameterValueChanged(object value);

        /// <summary>
        /// Updates the <see cref="ParameterValueSetBase" /> with the new
        /// <param name="newValueArray"></param>
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase" /></param>
        /// <param name="newValueArray"></param>
        /// <returns>A <see cref="Task" /></returns>
        /// <exception cref="InvalidOperationException">
        /// If the current <see cref="ParameterSwitchKind" /> is
        /// <see cref="ParameterSwitchKind.COMPUTED" />
        /// </exception>
        protected async Task UpdateValueSet(ParameterValueSetBase valueSet, ValueArray<string> newValueArray)
        {
            var clone = valueSet.Clone(false);

            switch (this.CurrentParameterSwitchKind)
            {
                case ParameterSwitchKind.MANUAL:
                    clone.Manual = newValueArray;
                    break;
                case ParameterSwitchKind.REFERENCE:
                    clone.Reference = newValueArray;
                    break;
                default:
                    throw new InvalidOperationException($"The value of the {nameof(this.ValueSet)} can't be manually changed with the switch on {ParameterSwitchKind.COMPUTED}");
            }

            await this.ParameterValueChanged.InvokeAsync(clone);
        }

        /// <summary>
        /// Initializes this viewmodel properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        private void InitializesProperties(bool readOnly)
        {
            this.initialReadOnlyValue = readOnly;

            this.UpdateParameterSwitchKind(this.ValueSet.ValueSwitch);
        }
    }
}
