// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cube.cs" company="RHEA System S.A.">
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
    /// Cube primitive type
    /// </summary>
    public class Cube : PositionablePrimitive
    {
        /// <summary>
        /// The width of the cube
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// The height of the cube
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// The depth of the cube
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Cube";

        /// <summary>
        /// Initializes a new instance of <see cref="Cube"/> class
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        public Cube(double width, double height, double depth)
        {
            this.Width = width; 
            this.Height = height; 
            this.Depth = depth;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Cube"/> class
        /// </summary>
        /// <param name="x">position along the x axis</param>
        /// <param name="y">position along the y axis</param>
        /// <param name="z">position along the z axis</param>
        /// <param name="width">the width of the cube</param>
        /// <param name="height">the height of the cube</param>
        /// <param name="depth">the depth of the cube</param>
        public Cube(double x, double y , double z, double width, double height, double depth)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
        }
    }
}
