// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorBaseViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Shared.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Utilities.DisposableObject;

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
        private readonly bool initialReadOnlyValue;

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
            this.ParameterType = parameterType;
            this.ValueSet = valueSet;
            this.initialReadOnlyValue = isReadOnly;
            this.ValueArrayIndex = valueArrayIndex;

            this.UpdateParameterSwitchKind(this.ValueSet.ValueSwitch);
        }

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
        public T ParameterType { get; set; }

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
        public IValueSet ValueSet { get; set; }

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
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
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
            clone.ValueSwitch = this.CurrentParameterSwitchKind;

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
    }
}
