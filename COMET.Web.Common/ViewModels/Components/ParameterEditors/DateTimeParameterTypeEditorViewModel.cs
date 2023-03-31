// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DateTimeParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using System.Globalization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// ViewModel used to edit <see cref="DateTimeParameterType" />
    /// </summary>
    public class DateTimeParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<DateTimeParameterType>
    {
        /// <summary>
        /// Creates a new instance of type <see cref="DateTimeParameterTypeEditorViewModel" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        public DateTimeParameterTypeEditorViewModel(DateTimeParameterType parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0) : base(parameterType, valueSet, isReadOnly, valueArrayIndex)
        {
            this.DateTimeString = valueSet.ActualValue.First();
        }

        /// <summary>
        /// Gets or sets the string representation of the <see cref="DateTime" />
        /// </summary>
        private string DateTimeString { get; set; }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public override async Task OnParameterValueChanged(object value)
        {
            var auxiliarDateTime = this.DateTimeString;
            var values = auxiliarDateTime.Split("T");

            if (value is TimeSpan time)
            {
                values[1] = time.ToString();
            }
            else if (value is DateTime dateTime)
            {
                values[0] = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            this.DateTimeString = string.Join('T', values);

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && this.AreChangesValid(value))
            {
                var modifiedValueArray = new ValueArray<string>(this.ValueArray)
                {
                    [this.ValueArrayIndex] = this.DateTimeString
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }
    }
}
