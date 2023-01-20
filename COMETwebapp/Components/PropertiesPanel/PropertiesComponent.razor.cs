// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesComponent.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.PropertiesPanel
{
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal;
    
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Components.PopUps;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    
    using Microsoft.AspNetCore.Components;
    
    /// <summary>
    /// The properties component used for displaying data about the selected primitive
    /// </summary>
    public partial class PropertiesComponent
    {
        /// <summary>
        /// Gets or sets the possible next selected <see cref="SceneObject"/>
        /// </summary>
        private SceneObject? PossibleNextSelected { get; set; }

        /// <summary>
        /// Gets or sets the original scene object without changes
        /// </summary>
        private SceneObject? OriginalSceneObject { get; set; }

        /// <summary>
        /// Backing field for the <see cref="SelectedSceneObject"/> property
        /// </summary>
        private SceneObject? selectedSceneObject;

        /// <summary>
        /// Gets or sets the <see cref="Primitive"/> to fill the panel
        /// </summary>
        public SceneObject? SelectedSceneObject
        {
            get => this.selectedSceneObject;
            set
            {
                this.OriginalSceneObject = value;
                this.selectedSceneObject = value is not null ? value.Clone() : value;
            }
        }

        /// <summary>
        /// Gets or sets the canvas where the 3D scene is drawn
        /// </summary>
        [Parameter]
        public CanvasComponent Canvas { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ConfirmChangeSelectionPopUp"/> component
        /// </summary>
        [Parameter]
        public ConfirmChangeSelectionPopUp ConfirmChangeSelectionPopUp { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase"/> to fill the details
        /// </summary>
        [Parameter]
        public ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="IIterationService"/>
        /// </summary>
        [Inject]
        public IIterationService IterationService { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        public ISessionAnchor SessionAnchor { get; set; }

        /// <summary>
        /// The list of parameters that the <see cref="SelectedPrimitive"/> uses
        /// </summary>
        [Parameter]
        public List<ParameterBase> ParametersInUse { get; set; } = new();

        /// <summary>
        /// A reference to the <see cref="DetailsComponent"/>
        /// </summary>
        public DetailsComponent? DetailsReference { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.ConfirmChangeSelectionPopUp.OnResponse += async (sender, response) =>
                {
                    if (response)
                    {
                        await this.OnSelectionMediatorReaction(this.PossibleNextSelected);
                    }
                    this.PossibleNextSelected = null;
                };

                this.SelectionMediator.OnTreeSelectionChanged += async (sender, node) =>
                {
                    if (this.CheckIfChangeSelectionIsSure())
                    {
                        await this.OnSelectionMediatorReaction(node.SceneObject);
                    }
                    else
                    {
                        this.ConfirmChangeSelectionPopUp.IsVisible = true;
                        this.PossibleNextSelected = node.SceneObject;
                    }
                };
                this.SelectionMediator.OnModelSelectionChanged += async (sender, sceneObject) =>
                {
                    if (this.CheckIfChangeSelectionIsSure())
                    {
                        await this.OnSelectionMediatorReaction(sceneObject);
                    }
                    else
                    {
                        this.ConfirmChangeSelectionPopUp.IsVisible = true;
                        this.PossibleNextSelected = sceneObject;
                    }
                };
            }
        }

        /// <summary>
        /// Checks if changin the selection is sure because the changes have been sent to server
        /// </summary>
        private bool CheckIfChangeSelectionIsSure()
        {
            return !this.GetChangedParametersKeyValue().Any();
        }

        /// <summary>
        /// Called when the <see cref="ISelectionMediator"/> has reacted
        /// </summary>
        /// <param name="sceneObject">the scene object that the <see cref="ISelectionMediator"/> has reacted to</param>
        private async Task OnSelectionMediatorReaction(SceneObject? sceneObject)
        {
            this.SelectedSceneObject = sceneObject;
            if (this.SelectedSceneObject is not null)
            {
                await this.Canvas.ClearTemporarySceneObjects();
                await this.Canvas.AddTemporarySceneObject(this.SelectedSceneObject);
                this.ParametersInUse = this.SelectedSceneObject.ParametersAsociated.OrderBy(x => x.ParameterType.ShortName).ToList();
                this.ParameterChanged(ParametersInUse.First());
            }
            await this.InvokeAsync(() => this.StateHasChanged());
        }

        /// <summary>
        /// When the button for submit changes is clicked
        /// </summary>
        public void OnSubmit()
        {
            var changedParametersKeyValue = this.GetChangedParametersKeyValue();

            foreach (var keyValue in changedParametersKeyValue)
            {
                var valueSet = keyValue.Value;

                if (valueSet is ParameterValueSetBase parameterValueSetBase)
                {
                    if (!this.IterationService.NewUpdates.Contains(parameterValueSetBase.Iid))
                    {
                        this.IterationService.NewUpdates.Add(parameterValueSetBase.Iid);
                        CDPMessageBus.Current.SendMessage<NewUpdateEvent>(new NewUpdateEvent(parameterValueSetBase.Iid));
                    }

                    var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                    var valueSetNewValue = valueSet.ActualValue;
                    clonedParameterValueSet.Manual = valueSetNewValue;
                    this.SessionAnchor.UpdateThings(new List<Thing>() { clonedParameterValueSet });
                }
            }
        }

        /// <summary>
        /// Gets the parameters that have changed from the original <see cref="SceneObject"/>
        /// </summary>
        /// <returns></returns>
        public Dictionary<ParameterBase, IValueSet> GetChangedParametersKeyValue()
        {
            Dictionary<ParameterBase, IValueSet> collection = new();

            if (this.DetailsReference is not null && this.OriginalSceneObject is not null)
            {
                var originalValueSetsCollection = this.OriginalSceneObject.GetValueSets();
                var temporaryValueSetsCollection = this.DetailsReference.GetAllValueSets();

                foreach (var originalValueSet in originalValueSetsCollection)
                {
                    var valueSet = originalValueSet.Value.ActualValue;

                    if (temporaryValueSetsCollection.ContainsKey(originalValueSet.Key))
                    {
                        var temporarySet = temporaryValueSetsCollection[originalValueSet.Key].ActualValue;

                        if (!valueSet.ContainsSameValues(temporarySet))
                        {
                            collection.Add(originalValueSet.Key, temporaryValueSetsCollection[originalValueSet.Key]);
                        }
                    }
                }
            }
            return collection;
        }

        /// <summary>
        /// Event for when a parameter item has been clicked
        /// </summary>
        /// <param name="parameterBase">the parameter clicked</param>
        public void ParameterChanged(ParameterBase parameterBase)
        {
            this.SelectedParameter = parameterBase;          
            this.StateHasChanged();
        }
    }
}
