// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceExtensions.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Extensions
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using FluentResults;

    /// <summary>
    /// Static class with extension methods for the <see cref="ISessionService"/>
    /// </summary>
    public static class SessionServiceExtensions
    {
        /// <summary>
        /// Marks a top element for a given iteration
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService"/> in which the iteration will be updated</param>
        /// <param name="iteration">The <see cref="Iteration"/> to be updated</param>
        /// <param name="element">The <see cref="ElementDefinition"/> to be marked as top element</param>
        /// <returns>A <see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Throws an <see cref="ArgumentException"/></exception>
        public static async Task<Result> MarkTopElement(this ISessionService sessionService, Iteration iteration, ElementDefinition element)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var iterationClone = iteration.Clone(false);
            iterationClone.TopElement = element;

            return await sessionService.UpdateThing(iteration.Container, iterationClone);
        }

        /// <summary>
        /// Adds a new parameter for a given element definition
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService"/> in which data will be updated</param>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> in which a new parameter will be added</param>
        /// <param name="group">The <see cref="ParameterGroup"/> of the new parameter</param>
        /// <param name="parameterType">The <see cref="ParameterType"/> of the new parameter</param>
        /// <param name="measurementScale">The <see cref="MeasurementScale"/> of the new parameter</param>
        /// <param name="owner">The <see cref="DomainOfExpertise"/> of the owner</param>
        /// <returns>A <see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Throws an <see cref="ArgumentNullException"/></exception>
        public static async Task<Result> AddParameter(this ISessionService sessionService, ElementDefinition elementDefinition, ParameterGroup group, ParameterType parameterType, MeasurementScale measurementScale, DomainOfExpertise owner)
        {
            if (elementDefinition == null)
            {
                throw new ArgumentNullException(nameof(elementDefinition), "The container ElementDefinition may not be null");
            }

            if (parameterType == null)
            {
                throw new ArgumentNullException(nameof(parameterType), "The ParameterType may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owner DomainOfExpertise may not be null");
            }

            if (sessionService.Session == null)
            {
                throw new ArgumentNullException(nameof(sessionService.Session), "The session may not be null");
            }

            var parameter = new Parameter(Guid.NewGuid(), null, null)
            {
                Owner = owner,
                ParameterType = parameterType,
                Scale = measurementScale,
                Group = group,
                ValueSet = { new ParameterValueSet() }
            };

            var clone = elementDefinition.Clone(false);
            clone.Parameter.Add(parameter);

            return await sessionService.CreateThing(clone, parameter);
        }
    }
}
