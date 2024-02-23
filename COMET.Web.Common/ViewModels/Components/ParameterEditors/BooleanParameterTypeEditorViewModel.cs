// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BooleanParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;

    /// <summary>
    /// ViewModel used to edit <see cref="BooleanParameterType" />
    /// </summary>
    public class BooleanParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<BooleanParameterType>
    {
		/// <summary>
		/// Creates a new instance of type <see cref="BooleanParameterTypeEditorViewModel" />
		/// </summary>
		/// <param name="parameterType">the parameter used for this editor view model</param>
		/// <param name="valueSet">the value set asociated to this editor</param>
		/// <param name="isReadOnly">The readonly state</param>
		/// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        public BooleanParameterTypeEditorViewModel(BooleanParameterType parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0) : base(parameterType, valueSet, isReadOnly, valueArrayIndex)
        {
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <param name="value">The new value</param>
        /// <returns>A <see cref="Task"/></returns>
        public override async Task OnParameterValueChanged(object value)
        {
            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && value is string valueString && this.AreChangesValid(value))
            {
                var modifiedValueArray = new ValueArray<string>(this.ValueArray)
                {
                    [this.ValueArrayIndex] = valueString
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }
    }
}
