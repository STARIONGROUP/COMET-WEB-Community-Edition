// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// ViewModel used to edit <see cref="QuantityKind" />
    /// </summary>
    public class QuantityKindParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<QuantityKind>
    {
        /// <summary>
        /// Creates a new instance of type <see cref="QuantityKindParameterTypeEditorViewModel" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        public QuantityKindParameterTypeEditorViewModel(QuantityKind parameterType, IValueSet valueSet, bool isReadOnly) : base(parameterType, valueSet, isReadOnly)
        {
            this.ParameterType = parameterType;
            this.ValueSet = valueSet;
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public override async Task OnParameterValueChanged(object value)
        {
            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && value is string valueString)
            {
                var modifiedValueArray = new ValueArray<string>(this.ValueSet.ActualValue)
                {
                    [0] = valueString
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }
    }
}
