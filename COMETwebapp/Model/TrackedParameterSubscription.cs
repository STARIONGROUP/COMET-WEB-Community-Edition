// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TrackedParameterSubscription.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Model
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Extensions;

    /// <summary>
    /// Handles all required information to track any updates on a <see cref="ParameterSubscription" />
    /// </summary>
    public class TrackedParameterSubscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedParameterSubscription" /> class.
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> to track</param>
        public TrackedParameterSubscription(ParameterSubscription subscription)
        {
            this.ParameterSubscriptionId = subscription.Iid;

            this.CountChanges = subscription.QueryParameterSubscriptionValueSetEvolution()
                .ToDictionary(k => k.Key, v => v.Value.Count);
        }

        /// <summary>
        /// Count changes related to the <see cref="ParameterSubscription" />
        /// </summary>
        public Dictionary<Guid, int> CountChanges { get; set; }

        /// <summary>
        /// The <see cref="Guid" /> of the <see cref="ParameterSubscription" />
        /// </summary>
        public Guid ParameterSubscriptionId { get; set; }

        /// <summary>
        /// Query all <see cref="Guid"/> of <see cref="ParameterSubscriptionValueSet"/> that had change with an other
        /// <see cref="TrackedParameterSubscription" /> related to the same <see cref="ParameterSubscription" />
        /// </summary>
        /// <param name="other">An other <see cref="TrackedParameterSubscription" /></param>
        /// <returns>The collection of <see cref="ParameterSubscriptionValueSet"/> id</returns>
        public IEnumerable<Guid> QueryChangedValueSet(TrackedParameterSubscription other)
        {
            if (other.ParameterSubscriptionId != this.ParameterSubscriptionId)
            {
                throw new ArgumentException("The provided TrackedParameterSubscription is not related to the same ParameterSubscription");
            }

            var changedValueSet = new List<Guid>();

            foreach (var change in other.CountChanges)
            {
                if (this.CountChanges.TryGetValue(change.Key, out var existingChange))
                {
                    if (change.Value - existingChange != 0)
                    {
                        changedValueSet.Add(change.Key);
                    }
                }
                else
                {
                    changedValueSet.Add(change.Key);
                }
            }

            return changedValueSet;
        }
    }
}
