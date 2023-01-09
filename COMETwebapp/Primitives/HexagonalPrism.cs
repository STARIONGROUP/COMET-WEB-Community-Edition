// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HexagonalPrism.cs" company="RHEA System S.A.">
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

    using COMETwebapp.Components.Viewer;

    /// <summary>
    /// Hexagonal prism primitive type
    /// </summary>
    public class HexagonalPrism : Primitive
    {
        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "HexagonalPrism";

        /// <summary>
        /// The radius of the cicumscribed circle
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// The height of the prism
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="HexagonalPrism"/>
        /// </summary>
        /// <param name="radius">the size of the circumradius</param>
        /// <param name="height">the height of the prism</param>
        public HexagonalPrism(double radius, double height)
        {
            this.Radius = radius;
            this.Height = height;
        }

        /// <summary>
        /// Set the dimensions of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public override void SetDimensionsFromElementUsageParameters()
        {
            var radiusValueSet = this.GetValueSet(SceneProvider.DiameterShortName);
            var heightValueSet = this.GetValueSet(SceneProvider.HeightShortName);

            if (radiusValueSet is not null && double.TryParse(radiusValueSet.ActualValue.First(), out double d))
            {
                this.Radius = d/2.0;
            }

            if (heightValueSet is not null && double.TryParse(heightValueSet.ActualValue.First(), out double h))
            {
                this.Height = h;
            }
        }

        /// <summary>
        /// Updates a property of the <see cref="Primitive"/> with the data of the <see cref="IValueSet"/>
        /// </summary>
        /// <param name="parameterTypeShortName">the short name for the parameter type that needs an update</param>
        /// <param name="newValue">the new value set</param>
        public override void UpdatePropertyWithParameterData(string parameterTypeShortName, IValueSet newValue)
        {
            base.UpdatePropertyWithParameterData(parameterTypeShortName, newValue);

            switch (parameterTypeShortName)
            {
                case SceneProvider.DiameterShortName:
                    if (double.TryParse(newValue.ActualValue.First(), out double d))
                    {
                        this.Radius = d/2.0;
                    }
                    break;
                case SceneProvider.HeightShortName:
                    if (double.TryParse(newValue.ActualValue.First(), out double h))
                    {
                        this.Height = h;
                    }
                    break;
            }
            this.Regenerate();
        }
    }
}
