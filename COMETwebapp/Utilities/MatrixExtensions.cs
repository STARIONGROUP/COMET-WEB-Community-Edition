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
    /// The format of the angle
    /// </summary>
    public enum AngleFormat
    {
        Degrees,
        Radians,
    }

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
        public static double[] ToEulerAngles(this double[] rotMatrix, AngleFormat outputAngleFormat = AngleFormat.Radians)
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

            if(outputAngleFormat == AngleFormat.Radians)
            {
                return new double[] { Rx, Ry, Rz };
            }
            else
            {
                return new double[] { Math.Round(Rx * 180.0/Math.PI,3), Math.Round(Ry * 180.0 / Math.PI,3), Math.Round(Rz * 180.0 / Math.PI,3) };
            }
        }

        /// <summary>
        /// Transforms the euler angles into a 3x3 rotation matrix.
        /// </summary>
        /// <param name="eulerAngles">euler angles in the expressed format</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the <paramref name="eulerAngles"/> can't be expressed as euler angles</exception>
        public static double[] ToRotationMatrix(this double[] eulerAngles, AngleFormat inputAngleFormat = AngleFormat.Radians)
        {
            if(eulerAngles.Length < 3)
            {
                throw new ArgumentException("To create a rotation matrix at least 3 angles are needed");
            }

            double[] rotMatrix = new double[9];

            double a1 = eulerAngles[0];
            double a2 = eulerAngles[1];
            double a3 = eulerAngles[2];

            if(inputAngleFormat == AngleFormat.Degrees)
            {
                a1 = a1 * Math.PI / 180.0;
                a2 = a2 * Math.PI / 180.0;
                a3 = a3 * Math.PI / 180.0;
            }

            double c1 = Math.Cos(a1);
            double c2 = Math.Cos(a2);
            double c3 = Math.Cos(a3);

            double s1 = Math.Sin(a1);
            double s2 = Math.Sin(a2);
            double s3 = Math.Sin(a3);

            //ZYX -> First x, Second Y, Third Z
            rotMatrix[0] = c2 * c3;
            rotMatrix[1] = s1 * s2 * c3 - c1 * s3;
            rotMatrix[2] = c1 * s2 * c3 + s1 * s3;
            rotMatrix[3] = c2 * s3;
            rotMatrix[4] = s1 * s2 * s3 + c1 * c3;
            rotMatrix[5] = c1 * s2 * s3 - s1 * c3;
            rotMatrix[6] = -s2;
            rotMatrix[7] = s1 * c2;
            rotMatrix[8] = c1 * c2;

            return rotMatrix;
        }
    }
}
