// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditorViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

    using ReactiveUI;

    /// <summary>
    /// ViewModel used to edit <see cref="EnumerationParameterType" />
    /// </summary>
    public class EnumerationParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<EnumerationParameterType>, IEnumerationParameterTypeEditorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />
        /// </summary>
        private bool isOnEditMode;

        /// <summary>
        /// Backing field for <see cref="SelectAllChecked" />
        /// </summary>
        private bool selectAllChecked;

		/// <summary>
		/// Creates a new instance of type <see cref="EnumerationParameterType" />
		/// </summary>
		/// <param name="parameterType">the parameter used for this editor view model</param>
		/// <param name="valueSet">the value set asociated to this editor</param>
		/// <param name="isReadOnly">The readonly state</param>
		/// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
		public EnumerationParameterTypeEditorViewModel(EnumerationParameterType parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0) : base(parameterType, valueSet, isReadOnly, valueArrayIndex)
        {
            this.EnumerationValueDefinitions = parameterType.ValueDefinition;

            this.SelectedEnumerationValueDefinitions = this.EnumerationValueDefinitions.Where(x => this.ValueArray[valueArrayIndex].Split(" | ")
                    .ToList()
                    .Contains(x.ShortName))
                .Select(x => x.Name);
        }

        /// <summary>
        /// The available <see cref="EnumerationValueDefinition" />s
        /// </summary>
        public IEnumerable<EnumerationValueDefinition> EnumerationValueDefinitions { get; set; }

        /// <summary>
        /// Names of selected <see cref="EnumerationValueDefinition" />s
        /// </summary>
        public IEnumerable<string> SelectedEnumerationValueDefinitions { get; set; }

        /// <summary>
        /// Indicates if all elements are checked
        /// </summary>
        public bool SelectAllChecked
        {
            get => this.selectAllChecked;
            set => this.RaiseAndSetIfChanged(ref this.selectAllChecked, value);
        }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// Method invoked when select all is checked
        /// </summary>
        public void OnSelectAllChanged(bool value)
        {
            if (this.SelectAllChecked && !value)
            {
                this.SelectedEnumerationValueDefinitions = new List<string>();
            }
            else if (value)
            {
                this.SelectedEnumerationValueDefinitions = new List<string>(this.ParameterType.ValueDefinition.Select(x => x.Name));
            }

            this.SelectAllChecked = value;
        }

        /// <summary>
        /// Method invoked when confirming selection of  <see cref="EnumerationValueDefinition" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnConfirmButtonClick()
        {
            var elements = this.EnumerationValueDefinitions.Where(x => this.SelectedEnumerationValueDefinitions.Contains(x.Name)).Select(x => x.ShortName);

            var value = string.Join(" | ", elements);

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && value is { } valueString && this.AreChangesValid(value))
            {
                if (!this.SelectedEnumerationValueDefinitions.Any())
                {
                    valueString = "-";
                }

                var modifiedValueArray = new ValueArray<string>(this.ValueArray)
                {
                    [this.ValueArrayIndex] = valueString
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }

            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method invoked when canceling the selection of <see cref="EnumerationValueDefinition" />
        /// </summary>
        public void OnCancelButtonClick()
        {
            this.SelectedEnumerationValueDefinitions = this.EnumerationValueDefinitions.Where(x => this.ValueArray.First().Split(" | ").ToList().Contains(x.ShortName)).Select(x => x.Name);
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Gets the enumeration value for the current <see cref="ValueArray{T}"/>
        /// </summary>
        /// <returns>The enumeration value</returns>
        public string GetEnumerationValue()
        {
            return this.ValueArray[this.ValueArrayIndex] == "-" ? "-" : this.EnumerationValueDefinitions.FirstOrDefault(x => x.ShortName == this.ValueArray[this.ValueArrayIndex])?.Name;
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <param name="value">The new value</param>
        /// <returns>A <see cref="Task" /></returns>
        public override async Task OnParameterValueChanged(object value)
        {
            var element = value.ToString() != "-" ? this.EnumerationValueDefinitions.Where(x => x.Name == value.ToString()).Select(x => x.ShortName).FirstOrDefault() : value.ToString();

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && element != null && this.AreChangesValid(element))
            {
                var modifiedValueArray = new ValueArray<string>(this.ValueArray)
                {
                    [this.ValueArrayIndex] = element
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }
    }
}
