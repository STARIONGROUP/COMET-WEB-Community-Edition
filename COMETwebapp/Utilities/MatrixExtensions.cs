// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixExtensions.cs" company="RHEA System S.A.">
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
    /// <summary>
    /// Static extension methods 
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Transforms the <param name="rotMatrix"/> that represents a rotation matrix of 3x3 to the rotation expressed in Euler Angles
        /// </summary>
        /// <param name="rotMatrix">the rotation matrix 3x3 stored in row-major order. For more info <see cref="https://en.wikipedia.org/wiki/Row-_and_column-major_order"/></param>
        /// <returns>The Euler Angles in form [Rx,Ry,Rz]</returns>
        /// <exception cref="ArgumentException">If the <paramref name="rotMatrix"/> can't be expressed as a 3x3 matrix </exception>
        public static double[] ToEulerAngles(this double[] rotMatrix)
        {
            double Rx = 0, Ry = 0, Rz = 0;

            if (rotMatrix.Length < 9)
            {
                throw new ArgumentException("The rotation Matrix needs to be at least 3x3 so at least an Array of 9 numbers is needed");
            }

            if (rotMatrix[6] != 1 && rotMatrix[6] != -1)
            {
                Ry = -Math.Asin(rotMatrix[6]);
                Rx = Math.Atan2(rotMatrix[7] / Math.Cos(Ry), rotMatrix[8] / Math.Cos(Ry));
                Rz = Math.Atan2(rotMatrix[3] / Math.Cos(Ry), rotMatrix[0] / Math.Cos(Ry));
            }
            else
            {
                Rz = 0;
                if (rotMatrix[6] == -1)
                {
                    Ry = Math.PI / 2.0;
                    Rx = Rz + Math.Atan2(rotMatrix[1], rotMatrix[2]);
                }
                else
                {
                    Ry = -Math.PI / 2.0;
                    Rx = -Rz + Math.Atan2(-rotMatrix[1], -rotMatrix[2]);
                }
            }
            return new double[] { Rx, Ry, Rz };
        }
    }
}
