// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Triangle.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// Triangle primitive type
    /// </summary>
    public class Triangle : BasicPrimitive
    {
        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Triangle";

        /// <summary>
        /// Base of the triangle
        /// </summary>
        public double Base { get; set; }

        /// <summary>
        /// Height of the triangle
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="Triangle"/>
        /// </summary>
        /// <param name="_base">the base of the <see cref="Triangle"/></param>
        /// <param name="height">the height of the <see cref="Triangle"/></param>
        public Triangle(double _base, double height)
        {
            this.Base = _base;
            this.Height = height;
        }

        /// <summary>
        /// Set the dimensions of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> used for the dimensioning</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to dimensioning the <see cref="BasicPrimitive"/></param>
        public override void SetDimensionsFromElementUsageParameters(ElementUsage elementUsage, Option selectedOption, List<ActualFiniteState> states)
        {
            var baseValueSet = this.GetElementUsageValueSet(elementUsage, selectedOption, states, Scene.WidthShortName);
            var heightValueSet = this.GetElementUsageValueSet(elementUsage, selectedOption, states, Scene.HeightShortName);

            if (baseValueSet is not null && double.TryParse(baseValueSet.ActualValue.First(), out double b))
            {
                this.Base = b;
            }

            if (heightValueSet is not null && double.TryParse(heightValueSet.ActualValue.First(), out double h))
            {
                this.Height = h;
            }
        }
    }
}
