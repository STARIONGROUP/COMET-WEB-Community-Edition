// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomPrimitive.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Jaime Bernar
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
    /// Custom Primitive type
    /// </summary>
    public class CustomPrimitive : Primitive
    {
        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "CustomPrimitive";

        /// <summary>
        /// The path to the folder of the specified file
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The name of the specified file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CustomPrimitive"/> class
        /// </summary>
        /// <param name="path">the path to the file</param>
        /// <param name="fileName">the file name with extension included</param>
        public CustomPrimitive(string path, string fileName)
        {
            this.Path = path;
            this.FileName = fileName;
        }
    }
}
