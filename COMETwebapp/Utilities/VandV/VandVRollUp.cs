// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRollUp.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Utilities.VandV
{
    /// <summary>
    /// The roll-up of a set of V&amp;V items, typically the items covering one requirement.
    /// </summary>
    /// <param name="Passed">The number of items that passed.</param>
    /// <param name="Failed">The number of items that failed.</param>
    /// <param name="Open">The number of items still open.</param>
    public readonly record struct VandVRollUp(int Passed, int Failed, int Open)
    {
        /// <summary>
        /// Gets the total number of items.
        /// </summary>
        public int Total => this.Passed + this.Failed + this.Open;

        /// <summary>
        /// Gets a value indicating whether the requirement counts as verified: it has items and every one of them passed,
        /// so a single open item keeps it out.
        /// </summary>
        public bool IsVerified => this.Total > 0 && this.Passed == this.Total;
    }
}
