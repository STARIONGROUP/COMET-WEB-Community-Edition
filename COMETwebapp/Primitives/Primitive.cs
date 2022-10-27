// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Primitive.cs" company="RHEA System S.A.">
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
    using System.Numerics;

    /// <summary>
    /// Base class for the wrapper classes around JS objects
    /// </summary>
    public abstract class Primitive
    {
        /// <summary>
        /// The base color of the primitive
        /// </summary>
        public Vector3 Color { get; set; }

        /// <summary>
        /// Property that defined the exact type of pritimive. Used in JS.
        /// </summary>
        public abstract string Type { get; protected set; }

        /// <summary>
        /// ID of the property. Used to identify the primitive between the interop C#-JS
        /// </summary>
        public string ID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Sets the diffuse color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveDiffuseColor(Vector3 color)
        {
            await JSInterop.Invoke("SetDiffuseColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Sets the specular color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveSpecularColor(Vector3 color)
        {
            await JSInterop.Invoke("SetSpecularColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Sets the emissive color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveEmissiveColor(Vector3 color)
        {
            await JSInterop.Invoke("SetEmissiveColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Sets the ambient color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveAmbientColor(Vector3 color)
        {
            await JSInterop.Invoke("SetAmbientColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Gets info of the entity that can be used to show the user
        /// </summary>
        /// <returns>A string containing the info</returns>
        public virtual string GetInfo()
        {
            return "Type: " + Type.ToString();
        }
    }
}
