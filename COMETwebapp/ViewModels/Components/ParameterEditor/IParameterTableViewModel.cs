// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DynamicData;

    /// <summary>
    /// Interface for the <see cref="ParameterTableViewModel"/>
    /// </summary>
    public interface IParameterTableViewModel : IHaveReusableRows
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
        /// Initializes this <see cref="IParameterTableViewModel"/>
        /// </summary>
        /// <param name="currentIteration">The current <see cref="Iteration"/></param>
        /// <param name="currentDomain">The <see cref="DomainOfExpertise"/></param>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option"/>s. <c>null</c> or an empty collection falls back to the
        /// <see cref="Iteration"/>'s default <see cref="Option"/>.
        /// </param>
        void InitializeViewModel(Iteration currentIteration, DomainOfExpertise currentDomain, IEnumerable<Option> selectedOptions);

        /// <summary>
        /// Update the current <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="currentDomain">The new <see cref="DomainOfExpertise"/></param>
        void UpdateDomain(DomainOfExpertise currentDomain);

        /// <summary>
        /// Apply filters based on a multi-select set of <see cref="Option"/>s, a multi-select set of
        /// <see cref="ElementBase"/>s, <see cref="ParameterType"/>, <see cref="Category"/> and
        /// <see cref="DomainOfExpertise"/>.
        /// </summary>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option"/>s. <c>null</c> or an empty collection falls back to the
        /// <see cref="Iteration"/>'s default <see cref="Option"/>.
        /// </param>
        /// <param name="selectedElementBases">
        /// The collection of <see cref="ElementBase"/>s to filter on. <c>null</c> or an empty collection means no
        /// element filter is applied; otherwise rows whose owning <see cref="ElementBase"/> is not in the
        /// collection are removed.
        /// </param>
        /// <param name="selectedParameterTypes">
        /// The collection of <see cref="ParameterType"/>s to filter on. <c>null</c> or an empty collection means
        /// no parameter-type filter is applied; otherwise rows whose <see cref="ParameterType"/> is not in the
        /// collection are removed.
        /// </param>
        /// <param name="selectedCategories">
        /// The collection of <see cref="Category"/>s to filter on. <c>null</c> or an empty collection means no
        /// category filter is applied; otherwise rows whose owning <see cref="ElementBase"/> does not carry at
        /// least one of these categories (transitively, including the referenced
        /// <see cref="ElementDefinition"/>'s categories for an <see cref="ElementUsage"/>, and super-categories)
        /// are removed.
        /// </param>
        /// <param name="isOwnedParameters">Value asserting that only <see cref="Thing"/>s owned by the current <see cref="DomainOfExpertise"/> should be visible.</param>
        void ApplyFilters(IEnumerable<Option> selectedOptions, IEnumerable<ElementBase> selectedElementBases, IEnumerable<ParameterType> selectedParameterTypes, IEnumerable<Category> selectedCategories, bool isOwnedParameters);
    }
}
