// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationData.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Represents data relative to an Iteration
    /// </summary>
    public class IterationData
    {
	    /// <summary>
	    /// Initializes a new instance of the <see cref="IterationData" /> class.
	    /// </summary>
	    /// <param name="iterationSetup">The <see cref="IterationSetup"/></param>
	    /// <param name="displayModelName">Value asserting that the <see cref="IterationName"/> should contains the name of the <see cref="EngineeringModelSetup"/></param>
	    public IterationData(IterationSetup iterationSetup, bool displayModelName = false)
        {
            this.IterationSetupId = iterationSetup.Iid;
            this.IterationName = $"Iteration - {iterationSetup.IterationNumber} - ";
            this.IterationName += iterationSetup.FrozenOn == null ? "Active" : iterationSetup.FrozenOn;

            if (displayModelName)
            {
	            var model = (EngineeringModelSetup)iterationSetup.Container;
	            this.IterationName = $"{model.Name} - {this.IterationName}";
            }
        }

        /// <summary>
        /// The <see cref="Guid" /> of the <see cref="IterationSetup"/>
        /// </summary>
        public Guid IterationSetupId { get; private set; }

        /// <summary>
        /// The Iteration name
        /// </summary>
        public string IterationName { get; private set; }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is IterationData otherData)
            {
                return this.IterationSetupId == otherData.IterationSetupId && this.IterationName == otherData.IterationName;
            }

            return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return this.IterationSetupId.GetHashCode() + this.IterationName.GetHashCode();
        }
    }
}
