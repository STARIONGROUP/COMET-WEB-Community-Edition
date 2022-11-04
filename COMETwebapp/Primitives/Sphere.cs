// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sphere.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Viewer;

    /// <summary>
    /// Sphere primitive type
    /// </summary>
    public class Sphere : BasicPrimitive
    {
        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Sphere";

        /// <summary>
        /// The radius of the <see cref="Sphere"/>
        /// </summary>
        public double Radius { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Sphere"/> class
        /// </summary>
        /// <param name="radius"></param>
        public Sphere(double radius)
        {
            this.Radius = radius;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Sphere"/> class
        /// </summary>
        /// <param name="x">position along the x axis</param>
        /// <param name="y">position along the y axis</param>
        /// <param name="z">position along the z axis</param>
        /// <param name="radius">the radius of the sphere</param>
        public Sphere(double x, double y, double z, double radius)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Radius = radius;
        }

        /// <summary>
        /// Set the dimensions of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> used for the dimensioning</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to dimensioning the <see cref="BasicPrimitive"/></param>
        public override void SetDimensionsFromElementUsageParameters(ElementUsage elementUsage, Option selectedOption, List<ActualFiniteState> states)
        {
            var diameterValueSet = this.GetElementUsageValueSet(elementUsage, selectedOption, states, SceneProvider.DiameterShortName);
            
            if(diameterValueSet is not null && double.TryParse(diameterValueSet.ActualValue.First(), out double d))
            {
                this.Radius =  d/ 2.0;
            }
        }
    }
}
