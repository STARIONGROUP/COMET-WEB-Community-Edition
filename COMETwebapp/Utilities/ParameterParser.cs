// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterParser.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Utilities
{
    using System.Globalization;
    using System.Numerics;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;

    using COMETwebapp.Extensions;
    using COMETwebapp.Model.Viewer.Primitives;

    /// <summary>
    /// Class that contains the common parsers used for the <see cref="ParameterBase"/>
    /// </summary>
    public static class ParameterParser
    {
        /// <summary>
        /// Collection of names and related shapes
        /// </summary>
        private static readonly Dictionary<string, Func<Primitive>> ShapeCreatorCollection = new()
        {
            { "box", () => new Cube(1, 1, 1) },
            { "cone", () => new Cone(1, 1) },
            { "cylinder", () => new Cylinder(1, 1) },
            { "sphere", () => new Sphere(1) },
            { "torus", () => new Torus(1, 1) },
            { "triprism", () => new TriangularPrism(1, 1) },
            { "disc", () => new Disc(1) },
            { "hexagonalprism", () => new HexagonalPrism(1, 1) },
            { "rectangle", () => new Rectangle(1, 1) },
            { "triangle", () => new EquilateralTriangle(1) }
        };

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Primitive"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitive"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Primitive ShapeKindParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException(nameof(parameterBase), "The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return ShapeKindParser(valueSet);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> into a <see cref="Primitive"/>
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>the primitive if the value can be parsed, null otherwise</returns>
        public static Primitive ShapeKindParser(IValueSet valueSet)
        {
            Primitive primitive = null;

            var shapeKind = valueSet?.ActualValue.FirstOrDefault()?.ToLowerInvariant();

            if (shapeKind != null && ShapeCreatorCollection.TryGetValue(shapeKind, out var primiviteFunc))
            {
                primitive = primiviteFunc.Invoke();
            }

            return primitive;
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Orientation"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitive"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Orientation OrientationParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException(nameof(parameterBase), "The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return OrientationParser(valueSet);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> into a <see cref="Orientation"/>
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>the Orientation if the value can be parsed, a Orientation.Identity otherwise</returns>
        public static Orientation OrientationParser(IValueSet valueSet)
        {
            return valueSet is not null ? valueSet.ParseIValueToOrientation(AngleFormat.Radians) : Orientation.Identity(AngleFormat.Radians);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Vector3"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitive"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Vector3 PositionParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException(nameof(parameterBase),"The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return PositionParser(valueSet);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> into a <see cref="Vector3"/>
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>the <see cref="Vector3"/> if the value can be parsed, zero vector otherwise</returns>
        public static Vector3 PositionParser(IValueSet valueSet)
        {
            if (valueSet is null)
            {
                return Vector3.Zero;
            }

            var values = valueSet.ParseIValueToPosition();
            return new Vector3((float)values[0], (float)values[1], (float)values[2]);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="double"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitive"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static double DoubleParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException(nameof(parameterBase),"The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return DoubleParser(valueSet);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> into a <see cref="double"/>
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>the <see cref="double"/> if the value can be parsed, NaN otherwise</returns>
        public static double DoubleParser(IValueSet valueSet)
        {
            if (valueSet is not null && double.TryParse(valueSet.ActualValue.First(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return double.NaN;
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Vector3"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitive"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Vector3 ColorParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException(nameof(parameterBase),"The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return ColorParser(valueSet);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> into a <see cref="Vector3"/>
        /// </summary>
        /// <param name="valueSet">the value set to parse</param>
        /// <returns>the <see cref="Vector3"/> if the value can be parsed, zero vector otherwise</returns>
        public static Vector3 ColorParser(IValueSet valueSet)
        {
            if (valueSet is null)
            {
                return Vector3.Zero;
            }

            var textColor = valueSet.ActualValue.First();
            return textColor.ParseToColorVector();
        }
    }
}
