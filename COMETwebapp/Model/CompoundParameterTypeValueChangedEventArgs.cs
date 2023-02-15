// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeValueChangedEventArgs.cs" company="RHEA System S.A.">
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
    /// <summary>
    /// Class for the arguments of the event raised when a value in a <see cref="CDP4Common.SiteDirectoryData.CompoundParameterType"/> has changed
    /// </summary>
    public class CompoundParameterTypeValueChangedEventArgs
    {
        /// <summary>
        /// Gets the index of the value changed in the value sets
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the new value of the value set for the corresponding <see cref="Index"/>
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new instance of type <see cref="CompoundParameterTypeValueChangedEventArgs"/>
        /// </summary>
        /// <param name="index">the index of the value changed in the value set</param>
        /// <param name="value">the new value</param>
        public CompoundParameterTypeValueChangedEventArgs(int index, string value)
        {
            this.Index = index;
            this.Value = value;
        }
    }
}
