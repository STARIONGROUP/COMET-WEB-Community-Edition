// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionablePrimitive.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using System.Numerics;

    public abstract class PositionablePrimitive : Primitive
    {
        /// <summary>
        /// Subtype of the primitive
        /// </summary>
        public string Subtype { get; } = "Positionable";

        /// <summary>
        /// Position along the X axis
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Position along the Y axis
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Position along the Z axis
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// Angle of rotation (radians) around X axis
        /// </summary>
        public double RX { get; private set; }

        /// <summary>
        /// Angle of rotation (radians) around Y axis
        /// </summary>
        public double RY { get; private set; }

        /// <summary>
        /// Angle of rotation (radians) around Z axis
        /// </summary>
        public double RZ { get; private set; }

        /// <summary>
        /// The current position of the <see cref="PositionablePrimitive"/>
        /// </summary>
        public Vector3 Position => new Vector3((float)this.X, (float)this.Y, (float)this.Z);

        /// <summary>
        /// The current orientation of the <see cref="PositionablePrimitive"/>
        /// </summary>
        public Vector3 Orientation => new Vector3((float)this.RX, (float)this.RY, (float)this.RZ);

        /// <summary>
        /// Sets a NEW translation to the primitive. Added to the previous one.
        /// </summary>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        public void SetTranslation(double x, double y, double z)
        {
            this.X += x;
            this.Y += y;
            this.Z += z;
            Scene.SetPrimitiveRotation(this, this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Sets a NEW rotation to the primitive. Added to the previous one.
        /// </summary>
        /// <param name="rx">angle of rotation in radians around X axis</param>
        /// <param name="ry">angle of rotation in radians around Y axis</param>
        /// <param name="rz">angle of rotation in radians around Z axis</param>
        public void SetRotation(double rx, double ry, double rz)
        {
            this.RX += rx;
            this.RY += ry;
            this.RZ += rz;
            Scene.SetPrimitiveRotation(this, this.RX, this.RY, this.RZ);
        }

        /// <summary>
        /// Reset all transformations of the primitive
        /// </summary>
        public void ResetTransformations()
        {
            this.ResetRotation();
            this.ResetTranslation();
        }

        /// <summary>
        /// Resets the translation of the primitive
        /// </summary>
        public void ResetTranslation()
        {
            this.X = this.Y = this.Z = 0;
        }

        /// <summary>
        /// Resets the rotation of the primitive
        /// </summary>
        public void ResetRotation()
        {
            this.RX = this.RY = this.RZ = 0;
        }

        /// <summary>
        /// Sets the position of a <see cref="Primitive"/> from the parameters of a <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> with the position parameter</param>
        public void SetPositionFromElementUsageParameter(ElementUsage elementUsage)
        {
            var parameter = elementUsage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.ShortName == "coord"
                                      && x.ParameterType is CompoundParameterType);

            if (parameter is not null)
            {
                string[]? translations = parameter?.ExtractActualValues(3);


                if (translations is not null && translations.All(x => x is not null))
                {
                    var x = double.Parse(translations[0]);
                    var y = double.Parse(translations[1]);
                    var z = double.Parse(translations[2]);
                    this.SetTranslation(x, y, z);
                }
            }
        }
    }
}
