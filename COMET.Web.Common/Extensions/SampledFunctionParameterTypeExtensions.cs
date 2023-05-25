// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SampledFunctionParameterTypeExtensions.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// Extension class for <see cref="SampledFunctionParameterType"/>
    /// </summary>
    public static class SampledFunctionParameterTypeExtensions
    {
        /// <summary>
        /// Query the representation of the <see cref="ValueArray{T}"/> for an <see cref="SampledFunctionParameterType"/>
        /// </summary>
        /// <param name="sfpt">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="currentValues">The associated <see cref="ValueArray{T}"/></param>
        /// <returns>The string representation</returns>
        public static string QueryValuesRepresentation(this SampledFunctionParameterType sfpt, ValueArray<string> currentValues)
        {
            return $"[{sfpt.QueryRowsCount(currentValues)}x{sfpt.NumberOfValues}]";
        }
        
        /// <summary>
        /// Queries the number of row that the <see cref="ValueArray{T}"/> of the <see cref="SampledFunctionParameterType"/> contains
        /// </summary>
        /// <param name="sfpt">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="currentValues">The current <see cref="ValueArray{T}"/></param>
        /// <returns>The number of rows</returns>
        public static int QueryRowsCount(this SampledFunctionParameterType sfpt, ValueArray<string> currentValues)
        {
            return currentValues.Count / sfpt.NumberOfValues;
        }

        /// <summary>
        /// Queries the <see cref="IParameterTypeAssignment"/> that correspond to a given index inside a <see cref="SampledFunctionParameterType"/>
        /// </summary>
        /// <param name="sfpt">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="index">The index to get the <see cref="IParameterTypeAssignment"/></param>
        /// <returns>The retrieved <see cref="IParameterTypeAssignment"/></returns>
        public static IParameterTypeAssignment QueryParameterTypeAtIndex(this SampledFunctionParameterType sfpt, int index)
        {
            var indexInsideRange = index%sfpt.NumberOfValues;

            if (indexInsideRange < sfpt.IndependentParameterType.Count)
            {
                return sfpt.IndependentParameterType[indexInsideRange];
            }
            else
            {
                return sfpt.DependentParameterType[indexInsideRange - sfpt.IndependentParameterType.Count];
            }
        }
    }
}
