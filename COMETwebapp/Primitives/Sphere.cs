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
        public Sphere()
        {
            this.ParameterActions.Add(SceneProvider.DiameterShortName, (vs) => this.SetDiameter(vs));
        }

        public void SetDiameter(IValueSet newValue = null)
        {
            var diameterValueSet = newValue is null ? this.GetValueSet(SceneProvider.DiameterShortName) : newValue; 

            if (diameterValueSet is not null && double.TryParse(diameterValueSet.ActualValue.First(), out double d))
            {
                this.Radius = d / 2.0;
            }
        }
    }
}
