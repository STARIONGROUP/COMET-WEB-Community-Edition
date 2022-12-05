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
    using CDP4Common.EngineeringModelData;

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
        public static double[] ParseIValueToPosition(this IValueSet? valueSet)
        {
            double x = 0, y = 0, z = 0;

            if (valueSet is not null)
            {
                if (valueSet.ActualValue.Count < 3)
                {
                    throw new ArgumentException("The value set must contain 3 values in order to compute the position");
                }

                _ = double.TryParse(valueSet.ActualValue[0], out x);
                _ = double.TryParse(valueSet.ActualValue[1], out y);
                _ = double.TryParse(valueSet.ActualValue[2], out z);
            }

            return new double[] { x, y, z };
        }

        /// <summary>
        /// Parses an <see cref="IValueSet"/> to rotation matrix
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>And array of type [Rx,Ry,Rz]</returns>
        public static double[] ParseIValueToRotationMatrix(this IValueSet? valueSet)
        {
            double[] rotMatrix = new double[9];

            if (valueSet is not null)
            {
                if(valueSet.ActualValue.Count < 9)
                {
                    throw new ArgumentException("The value set must contain 9 values in order to compute the rotation matrix");
                }

                rotMatrix[0] = rotMatrix[4] = rotMatrix[8] = 1.0;

                for (int i = 0; i < 9; i++)
                {
                    if(double.TryParse(valueSet.ActualValue[i], out var value))
                    {
                        rotMatrix[i] = value;
                    }
                }
            }

            return rotMatrix;
        }

        /// <summary>
        /// Parses an <see cref="IValueSet"/> to Euler Angles
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>And array of type [Rx,Ry,Rz]</returns>
        public static double[] ParseIValueToEulerAngles(this IValueSet? valueSet)
        {
            return valueSet.ParseIValueToRotationMatrix().ToEulerAngles();
        }
    }
}
