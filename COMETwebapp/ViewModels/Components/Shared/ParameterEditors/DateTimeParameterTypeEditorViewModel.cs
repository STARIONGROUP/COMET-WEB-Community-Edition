// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DateTimeParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using System.Globalization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    /// <summary>
    /// ViewModel used to edit <see cref="DateTimeParameterType"/>
    /// </summary>
    public class DateTimeParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<DateTimeParameterType>
    {
        /// <summary>
        /// Gets or sets the string representation of the <see cref="DateTime"/>
        /// </summary>
        private string DateTimeString { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="DateTimeParameterTypeEditorViewModel"/>
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        public DateTimeParameterTypeEditorViewModel(DateTimeParameterType parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0) : base(parameterType,valueSet, isReadOnly, valueArrayIndex)
        {
            this.DateTimeString = valueSet.ActualValue.First();
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public override async Task OnParameterValueChanged(object value)
        {
            var auxiliarDateTime = this.DateTimeString;
            var values = auxiliarDateTime.Split("T");

            if (value is TimeSpan time)
            {
                values[1] = time.ToString();
            }
            else if(value is DateTime dateTime) 
            {
                values[0] = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            this.DateTimeString = string.Join('T',values);

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
