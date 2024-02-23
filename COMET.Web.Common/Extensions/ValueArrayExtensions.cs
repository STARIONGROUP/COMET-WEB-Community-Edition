// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueArrayExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
	/// Static class for the <see cref="ValueArray{T}" /> extensions
	/// </summary>
	public static class ValueArrayExtensions
    {
        /// <summary>
        /// Adds new default values inside a <see cref="ValueArray{T}"/>
        /// </summary>
        /// <param name="valueArray">The current <see cref="ValueArray{T}"/></param>
        /// <param name="numberOfValues">The number of values to add</param>
        /// <returns>The newly updated <see cref="ValueArray{T}"/></returns>
        public static ValueArray<string> AddNewValues(this ValueArray<string> valueArray, int numberOfValues)
        {
            var currentValues = valueArray.ToList();

            for (var componentIndex = 0; componentIndex < numberOfValues; componentIndex++)
            {
                currentValues.Add("-");
            }

            return new ValueArray<string>(currentValues);
		}

        /// <summary>
        /// Removes new values inside a <see cref="ValueArray{T}"/>
        /// </summary>
        /// <param name="valueArray">The current <see cref="ValueArray{T}"/></param>
        /// <param name="numberOfValues">The number of values to remove</param>
        /// <returns>The newly updated <see cref="ValueArray{T}"/></returns>
        public static ValueArray<string> RemovesValues(this ValueArray<string> valueArray, int numberOfValues)
        {
            var currentValues = valueArray.ToList();

            if ( currentValues.Count < numberOfValues )
            {
                throw new ArgumentOutOfRangeException($"The requested number of values to delete ({numberOfValues}) can not be greater than the lenght of the ValueArray");
            }

            currentValues.RemoveRange(currentValues.Count -1 -numberOfValues, numberOfValues);
            return new ValueArray<string>(currentValues);
        }
    }
}
