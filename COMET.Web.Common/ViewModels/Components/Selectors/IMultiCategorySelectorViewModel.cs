// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IMultiCategorySelectorViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// View Model that enables the user to select one or more <see cref="Category" />s for use as a multi-select
    /// filter scoped to the current <see cref="CDP4Common.EngineeringModelData.Iteration" />'s reference data
    /// libraries. The set of <see cref="ClassKind" />s a <see cref="Category" /> must be permissible for is
    /// configurable via <see cref="ApplicableClassKinds" />, so the same view model can drive Element-,
    /// ParameterType-, Requirement- or unrestricted category pickers.
    /// </summary>
    public interface IMultiCategorySelectorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the currently selected <see cref="Category" />s. An empty collection means "no filter" —
        /// every <see cref="Category" /> in <see cref="AvailableCategories" /> matches.
        /// </summary>
        IEnumerable<Category> SelectedCategories { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="Category" />s that the user can pick from. Populated from the
        /// current <see cref="CDP4Common.EngineeringModelData.Iteration" />'s accessible reference data libraries
        /// and filtered to categories whose <see cref="Category.PermissibleClass" /> intersects
        /// <see cref="ApplicableClassKinds" />. When <see cref="ApplicableClassKinds" /> is empty or
        /// <see langword="null" /> every <see cref="Category" /> reachable through the RDL chain is exposed.
        /// </summary>
        IEnumerable<Category> AvailableCategories { get; }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind" />s a <see cref="Category" /> must be permissible for to appear
        /// in <see cref="AvailableCategories" />. An empty or <see langword="null" /> collection disables the
        /// <see cref="Category.PermissibleClass" /> filter and surfaces every <see cref="Category" /> reachable
        /// from the current <see cref="CDP4Common.EngineeringModelData.Iteration" />'s reference data libraries.
        /// Set this before assigning <see cref="IBelongsToIterationSelectorViewModel.CurrentIteration" />; the
        /// value is read on the next <see cref="MultiCategorySelectorViewModel.UpdateProperties" /> pass.
        /// </summary>
        IEnumerable<ClassKind> ApplicableClassKinds { get; set; }
    }
}
