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

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Utilities;
    
    /// <summary>
    /// Represents an <see cref="CDP4Common.EngineeringModelData.ElementUsage"/> on the Scene from the selected <see cref="Option"/> and <see cref="ActualFiniteState"/>
    /// </summary>
    public abstract class Primitive
    {        
        /// <summary>
        /// Rendering group of this <see cref="Primitive"/>. Default is 0. Valid Range[0,4].
        /// </summary>
        public int RenderingGroup { get; set; } 

        /// <summary>
        /// The default color if the <see cref="Color"/> has not been defined.
        /// </summary>
        public static Vector3 DefaultColor { get; } = new Vector3(210, 210, 210);

        /// <summary>
        /// Property that defined the exact type of pritimive. Used in JS.
        /// </summary>
        public abstract string Type { get; protected set; }

        /// <summary>
        /// Gets or sets if the primitive has halo
        /// </summary>
        public bool HasHalo { get; set; }

        /// <summary>
        /// The base color of the primitive
        /// </summary>
        public Vector3 Color { get; set; }

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
        /// Angle of rotation (radians) around X axis.
        /// </summary>
        public double RX { get; set; }

        /// <summary>
        /// Angle of rotation (radians) around Y axis.
        /// </summary>
        public double RY { get; set; }

        /// <summary>
        /// Angle of rotation (radians) around Z axis.
        /// </summary>
        public double RZ { get; set; }

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
        /// Sets the color of this <see cref="Primitive"/>.
        /// </summary>
        /// <param name="r">red component of the color in range [0,255]</param>
        /// <param name="g">green component of the color in range [0,255]</param>
        /// <param name="b">blue component of the color in range [0,255]</param>
        /// <returns></returns>
        public void SetColor(float r, float g, float b)
        {
            this.Color = new Vector3(r, g, b);
        }

        /// <summary>
        /// Parses the <paramref name="valueSet"/> into the corresponding property depending on the <paramref name="parameterBase"/>
        /// </summary>
        /// <param name="parameterBase">the parameter base related to the property</param>
        /// <param name="valueSet">the value set to be parsed</param>
        public virtual void ParseParameter(ParameterBase parameterBase, IValueSet valueSet)
        {
            var parameterTypeShortName = parameterBase.ParameterType.ShortName;            

            switch (parameterTypeShortName)
            {
                case SceneSettings.OrientationShortName:
                    var orientation = ParameterParser.OrientationParser(valueSet);
                    this.RX = orientation.X;
                    this.RY = orientation.Y;
                    this.RZ = orientation.Z;
                    break;
                case SceneSettings.PositionShortName:
                    var position = ParameterParser.PositionParser(valueSet);
                    this.X = position.X;
                    this.Y = position.Y;
                    this.Z = position.Z;
                    break;

                case SceneSettings.ColorShortName:
                    this.Color = ParameterParser.ColorParser(valueSet);
                    break;
            }
        }
    }
}
