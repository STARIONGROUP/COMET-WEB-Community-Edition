// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterExtensions.cs" company="RHEA System S.A.">
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
namespace COMETwebapp
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Class containing extension methods for the <see cref="Parameter"/> or <see cref="ParameterOverride"/>
    /// </summary>
    public static class ParameterExtensions
    {
        /// <summary>
        /// Extract the specified number of values from the parameter
        /// </summary>
        /// <param name="parameter">the <see cref="Parameter"/> to extract the data from</param>
        /// <param name="numberOfValues">number of values to extract</param>
        /// <returns>the extracted values</returns>
        /// <exception cref="ArgumentException">if the parameter is null</exception>
        public static string[] ExtractActualValues(this Parameter parameter, int numberOfValues)
        {
            string[] values = new string[numberOfValues];

            if (parameter is not null)
            {
                var valueSet = parameter.ValueSet;
                var validValues = valueSet.FindAll(x => x is not null).ToList();

                if (validValues.Count > 0)
                {
                    var validValue = validValues.First();

                    for (int i = 0; i < numberOfValues; i++)
                    {
                        values[i] = validValue.ActualValue[i];
                    }
                }
            }
            else
            {
                throw new ArgumentException("The parameter can't be null");
            }

            return values;
        }
    }
}
