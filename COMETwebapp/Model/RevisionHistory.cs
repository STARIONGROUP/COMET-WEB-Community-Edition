// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevisionHistory.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Model
{
    using CDP4Common.Types;

    /// <summary>
    /// Represents value for ParameterSubcriptionValueSet history
    /// </summary>
    public class RevisionHistory
    {
        /// <summary>
        /// Initializes a new <see cref="RevisionHistory" />
        /// </summary>
        /// <param name="revisionNumber">The revision number</param>
        /// <param name="valueArray">The associtated <see cref="ValueArray{T}" /></param>
        public RevisionHistory(int revisionNumber, ValueArray<string> valueArray)
        {
            this.RevisionNumber = revisionNumber.ToString();

            if (double.TryParse(valueArray[0], out var value))
            {
                this.ActualValue = value;
            }
            else
            {
                this.ActualValue = valueArray[0];
            }
        }

        /// <summary>
        /// Initializes a new <see cref="RevisionHistory" />
        /// </summary>
        public RevisionHistory()
        {
        }

        /// <summary>
        /// Revision Number in string of the ParameterSubcriptionValueSet
        /// </summary>
        public string RevisionNumber { get; set; }

        /// <summary>
        /// Actual value of the ParameterSubcriptionValueSet
        /// </summary>
        public object ActualValue { get; set; }
    }
}
