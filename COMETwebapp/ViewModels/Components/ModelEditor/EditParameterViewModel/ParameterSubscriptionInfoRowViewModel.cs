// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSubscriptionInfoRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Read-only projection of a <see cref="ParameterSubscription" /> value set for display in the Subscriptions
    /// tab of the Edit Parameter dialog: the subscribing domain, the applicable option/state, the switch, and the
    /// current value. Editing a subscription's value is not offered here — the shared value editors only write
    /// <see cref="ParameterValueSetBase" />, not <see cref="ParameterSubscriptionValueSet" />.
    /// </summary>
    public class ParameterSubscriptionInfoRowViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ParameterSubscriptionInfoRowViewModel" /> class.
        /// </summary>
        /// <param name="ownerShortName">The short name of the subscribing <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />.</param>
        /// <param name="optionName">The name of the applicable <see cref="Option" />, or an empty string.</param>
        /// <param name="stateName">The name of the applicable <see cref="ActualFiniteState" />, or an empty string.</param>
        /// <param name="switchValue">The <see cref="ParameterSwitchKind" /> of the subscription value set.</param>
        /// <param name="value">The formatted current value of the subscription.</param>
        public ParameterSubscriptionInfoRowViewModel(string ownerShortName, string optionName, string stateName, string switchValue, string value)
        {
            this.OwnerShortName = ownerShortName;
            this.OptionName = optionName;
            this.StateName = stateName;
            this.SwitchValue = switchValue;
            this.Value = value;
        }

        /// <summary>
        /// Gets the short name of the subscribing <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />.
        /// </summary>
        public string OwnerShortName { get; }

        /// <summary>
        /// Gets the name of the applicable <see cref="Option" />, or an empty string.
        /// </summary>
        public string OptionName { get; }

        /// <summary>
        /// Gets the name of the applicable <see cref="ActualFiniteState" />, or an empty string.
        /// </summary>
        public string StateName { get; }

        /// <summary>
        /// Gets the <see cref="ParameterSwitchKind" /> of the subscription value set, as a display string.
        /// </summary>
        public string SwitchValue { get; }

        /// <summary>
        /// Gets the formatted current value of the subscription.
        /// </summary>
        public string Value { get; }
    }
}
