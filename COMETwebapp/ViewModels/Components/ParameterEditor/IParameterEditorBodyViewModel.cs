// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterEditorBodyViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    /// <summary>
    /// View Model that handle the logic for the Parameter Editor application
    /// </summary>
    public interface IParameterEditorBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISubscriptionService" />
        /// </summary>
        ISubscriptionService SubscriptionService { get; set; }

        /// <summary>
        /// Gets the <see cref="IMultiElementBaseSelectorViewModel" /> driving the building-block filter. An empty
        /// selection means no element filtering is applied.
        /// </summary>
        public IMultiElementBaseSelectorViewModel ElementSelector { get; }

        /// <summary>
        /// Gets the <see cref="IMultiOptionSelectorViewModel" /> driving the option filter. An empty selection
        /// falls back to the <see cref="CDP4Common.EngineeringModelData.Iteration" />'s default
        /// <see cref="CDP4Common.EngineeringModelData.Option" />.
        /// </summary>
        public IMultiOptionSelectorViewModel OptionSelector { get; }

        /// <summary>
        /// Gets the <see cref="IMultiParameterTypeSelectorViewModel" /> driving the parameter-type filter.
        /// An empty selection means no parameter-type filtering is applied.
        /// </summary>
        public IMultiParameterTypeSelectorViewModel ParameterTypeSelector { get; }

        /// <summary>
        /// Gets the <see cref="IMultiCategorySelectorViewModel" /> driving the category filter on the building
        /// blocks (<see cref="CDP4Common.EngineeringModelData.ElementBase" />). An empty selection means no
        /// category filtering is applied.
        /// </summary>
        public IMultiCategorySelectorViewModel CategorySelector { get; }

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        bool IsOwnedParameters { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel" />
        /// </summary>
        public IParameterTableViewModel ParameterTableViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IBatchParameterEditorViewModel" />
        /// </summary>
        IBatchParameterEditorViewModel BatchParameterEditorViewModel { get; set; }

        /// <summary>
        /// Apply all the filters on the <see cref="IParameterTableViewModel" />
        /// </summary>
        void ApplyFilters();
    }
}
