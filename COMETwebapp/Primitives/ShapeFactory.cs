// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShapeFactory.cs" company="RHEA System S.A.">
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

    public class ShapeFactory : IShapeFactory
    {
        /// <summary>
        /// Tries to get a <see cref="Primitive"/> from the <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> wiht the shape parameter</param>
        /// <param name="basicShape">the basic shape</param>
        /// <returns>True if the conversion succeed, false otherwise</returns>
        public bool TryGetPrimitiveFromElementUsageParameter(ElementUsage elementUsage, out Primitive basicShape)
        {
            var parameter = elementUsage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.ShortName == "kind"
                      && (x.ParameterType is EnumerationParameterType || x.ParameterType is TextParameterType));

            if (parameter is not null)
            {
                string? shapekind = parameter?.ExtractActualValues(1).First();
                switch (shapekind?.ToLowerInvariant())
                {
                    case "box": basicShape = new Cube(1, 1, 1); return true;
                    case "cylinder": basicShape = new Cylinder(1, 1); return true;
                    case "sphere": basicShape = new Sphere(1); return true;
                    case "torus": basicShape = new Torus(1, 1); return true;
                    default: basicShape = null; return false;
                }
            }
            else
            {
                basicShape = null;
                return false;
            }
        }
    }
}
