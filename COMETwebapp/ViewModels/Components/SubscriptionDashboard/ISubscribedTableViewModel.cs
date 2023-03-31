// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISubscribedTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows;

    using DynamicData;

    /// <summary>
    /// View Model that provide content related to <see cref="ElementBase" /> that contains
    /// <see cref="ParameterOrOverrideBase" /> with <see cref="ParameterSubscription" /> owned by the current
    /// <see cref="DomainOfExpertise" />
    /// </summary>
    public interface ISubscribedTableViewModel
    {
        /// <summary>
        /// A reactive collection of <see cref="ParameterSubscriptionRowViewModel" /> to display
        /// </summary>
        public SourceList<ParameterSubscriptionRowViewModel> Rows { get; }

        /// <summary>
        /// Value asserting that any <see cref="ParameterSubscription" /> had changed with an <see cref="Iteration" /> update
        /// </summary>
        bool DidSubscriptionsChanged { get; }

        /// <summary>
        /// Value indicating if the rows should only display <see cref="ParameterSubscription" /> that has changed
        /// </summary>
        bool ShowOnlyChangedSubscription { get; set; }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="subscriptions">A collection of <see cref="ParameterSubscription" /></param>
        /// <param name="availableOptions">A collection of available <see cref="Option" /></param>
        /// <param name="newIteration">The current <see cref="Iteration" /></param>
        public void UpdateProperties(IEnumerable<ParameterSubscription> subscriptions, IEnumerable<Option> availableOptions, Iteration newIteration);

        /// <summary>
        /// Apply filters on <see cref="ParameterSubscriptionRowViewModel" /> based on the <see cref="Option" /> and
        /// <see cref="ParameterType" />
        /// </summary>
        /// <param name="selectedOption">The selected <see cref="Option" /></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType" /></param>
        public void ApplyFilters(Option selectedOption, ParameterType selectedParameterType);

        /// <summary>
        /// Validates all changes of <see cref="ParameterValueSetBase" /> related to the current <see cref="Iteration" />
        /// </summary>
        void ValidateAllChanges();
    }
}
