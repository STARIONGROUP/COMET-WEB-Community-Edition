// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicPrimitive.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// Class fot primitives that can be positioned and rotated in space
    /// </summary>
    public abstract class BasicPrimitive : Primitive
    {
        /// <summary>
        /// Subtype of the primitive
        /// </summary>
        public string Subtype { get; } = "BasicPrimitive";

        /// <summary>
        /// Position along the X axis
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Position along the Y axis
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Position along the Z axis
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// Angle of rotation (radians) around X axis.
        /// </summary>
        public double RX { get; private set; }

        /// <summary>
        /// Angle of rotation (radians) around Y axis.
        /// </summary>
        public double RY { get; private set; }

        /// <summary>
        /// Angle of rotation (radians) around Z axis.
        /// </summary>
        public double RZ { get; private set; }

        /// <summary>
        /// Sets a NEW translation to the primitive. 
        /// </summary>
        /// <param name="x">translation along X axis</param>
        /// <param name="y">translation along Y axis</param>
        /// <param name="z">translation along Z axis</param>
        public void SetTranslation(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            Scene.SetPrimitivePosition(this, this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Sets a NEW rotation to the primitive. 
        /// </summary>
        /// <param name="rx">angle of rotation in radians around X axis</param>
        /// <param name="ry">angle of rotation in radians around Y axis</param>
        /// <param name="rz">angle of rotation in radians around Z axis</param>
        public void SetRotation(double rx, double ry, double rz)
        {
            this.RX = rx;
            this.RY = ry;
            this.RZ = rz;
            Scene.SetPrimitiveRotation(this, this.RX, this.RY, this.RZ);
        }

        /// <summary>
        /// Adds a translation to the primitive.
        /// </summary>
        public void AddTranslation(double x, double y, double z) 
        {
            this.X += x;
            this.Y += y;
            this.Z += z;
            this.SetTranslation(this.X,this.Y,this.Z);
        }

        /// <summary>
        /// Adds a rotation to the primitive
        /// </summary>
        public void AddRotation(double rx, double ry, double rz)
        {
            this.RX += rx;
            this.RY += ry;
            this.RZ += rz;
            this.SetRotation(this.RX, this.RY, this.RZ);
        }

        /// <summary>
        /// Reset all transformations of the primitive
        /// </summary>
        public void ResetTransformations()
        {
            this.ResetRotation();
            this.ResetTranslation();
        }

        /// <summary>
        /// Resets the translation of the primitive
        /// </summary>
        public void ResetTranslation()
        {
            this.X = this.Y = this.Z = 0;
        }

        /// <summary>
        /// Resets the rotation of the primitive
        /// </summary>
        public void ResetRotation()
        {
            this.RX = this.RY = this.RZ = 0;
        }

        /// <summary>
        /// Set the position of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> used for the positioning</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to position the <see cref="BasicPrimitive"/></param>
        public Task SetPositionFromElementUsageParameters(ElementUsage elementUsage, Option selectedOption, List<ActualFiniteState> states)
        {
            IValueSet? valueSet = null;

            valueSet = this.GetElementUsageValueSet(elementUsage, selectedOption, states, Scene.PositionShortName);

            if (valueSet is not null &&
                double.TryParse(valueSet.ActualValue[0], out var x) &&
                double.TryParse(valueSet.ActualValue[1], out var y) &&
                double.TryParse(valueSet.ActualValue[2], out var z))
            {
                this.SetTranslation(x / 1000.0, y / 1000.0, z / 1000.0);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the value sets asociated to an element usage depending on the selected option and the available states
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> used for query the value set</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">The available states</param>
        /// <returns></returns>
        protected IValueSet? GetElementUsageValueSet(ElementUsage elementUsage, Option selectedOption, List<ActualFiniteState> states, string parameterTypeShortName)
        {
            ParameterBase? parameterBase = null;
            IValueSet? valueSet = null;

            if (elementUsage.ParameterOverride.Count > 0)
            {
                parameterBase = elementUsage.ParameterOverride.FirstOrDefault(x => x.ParameterType.ShortName == parameterTypeShortName
                                                                                   && x.ParameterType.GetType() == Scene.GetParameterTypeFromParameterShortName(parameterTypeShortName));
            }

            if (parameterBase is null)
            {
                parameterBase = elementUsage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.ShortName == parameterTypeShortName
                                                                                             && x.ParameterType.GetType() == Scene.GetParameterTypeFromParameterShortName(parameterTypeShortName));
            }

            if (parameterBase is not null)
            {
                if(states.Count > 0)
                {
                    foreach (var actualFiniteState in states)
                    {
                        valueSet = parameterBase.QueryParameterBaseValueSet(selectedOption, actualFiniteState);
                        if (valueSet is not null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    valueSet = parameterBase.QueryParameterBaseValueSet(selectedOption, null);
                }
            }

            return valueSet;
        }

        /// <summary>
        /// Set the dimensions of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> used for the dimensioning</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">the <see cref="ActualFiniteState"/> that are going to be used to dimensioning the <see cref="BasicPrimitive"/></param>
        public abstract void SetDimensionsFromElementUsageParameters(ElementUsage elementUsage, Option selectedOption, List<ActualFiniteState> states);
    }
}
