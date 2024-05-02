﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Orientation.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Model
{
    using COMET.Web.Common.Enumerations;

    /// <summary>
    /// Class to represent a orientation in space.
    /// </summary>
    public class Orientation
    {
        /// <summary>
        /// Angle format of the EulerAngles
        /// </summary>
        private AngleFormat angleFormat = AngleFormat.Degrees;

        /// <summary>
        /// Backing field for the <see cref="X" /> property
        /// </summary>
        private double x;

        /// <summary>
        /// Backinf field for the <see cref="Y" /> property
        /// </summary>
        private double y;

        /// <summary>
        /// Backing field for the <see cref="Z" /> property
        /// </summary>
        private double z;

        /// <summary>
        /// Creates a new instance of type <see cref="Orientation" />
        /// </summary>
        /// <param name="x">angle around X axis, if the angle format is in radians the angle is transformed in the range [0,2PI]</param>
        /// <param name="y">angle around Y axis, if the angle format is in radians the angle is transformed in the range [0,2PI]</param>
        /// <param name="z">angle around Z axis, if the angle format is in radians the angle is transformed in the range [0,2PI]</param>
        /// <param name="angleFormat">the angle format for computing the angles</param>
        public Orientation(double x, double y, double z, AngleFormat angleFormat = AngleFormat.Degrees)
        {
            this.Matrix = new double[9];

            if (angleFormat == AngleFormat.Radians)
            {
                this.X = WrapAngleInRange(x, 0.0, 2.0 * Math.PI);
                this.Y = WrapAngleInRange(y, 0.0, 2.0 * Math.PI);
                this.Z = WrapAngleInRange(z, 0.0, 2.0 * Math.PI);
            }
            else
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            this.AngleFormat = angleFormat;
        }

        /// <summary>
        /// Creates a new instance of type <see cref="Orientation" />
        /// </summary>
        /// <param name="matrix">the orientation matrix</param>
        /// <param name="angleFormat">the angle format for computing the angles</param>
        public Orientation(double[] matrix, AngleFormat angleFormat = AngleFormat.Degrees)
        {
            this.Matrix = new double[9];
            var eulerValues = ExtractAnglesFromMatrix(matrix, angleFormat);
            this.X = eulerValues[0];
            this.Y = eulerValues[1];
            this.Z = eulerValues[2];
            this.AngleFormat = angleFormat;
        }

        /// <summary>
        /// Angle of rotation around X axis
        /// </summary>
        public double X
        {
            get => this.x;
            set
            {
                this.x = value;
                this.RecomputeMatrix();
            }
        }

        /// <summary>
        /// Angle of rotation around Y axis
        /// </summary>
        public double Y
        {
            get => this.y;
            set
            {
                this.y = value;
                this.RecomputeMatrix();
            }
        }

        /// <summary>
        /// Angle of rotation around Z axis
        /// </summary>
        public double Z
        {
            get => this.z;
            set
            {
                this.z = value;
                this.RecomputeMatrix();
            }
        }

        /// <summary>
        /// Format of the angles <see cref="X" />, <see cref="Y" /> and <see cref="Z" />
        /// </summary>
        public AngleFormat AngleFormat
        {
            get => this.angleFormat;
            set
            {
                this.angleFormat = value;
                this.RecomputeMatrix();
            }
        }

        /// <summary>
        /// Matrix of rotation computed by the angles <see cref="X" />, <see cref="Y" /> and <see cref="Z" />
        /// </summary>
        public double[] Matrix { get; private set; }

        /// <summary>
        /// Gets the euler angles represented in this orientation
        /// </summary>
        public double[] Angles => new[] { this.X, this.Y, this.Z };

        /// <summary>
        /// Returns a orientation that represents the identity matrix
        /// </summary>
        /// <returns>the orientation</returns>
        public static Orientation Identity(AngleFormat angleFormat = AngleFormat.Degrees)
        {
            return new Orientation(0.0, 0.0, 0.0) { AngleFormat = angleFormat };
        }

        /// <summary>
        /// Extract the angles from the orientation matrix
        /// </summary>
        /// <param name="matrix">the orientation matrix</param>
        /// <param name="outputAngleFormat">the output format of the angles</param>
        /// <returns>the angles in an array of type [Rx,Ry,Rz]</returns>
        /// <exception cref="ArgumentNullException">if the matrix is null</exception>
        /// <exception cref="ArgumentException">if the matrix don't have the correct size</exception>
        public static double[] ExtractAnglesFromMatrix(double[] matrix, AngleFormat outputAngleFormat = AngleFormat.Degrees)
        {
            double rx, ry, rz;

            ArgumentNullException.ThrowIfNull(matrix);

            if (matrix.Length != 9)
            {
                throw new ArgumentException("The Matrix needs to have 9 values");
            }

            if (matrix[6] != 1.0 && matrix[6] != -1.0)
            {
                ry = -Math.Asin(matrix[6]);
                rx = Math.Atan2(matrix[7] / Math.Cos(ry), matrix[8] / Math.Cos(ry));
                rz = Math.Atan2(matrix[3] / Math.Cos(ry), matrix[0] / Math.Cos(ry));
            }
            else
            {
                rz = 0;

                if (matrix[6] == -1.0)
                {
                    ry = Math.PI / 2.0;
                    rx = rz + Math.Atan2(matrix[1], matrix[2]);
                }
                else
                {
                    ry = -Math.PI / 2.0;
                    rx = -rz + Math.Atan2(-matrix[1], -matrix[2]);
                }
            }

            if (outputAngleFormat != AngleFormat.Radians)
            {
                return new[] { Math.Round(rx * 180.0 / Math.PI, 3), Math.Round(ry * 180.0 / Math.PI, 3), Math.Round(rz * 180.0 / Math.PI, 3) };
            }

            rx = WrapAngleInRange(rx, 0.0, 2.0 * Math.PI);
            ry = WrapAngleInRange(ry, 0.0, 2.0 * Math.PI);
            rz = WrapAngleInRange(rz, 0.0, 2.0 * Math.PI);

            return new[] { rx, ry, rz };
        }

        /// <summary>
        /// Wraps the angle in the defined range
        /// </summary>
        /// <param name="angle">the angle to wrap</param>
        /// <param name="lower">the lower limit of the range</param>
        /// <param name="upper">the upper limit of the range</param>
        /// <returns>the wrapped angle</returns>
        public static double WrapAngleInRange(double angle, double lower, double upper)
        {
            var distance = upper - lower;
            var times = Math.Floor((angle - lower) / distance);

            return angle - times * distance;
        }

        /// <summary>
        /// Recomputes <see cref="Matrix" />
        /// </summary>
        private void RecomputeMatrix()
        {
            var a1 = this.X;
            var a2 = this.Y;
            var a3 = this.Z;

            if (this.AngleFormat == AngleFormat.Degrees)
            {
                a1 = a1 * Math.PI / 180.0;
                a2 = a2 * Math.PI / 180.0;
                a3 = a3 * Math.PI / 180.0;
            }

            var c1 = Math.Cos(a1);
            var c2 = Math.Cos(a2);
            var c3 = Math.Cos(a3);

            var s1 = Math.Sin(a1);
            var s2 = Math.Sin(a2);
            var s3 = Math.Sin(a3);

            //ZYX -> First X, Second Y, Third Z
            this.Matrix[0] = c2 * c3;
            this.Matrix[1] = s1 * s2 * c3 - c1 * s3;
            this.Matrix[2] = c1 * s2 * c3 + s1 * s3;
            this.Matrix[3] = c2 * s3;
            this.Matrix[4] = s1 * s2 * s3 + c1 * c3;
            this.Matrix[5] = c1 * s2 * s3 - s1 * c3;
            this.Matrix[6] = -s2;
            this.Matrix[7] = s1 * c2;
            this.Matrix[8] = c1 * c2;
        }
    }
}
