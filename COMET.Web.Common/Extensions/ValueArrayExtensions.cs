// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueArrayExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Extensions
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
