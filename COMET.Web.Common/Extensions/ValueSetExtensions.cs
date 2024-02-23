// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueSetExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using System.Globalization;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;

    /// <summary>
    /// Static extension methods for <see cref="IValueSet" />
    /// </summary>
    public static class ValueSetExtensions
    {
        /// <summary>
        /// Parses an <see cref="IValueSet" /> to translations along main axes
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>An array of type [X,Y,Z]</returns>
        public static double[] ParseIValueToPosition(this IValueSet valueSet)
        {
            if (valueSet.ActualValue.Count != 3)
            {
                throw new ArgumentException("The value set must contain 3 values");
            }

            return valueSet.ToDoubles(out var result) ? result.ToArray() : new double[] { 0, 0, 0 };
        }

        /// <summary>
        /// Parses an <see cref="IValueSet" /> to rotation matrix
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <param name="angleFormat">The format of the angle</param>
        /// <returns>And array of type [Rx,Ry,Rz]</returns>
        public static Orientation ParseIValueToOrientation(this IValueSet valueSet, AngleFormat angleFormat)
        {
            if (valueSet.ToDoubles(out var result))
            {
                return result.ToArray().ToOrientation(valueSet.ActualValue.Count == 9, angleFormat);
            }

            return new Orientation(0, 0, 0);
        }

        /// <summary>
        /// Parses the value set to doubles
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <param name="result">the result of the parse</param>
        /// <returns>true if the parse succeed, false otherwise</returns>
        public static bool ToDoubles(this IValueSet valueSet, out IEnumerable<double> result)
        {
            var values = new List<double>();

            foreach (var value in valueSet.ActualValue)
            {
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var validValue))
                {
                    values.Add(validValue);
                }
                else
                {
                    result = Enumerable.Empty<double>();
                    return false;
                }
            }

            result = values;
            return true;
        }
    }
}
