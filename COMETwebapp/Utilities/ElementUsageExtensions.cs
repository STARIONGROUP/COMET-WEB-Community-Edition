// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Utilities
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// static extension methods for <see cref="ElementUsage"/>
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

            if(elementBase is ElementDefinition elementDefinition)
            {
                parameters.AddRange(elementDefinition.GetParametersInUse());
            }
            else if(elementBase is ElementUsage elementUsage)
            {
                parameters.AddRange(elementUsage.GetParametersInUse());
            }

            return parameters.OrderBy(x => x.ParameterType.ShortName).ToList();
        }

        /// <summary>
        /// Gets the <see cref="ParameterBase"/> that an <see cref="ElementDefinition"/> uses
        /// </summary>
        /// <param name="elementDefinition">the element definition</param>
        /// <returns>a <see cref="IEnumerable{T}"/> with the <see cref="ParameterBase"/></returns>
        public static IEnumerable<ParameterBase> GetParametersInUse(this ElementDefinition elementDefinition)
        {
            var parameters = new List<ParameterBase>();

            elementDefinition.Parameter.ForEach(x =>
            {
                if (!parameters.Any(par => par.ParameterType.ShortName == x.ParameterType.ShortName))
                {
                    parameters.Add(x);
                }
            });

            return parameters.OrderBy(x => x.ParameterType.ShortName).ToList();
        }

        /// <summary>
        /// Gets the <see cref="ParameterBase"/> that an <see cref="ElementUsage"/> uses
        /// </summary>
        /// <param name="elementUsage">the element usage</param>
        /// <returns>a <see cref="IEnumerable{T}"/> with the <see cref="ParameterBase"/></returns>
        public static IEnumerable<ParameterBase> GetParametersInUse(this ElementUsage elementUsage)
        {
            var parameters = new List<ParameterBase>();

            parameters.AddRange(elementUsage.ParameterOverride);

            elementUsage.ElementDefinition.Parameter.ForEach(x =>
            {
                if (!parameters.Any(par => par.ParameterType.ShortName == x.ParameterType.ShortName))
                {
                    parameters.Add(x);
                }
            });

            return parameters.OrderBy(x => x.ParameterType.ShortName).ToList();            
        }
    }
}
