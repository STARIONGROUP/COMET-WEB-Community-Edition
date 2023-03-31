// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;

    /// <summary>
    /// Static extension methods
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Transforms a set of doubles to a <see cref="Orientation" />
        /// </summary>
        /// <param name="values">values to transform</param>
        /// <param name="isMatrix">true if the values corresponds to a rotation matrix, false if correspond to euler angles</param>
        /// <param name="angleFormat">the format of the angle</param>
        /// <returns>the orientation</returns>
        /// <exception cref="ArgumentException">if the rotation matrix or euler angles don't have the correct format.</exception>
        public static Orientation ToOrientation(this IList<double> values, bool isMatrix, AngleFormat angleFormat)
        {
            if (isMatrix)
            {
                if (values.Count != 9)
                {
                    throw new ArgumentException("A rotation matrix must have 9 values");
                }

                var eulerAngles = ToEulerAngles(values, angleFormat);
                return new Orientation(eulerAngles[0], eulerAngles[1], eulerAngles[2]) { AngleFormat = angleFormat };
            }

            if (values.Count != 3)
            {
                throw new ArgumentException("Euler angles must be 3 values");
            }

            return new Orientation(values[0], values[1], values[2]) { AngleFormat = angleFormat };
        }

        /// <summary>
        /// Transforms the
        /// <param name="rotMatrix" />
        /// that represents a rotation matrix of 3x3 to the rotation expressed in Euler Angles
        /// </summary>
        /// <param name="rotMatrix">
        /// the rotation matrix 3x3 stored in row-major order. For more info
        /// <see cref="https://en.wikipedia.org/wiki/Row-_and_column-major_order" />
        /// </param>
        /// <param name="outputAngleFormat">The <see cref="AngleFormat" /></param>
        /// <returns>The Euler Angles in form [Rx,Ry,Rz]</returns>
        /// <exception cref="ArgumentException">If the <paramref name="rotMatrix" /> can't be expressed as a 3x3 matrix </exception>
        public static double[] ToEulerAngles(this IList<double> rotMatrix, AngleFormat outputAngleFormat = AngleFormat.Radians)
        {
            double rx, ry, rz;

            if (rotMatrix.Count != 9)
            {
                throw new ArgumentException("The rotation Matrix needs to be at least 3x3 so an Array of 9 numbers is needed");
            }

            if (rotMatrix[6] != 1 && rotMatrix[6] != -1)
            {
                ry = -Math.Asin(rotMatrix[6]);
                rx = Math.Atan2(rotMatrix[7] / Math.Cos(ry), rotMatrix[8] / Math.Cos(ry));
                rz = Math.Atan2(rotMatrix[3] / Math.Cos(ry), rotMatrix[0] / Math.Cos(ry));
            }
            else
            {
                rz = 0;

                if (rotMatrix[6] == -1)
                {
                    ry = Math.PI / 2.0;
                    rx = rz + Math.Atan2(rotMatrix[1], rotMatrix[2]);
                }
                else
                {
                    ry = -Math.PI / 2.0;
                    rx = -rz + Math.Atan2(-rotMatrix[1], -rotMatrix[2]);
                }
            }

            return outputAngleFormat == AngleFormat.Radians
                ? new[] { rx, ry, rz }
                : new[] { Math.Round(rx * 180.0 / Math.PI, 3), Math.Round(ry * 180.0 / Math.PI, 3), Math.Round(rz * 180.0 / Math.PI, 3) };
        }
    }
}
