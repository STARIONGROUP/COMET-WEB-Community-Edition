// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementBaseExtensions.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Extension methods for <see cref="ElementBase"/>
    /// </summary>
    public static class ElementBaseExtensions
    {
        /// <summary>
        /// Queries the <see cref="ParameterOrOverrideBase"/> of the <paramref name="elementBase"/> and all the contained elements. Goes full deep in the hierarchy
        /// </summary>
        /// <param name="elementBase">the <see cref="ElementBase"/> where to start the search</param>
        /// <returns>a collection of <see cref="ParameterOrOverrideBase"/></returns>
        public static IEnumerable<ParameterOrOverrideBase> QueryParameterAndOverrideBasesDeep(this ElementBase elementBase)
        {
            var parameters = new List<ParameterOrOverrideBase>();
            QueryParameterAndOverrideBasesDeepHelper(elementBase, ref parameters);
            return parameters;
        }

        /// <summary>
        /// Helper method for the <see cref="QueryParameterAndOverrideBasesDeep(ElementBase)"/>
        /// </summary>
        /// <param name="thing">the current evaluated <see cref="Thing"/></param>
        /// <param name="parameters">the current found <see cref="ParameterOrOverrideBase"/></param>
        private static void QueryParameterAndOverrideBasesDeepHelper(Thing thing, ref List<ParameterOrOverrideBase> parameters)
        {
            if (thing is ElementDefinition elementDefinition)
            {
                foreach (var elementUsage in elementDefinition.ContainedElement)
                {
                    QueryParameterAndOverrideBasesDeepHelper(elementUsage, ref parameters);
                }

                foreach (var parameter in elementDefinition.Parameter)
                {
                    QueryParameterAndOverrideBasesDeepHelper(parameter, ref parameters);
                }
            }
            else if (thing is ElementUsage elementUsage)
            {
                QueryParameterAndOverrideBasesDeepHelper(elementUsage.ElementDefinition, ref parameters);
            }
            else if (thing is ParameterOrOverrideBase parameterOrOverride)
            {
                parameters.Add(parameterOrOverride);
            }
        }
    }
}
