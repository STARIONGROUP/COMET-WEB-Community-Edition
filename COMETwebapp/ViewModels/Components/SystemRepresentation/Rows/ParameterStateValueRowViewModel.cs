// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterStateValueRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation.Rows
{
    /// <summary>
    /// Immutable row view model representing the values of a single <see cref="CDP4Common.EngineeringModelData.ActualFiniteState" />
    /// for a state-dependent <see cref="CDP4Common.EngineeringModelData.Parameter" />.
    /// One instance is produced per state value-set when <see cref="ElementDefinitionDetailsRowViewModel.IsStateDependent" /> is <c>true</c>.
    /// </summary>
    public class ParameterStateValueRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterStateValueRowViewModel" /> class.
        /// </summary>
        /// <param name="stateName">The name of the <see cref="CDP4Common.EngineeringModelData.ActualFiniteState" />.</param>
        /// <param name="actualValue">The formatted actual value string for this state.</param>
        /// <param name="publishedValue">The formatted published value string for this state.</param>
        /// <param name="switchValue">The string representation of the <see cref="CDP4Common.EngineeringModelData.ParameterSwitchKind" /> for this state.</param>
        public ParameterStateValueRowViewModel(string stateName, string actualValue, string publishedValue, string switchValue)
        {
            this.StateName = stateName;
            this.ActualValue = actualValue;
            this.PublishedValue = publishedValue;
            this.SwitchValue = switchValue;
        }

        /// <summary>
        /// Gets the name of the <see cref="CDP4Common.EngineeringModelData.ActualFiniteState" /> this row represents.
        /// </summary>
        public string StateName { get; }

        /// <summary>
        /// Gets the formatted actual value string for this state, including the measurement scale suffix when applicable.
        /// </summary>
        public string ActualValue { get; }

        /// <summary>
        /// Gets the formatted published value string for this state, including the measurement scale suffix when applicable.
        /// </summary>
        public string PublishedValue { get; }

        /// <summary>
        /// Gets the string representation of the value switch (<see cref="CDP4Common.EngineeringModelData.ParameterSwitchKind" />) for this state.
        /// </summary>
        public string SwitchValue { get; }
    }
}
