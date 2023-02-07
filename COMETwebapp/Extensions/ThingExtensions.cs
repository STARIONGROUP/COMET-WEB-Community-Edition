// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ThingExtensions.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Extensions
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Extension class for <see cref="Thing" />
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
        /// Gets a collection of used <see cref="ParameterType" /> inside an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The collection of used <see cref="ParameterType" /></returns>
        public static IEnumerable<ParameterType> GetUsedParameterTypes(this Iteration iteration)
        {
            return iteration.Element.SelectMany(x => x.Parameter).Select(x => x.ParameterType).DistinctBy(x => x.Iid);
        }

        /// <summary>
        /// Get all <see cref="ParameterValueSetBase" /> of the given iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>A collection of <see cref="ParameterValueSetBase" /></returns>
        public static IEnumerable<ParameterValueSetBase> GetParameterValueSetBase(this Iteration iteration)
        {
            var valueSets = new List<ParameterValueSetBase>();

            if (iteration.TopElement != null)
            {
                valueSets.AddRange(iteration.TopElement.Parameter.SelectMany(x => x.ValueSet));
            }

            foreach (var elementUsage in iteration.Element.SelectMany(elementDefinition => elementDefinition.ContainedElement))
            {
                if (!elementUsage.ParameterOverride.Any())
                {
                    valueSets.AddRange(elementUsage.ElementDefinition.Parameter.SelectMany(x => x.ValueSet));
                }
                else
                {
                    valueSets.AddRange(elementUsage.ParameterOverride.SelectMany(x => x.ValueSet));

                    valueSets.AddRange(elementUsage.ElementDefinition.Parameter.Where(x => elementUsage.ParameterOverride.All(o => o.Parameter.Iid != x.Iid))
                        .SelectMany(x => x.ValueSet));
                }
            }

            return valueSets.DistinctBy(x => x.Iid);
        }

        /// <summary>
        /// Gets all <see cref="NestedParameter" /> that belongs to a given <see cref="Option" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to get the <see cref="NestedParameter" />s</param>
        /// <param name="option">The <see cref="Option" /></param>
        /// <returns>A collection of <see cref="NestedParameter" /></returns>
        public static IEnumerable<NestedParameter> GetNestedParameters(this Iteration iteration, Option option)
        {
            var generator = new NestedElementTreeGenerator();
            return iteration.TopElement == null ? Enumerable.Empty<NestedParameter>() : generator.GetNestedParameters(option);
        }

        /// <summary>
        /// Get all the unreferenced <see cref="ElementDefinition" /> in an <see cref="Iteration" />
        /// An unreferenced element is an element with no associated ElementUsage
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>All unreferenced <see cref="ElementDefinition" /></returns>
        public static IEnumerable<ElementDefinition> GetUnreferencedElements(this Iteration iteration)
        {
            var elementUsages = iteration.Element.SelectMany(x => x.ContainedElement).ToList();

            var associatedElementDefinitions = elementUsages.Select(x => x.ElementDefinition);

            var unreferencedElementDefinitions = iteration.Element.ToList();
            unreferencedElementDefinitions.RemoveAll(x => associatedElementDefinitions.Any(e => e.Iid == x.Iid));
            unreferencedElementDefinitions.RemoveAll(x => x.Iid == iteration.TopElement.Iid);

            return unreferencedElementDefinitions;
        }

        /// <summary>
        /// Get unused <see cref="ElementDefinition" /> in an <see cref="Iteration" />
        /// An unused element is an element not used in an option
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>All unused <see cref="ElementDefinition" /></returns>
        public static IEnumerable<ElementDefinition> GetUnusedElementDefinitions(this Iteration iteration)
        {
            var nestedElements = iteration.GetNestedElements().ToList();

            var associatedElements = nestedElements.SelectMany(x => x.ElementUsage.Select(e => e.ElementDefinition))
                .DistinctBy(x => x.Iid).ToList();

            var unusedElementDefinitions = iteration.Element.ToList();
            unusedElementDefinitions.RemoveAll(e => associatedElements.Any(x => x.Iid == e.Iid));
            unusedElementDefinitions.RemoveAll(x => x.Iid == iteration.TopElement?.Iid);
            return unusedElementDefinitions;
        }

        /// <summary>
        /// Get all <see cref="NestedElement" /> of the given <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> </param>
        /// <returns>A collection of <see cref="NestedElement" /></returns>
        public static IEnumerable<NestedElement> GetNestedElements(this Iteration iteration)
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
        /// Get all <see cref="NestedElement" /> of the given <see cref="Iteration" /> based on <see cref="Option"/>
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> </param>
        /// <param name="option">The <see cref="Option"/></param>
        /// <returns>A collection of <see cref="NestedElement" /></returns>
        public static IEnumerable<NestedElement> GetNestedElements(this Iteration iteration, Option option)
        {
            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = new List<NestedElement>();

            if (iteration.TopElement != null)
            {
                nestedElements.AddRange(nestedElementTreeGenerator.Generate(option));
            }

            return nestedElements;
        }
    }
}
