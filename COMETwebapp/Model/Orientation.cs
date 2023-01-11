// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Orientation.cs" company="RHEA System S.A.">
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
            get { return x; }
            set { x = value; this.RecomputeMatrix(); }
        }

        /// <summary>
        /// Angle of rotation around Y axis
        /// </summary>
        public double Y
        {
            get { return y; }
            set { y = value; this.RecomputeMatrix(); }
        }

        /// <summary>
        /// Angle of rotation around Z axis
        /// </summary>
        public double Z
        {
            get { return z; }
            set { z = value; this.RecomputeMatrix(); }
        }

        /// <summary>
        /// Format of the angles <see cref="X"/>, <see cref="Y"/> and <see cref="Z"/>
        /// </summary>
        public AngleFormat AngleFormat
        {
            get { return angleFormat; }
            set { angleFormat = value; this.RecomputeMatrix(); }
        }

        /// <summary>
        /// Matrix of rotation computed by the angles <see cref="X"/>, <see cref="Y"/> and <see cref="Z"/> 
        /// </summary>
        public double[] Matrix { get; private set; }

        /// <summary>
        /// Gets the euler angles represented in this orientation
        /// </summary>
        public double[] Angles => new double[] { X, Y, Z };

        /// <summary>
        /// Creates a new instance of type <see cref="Orientation"/>
        /// </summary>
        /// <param name="x">angle around X axis</param>
        /// <param name="y">angle around Y axis</param>
        /// <param name="z">angle around Z axis</param>
        public Orientation(double x, double y, double z)
        {
            this.Matrix = new double[9];
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Returns a orientation that represents the identity matrix
        /// </summary>
        /// <returns>the orientation</returns>
        public static Orientation Identity()
        {
            return new Orientation(0.0, 0.0, 0.0);
        }

        /// <summary>
        /// Recomputes <see cref="Matrix"/>
        /// </summary>
        public void RecomputeMatrix()
        {
            double a1 = X;
            double a2 = Y;
            double a3 = Z;

            if (this.AngleFormat == AngleFormat.Degrees)
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