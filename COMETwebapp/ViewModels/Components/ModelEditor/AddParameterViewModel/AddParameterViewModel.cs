// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AddParameterViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model for the <see cref="AddParameterViewModel" /> component
    /// </summary>
    public class AddParameterViewModel : DisposableObject, IAddParameterViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddParameterViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public AddParameterViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;
            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterTypeSelectorViewModel.SelectedParameterType).Subscribe(this.OnParameterTypeChange));
            var callbackFactory = new EventCallbackFactory();

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = callbackFactory.Create<DomainOfExpertise>(this, selectedOwner => { this.Parameter.Owner = selectedOwner; })
            };

            this.MeasurementScaleSelectorViewModel = new MeasurementScaleSelectorViewModel(sessionService)
            {
                OnSelectedMeasurementScaleChange = callbackFactory.Create<MeasurementScale>(this, selectedScale => { this.Parameter.Scale = selectedScale; })
            };
        }

        /// <summary>
        /// Gets or sets the current <see cref="Iteration" />
        /// </summary>
        private Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Gets the <see cref="IMeasurementScaleSelectorViewModel" />
        /// </summary>
        public IMeasurementScaleSelectorViewModel MeasurementScaleSelectorViewModel { get; private set; }

        /// <summary>
        /// The callback executed when the method <see cref="AddParameterToElementDefinition" /> was executed
        /// </summary>
        public EventCallback OnParameterAdded { get; set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelectorViewModel { get; private set; } = new ParameterTypeSelectorViewModel { QueryOnlyUsedParameterTypes = false };

        /// <summary>
        /// The <see cref="ElementDefinition" /> to create or edit
        /// </summary>
        public Parameter Parameter { get; set; } = new();

        /// <summary>
        /// The <see cref="ElementDefinition" /> to create or edit
        /// </summary>
        public ElementDefinition SelectedElementDefinition { get; private set; } = new();

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

        /// <summary>
        /// The collection of <see cref="ActualFiniteStateList" /> to list for selection
        /// </summary>
        public IEnumerable<ActualFiniteStateList> PossibleFiniteStates { get; private set; }

        /// <summary>
        /// The collection of <see cref="ParameterGroup" /> to list for selection
        /// </summary>
        public IEnumerable<ParameterGroup> ParameterGroups => this.SelectedElementDefinition.ParameterGroup;

        /// <summary>
        /// Sets the <see cref="SelectedElementDefinition" />
        /// </summary>
        /// <param name="selectedElementDefinition"></param>
        public void SetSelectedElementDefinition(ElementDefinition selectedElementDefinition)
        {
            if (selectedElementDefinition is null)
            {
                return;
            }

            this.SelectedElementDefinition = selectedElementDefinition;
            var elementDefinitionParameterTypes = this.SelectedElementDefinition.Parameter.Select(x => x.ParameterType);
            this.ParameterTypeSelectorViewModel.ExcludeAvailableParameterTypes(elementDefinitionParameterTypes.Select(x => x.Iid));

            this.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise = selectedElementDefinition
                .GetContainerOfType<EngineeringModel>()
                .EngineeringModelSetup.ActiveDomain
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(true);
        }

        /// <summary>
        /// Initializes the current view model
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        public void InitializeViewModel(Iteration iteration)
        {
            this.CurrentIteration = iteration;
            this.PossibleFiniteStates = iteration.ActualFiniteStateList;
            this.ParameterTypeSelectorViewModel.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
        }

        /// <summary>
        /// Adds a parameter of type selected from <see cref="ParameterTypeSelectorViewModel" /> to the
        /// <see cref="SelectedElementDefinition" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task AddParameterToElementDefinition()
        {
            var elementDefinitionClone = this.SelectedElementDefinition.Clone(false);
            var selectedParameterType = this.ParameterTypeSelectorViewModel.SelectedParameterType;
            var parameterClone = this.Parameter.Clone(false);

            parameterClone.ParameterType = selectedParameterType;
            elementDefinitionClone.Parameter.Add(parameterClone);
            await this.sessionService.CreateOrUpdateThings(elementDefinitionClone, [elementDefinitionClone, parameterClone]);

            await this.OnParameterAdded.InvokeAsync();
        }

        /// <summary>
        /// Resets this view model properties values
        /// </summary>
        public void ResetValues()
        {
            this.Parameter = new Parameter();
            this.ParameterTypeSelectorViewModel.SelectedParameterType = null;
            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(true);
        }

        /// <summary>
        /// Method executed every time the parameter type has changed
        /// </summary>
        /// <param name="parameterType">The new parameter type</param>
        private void OnParameterTypeChange(ParameterType parameterType)
        {
            this.Parameter.ParameterType = parameterType;

            if (this.Parameter.ParameterType is not QuantityKind quantityKind)
            {
                return;
            }

            this.MeasurementScaleSelectorViewModel.AvailableMeasurementScales = quantityKind.AllPossibleScale;
            this.MeasurementScaleSelectorViewModel.SelectedMeasurementScale = quantityKind.DefaultScale;
        }
    }
}
