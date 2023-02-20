// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DetailsComponentViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model that provide details for <see cref="ParameterBase"/>
    /// </summary>
    public class DetailsComponentViewModel : ReactiveObject, IDetailsComponentViewModel
    {
        /// <summary>
        /// The collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/> 
        /// </summary>
        private Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// Gets or sets if the component is visible
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the selected parameter used for the details
        /// </summary>
        public ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// Gets or sets the current value set
        /// </summary>
        public IValueSet CurrentValueSet { get; set; }

        /// <summary> 
        /// Event callback for when a value of the <see cref="SelectedParameter"/> has changed 
        /// </summary> 
        public EventCallback<Dictionary<ParameterBase, IValueSet>> OnParameterValueChanged { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="DetailsComponentViewModel"/>
        /// </summary>
        /// <param name="isVisible">if the component is visible/></param>
        /// <param name="selectedParameter">the selected parameter</param>
        /// <param name="parameterValueSetRelations">the relations between the <see cref="ParameterBase"/> and the <see cref="IValueSet"/></param>
        /// <param name="onParameterValueSetChanged">event callback for when a <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/> has changed</param>
        public DetailsComponentViewModel(bool isVisible, ParameterBase selectedParameter, Dictionary<ParameterBase, IValueSet> parameterValueSetRelations,
            EventCallback<Dictionary<ParameterBase,IValueSet>> onParameterValueSetChanged)
        {
            this.IsVisible = isVisible;
            this.SelectedParameter = selectedParameter;
            this.ParameterValueSetRelations = parameterValueSetRelations;
            this.OnParameterValueChanged = onParameterValueSetChanged;

            if (this.ParameterValueSetRelations is not null && this.SelectedParameter is not null)
            {
                this.CurrentValueSet = this.ParameterValueSetRelations.ContainsKey(this.SelectedParameter) ? this.ParameterValueSetRelations[this.SelectedParameter] : null;
            }
        }

        /// <summary>
        /// Event for when the value of the parameter has changed
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="value">the value of the <see cref="IValueSet"/> changed</param>
        public async void OnParameterValueChange(int changedIndex, string value)
        {
            var modifiedValueArray = new ValueArray<string>(this.CurrentValueSet.ActualValue);
            modifiedValueArray[changedIndex] = value;

            if (this.CurrentValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var sendingParameterValueSetBase = parameterValueSetBase.Clone(false);
                sendingParameterValueSetBase.Manual = modifiedValueArray;
                sendingParameterValueSetBase.ValueSwitch = ParameterSwitchKind.MANUAL;

                var parameterValueSetRelations = new Dictionary<ParameterBase, IValueSet>()
                {
                    {this.SelectedParameter, sendingParameterValueSetBase},
                };

                await this.OnParameterValueChanged.InvokeAsync(parameterValueSetRelations);
            }
        }

        /// <summary>
        /// Event for when the value of a parameter has changed
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="e">Supplies information about an change event that is being raised.</param>
        public async void OnParameterValueChange(int changedIndex, ChangeEventArgs e)
        {
            var value = e.Value as string;
            this.OnParameterValueChange(changedIndex, value);
        }
    }
}
