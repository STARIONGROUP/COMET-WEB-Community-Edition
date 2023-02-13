// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Orientation.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Model
{
    using COMETwebapp.Enumerations;
    using COMETwebapp.Utilities;

    /// <summary>
    /// Class to represent a orientation in space.
    /// </summary>
    public class Orientation
    {
        /// <summary>
        /// Backing field for the <see cref="X"/> property
        /// </summary>
        private double x;

        /// <summary>
        /// Backinf field for the <see cref="Y"/> property
        /// </summary>
        private double y;

        /// <summary>
        /// Backing field for the <see cref="Z"/> property
        /// </summary>
        private double z;

        /// <summary>
        /// Angle format of the EulerAngles
        /// </summary>
        private AngleFormat angleFormat = AngleFormat.Degrees;

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
        /// Format of the angles <see cref="X"/>, <see cref="Y"/> and <see cref="Z"/>
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
        /// Matrix of rotation computed by the angles <see cref="X"/>, <see cref="Y"/> and <see cref="Z"/> 
        /// </summary>
        public double[] Matrix { get; private set; }

        /// <summary>
        /// Gets the euler angles represented in this orientation
        /// </summary>
        public double[] Angles => new double[] { this.X, this.Y, this.Z };

        /// <summary>
        /// Creates a new instance of type <see cref="Orientation"/>
        /// </summary>
        /// <param name="x">angle around X axis</param>
        /// <param name="y">angle around Y axis</param>
        /// <param name="z">angle around Z axis</param>
        /// <param name="angleFormat">the angle format for computing the angles</param> 
        public Orientation(double x, double y, double z, AngleFormat angleFormat = AngleFormat.Degrees)
        {
            this.Matrix = new double[9];
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.AngleFormat = angleFormat;
        }

        /// <summary> 
        /// Creates a new instance of type <see cref="Orientation"/> 
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
            double rx = 0, ry = 0, rz = 0;

            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

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

            if (outputAngleFormat == AngleFormat.Radians)
            {
                return new [] { rx, ry, rz };
            }
            else
            {
                return new [] { Math.Round(rx * 180.0 / Math.PI, 3), Math.Round(ry * 180.0 / Math.PI, 3), Math.Round(rz * 180.0 / Math.PI, 3) };
            }
        }

        /// <summary>
        /// Recomputes <see cref="Matrix"/>
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