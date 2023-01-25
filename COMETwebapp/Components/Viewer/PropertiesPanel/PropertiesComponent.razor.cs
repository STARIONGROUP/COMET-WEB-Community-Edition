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

namespace COMETwebapp.Components.Viewer.PropertiesPanel
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using CDP4Dal;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Components.Viewer.PopUps;
    using COMETwebapp.Extensions;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.Services.IterationServices;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The properties component used for displaying data about the selected primitive
    /// </summary>
    public partial class PropertiesComponent
    {
        /// <summary>
        /// Gets or sets the canvas where the 3D scene is drawn
        /// </summary>
        [Parameter]
        public CanvasComponent Canvas { get; set; }

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
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase"/> to fill the details
        /// </summary>
        public ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// The list of parameters that the <see cref="SelectedPrimitive"/> uses
        /// </summary>
        public List<ParameterBase> ParametersInUse { get; set; } = new();

        /// <summary>
        /// A reference to the <see cref="DetailsComponent"/>
        /// </summary>
        public DetailsComponent DetailsReference { get; set; }

        /// <summary> 
        /// Gets or sets if this component is visible 
        /// </summary> 
        private bool IsVisible { get; set; }

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
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.SelectionMediator.OnTreeSelectionChanged += (sender, node) =>
                {
                    this.OnSelectionChanged(node.SceneObject);
                };
                this.SelectionMediator.OnModelSelectionChanged += (sender, sceneObject) =>
                {
                    this.OnSelectionChanged(sceneObject);
                };
            }
        }

        /// <summary> 
        /// Called when the selection of a <see cref="SceneObject"/> has changed 
        /// </summary> 
        /// <param name="sceneObject">the changed object</param> 
        /// <returns>an asynchronous operation</returns> 
        private void OnSelectionChanged(SceneObject sceneObject)
        {
            this.IsVisible = sceneObject is not null ? true : false;
            if(this.SelectionMediator.SelectedSceneObjectClone is not null)
            {
                this.ParametersInUse = this.SelectionMediator.SelectedSceneObjectClone?.ParametersAsociated.OrderBy(x => x.ParameterType.ShortName).ToList();
                this.SelectedParameterChanged(ParametersInUse.First());
            }
            this.StateHasChanged();
        }

        /// <summary>
        /// When the button for submit changes is clicked
        /// </summary>
        public void OnSubmit()
        {
            var changedParametersKeyValue = GetParameterValueSetRelationsChanges();

            foreach (var keyValue in changedParametersKeyValue)
            {
                var valueSet = keyValue.Value;

                if (valueSet is ParameterValueSetBase parameterValueSetBase && !this.IterationService.NewUpdates.Contains(parameterValueSetBase.Iid))
                {
                    this.IterationService.NewUpdates.Add(parameterValueSetBase.Iid);
                    CDPMessageBus.Current.SendMessage(new NewUpdateEvent(parameterValueSetBase.Iid));

                    var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                    var valueSetNewValue = valueSet.ActualValue;
                    clonedParameterValueSet.Manual = valueSetNewValue;
                    this.SessionService.UpdateThings(this.SessionService.DefaultIteration, new List<Thing>() { clonedParameterValueSet });
                }
            }
            this.SelectionMediator.SceneObjectHasChanges = false;
        }

        /// <summary>
        /// Gets the values that have changed for two ParameterValueSetRelation
        /// </summary>
        /// <returns>the values changed</returns>
        private Dictionary<ParameterBase, IValueSet> GetParameterValueSetRelationsChanges()
        {
            var relation1 = this.SelectionMediator.SelectedSceneObject?.GetParameterValueSetRelations();
            var relation2 = this.DetailsReference.GetParameterValueSetRelations();

            if(relation1 != null && relation2 != null)
            {
                var changes = relation1.GetChangesOnParameters(relation2);
                this.SelectionMediator.SceneObjectHasChanges = changes.Any();
                return changes;
            }

            return new();
        }

        /// <summary>
        /// Event for when a parameter has changed it's value
        /// </summary>
        private void OnParameterValueChanged()
        {
            this.StateHasChanged();
        }

        /// <summary> 
        /// Event for when a parameter item has been clicked 
        /// </summary> 
        /// <param name="parameterBase">the parameter clicked</param> 
        private void SelectedParameterChanged(ParameterBase parameterBase)
        {
            this.SelectedParameter = parameterBase;
        }
    }
}
