// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PropertiesComponentViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="PropertiesComponent" />
    /// </summary>
    public class PropertiesComponentViewModel : ReactiveObject, IPropertiesComponentViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="IsVisible" />
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Backing field for the <see cref="ParameterHaveChanges" />
        /// </summary>
        private bool parameterHaveChanges;

        /// <summary>
        /// Backing field for the <see cref="ParametersInUse" />
        /// </summary>
        private List<ParameterBase> parametersInUse = new();

        /// <summary>
        /// Backing field for the <see cref="SelectedParameter" />
        /// </summary>
        private ParameterBase selectedParameter;

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Creates a new instance of type <see cref="PropertiesComponentViewModel" />
        /// </summary>
        /// <param name="babylonInterop">the <see cref="IBabylonInterop" /></param>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="selectionMediator">the <see cref="ISelectionMediator" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public PropertiesComponentViewModel(IBabylonInterop babylonInterop, ISessionService sessionService, ISelectionMediator selectionMediator, ICDPMessageBus messageBus)
        {
            this.BabylonInterop = babylonInterop;
            this.SessionService = sessionService;
            this.SelectionMediator = selectionMediator;
            this.messageBus = messageBus;

            this.OnParameterValueSetChanged = new EventCallbackFactory().Create(this, async ((IValueSet,int) valueSet) => { await this.ParameterValueSetChanged(valueSet); });

            this.SelectionMediator.OnTreeSelectionChanged += nodeViewModel => this.OnSelectionChanged(nodeViewModel.SceneObject);
            this.SelectionMediator.OnModelSelectionChanged += this.OnSelectionChanged;
        }

        /// <summary>
        /// Gets or sets the <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> that have changed;
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> ChangedParameterValueSetRelations { get; set; } = new();

        /// <summary>
        /// Injected property to get access to <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator" />
        /// </summary>
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        public IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase" /> and <see cref="IValueSet" /> of the selected <see cref="SceneObject" />
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase" /> to fill the details
        /// </summary>
        public ParameterBase SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// The list of parameters that the selected <see cref="SceneObject" /> uses
        /// </summary>
        public List<ParameterBase> ParametersInUse
        {
            get => this.parametersInUse;
            set => this.RaiseAndSetIfChanged(ref this.parametersInUse, value);
        }

        /// <summary>
        /// Gets or sets if the parameters have changes
        /// </summary>
        public bool ParameterHaveChanges
        {
            get => this.parameterHaveChanges;
            set => this.RaiseAndSetIfChanged(ref this.parameterHaveChanges, value);
        }

        /// <summary>
        /// Gets or sets if this component is visible
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Event callback for when a <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> has changed
        /// </summary>
        public EventCallback<(IValueSet,int)> OnParameterValueSetChanged { get; set; }

        /// <summary>
        /// When the button for submit changes is clicked
        /// </summary>
        public void OnSubmit()
        {
            this.SelectionMediator.SceneObjectHasChanges = false;
            this.ParameterHaveChanges = false;

            foreach (var valueSet in this.ChangedParameterValueSetRelations.Select(keyValue => keyValue.Value))
            {
                if (valueSet is not ParameterValueSetBase parameterValueSetBase)
                {
                    continue;
                }

                var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                var valueSetNewValue = valueSet.ActualValue;
                clonedParameterValueSet.Manual = valueSetNewValue;
                this.SessionService.CreateOrUpdateThings(parameterValueSetBase.GetContainerOfType<Iteration>().Clone(false), new List<Thing> { clonedParameterValueSet });
            }

            this.ParameterHaveChanges = false;
        }

        /// <summary>
        /// Gets the current used <see cref="IValueSet" />
        /// </summary>
        /// <returns>the <see cref="IValueSet" /></returns>
        public IValueSet GetUsedValueSet()
        {
            if (this.SelectedParameter is not null && this.ParameterValueSetRelations.TryGetValue(this.SelectedParameter, out var valueSet))
            {
                return valueSet;
            }

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="IDetailsComponentViewModel" />
        /// </summary>
        /// <returns>
        /// a <see cref="IDetailsComponentViewModel" /> based on this <see cref="IPropertiesComponentViewModel" />
        /// </returns>
        public IDetailsComponentViewModel CreateDetailsComponentViewModel()
        {
            return new DetailsComponentViewModel(this.IsVisible, this.SelectedParameter?.ParameterType, this.GetUsedValueSet(), this.OnParameterValueSetChanged, this.messageBus);
        }

        /// <summary>
        /// Event for when a <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> has changed.
        /// </summary>
        /// <param name="valueTuple">The updated <see cref="IValueSet"/> with the index</param>
        public Task ParameterValueSetChanged((IValueSet valueSet,int _) valueTuple)
        {
            if (valueTuple.valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var validationMessageBuilder = new StringBuilder();
                var newValueArray = new ValueArray<string>(parameterValueSetBase.ActualValue);

                if(this.SelectedParameter.ParameterType is CompoundParameterType compoundParameterType)
                {
                    var components = compoundParameterType.Component.ToList();
                    
                    for(var componentIndex = 0; componentIndex < components.Count; componentIndex++)
                    {
                        var value = parameterValueSetBase.ActualValue[componentIndex];
                        validationMessageBuilder .Append(ParameterValueValidator.Validate(value, components[componentIndex].ParameterType, components[componentIndex]?.Scale));
                    }
                }
                else
                {
                    validationMessageBuilder.Append(ParameterValueValidator.Validate(parameterValueSetBase.ActualValue.First(), this.SelectedParameter.ParameterType, this.SelectedParameter?.Scale));
                }

                var validationMessage = validationMessageBuilder.ToString();

                if (!string.IsNullOrEmpty(validationMessage))
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
                    this.ChangedParameterValueSetRelations[this.SelectedParameter] = clonedValueSetBase;

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

                    return this.BabylonInterop.RegenerateMesh(this.SelectionMediator?.SelectedSceneObjectClone);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the selection of a <see cref="SceneObject" /> has changed
        /// </summary>
        /// <param name="sceneObject">the changed object</param>
        private void OnSelectionChanged(SceneObject sceneObject)
        {
            this.IsVisible = sceneObject is not null;

            if (this.SelectionMediator.SelectedSceneObjectClone is not null)
            {
                this.ParameterValueSetRelations = this.SelectionMediator.SelectedSceneObjectClone.GetParameterValueSetRelations();

                if (this.SelectionMediator.SelectedSceneObjectClone.ParametersAsociated is not null)
                {
                    this.ParametersInUse = this.SelectionMediator.SelectedSceneObjectClone.ParametersAsociated.OrderBy(x => x.ParameterType.ShortName).ToList();

                    if (this.ParametersInUse is not null && this.ParametersInUse.Any())
                    {
                        this.SelectedParameter = this.ParametersInUse[0];
                    }
                }
            }
        }
    }
}
