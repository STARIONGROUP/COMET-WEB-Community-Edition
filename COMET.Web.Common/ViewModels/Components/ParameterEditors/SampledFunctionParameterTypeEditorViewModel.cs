// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SampledFunctionParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Extensions;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model used to edit <see cref="SampledFunctionParameterType" />
    /// </summary>
    public class SampledFunctionParameterTypeEditorViewModel : HaveComponentParameterTypeEditor<SampledFunctionParameterType>, ISampledFunctionParameterTypeEditorViewModel
    {
        /// <summary>
        /// The initial <see cref="ParameterValueSetBase"/>
        /// </summary>
        private readonly ParameterValueSetBase initialValueSet;

        /// <summary>
        /// The <see cref="EventCallback{TValue}"/> to update the current valueSet
        /// </summary>
        private readonly EventCallback<(IValueSet, int)> updateCallback;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorBaseViewModel{T}" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        public SampledFunctionParameterTypeEditorViewModel(SampledFunctionParameterType parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0)
            : base(parameterType, (valueSet as ParameterValueSetBase)!.Clone(false), isReadOnly, valueArrayIndex)
        {
            if (this.ValueSet is ParameterValueSetBase valueSetBase)
            {
                this.initialValueSet = valueSetBase.Original as ParameterValueSetBase;
            }

            this.updateCallback = new EventCallbackFactory().Create<(IValueSet,int)>(this, this.UpdateValueSetFromContainedComponent);

            this.ParameterTypeAssignments = parameterType.IndependentParameterType
                .Union(parameterType.DependentParameterType.OfType<IParameterTypeAssignment>()).ToList();
        }

        /// <summary>
        /// Update the current <see cref="ValueArray{T}"/> based on an updated <see cref="IValueSet"/>
        /// </summary>
        /// <param name="valueTuple">The updated <see cref="IValueSet"/> with the index</param>
        private void UpdateValueSetFromContainedComponent((IValueSet valueSet, int valueIndex) valueTuple)
        {
            var values = this.CurrentParameterSwitchKind switch
            {
                ParameterSwitchKind.MANUAL => valueTuple.valueSet.Manual,
                ParameterSwitchKind.REFERENCE => valueTuple.valueSet.Reference,
                _ => this.ValueArray
            };

            this.ValueArray[valueTuple.valueIndex] = values[valueTuple.valueIndex];
        }

        /// <summary>
        /// A collection of all <see cref="IParameterTypeAssignment" />
        /// </summary>
        public IReadOnlyList<IParameterTypeAssignment> ParameterTypeAssignments { get; }

        /// <summary>
        /// Value asserting that it is allowed to remove a row
        /// </summary>
        public bool CanRemoveRow => this.ValueArray.Count > this.ParameterType.NumberOfValues && !this.IsReadOnly;

        /// <summary>
        /// Creates a view model for the corresponding <see cref="IParameterTypeAssignment" />
        /// </summary>
        /// <param name="valueArrayIndex">
        /// the index of the
        /// <see cref="IParameterTypeAssignment" /> in the <see cref="SampledFunctionParameterType" />
        /// </param>
        /// <returns>the <see cref="IParameterTypeAssignment" /></returns>
        public IParameterTypeEditorSelectorViewModel CreateParameterTypeEditorSelectorViewModel(int valueArrayIndex)
        {
            var parameterTypeAssignement = this.ParameterType.QueryParameterTypeAtIndex(valueArrayIndex);
            return this.CreateParameterTypeEditorSelectorViewModel(parameterTypeAssignement.ParameterType, valueArrayIndex, parameterTypeAssignement.MeasurementScale, this.updateCallback);
        }

        /// <summary>
        /// Add a new row of default values
        /// </summary>
        public void AddRow()
        {
            if (this.ValueSet is not ParameterValueSetBase valueSetBase || this.IsReadOnly)
            {
                return;
            }

            this.ValueArray = this.ValueArray.AddNewValues(this.ParameterType.NumberOfValues);

            this.UpdateValueSet(valueSetBase);
        }

        /// <summary>
        /// Remove a row of values
        /// </summary>
        public void RemoveRow()
        {
            if (this.ValueSet is not ParameterValueSetBase valueSetBase || !this.CanRemoveRow)
            {
                return;
            }

            this.ValueArray = this.ValueArray.RemovesValues(this.ParameterType.NumberOfValues);
            this.UpdateValueSet(valueSetBase);
        }

        /// <summary>
        /// Update the <see cref="ParameterValueSetBase"/> with the <see cref="ValueArray{T}"/>
        /// </summary>
        /// <param name="valueSetBase">The <see cref="ParameterValueSetBase"/></param>
        private void UpdateValueSet(ParameterValueSetBase valueSetBase)
        {
            switch (this.CurrentParameterSwitchKind)
            {
                case ParameterSwitchKind.MANUAL:
                    valueSetBase.Manual = this.ValueArray;
                    break;
                case ParameterSwitchKind.REFERENCE:
                    valueSetBase.Reference = this.ValueArray;
                    break;
                case ParameterSwitchKind.COMPUTED:
                default:
                    return;
            }
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <param name="value">The new value</param>
        /// <returns>A <see cref="Task" /></returns>
        public override async Task OnParameterValueChanged(object value)
        {
            if (this.ValueSet is ParameterValueSetBase)
            {
                await this.UpdateValueSet(this.initialValueSet, this.ValueArray);
            }
        }

        /// <summary>
        /// Reset changes that could have been set to the <see cref="ValueArray{T}" />
        /// </summary>
        public void ResetChanges()
        {
            this.ValueSet = this.initialValueSet.Clone(false);
            this.UpdateParameterSwitchKind(this.CurrentParameterSwitchKind);
        }
    }
}
