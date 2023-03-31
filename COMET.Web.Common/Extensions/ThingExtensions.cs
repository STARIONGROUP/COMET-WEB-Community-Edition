// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ThingExtensions.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Extension class for <see cref="Thing"/>
    /// </summary>
    public static class ThingExtensions
    {
		/// <summary>
        /// Gets the name of the <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The name of the <see cref="Iteration" /></returns>
        public static string GetName(this Iteration iteration)
        {
            var engineeringSetup = (EngineeringModelSetup)iteration.IterationSetup.Container;
            return $"{engineeringSetup.Name} - Iteration {iteration.IterationSetup.IterationNumber}";
        }

        /// <summary>
        /// Queries all <see cref="ElementDefinition" /> that are used inside an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>A collection of <see cref="ElementDefinition" /></returns>
        public static IEnumerable<ElementDefinition> QueryUsedElementDefinitions(this Iteration iteration)
        {
            if (iteration is null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var elementBase = iteration.QueryNestedElements();
            var elementDefinitions = new List<ElementDefinition>();

            foreach (var nestedElement in elementBase)
            {
                switch (nestedElement.GetElementBase())
                {
                    case ElementDefinition elementDefinition:
                        elementDefinitions.Add(elementDefinition);
                        break;
                    case ElementUsage elementUsage:
                        elementDefinitions.Add(elementUsage.ElementDefinition);
                        break;
                }
            }

            return elementDefinitions.DistinctBy(x => x.Iid);
        }

        /// <summary>
        /// Queries all <see cref="NestedElement" /> of the given <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> </param>
        /// <returns>A collection of <see cref="NestedElement" /></returns>
        public static IEnumerable<NestedElement> QueryNestedElements(this Iteration iteration)
        {
            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = new List<NestedElement>();

            if (iteration.TopElement != null)
            {
                nestedElements.AddRange(iteration.Option.SelectMany(o => nestedElementTreeGenerator.Generate(o)));
            }

            return nestedElements;
        }

        /// <summary>
        /// Queries all <see cref="NestedElement" /> of the given <see cref="Iteration" /> based on <see cref="Option" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> </param>
        /// <param name="option">The <see cref="Option" /></param>
        /// <returns>A collection of <see cref="NestedElement" /></returns>
        public static IEnumerable<NestedElement> QueryNestedElements(this Iteration iteration, Option option)
        {
            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = new List<NestedElement>();

            if (iteration.TopElement != null)
            {
                nestedElements.AddRange(nestedElementTreeGenerator.Generate(option));
            }

            return nestedElements;
        }

        /// <summary>
        /// Queries used <see cref="ParameterType" /> inside an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The collection of used <see cref="ParameterType" /></returns>
        public static IEnumerable<ParameterType> QueryUsedParameterTypes(this Iteration iteration)
        {
            return iteration.Element.SelectMany(x => x.Parameter).Select(x => x.ParameterType).DistinctBy(x => x.Iid);
        }
    }
}
