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
        /// Queries used <see cref="ParameterType" /> inside an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The collection of used <see cref="ParameterType" /></returns>
        public static IEnumerable<ParameterType> QueryUsedParameterTypes(this Iteration iteration)
        {
            return iteration.Element.SelectMany(x => x.Parameter).Select(x => x.ParameterType).DistinctBy(x => x.Iid);
        }

        /// <summary>
        /// Queries all <see cref="ParameterValueSetBase" /> of the given iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>A collection of <see cref="ParameterValueSetBase" /></returns>
        public static IEnumerable<ParameterValueSetBase> QueryParameterValueSetBase(this Iteration iteration)
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
        /// Queries all <see cref="NestedParameter" /> that belongs to a given <see cref="Option" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to get the <see cref="NestedParameter" />s</param>
        /// <param name="option">The <see cref="Option" /></param>
        /// <returns>A collection of <see cref="NestedParameter" /></returns>
        public static IEnumerable<NestedParameter> QueryNestedParameters(this Iteration iteration, Option option)
        {
            var generator = new NestedElementTreeGenerator();
            return iteration.TopElement == null ? Enumerable.Empty<NestedParameter>() : generator.GetNestedParameters(option);
        }

        /// <summary>
        /// Queries all the unreferenced <see cref="ElementDefinition" /> in an <see cref="Iteration" />
        /// An unreferenced element is an element with no associated ElementUsage
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>All unreferenced <see cref="ElementDefinition" /></returns>
        public static IEnumerable<ElementDefinition> QueryUnreferencedElements(this Iteration iteration)
        {
            var elementUsages = iteration.Element.SelectMany(x => x.ContainedElement).ToList();

            var associatedElementDefinitions = elementUsages.Select(x => x.ElementDefinition);

            var unreferencedElementDefinitions = iteration.Element.ToList();
            unreferencedElementDefinitions.RemoveAll(x => associatedElementDefinitions.Any(e => e.Iid == x.Iid));
            unreferencedElementDefinitions.RemoveAll(x => x.Iid == iteration.TopElement.Iid);

            return unreferencedElementDefinitions;
        }

        /// <summary>
        /// Queries unused <see cref="ElementDefinition" /> in an <see cref="Iteration" />
        /// An unused element is an element not used in an option
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>All unused <see cref="ElementDefinition" /></returns>
        public static IEnumerable<ElementDefinition> QueryUnusedElementDefinitions(this Iteration iteration)
        {
            var nestedElements = iteration.QueryNestedElements().ToList();

            var associatedElements = nestedElements.SelectMany(x => x.ElementUsage.Select(e => e.ElementDefinition))
                .DistinctBy(x => x.Iid).ToList();

            var unusedElementDefinitions = iteration.Element.ToList();
            unusedElementDefinitions.RemoveAll(e => associatedElements.Any(x => x.Iid == e.Iid));
            unusedElementDefinitions.RemoveAll(x => x.Iid == iteration.TopElement?.Iid);
            return unusedElementDefinitions;
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
        /// Queries all <see cref="ParameterSubscription" /> contained into an <see cref="Iteration" /> for a given
        /// <see cref="DomainOfExpertise" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        /// <returns>A collection of <see cref="ParameterSubscription" /></returns>
        public static IEnumerable<ParameterSubscription> QueryParameterSubscriptions(this Iteration iteration, DomainOfExpertise domain)
        {
            var subscriptions = new List<ParameterSubscription>();

            if (iteration.TopElement != null)
            {
                subscriptions.AddRange(iteration.TopElement.QueryParameterSubscriptions(domain));
            }

            subscriptions.AddRange(iteration.Element.SelectMany(x => x.ContainedElement).SelectMany(x => x.QueryParameterSubscriptions(domain)));
            return subscriptions.DistinctBy(x => x.Iid).OrderBy(p => p.ParameterType.Name);
        }

        /// <summary>
        /// Queries all <see cref="ParameterSubscription" /> contained into an <see cref="ElementBase" /> for a given
        /// <see cref="DomainOfExpertise" />
        /// </summary>
        /// <param name="element">The <see cref="ElementBase" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        /// <returns>A collection of <see cref="ParameterSubscription" /></returns>
        public static IEnumerable<ParameterSubscription> QueryParameterSubscriptions(this ElementBase element, DomainOfExpertise domain)
        {
            var subscriptions = new List<ParameterSubscription>();

            switch (element)
            {
                case ElementDefinition elementDefinition:
                    subscriptions.AddRange(elementDefinition.Parameter.QueryParameterSubscriptions(domain));
                    break;
                case ElementUsage elementUsage when !elementUsage.ParameterOverride.Any():
                    return elementUsage.ElementDefinition.QueryParameterSubscriptions(domain);
                case ElementUsage elementUsage:
                    var notOverridenParameters = elementUsage.ElementDefinition.Parameter.Where(x => elementUsage.ParameterOverride.All(p => p.Parameter.Iid != x.Iid));

                    subscriptions.AddRange(elementUsage.ParameterOverride.QueryParameterSubscriptions(domain));

                    subscriptions.AddRange(notOverridenParameters.QueryParameterSubscriptions(domain));
                    break;
            }

            return subscriptions;
        }

        /// <summary>
        /// Queries all <see cref="ParameterSubscription" /> contained into a collection of <see cref="ParameterOrOverrideBase" />
        /// for a given <see cref="DomainOfExpertise" />
        /// </summary>
        /// <param name="parameterOrOverrideBases">The collection of <see cref="ParameterOrOverrideBase" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        /// <returns>A collection of <see cref="ParameterSubscription" /></returns>
        public static IEnumerable<ParameterSubscription> QueryParameterSubscriptions(this IEnumerable<ParameterOrOverrideBase> parameterOrOverrideBases,
            DomainOfExpertise domain)
        {
            return parameterOrOverrideBases.Where(x => x.Owner.Iid != domain.Iid)
                .SelectMany(x => x.ParameterSubscription.Where(p => p.Owner.Iid == domain.Iid));
        }
    }
}
