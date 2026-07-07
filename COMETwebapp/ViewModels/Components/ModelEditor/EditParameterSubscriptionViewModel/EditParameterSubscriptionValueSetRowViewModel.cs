// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterSubscriptionValueSetRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel
{
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel;

    /// <summary>
    /// One display row of the Edit Parameter Subscription grid: for a scalar parameter one row per subscription value
    /// set, for a <see cref="CDP4Common.SiteDirectoryData.CompoundParameterType" /> one row per component (as in the
    /// parameter dialog). Wraps the reused <see cref="EditParameterValueSetRowViewModel" /> component row (which
    /// carries the editable Manual editor, the shared switch, and the read-only Actual/Computed columns) and adds the
    /// subscription-specific Owner and read-only Reference columns.
    /// </summary>
    public class EditParameterSubscriptionValueSetRowViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EditParameterSubscriptionValueSetRowViewModel" /> class.
        /// </summary>
        /// <param name="componentRow">The reused component/scalar row that hosts the editors and read-only values.</param>
        /// <param name="ownerShortName">The short name of the subscribing domain.</param>
        /// <param name="referenceValue">The read-only Reference value for this component (derived from the owner).</param>
        /// <param name="optionName">The applicable option name (from the subscription value set), or an empty string.</param>
        /// <param name="stateName">The applicable state name (from the subscription value set), or an empty string.</param>
        public EditParameterSubscriptionValueSetRowViewModel(EditParameterValueSetRowViewModel componentRow, string ownerShortName, string referenceValue, string optionName, string stateName)
        {
            this.ComponentRow = componentRow;
            this.OwnerShortName = ownerShortName;
            this.ReferenceValue = referenceValue;
            this.OptionName = optionName;
            this.StateName = stateName;
        }

        /// <summary>
        /// Gets the reused component/scalar row hosting the editable Manual editor + switch and the read-only
        /// Actual/Computed values (and, for compound, the component Name / Parameter Type / Scale).
        /// </summary>
        public EditParameterValueSetRowViewModel ComponentRow { get; }

        /// <summary>
        /// Gets the short name of the subscribing <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />.
        /// </summary>
        public string OwnerShortName { get; }

        /// <summary>
        /// Gets the read-only Reference value for this row (derived from the owner's value set).
        /// </summary>
        public string ReferenceValue { get; }

        /// <summary>
        /// Gets the component short name (empty for a scalar subscription).
        /// </summary>
        public string Name => this.ComponentRow.Name;

        /// <summary>
        /// Gets the applicable option name (from the subscription value set), or an empty string. Not taken from the
        /// component row because the throwaway proxy value set has no derived option/state.
        /// </summary>
        public string OptionName { get; }

        /// <summary>
        /// Gets the applicable state name (from the subscription value set), or an empty string.
        /// </summary>
        public string StateName { get; }

        /// <summary>
        /// Gets the read-only actual value.
        /// </summary>
        public string ActualValue => this.ComponentRow.ActualValue;

        /// <summary>
        /// Gets the read-only computed value (the owner's published value).
        /// </summary>
        public string ComputedValue => this.ComponentRow.ComputedValue;

        /// <summary>
        /// Gets the component parameter type name (for compound subscriptions).
        /// </summary>
        public string ParameterTypeName => this.ComponentRow.ParameterTypeName;

        /// <summary>
        /// Gets the component scale short name (for compound subscriptions).
        /// </summary>
        public string ScaleShortName => this.ComponentRow.ScaleShortName;
    }
}
