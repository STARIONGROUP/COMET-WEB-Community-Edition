// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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

    using ReactiveUI;

    using System.Collections.Generic;

    /// <summary>
    /// ViewModel used to edit <see cref="EnumerationParameterType"/>
    /// </summary>
    public class EnumerationParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<EnumerationParameterType>, IEnumerationParameterTypeEditorViewModel 
    {
        /// <summary>
        ///    The available <see cref="EnumerationValueDefinition"/>s
        /// </summary>
        public IEnumerable<EnumerationValueDefinition> EnumerationValueDefinitions { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="EnumerationParameterType"/>
        /// </summary>
        /// <param name="parameterType">the parameter used for this editor view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        public EnumerationParameterTypeEditorViewModel(EnumerationParameterType parameterType, IValueSet valueSet, bool isReadOnly, int compoundIndex = -1) : base(parameterType, valueSet, isReadOnly, compoundIndex)
        {
            this.EnumerationValueDefinitions = parameterType.ValueDefinition;
            
            if (compoundIndex != -1)
            {
                this.SelectedEnumerationValueDefinitions = this.EnumerationValueDefinitions.Where(x => ValueArray[compoundIndex].Split('|').ToList().Contains(x.ShortName)).Select(x => x.Name);
            }
            else
            {
                this.SelectedEnumerationValueDefinitions = this.EnumerationValueDefinitions.Where(x => ValueArray.First().Split('|').ToList().Contains(x.ShortName)).Select(x => x.Name);
            }
        }

        /// <summary>
        ///    Names of selected <see cref="EnumerationValueDefinition"/>s
        /// </summary>
        public IEnumerable<string> SelectedEnumerationValueDefinitions { get; set; }

        /// <summary>
        ///     Backing field for <see cref="SelectAllChecked" />
        /// </summary>
        private bool selectAllChecked;

        /// <summary>
        /// Indicates if all elements are checked
        /// </summary>
        public bool SelectAllChecked
        {
            get => this.selectAllChecked;
            set => this.RaiseAndSetIfChanged(ref this.selectAllChecked, value);
        }
        
        /// <summary>
        ///     Backing field for <see cref="IsOnEditMode" />
        /// </summary>
        private bool isOnEditMode;

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
        /// Method invoked when confirming selection of  <see cref="EnumerationValueDefinition"/>
        /// </summary>
        public async void OnConfirmButtonClick()
        {
            IEnumerable<string> elements = this.EnumerationValueDefinitions.Where(x => SelectedEnumerationValueDefinitions.Contains(x.Name)).Select(x => x.ShortName);

            var value = string.Join(" | ", elements);

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && value is string valueString)
            {
                ValueArray<string> modifiedValueArray;

                if (!this.SelectedEnumerationValueDefinitions.Any())
                {
                    valueString = "-";
                }
                
                if(this.CompoundIndex != -1)
                {
                    modifiedValueArray = new ValueArray<string>(this.ValueSet.ActualValue)
                    {
                        [this.CompoundIndex] = valueString
                    };
                }
                else
                {
                    modifiedValueArray = new ValueArray<string>(this.ValueSet.ActualValue)
                    {
                        [0] = valueString
                    };
                }  
                
                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }

            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method invoked when canceling the selection of <see cref="EnumerationValueDefinition"/>
        /// </summary>
        public void OnCancelButtonClick()
        {
            this.SelectedEnumerationValueDefinitions = this.EnumerationValueDefinitions.Where(x => ValueArray.First().Split('|').ToList().Contains(x.ShortName)).Select(x => x.Name);
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public override async Task OnParameterValueChanged(object value)
        {
            ValueArray<string> modifiedValueArray;

            var element = value != "-" ? this.EnumerationValueDefinitions.Where(x => x.Name == value).Select(x => x.ShortName).FirstOrDefault() : value;

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && element is string valueString)
            {
                if (this.CompoundIndex != -1)
                {
                    modifiedValueArray = new ValueArray<string>(this.ValueSet.ActualValue)
                    {
                        [this.CompoundIndex] = valueString
                    };
                }
                else
                {
                    modifiedValueArray = new ValueArray<string>(this.ValueSet.ActualValue)
                    {
                        [0] = valueString
                    };
                }

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }
    }
}
