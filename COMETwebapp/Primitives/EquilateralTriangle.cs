// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EquilateralTriangle.cs" company="RHEA System S.A.">
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
    /// Equilateral Triangle primitive type
    /// </summary>
    public class EquilateralTriangle : BasicPrimitive
    {
        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "EquilateralTriangle";

        /// <summary>
        /// The radius of the cicumscribed circle
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="EquilateralTriangle"/>
        /// </summary>
        public EquilateralTriangle()
        {
            this.ParameterActions.Add(SceneProvider.DiameterShortName, (vs) => this.SetDiameter(vs));
        }

        /// <summary>
        /// Sets the diameter of the <see cref="EquilateralTriangle"/>
        /// </summary>
        /// <param name="newValue">if the value is null the value it's computed from the asociated parameter</param>
        public void SetDiameter(IValueSet newValue = null)
        {
            var radiusValueSet = newValue is null ? this.GetValueSet(SceneProvider.DiameterShortName) : newValue;

            if (radiusValueSet is not null && double.TryParse(radiusValueSet.ActualValue.First(), out double d))
            {
                this.Radius = d / 2.0;
            }
        }
    }
}
