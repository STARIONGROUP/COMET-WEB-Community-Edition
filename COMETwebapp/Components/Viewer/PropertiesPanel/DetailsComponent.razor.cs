// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsComponent.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Components.Viewer.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

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
        public IJSRuntime JSInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/> of the <see cref="PrimitiveSelected"/> property
        /// </summary>
        private Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// Gets or sets if the component is visible
        /// </summary>
        [Parameter]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Backing field for the <see cref="parameterSelected"/> property
        /// </summary>
        private ParameterBase parameterSelected;

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
        /// Gets or sets the <see cref="ISelectionMediator"/> 
        /// </summary> 
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary> 
        /// Event callback for when a value of the <see cref="ParameterSelected"/> has changed 
        /// </summary> 
        [Parameter]
        public EventCallback OnParameterValueChanged { get; set; }

        /// <summary> 
        /// Method invoked after each time the component has been rendered. Note that the component does 
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because 
        /// that would cause an infinite render loop. 
        /// </summary> 
        /// <param name="firstRender"> 
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRender(bool)"/> has been invoked 
        /// on this component instance; otherwise <c>false</c>. 
        /// </param> 
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns> 
        /// <remarks> 
        /// The <see cref="OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods 
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>. 
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed 
        /// once. 
        /// </remarks> 
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                this.SelectionMediator.OnTreeSelectionChanged += (sender, node) =>
                {
                    this.OnSelectionChanged();
                };
                this.SelectionMediator.OnModelSelectionChanged += (sender, sceneObject) =>
                {
                    this.OnSelectionChanged();
                };
            }
        }

        /// <summary> 
        /// Callback method for when a new <see cref="SceneObject"/> has been selected 
        /// </summary> 
        private void OnSelectionChanged()
        {
            if (this.SelectionMediator.SelectedSceneObjectClone != null)
            {
                this.ParameterValueSetRelations = this.SelectionMediator.SelectedSceneObjectClone.GetParameterValueSetRelations();
            }
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to the <see cref="ParameterSelected"/> property
        /// </summary>
        /// <returns>the set</returns>
        public IValueSet GetSelectedParameterValueSet()
        {
            if(this.ParameterValueSetRelations is not null)
            {
                return this.ParameterValueSetRelations.ContainsKey(this.ParameterSelected) ? this.ParameterValueSetRelations[this.ParameterSelected] : null;
            }
            return null;
        }

        /// <summary>
        /// Gets the value sets with the new values
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> GetParameterValueSetRelations()
        {
            return this.ParameterValueSetRelations;
        }

        /// <summary>
        /// Event for when the value of a parameter has changed
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="e">Supplies information about an change event that is being raised.</param>
        public Task OnParameterValueChange(int changedIndex, ChangeEventArgs e)
        {
            //TODO: Validate data 
            return this.ParameterChanged(changedIndex, e.Value as string ?? string.Empty);
        }

        /// <summary>
        /// Sets that a parameter has changed in the value array
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="value">the new value at that <paramref name="changedIndex"/></param>
        public async Task ParameterChanged(int changedIndex, string value)
        {
            //TODO: Validate data  
            var valueSet = this.ParameterValueSetRelations[this.ParameterSelected];
            ValueArray<string> newValueArray = new ValueArray<string>(valueSet.ActualValue);
            newValueArray[changedIndex] = value;

            if (valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedValueSetBase = parameterValueSetBase.Clone(false);
                clonedValueSetBase.Manual = newValueArray;
                this.ParameterValueSetRelations[this.ParameterSelected] = clonedValueSetBase;

                this.SelectionMediator?.SelectedSceneObjectClone?.UpdateParameter(this.ParameterSelected, clonedValueSetBase);

                if (this.ParameterSelected.ParameterType.ShortName == SceneSettings.ShapeKindShortName)
                {
                    if (this.SelectionMediator?.SelectedSceneObjectClone?.Primitive is not null)
                    {
                        this.SelectionMediator.SelectedSceneObjectClone.Primitive.HasHalo = true;
                    }
                    var parameters = this.ParameterValueSetRelations.Keys.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);
                    foreach (var parameter in parameters)
                    {
                        this.SelectionMediator?.SelectedSceneObjectClone?.UpdateParameter(parameter, this.ParameterValueSetRelations[parameter]);
                    }
                }

                string jsonSceneObject = JsonConvert.SerializeObject(this.SelectionMediator?.SelectedSceneObjectClone, Formatting.Indented);
                await this.JSInterop.InvokeVoidAsync("RegenMesh", jsonSceneObject);
            }

            await this.ParameterValueChanged();
        }

        /// <summary> 
        /// Calls the eventcallback <see cref="OnParameterValueChanged"/> 
        /// </summary> 
        /// <returns>an asynchronous operation</returns> 
        public async Task ParameterValueChanged()
        {
            await this.OnParameterValueChanged.InvokeAsync();
        }
    }
}
