
// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ClassKindWrapper.cs" company="RHEA System S.A.">
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
namespace COMETwebapp.Wrappers
{
    using CDP4Common.CommonData;
    
    using COMETwebapp.Extensions;

    /// <summary>
    ///     Wrapper class for the <see cref="ClassKind" /> enumerator to be used inside DevExpress componenet
    /// </summary>
    public class ClassKindWrapper
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClassKindWrapper" /> class.
        /// </summary>
        /// <param name="classKind">The <see cref="ClassKind" /></param>
        public ClassKindWrapper(ClassKind classKind)
        {
            this.ClassKind = classKind;
            this.ClassKindName = classKind.GetEnumDisplayName();
        }

        /// <summary>
        ///     The <see cref="ClassKind" />
        /// </summary>
        public ClassKind ClassKind { get; }

        /// <summary>
        ///     The associated name
        /// </summary>
        public string ClassKindName { get; }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///     <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ClassKindWrapper classKindWrapper)
            {
                return classKindWrapper.ClassKind == this.ClassKind;
            }

            return false;
        }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.ClassKind);
        }
    }
}
