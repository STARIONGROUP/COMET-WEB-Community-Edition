// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PropertiesComponentViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.Types;
    
    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="PropertiesComponent"/>
    /// </summary>
    public class PropertiesComponentViewModel : ReactiveObject, IPropertiesComponentViewModel
    {
        /// <summary>
        /// Injected property to get access to <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        [Inject]
        public IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/> of the selected <see cref="SceneObject"/>
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// Backing field for the <see cref="SelectedParameter"/>
        /// </summary>
        private ParameterBase selectedParameter;

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase"/> to fill the details
        /// </summary>
        public ParameterBase SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// Backing field for the <see cref="ParametersInUse"/>
        /// </summary>
        private List<ParameterBase> parametersInUse = new();

        /// <summary>
        /// The list of parameters that the selected <see cref="SceneObject"/> uses
        /// </summary>
        public List<ParameterBase> ParametersInUse
        {
            get => this.parametersInUse;
            set => this.RaiseAndSetIfChanged(ref this.parametersInUse, value);
        }

        /// <summary>
        /// Backing field for the <see cref="ParameterHaveChanges"/>
        /// </summary>
        private bool parameterHaveChanges;

        /// <summary>
        /// Gets or sets if the parameters have changes
        /// </summary>
        public bool ParameterHaveChanges
        {
            get => this.parameterHaveChanges;
            set => this.RaiseAndSetIfChanged(ref this.parameterHaveChanges, value);
        }

        /// <summary>
        /// Backing field for the <see cref="IsVisible"/>
        /// </summary>
        private bool isVisible;

        /// <summary> 
        /// Gets or sets if this component is visible 
        /// </summary> 
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Event callback for when a <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/> has changed
        /// </summary>
        public EventCallback<IValueSet> OnParameterValueSetChanged { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/> that have changed;
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> ChangedParameterValueSetRelations { get; set; } = new();

        /// <summary>
        /// Creates a new instance of type <see cref="PropertiesComponentViewModel"/>
        /// </summary>
        /// <param name="babylonInterop">the <see cref="IBabylonInterop"/></param>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="selectionMediator">the <see cref="ISelectionMediator"/></param>
        public PropertiesComponentViewModel(IBabylonInterop babylonInterop, ISessionService sessionService, ISelectionMediator selectionMediator)
        {
            this.BabylonInterop = babylonInterop;
            this.SessionService = sessionService;
            this.SelectionMediator = selectionMediator;

            this.OnParameterValueSetChanged = new EventCallbackFactory().Create(this, async (IValueSet valueSet) => 
            { 
                await this.ParameterValueSetChanged(valueSet); 
            });

            this.SelectionMediator.OnTreeSelectionChanged += (nodeViewModel) => this.OnSelectionChanged(nodeViewModel.Node.SceneObject);
            this.SelectionMediator.OnModelSelectionChanged += this.OnSelectionChanged;
        }              

        /// <summary> 
        /// Called when the selection of a <see cref="SceneObject"/> has changed 
        /// </summary> 
        /// <param name="sceneObject">the changed object</param> 
        private void OnSelectionChanged(SceneObject sceneObject)
        {
            this.IsVisible = sceneObject is not null;
            
            if (this.SelectionMediator.SelectedSceneObjectClone is not null)
            {
                this.ParameterValueSetRelations = this.SelectionMediator.SelectedSceneObjectClone.GetParameterValueSetRelations();
                
                if(this.SelectionMediator.SelectedSceneObjectClone.ParametersAsociated is not null)
                {
                    this.ParametersInUse = this.SelectionMediator.SelectedSceneObjectClone.ParametersAsociated.OrderBy(x => x.ParameterType.ShortName).ToList();
                    
                    if(this.ParametersInUse is not null && this.ParametersInUse.Any())
                    {
                        this.SelectedParameter = this.ParametersInUse.First();
                    }
                    else
                    {
                        this.SelectedParameter = null;
                    }
                }
            }
        }

        /// <summary>
        /// When the button for submit changes is clicked
        /// </summary>
        public void OnSubmit()
        {
            this.SelectionMediator.SceneObjectHasChanges = false;
            this.ParameterHaveChanges = false;

            foreach (var keyValue in this.ChangedParameterValueSetRelations)
            {
                var valueSet = keyValue.Value;

                if (valueSet is ParameterValueSetBase parameterValueSetBase)
                {
                    var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                    var valueSetNewValue = valueSet.ActualValue;
                    clonedParameterValueSet.Manual = valueSetNewValue;
                    this.SessionService.UpdateThings(parameterValueSetBase.GetContainerOfType<Iteration>(), new List<Thing>() { clonedParameterValueSet });
                }
            }

            this.ParameterHaveChanges = false;
        }

        /// <summary>
        /// Event for when a <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/> has changed.
        /// </summary>
        /// <param name="valueSet"></param>
        public async Task ParameterValueSetChanged(IValueSet valueSet)
        {
            if (valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var newValueArray = new ValueArray<string>(valueSet.ActualValue);

                var validationMessage = ParameterValueValidator.Validate(valueSet.ActualValue.First(), this.SelectedParameter.ParameterType, this.SelectedParameter?.Scale);

                if (validationMessage != null)
                {
                    this.ParameterHaveChanges = false;
                }
                else
                {

                    this.SelectionMediator.SceneObjectHasChanges = true;
                    this.ParameterHaveChanges = true;

                    var clonedValueSetBase = parameterValueSetBase.Clone(false);
                    clonedValueSetBase.Manual = newValueArray;
                    this.ParameterValueSetRelations[this.SelectedParameter] = clonedValueSetBase;

                    if (this.ChangedParameterValueSetRelations.ContainsKey(this.SelectedParameter))
                    {
                        this.ChangedParameterValueSetRelations[this.SelectedParameter] = clonedValueSetBase;
                    }
                    else
                    {
                        this.ChangedParameterValueSetRelations.Add(this.SelectedParameter, clonedValueSetBase);
                    }

                    this.SelectionMediator.SelectedSceneObjectClone.UpdateParameter(this.SelectedParameter, clonedValueSetBase);

                    if (this.SelectedParameter.ParameterType.ShortName == SceneSettings.ShapeKindShortName)
                    {
                        if (this.SelectionMediator.SelectedSceneObjectClone.Primitive is not null)
                        {
                            this.SelectionMediator.SelectedSceneObjectClone.Primitive.HasHalo = true;
                        }

                        var parameters = this.ParameterValueSetRelations.Keys.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);

                        foreach (var parameter in parameters)
                        {
                            this.SelectionMediator.SelectedSceneObjectClone?.UpdateParameter(parameter, this.ParameterValueSetRelations[parameter]);
                        }
                    }

                    await this.BabylonInterop.RegenerateMesh(this.SelectionMediator?.SelectedSceneObjectClone);
                }
            }
        }

        /// <summary>
        /// Gets the current used <see cref="IValueSet"/>
        /// </summary>
        /// <returns>the <see cref="IValueSet"/></returns>
        public IValueSet GetUsedValueSet()
        {
            if (this.SelectedParameter is not null && this.ParameterValueSetRelations.TryGetValue(this.SelectedParameter, out var valueSet))
            {
                return valueSet;
            }

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="IDetailsComponentViewModel"/>
        /// </summary>
        /// <returns>a <see cref="IDetailsComponentViewModel"/> based on this <see cref="IPropertiesComponentViewModel"/></returns>
        public IDetailsComponentViewModel CreateDetailsComponentViewModel()
        {
            return new DetailsComponentViewModel(this.IsVisible, this.SelectedParameter?.ParameterType, this.GetUsedValueSet(), this.OnParameterValueSetChanged);
        }
    }
}
