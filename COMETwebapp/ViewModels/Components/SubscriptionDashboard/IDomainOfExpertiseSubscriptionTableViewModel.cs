// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IDomainOfExpertiseSubscriptionTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DynamicData;

    /// <summary>
    /// View Model that provides content related to owned <see cref="ParameterOrOverrideBase"/> where other
    /// <see cref="DomainOfExpertise"/> placed <see cref="ParameterSubscription"/>
    /// </summary>
    public interface IDomainOfExpertiseSubscriptionTableViewModel
    {
        /// <summary>
        /// A reactive collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel"/>
        /// </summary>
        SourceList<OwnedParameterOrOverrideBaseRowViewModel> Rows { get; }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase"/></param>
        void UpdateProperties(IEnumerable<ParameterOrOverrideBase> parameters);

        /// <summary>
        /// Apply filters on <see cref="OwnedParameterOrOverrideBaseRowViewModel" /> based on a multi-select set of
        /// <see cref="Option" />s and a multi-select set of <see cref="ParameterType" />s
        /// </summary>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option" />s. <c>null</c> or an empty collection means no option
        /// filtering is applied.
        /// </param>
        /// <param name="selectedParameterTypes">
        /// The collection of selected <see cref="ParameterType" />s. <c>null</c> or an empty collection means no
        /// parameter-type filtering is applied.
        /// </param>
        void ApplyFilters(IEnumerable<Option> selectedOptions, IEnumerable<ParameterType> selectedParameterTypes);
    }
}
