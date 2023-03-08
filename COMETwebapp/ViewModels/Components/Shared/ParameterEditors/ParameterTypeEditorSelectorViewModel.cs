// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorViewModel.cs" company="RHEA System S.A.">
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

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Shared.ParameterTypeEditors.ParameterTypeEditorSelector" />
    /// </summary>
    public class ParameterTypeEditorSelectorViewModel : IParameterTypeEditorSelectorViewModel
    {
        /// <summary>
        /// Gets if the Editor is readonly.
        /// </summary>
        private readonly bool isReadOnly;

        /// <summary>
        /// The <see cref="IHaveValueSetViewModel" />
        /// </summary>
        private IHaveValueSetViewModel haveValueSetViewModel;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorSelectorViewModel" />
        /// </summary>
        /// <param name="parameterType">the <see cref="ParameterType" /> used for this view model</param>
        /// <param name="valueSet">the value set asociated to the ParameterTypeEditor</param>
        /// <param name="isReadOnly">Value asserting that the <see cref="IParameterEditorBaseViewModel{T}" /> should be readonly</param>
        public ParameterTypeEditorSelectorViewModel(ParameterType parameterType, IValueSet valueSet, bool isReadOnly)
        {
            this.ParameterType = parameterType;
            this.ValueSet = valueSet;
            this.isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Gets or sets the value set of this <see cref="ParameterType" />
        /// </summary>
        public IValueSet ValueSet { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" />
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Event Callback for when a value has changed on the parameter
        /// </summary>
        public EventCallback<IValueSet> ParameterValueChanged { get; set; }

        /// <summary>
        /// Creates a view model for the corresponding editor
        /// </summary>
        /// <typeparam name="T">the parameter type</typeparam>
        /// <returns>the view model</returns>
        public IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T : ParameterType
        {
            this.haveValueSetViewModel = this.ParameterType switch
            {
                BooleanParameterType booleanParameterType => new BooleanParameterTypeEditorViewModel(booleanParameterType, this.ValueSet, this.isReadOnly),
                CompoundParameterType compoundParameterType => new CompoundParameterTypeEditorViewModel(compoundParameterType, this.ValueSet, this.isReadOnly),
                DateParameterType dateParameterType => new DateParameterTypeEditorViewModel(dateParameterType, this.ValueSet, this.isReadOnly),
                DateTimeParameterType dateTimeParameterType => new DateTimeParameterTypeEditorViewModel(dateTimeParameterType, this.ValueSet, this.isReadOnly),
                EnumerationParameterType enumerationParameterType => new EnumerationParameterTypeEditorViewModel(enumerationParameterType, this.ValueSet, this.isReadOnly),
                QuantityKind quantityKind => new QuantityKindParameterTypeEditorViewModel(quantityKind, this.ValueSet, this.isReadOnly),
                TextParameterType textParameterType => new TextParameterTypeEditorViewModel(textParameterType, this.ValueSet, this.isReadOnly),
                TimeOfDayParameterType timeOfDayParameterType => new TimeOfDayParameterTypeEditorViewModel(timeOfDayParameterType, this.ValueSet, this.isReadOnly),
                _ => throw new NotImplementedException($"The ViewModel for the {this.ParameterType} has not been implemented")
            };

            var parameterViewModel = (this.haveValueSetViewModel as IParameterEditorBaseViewModel<T>)!;
            parameterViewModel.ParameterValueChanged = this.ParameterValueChanged;
            return parameterViewModel;
        }

        /// <summary>
        /// Updates the <see cref="ParameterSwitchKind" /> value
        /// </summary>
        /// <param name="switchValue">The <see cref="ParameterSwitchKind" /></param>
        public void UpdateSwitchKind(ParameterSwitchKind switchValue)
        {
            this.haveValueSetViewModel?.UpdateParameterSwitchKind(switchValue);
        }
    }
}
