// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Line.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Model.Viewer.Primitives
{
    using System.Numerics;

    /// <summary>
    /// Line primitive type
    /// </summary>
    public class Line : Primitive
    {
        /// <summary>
        /// The first point of the <see cref="Line"/>
        /// </summary>
        public Vector3 P0 { get; set; }

        /// <summary>
        /// The second point of the <see cref="Line"/>
        /// </summary>
        public Vector3 P1 { get; set; }

        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Line";

        /// <summary>
        /// Initializes a new instance of <see cref="Line"/> class
        /// </summary>
        /// <param name="p0">the first point of the line</param>
        /// <param name="p1">the second point of the line</param>
        public Line(Vector3 p0, Vector3 p1)
        {
            this.P0 = p0;
            this.P1 = p1;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Line"/> class
        /// </summary>
        /// <param name="x0">x coordinate of the first point</param>
        /// <param name="y0">y coordinate of the first point</param>
        /// <param name="z0">z coordinate of the first point</param>
        /// <param name="x1">x coordinate of the second point</param>
        /// <param name="y1">y coordinate of the second point</param>
        /// <param name="z1">z coordinate of the second point</param>
        public Line(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            this.P0 = new Vector3(x0, y0, z0);
            this.P1 = new Vector3(x1, y1, z1);
        }
    }
}
