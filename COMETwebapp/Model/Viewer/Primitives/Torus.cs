﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Torus.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Utilities;

    /// <summary>
    /// Torus primitive type
    /// </summary>
    public class Torus : Primitive
    {
        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Torus";

        /// <summary>
        /// Diameter of the <see cref="Torus"/>
        /// </summary>
        public double Diameter { get; private set; }

        /// <summary>
        /// Thickness of the <see cref="Torus"/>
        /// </summary>
        public double Thickness { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Torus"/> class
        /// </summary>
        /// <param name="diameter">the diameter of the <see cref="Torus"/></param>
        /// <param name="thickness">The thickness of the <see cref="Torus"/></param>
        public Torus(double diameter, double thickness)
        {
            this.Diameter = diameter;
            this.Thickness = thickness;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Torus"/> class
        /// </summary>
        /// <param name="x">position along the x axis</param>
        /// <param name="y">position along the y axis</param>
        /// <param name="z">position along the z axis</param>
        /// <param name="diameter">the diameter of the <see cref="Torus"/></param>
        /// <param name="thickness">The thickness of the <see cref="Torus"/></param>
        public Torus(double x, double y, double z, double diameter, double thickness)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Diameter = diameter;
            this.Thickness = thickness;
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
                    this.Diameter = ParameterParser.DoubleParser(valueSet);
                    break;
                case SceneSettings.ThicknessShortName:
                    this.Thickness = ParameterParser.DoubleParser(valueSet);
                    break;
            }
        }
    }
}
