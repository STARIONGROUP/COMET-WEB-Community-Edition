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
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Interoperability;
    using COMETwebapp.Model;
    
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
        /// Backing field for the <see cref="SelectedSceneObject"/> property
        /// </summary>
        private SceneObject selectedSceneObject;
                
        /// <summary>
        /// Gets or sets the selected scene object used for the details panel.
        /// </summary>
        [Parameter]
        public SceneObject SelectedSceneObject
        {
            get => this.selectedSceneObject;
            set
            {
                if(this.selectedSceneObject != value)
                {
                    this.selectedSceneObject = value;
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
            if (this.SelectedSceneObject is not null)
            {
                this.ValueSetsCollection = this.SelectedSceneObject.GetValueSets();
            }
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to the <see cref="ParameterSelected"/> property
        /// </summary>
        /// <returns>the set</returns>
        public IValueSet GetValueSet()
        {
            return this.ValueSetsCollection.ContainsKey(this.ParameterSelected) ? this.ValueSetsCollection[this.ParameterSelected] : null;
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
        /// Gets the value sets with the new values
        /// </summary>
        public Dictionary<ParameterBase,IValueSet> GetAllValueSets()
        {
            return this.ValueSetsCollection;
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
            //TODO: Validate data 
            var valueSet = this.ValueSetsCollection[this.ParameterSelected];
            ValueArray<string> newValueArray = new ValueArray<string>(valueSet.ActualValue);
            newValueArray[changedIndex] = value;

            if (valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedValueSetBase = parameterValueSetBase.Clone(false);
                clonedValueSetBase.Manual = newValueArray;
                this.ValueSetsCollection[this.ParameterSelected] = clonedValueSetBase;

                this.SelectedSceneObject.UpdateParameter(this.ParameterSelected, clonedValueSetBase);

                if (this.ParameterSelected.ParameterType.ShortName == SceneSettings.ShapeKindShortName)
                {
                    if(this.SelectedSceneObject.Primitive is not null)
                    {
                        this.SelectedSceneObject.Primitive.HasHalo = true;
                    }
                    var parameters = this.ValueSetsCollection.Keys.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);
                    foreach (var parameter in parameters)
                    {
                        this.SelectedSceneObject.UpdateParameter(parameter, this.ValueSetsCollection[parameter]);
                    }
                }

                string jsonSceneObject = JsonConvert.SerializeObject(this.SelectedSceneObject, Formatting.Indented);
                await this.JSInterop.Invoke("RegenMesh", jsonSceneObject);
            }
        }
    }
}
