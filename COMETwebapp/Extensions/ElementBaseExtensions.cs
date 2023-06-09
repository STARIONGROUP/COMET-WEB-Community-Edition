// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// static extension methods for <see cref="ElementBase"/>
    /// </summary>
    public static class ElementBaseExtensions
    {
        /// <summary> 
        /// Gets the <see cref="ParameterBase"/> that an <see cref="ElementBase"/> uses 
        /// </summary> 
        /// <param name="elementBase">the element base</param> 
        /// <returns>a <see cref="IEnumerable{T}"/> with the <see cref="ParameterBase"/></returns> 
        public static IEnumerable<ParameterBase> GetParametersInUse(this ElementBase elementBase)
        {
            var parameters = new List<ParameterBase>();

            if (elementBase is ElementDefinition elementDefinition)
            {
                parameters.AddRange(elementDefinition.Parameter.Distinct());
            }
            else if (elementBase is ElementUsage elementUsage)
            {
                parameters.AddRange(elementUsage.ParameterOverride);

                foreach (var parameter in elementUsage.ElementDefinition.Parameter)
                {
                    if (parameters.All(p => p.ParameterType.Iid != parameter.ParameterType.Iid))
                    {
                        parameters.Add(parameter);
                    }
                }
            }

            return parameters.OrderBy(x => x.ParameterType.ShortName).ToList();
        }
        
        /// <summary>
        /// Filters the <param name="elements"/> by the <param name="state"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <param name="state">the state used to filter</param>
        /// <returns>the filtered elements</returns>
        public static IEnumerable<ElementBase> FilterByState(this IEnumerable<ElementBase> elements, ActualFiniteState state)
        {
            var filteredElements = new List<ElementBase>();

            foreach (var element in elements)
            {
                if (element is ElementDefinition elementDefinition)
                {
                    elementDefinition.Parameter.ForEach(p =>
                    {
                        p.ValueSet.ForEach(v =>
                        {
                            if (v.ActualState != null && v.ActualState == state)
                            {
                                filteredElements.Add(element);
                            }
                        });
                    });
                }
                else if (element is ElementUsage elementUsage)
                {
                    if (elementUsage.ParameterOverride.Any())
                    {
                        elementUsage.ParameterOverride.ForEach(p =>
                        {
                            p.ValueSet.ForEach(v =>
                            {
                                if (v.ActualState != null && v.ActualState == state)
                                {
                                    filteredElements.Add(element);
                                }
                            });
                        });
                    }

                    elementUsage.ElementDefinition.Parameter.ForEach(p =>
                    {
                        p.ValueSet.ForEach(v =>
                        {
                            if (v.ActualState != null && v.ActualState == state)
                            {
                                filteredElements.Add(element);
                            }
                        });
                    });
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Queries the element usages children for an <see cref="ElementBase"/>
        /// </summary>
        /// <param name="elementBase">the <see cref="ElementBase"/></param>
        /// <returns>a collection of <see cref="ElementUsage"/></returns>
        public static IEnumerable<ElementUsage> QueryElementUsageChildrenFromElementBase(this ElementBase elementBase)
        {
            return elementBase switch
            {
                ElementDefinition elementDefinition => elementDefinition.ContainedElement,
                ElementUsage elementUsage => elementUsage.ElementDefinition?.ContainedElement,
                _ => Enumerable.Empty<ElementUsage>()
            };
        }
    }
}
