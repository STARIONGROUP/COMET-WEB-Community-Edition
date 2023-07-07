// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterOrOverrideBaseExtensions.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Extension methods for <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public static class ParameterOrOverrideBaseExtensions
    {
        /// <summary>
        /// Checks that all the parameters have a value different that the initial value "-"
        /// </summary>
        /// <param name="parameters">the parameters to check</param>
        /// <returns>true if all the parameters have a value, false otherwise</returns>
        public static bool AllParametersHaveValues(this IEnumerable<ParameterOrOverrideBase> parameters)
        {
            var stringValues = parameters.SelectMany(x => x.ValueSets).SelectMany(x => x.ActualValue).ToList();
            return stringValues.All(x => x != "-");
        }

        /// <summary>
        /// Checks that all the parameters have been modified before the <paramref name="dateTime"/>
        /// </summary>
        /// <param name="parameters">the parameters to check</param>
        /// <param name="dateTime">the date time to check</param>
        /// <returns>true if all the parameters have been modified before the date, false otherwise</returns>
        public static bool AllParametersFilledBefore(this IEnumerable<ParameterOrOverrideBase> parameters, DateTime dateTime)
        {
            return parameters.SelectMany(x => x.ValueSets).Cast<ParameterValueSetBase>().All(x => x.ModifiedOn < dateTime);
        }
    }
}
