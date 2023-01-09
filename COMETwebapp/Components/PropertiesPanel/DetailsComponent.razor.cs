// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsComponent.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Primitives;

    using Microsoft.AspNetCore.Components;
    using Newtonsoft.Json;

    /// <summary>
    /// The component used for showing the details of the <see cref="PrimitiveSelected"/>
    /// </summary>
    public partial class DetailsComponent
    {
        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        [Inject]
        public IJSInterop JSInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/> of the <see cref="PrimitiveSelected"/> property
        /// </summary>
        private Dictionary<ParameterBase, IValueSet> ValueSetsCollection { get; set; }

        /// <summary>
        /// Backing field for the <see cref="parameterSelected"/> property
        /// </summary>
        private ParameterBase parameterSelected;

        /// <summary>
        /// Backing field for the <see cref="PrimitiveSelected"/> property
        /// </summary>
        private Primitive primitiveSelected;

        /// <summary>
        /// Gets or sets the selected primitive used for the details
        /// </summary>
        [Parameter]
        public Primitive PrimitiveSelected
        {
            get => this.primitiveSelected;
            set
            {
                if (this.primitiveSelected != value)
                {                   
                    this.primitiveSelected = value;
                    this.InitValueSet();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected parameter used for the details
        /// </summary>
        [Parameter]
        public ParameterBase ParameterSelected
        {
            get => this.parameterSelected;
            set
            {
                if (this.parameterSelected != value)
                {
                    this.parameterSelected = value;
                }
            }
        }

        /// <summary>
        /// Inits the <see cref="ValueSetsCollection"/>
        /// </summary>
        private void InitValueSet()
        {
            if (this.PrimitiveSelected is not null)
            {
                this.ValueSetsCollection = this.PrimitiveSelected.GetValueSets();
            }
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to the <see cref="ParameterSelected"/> property
        /// </summary>
        /// <returns>the set</returns>
        public IValueSet GetValueSet()
        {
            return this.ValueSetsCollection[this.ParameterSelected];
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to the <paramref name="parameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The parameter for the set wants to be retrieved</param>
        /// <returns>the set</returns>
        public IValueSet GetValueSet(ParameterBase parameterBase)
        {
            return this.ValueSetsCollection[parameterBase];
        }

        /// <summary>
        /// Event for when the value of a parameter has changed
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="e">Supplies information about an change event that is being raised.</param>
        public void OnParameterValueChange(int changedIndex, ChangeEventArgs e)
        {
            //TODO: Validate data 
            this.ParameterChanged(changedIndex, e.Value as string ?? string.Empty);            
        }

        /// <summary>
        /// Sets that a parameter has changed in the value array
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="value">the new value at that <paramref name="changedIndex"/></param>
        public async void ParameterChanged(int changedIndex, string value)
        {
            var valueSet = this.ValueSetsCollection[this.ParameterSelected];

            ValueArray<string> newValueArray = new ValueArray<string>(valueSet.ActualValue);
            newValueArray[changedIndex] = value;

            if (valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedValueSetBase = parameterValueSetBase.Clone(false);
                clonedValueSetBase.Manual = newValueArray;
                this.ValueSetsCollection[this.ParameterSelected] = clonedValueSetBase;
                this.PrimitiveSelected.UpdatePropertyWithParameterData(this.ParameterSelected.ParameterType.ShortName, clonedValueSetBase);

                string jsonPrimitive = JsonConvert.SerializeObject(this.primitiveSelected, Formatting.Indented);
                await JSInterop.Invoke("RegenMesh", jsonPrimitive);
            }
        }
    }
}
