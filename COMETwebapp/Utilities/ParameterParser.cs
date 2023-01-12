// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterParser.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Utilities
{
    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;
    using System.Globalization;
    using System.Numerics;

    /// <summary>
    /// Class that contains the common parsers used for the <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterParser
    {
        /// <summary>
        /// Collection of names and related shapes
        /// </summary>
        private static Dictionary<string, Func<Primitive>> ShapeCreatorCollection = new Dictionary<string, Func<Primitive>>()
        {
            {"box", () => new Cube(1, 1, 1) },
            {"cone", () => new Cone(1, 1) },
            {"cylinder", () => new Cylinder(1, 1) },
            {"sphere", () => new Sphere(1) },
            {"torus", () => new Torus(1, 1) },
            {"triprism", () => new TriangularPrism(1, 1) },
            {"disc", () => new Disc(1) },
            {"hexagonalprism", () => new HexagonalPrism(1, 1) },
            {"rectangle", () => new Rectangle(1, 1) },
            {"triangle", () => new EquilateralTriangle(1) },
        };

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Primitive"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitives"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Primitive? ShapeKindParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException("The parameter base can not be null");
            }
            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return ShapeKindParser(valueSet);
        }

        public static Primitive? ShapeKindParser(IValueSet? valueSet)
        {
            Primitive? primitive = null;

            if (valueSet is not null)
            {
                string? shapeKind = valueSet.ActualValue.FirstOrDefault()?.ToLowerInvariant();

                if (shapeKind is not null && ShapeCreatorCollection.ContainsKey(shapeKind))
                {
                    primitive = ShapeCreatorCollection[shapeKind].Invoke();
                }
            }

            return primitive;
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Orientation"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitives"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Orientation OrientationParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException("The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return OrientationParser(valueSet);
        }

        public static Orientation OrientationParser(IValueSet? valueSet)
        {
            if (valueSet is not null)
            {
                return valueSet.ParseIValueToOrientation(Enumerations.AngleFormat.Radians);
            }

            return Orientation.Identity(Enumerations.AngleFormat.Radians);
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="Vector3"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitives"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Vector3 PositionParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException("The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return PositionParser(valueSet);
        }

        public static Vector3 PositionParser(IValueSet? valueSet)
        {
            if (valueSet is not null)
            {
                var values = valueSet.ParseIValueToPosition();
                return new Vector3((float)values[0], (float)values[1], (float)values[2]);
            }

            return Vector3.Zero;
        }

        /// <summary>
        /// Tries to parse the <see cref="IValueSet"/> of a <see cref="ParameterBase"/> into a <see cref="double"/>
        /// </summary>
        /// <param name="parameterBase">the parameter to parse</param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the parsed <see cref="Primitives"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static double DoubleParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException("The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return DoubleParser(valueSet);
        }

        public static double DoubleParser(IValueSet? valueSet)
        {
            if (valueSet is not null && double.TryParse(valueSet.ActualValue.First(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
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
        /// <returns>the parsed <see cref="Primitives"/></returns>
        /// <exception cref="ArgumentNullException">if the parameter is null</exception>
        public static Vector3 ColorParser(ParameterBase parameterBase, Option option, List<ActualFiniteState> states)
        {
            if (parameterBase == null)
            {
                throw new ArgumentNullException("The parameter base can not be null");
            }

            var valueSet = parameterBase.GetValueSetFromOptionAndStates(option, states);
            return ColorParser(valueSet);
        }

        public static Vector3 ColorParser(IValueSet? valueSet)
        {
            if (valueSet is not null)
            {
                string textColor = valueSet.ActualValue.First();
                return textColor.ParseToColorVector();
            }

            return Vector3.Zero;
        }
    }
}
