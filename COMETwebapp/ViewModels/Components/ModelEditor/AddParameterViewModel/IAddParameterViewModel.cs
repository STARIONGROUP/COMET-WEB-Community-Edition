// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IAddParameterViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Interface for the <see cref="ElementDefinitionTableViewModel" />
    /// </summary>
    public interface IAddParameterViewModel
    {
        /// <summary>
        /// The <see cref="ElementDefinition" /> to create or edit
        /// </summary>
        ElementDefinition SelectedElementDefinition { get; }

        /// <summary>
        /// The <see cref="ElementDefinition" /> to create or edit
        /// </summary>
        Parameter Parameter { get; set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel"/>
        /// </summary>
        IParameterTypeSelectorViewModel ParameterTypeSelectorViewModel { get; }

        /// <summary>
        /// The collection of <see cref="MeasurementScale" /> to list for selection, if the parameter type is quantity kind
        /// </summary>
        IEnumerable<MeasurementScale> MeasurementScales { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterGroup" /> to list for selection
        /// </summary>
        IEnumerable<ParameterGroup> ParameterGroups { get; }

        /// <summary>
        /// The callback executed when the method <see cref="AddParameterToElementDefinition"/> was executed
        /// </summary>
        EventCallback OnParameterAdded { get; set; }

        /// <summary>
        /// The collection of <see cref="ActualFiniteStateList" /> to list for selection
        /// </summary>
        IEnumerable<ActualFiniteStateList> PossibleFiniteStates { get; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Adds a parameter of type selected from <see cref="AddParameterViewModel.ParameterTypeSelectorViewModel"/> to the <see cref="AddParameterViewModel.SelectedElementDefinition"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task AddParameterToElementDefinition();

        /// <summary>
        /// Initializes the current view model
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        void InitializeViewModel(Iteration iteration);

        /// <summary>
        /// Sets the <see cref="AddParameterViewModel.SelectedElementDefinition"/>
        /// </summary>
        /// <param name="selectedElementDefinition"></param>
        void SetSelectedElementDefinition(ElementDefinition selectedElementDefinition);

        /// <summary>
        /// Resets this view model properties values
        /// </summary>
        void ResetValues();
    }
}
