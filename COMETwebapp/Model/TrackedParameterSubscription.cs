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
           
            foreach (var parameterSubscriptionValueSet in subscription.ValueSet)
            {
                this.ParameterValueSetsCurrentRevision[parameterSubscriptionValueSet.Iid] = parameterSubscriptionValueSet.RevisionNumber;
            }
        }

        /// <summary>
        /// The <see cref="Guid" /> of the <see cref="ParameterSubscription" />
        /// </summary>
        public Guid ParameterSubscriptionId { get; set; }

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> of revision number of the <see cref="ParameterSubscriptionValueSet" />
        /// of the <see cref="ParameterSubscription" />
        /// </summary>
        public Dictionary<Guid,int> ParameterValueSetsCurrentRevision { get; } = new ();

        /// <summary>
        /// Computes the number of updates with an other <see cref="TrackedParameterSubscription"/> related to the same <see cref="ParameterSubscription"/>
        /// </summary>
        /// <param name="other">An other <see cref="TrackedParameterSubscription"/></param>
        /// <returns>The number of updates</returns>
        public int ComputeNumberOfUpdates(TrackedParameterSubscription other)
        {
            if (other.ParameterSubscriptionId != this.ParameterSubscriptionId)
            {
                throw new ArgumentException("The provided TrackedParameterSubscription is not related to the same ParameterSubscription");
            }

            var updateCount = 0;

            foreach (var valueSet in this.ParameterValueSetsCurrentRevision)
            {
                if (!other.ParameterValueSetsCurrentRevision.ContainsKey(valueSet.Key))
                {
                    updateCount++;
                }

                if (other.ParameterValueSetsCurrentRevision[valueSet.Key] != valueSet.Value)
                {
                    updateCount++;
                }
            }

            return updateCount;
        }
    }
}
