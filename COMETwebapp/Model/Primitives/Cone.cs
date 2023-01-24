// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cone.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Model.Primitives
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities;
    using System.Collections.Generic;

    using COMETwebapp.Components.Viewer.Canvas;

    /// <summary>
    /// Cone primitive type
    /// </summary>
    public class Cone : Primitive
    {
        /// <summary>
        /// The radius of the base of the cone
        /// </summary>
        public double Radius { get; private set; }

        /// <summary>
        /// The height of the cone
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Cone";

        /// <summary>
        /// Initializes a new instance of <see cref="Cone" class/>
        /// </summary>
        /// <param name="radius">the radius of the base</param>
        /// <param name="height">the height of the cone</param>
        public Cone(double radius, double height)
        {
            Radius = radius;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Cone" class/>
        /// </summary>
        /// <param name="x">position along the x axis</param>
        /// <param name="y">position along the y axis</param>
        /// <param name="z">position along the z axis</param>
        /// <param name="radius">the radius of the base</param>
        /// <param name="height">the height of the cone</param>
        public Cone(double x, double y, double z, double radius, double height)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            Height = height;
        }

        /// <summary>
        /// Parses the <paramref name="valueSet"/> into the corresponding property depending on the <paramref name="parameterBase"/>
        /// </summary>
        /// <param name="parameterBase">the parameter base related to the property</param>
        /// <param name="valueSet">the value set to be parsed</param>
        public override void ParseParameter(ParameterBase parameterBase, IValueSet valueSet)
        {
            base.ParseParameter(parameterBase, valueSet);

            switch (parameterBase.ParameterType.ShortName)
            {
                case SceneSettings.DiameterShortName:
                    Radius = ParameterParser.DoubleParser(valueSet) / 2.0;
                    break;
                case SceneSettings.HeightShortName:
                    Height = ParameterParser.DoubleParser(valueSet);
                    break;
            }
        }
    }
}
