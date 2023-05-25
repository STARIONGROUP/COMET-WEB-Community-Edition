// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DynamicData;

    /// <summary>
    /// Interface for the <see cref="ParameterTableViewModel"/>
    /// </summary>
    public interface IParameterTableViewModel
    {
        /// <summary>
        /// Gets the collection of the <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        SourceList<ParameterBaseRowViewModel> Rows { get; }

        /// <summary>
        /// The <see cref="IHaveComponentParameterTypeEditor"/> to show in the popup
        /// </summary>
        IHaveComponentParameterTypeEditor HaveComponentParameterTypeEditorViewModel { get; set; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnEditMode { get; set; }

        /// <summary>
        /// Value indicating if the <see cref="ElementDefinition"/> is top element
        /// </summary>
        bool IsTopElement { get; set; }

        /// <summary>
        /// The <see cref="ElementDefinition" /> to create or edit
        /// </summary>
        ElementDefinition ElementDefinition { get; set; } 

        /// <summary>
        /// Initializes this <see cref="IParameterTableViewModel"/>
        /// </summary>
        /// <param name="currentIteration">The current <see cref="Iteration"/></param>
        /// <param name="currentDomain">The <see cref="DomainOfExpertise"/></param>
        /// <param name="selectedOption">The select <see cref="Option"/></param>
        void InitializeViewModel(Iteration currentIteration, DomainOfExpertise currentDomain, Option selectedOption);

        /// <summary>
        /// Update the current <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="currentDomain">The new <see cref="DomainOfExpertise"/></param>
        void UpdateDomain(DomainOfExpertise currentDomain);

        /// <summary>
        /// A collection of available <see cref="Category" />s
        /// </summary>
        IEnumerable<Category> AvailableCategories { get; set; }

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />s
        /// </summary>
        IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

        /// <summary>
        /// Selected super <see cref="Category" />
        /// </summary>
        IEnumerable<Category> SelectedCategories { get; set; }

        /// <summary>
        /// Apply filters based on <see cref="Option"/>, <see cref="ElementBase"/>, <see cref="ParameterType"/> and <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="selectedOption">The selected <see cref="Option"/></param>
        /// <param name="selectedElementBase">The selected <see cref="ElementBase"/></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType"/></param>
        /// <param name="isOwnedParameters">Value asserting that the only <see cref="Thing"/> owned by the current <see cref="DomainOfExpertise"/> should be visible</param>
        void ApplyFilters(Option selectedOption, ElementBase selectedElementBase, ParameterType selectedParameterType, bool isOwnedParameters);

        /// <summary>
        /// Remove rows related to a <see cref="Thing"/> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="Thing"/></param>
        void RemoveRows(IEnumerable<Thing> deletedThings);

        /// <summary>
        /// Add rows related to <see cref="Thing"/> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing"/></param>
        void AddRows(IEnumerable<Thing> addedThings);

        /// <summary>
        /// Updates rows related to <see cref="Thing"/> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing"/></param>
        void UpdateRows(IEnumerable<Thing> updatedThings);

        /// <summary>
        /// Tries to create a new <see cref="ElementDefinition" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task AddingElementDefinition();

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        void OnInitialized();
    }
}
