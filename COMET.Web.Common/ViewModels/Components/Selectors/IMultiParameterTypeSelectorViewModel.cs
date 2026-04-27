// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IMultiParameterTypeSelectorViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// View Model that enables the user to select one or more <see cref="ParameterType" />s for use as a multi-select filter.
    /// </summary>
    public interface IMultiParameterTypeSelectorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the currently selected <see cref="ParameterType" />s. An empty collection means
        /// "no filter" — every <see cref="ParameterType" /> in <see cref="AvailableParameterTypes" /> matches.
        /// </summary>
        IEnumerable<ParameterType> SelectedParameterTypes { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="ParameterType" />s that the user can pick from.
        /// </summary>
        IEnumerable<ParameterType> AvailableParameterTypes { get; }

        /// <summary>
        /// Restricts the <see cref="AvailableParameterTypes" /> collection to only the entries whose
        /// <see cref="ParameterType.Iid" /> appears in <paramref name="parameterTypesId" />. Any entries in
        /// <see cref="SelectedParameterTypes" /> that no longer appear in <see cref="AvailableParameterTypes" />
        /// are dropped from the selection.
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="System.Guid" />s identifying the <see cref="ParameterType" />s to retain.</param>
        void FilterAvailableParameterTypes(IEnumerable<Guid> parameterTypesId);

        /// <summary>
        /// Excludes the entries whose <see cref="ParameterType.Iid" /> appears in <paramref name="parameterTypesId" />
        /// from <see cref="AvailableParameterTypes" />.
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="System.Guid" />s identifying the <see cref="ParameterType" />s to exclude.</param>
        void ExcludeAvailableParameterTypes(IEnumerable<Guid> parameterTypesId);
    }
}
