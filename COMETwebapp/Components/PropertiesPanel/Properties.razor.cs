// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Properties.razor.cs" company="RHEA System S.A.">
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
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal;

    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The properties component used for displaying data about the selected primitive
    /// </summary>
    public partial class Properties 
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedPrimitive"/> property
        /// </summary>
        private Primitive selectedPrimitive;

        /// <summary>
        /// Gets or sets the <see cref="Primitive"/> to fill the panel
        /// </summary>
        [Parameter]
        public Primitive SelectedPrimitive
        {
            get => selectedPrimitive;
            set
            {
                selectedPrimitive = value.Clone();
                this.BabylonCanvas.ClearTemporarySceneObjects();
                this.BabylonCanvas.AddTemporarySceneObject(new Model.SceneObject(this.selectedPrimitive));
                this.InitPanelProperties();
            }
        }

        /// <summary>
        /// Gets or sets the canvas where the 3D scene is drawn
        /// </summary>
        [Parameter]
        public BabylonCanvas BabylonCanvas { get; set; }

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
        public DetailsComponent DetailsReference { get; set; }

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
                this.InitPanelProperties();
            }
        }

        /// <summary>
        /// Initializes the properties for the panel
        /// </summary>
        private void InitPanelProperties()
        {
            this.ParametersInUse = this.SelectedPrimitive.ElementUsage.GetParametersInUse().OrderBy(x=>x.ParameterType.ShortName).ToList();
            this.ParameterChanged(ParametersInUse.First());
        }

        /// <summary>
        /// When the button for submit changes is clicked
        /// </summary>
        public void OnSubmit()
        {
            var collection = this.SelectedPrimitive.GetValueSets();

            foreach(var key in collection.Keys)
            {
                var valueSet = collection[key];

                if(valueSet is not null && valueSet is ParameterValueSetBase parameterValueSetBase)
                {
                    if (!this.IterationService.NewUpdates.Contains(parameterValueSetBase.Iid))
                    {
                        this.IterationService.NewUpdates.Add(parameterValueSetBase.Iid);
                        CDPMessageBus.Current.SendMessage<NewUpdateEvent>(new NewUpdateEvent(parameterValueSetBase.Iid));
                    }
                    var clonedParameterValueSet = parameterValueSetBase.Clone(false);

                    var newValue = this.DetailsReference.GetValueSet(key).Manual;

                    clonedParameterValueSet.Manual = newValue;

                    this.SessionAnchor.UpdateThings(new List<Thing>()
                    {
                        clonedParameterValueSet
                    });
                }
            }
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
