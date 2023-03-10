﻿// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ValueArrayExtensions.cs" company="RHEA System S.A."> 
//    Copyright (c) 2022 RHEA System S.A. 
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar 
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

namespace COMETwebapp.Extensions
{
    using CDP4Common.Types;

    /// <summary>
    /// Static class for the <see cref="ValueArray{T}" extensions/>
    /// </summary>
    public static class ValueArrayExtensions
    {
        /// <summary>
        /// Checks if two <see cref="ValueArray{T}"/> contains the same exact values 
        /// </summary>
        /// <typeparam name="T">the type of the parameter</typeparam>
        /// <param name="valueArray">the first value array to compare</param>
        /// <param name="comparison">the second value array to compare</param>
        /// <returns>True if the contained values are the same, false otherwise</returns>
        public static bool ContainsSameValues<T>(this ValueArray<T> valueArray, ValueArray<T> comparison) where T : class
        {
            if (comparison is null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            if (valueArray.Count != comparison.Count)
            {
                return false;
            }

            return !valueArray.Where((t, i) => !t.Equals(comparison[i])).Any();
        }
    }
}