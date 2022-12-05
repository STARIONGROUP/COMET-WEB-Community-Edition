// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IValueSetExtensions.cs" company="RHEA System S.A.">
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
    using System.Globalization;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;

    /// <summary>
    /// Static extension methods for <see cref="IValueSet"/>
    /// </summary>
    public static class IValueSetExtensions
    {
        /// <summary>
        /// Parses an <see cref="IValueSet"/> to translations along main axes
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>An array of type [X,Y,Z]</returns>
        public static double[] ParseIValueToPosition(this IValueSet valueSet)
        {
            if (valueSet.ActualValue.Count != 3)
            {
                throw new ArgumentException("The value set must contain 3 values");
            }
            else if (valueSet.ToDoubles(out var result))
            {
                return result.ToArray();
            }
            else
            {
                return new double[] { 0, 0, 0 };
            }
        }

        /// <summary>
        /// Parses the values of the <see cref="IValueSet"/> to <see cref="Orientation"/>
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>the orientation</returns>
        public static Orientation ParseIValueToOrientation(this IValueSet valueSet, AngleFormat angleFormat)
        {
            if (valueSet.ToDoubles(out var result))
            {
                return result.ToArray().ToOrientation(valueSet.ActualValue.Count == 9, angleFormat);
            }

            return new Orientation(0, 0, 0);
        }

        /// <summary>
        /// Parses the <see cref="IValueSet"/> values to doubles
        /// </summary>
        /// <param name="valueSet">the set to parse</param>
        /// <param name="result">the result of the parse</param>
        /// <returns>true if the parse succeed, false otherwise</returns>
        public static bool ToDoubles(this IValueSet valueSet, out IEnumerable<double> result)
        {
            var values = new List<double>();

            foreach(var value in valueSet.ActualValue)
            {
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var validValue))
                {
                    values.Add(validValue);
                }
                else
                {
                    result = new double[] {};
                    return false;
                }
            }

            result = values;
            return true;
        }
    }
}
