// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cylinder.cs" company="RHEA System S.A.">
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
namespace COMETwebapp.Primitives
{
    /// <summary>
    /// Cylinder primitive type
    /// </summary>
    public class Cylinder : PositionablePrimitive
    {
        /// <summary>
        /// Radius of the <see cref="Cylinder"/>
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Height of the <see cref="Cylinder"/>
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Cylinder";

        /// <summary>
        /// Initializes a new instance of <see cref="Cylinder"/> class
        /// </summary>
        /// <param name="radius">the radius of the <see cref="Cylinder"/></param>
        /// <param name="height">the height of the <see cref="Cylinder"/></param>
        public Cylinder(double radius, double height)
        {
            this.Radius = radius;
            this.Height = height;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Cylinder"/> class
        /// </summary>
        /// <param name="x">position along the x axis</param>
        /// <param name="y">position along the y axis</param>
        /// <param name="z">position along the z axis</param>
        /// <param name="radius">the radius of the <see cref="Cylinder"/></param>
        /// <param name="height">the height of the <see cref="Cylinder"/></param>
        public Cylinder(double x, double y, double z, double radius, double height)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Radius = radius;
            this.Height = height;
        }
    }
}
