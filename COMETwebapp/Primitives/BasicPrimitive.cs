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

    using COMETwebapp.Components.Viewer;

    using COMETwebapp.Utilities;

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
        public double X { get; protected set; }

        /// <summary>
        /// Position along the Y axis
        /// </summary>
        public double Y { get; protected set; }

        /// <summary>
        /// Position along the Z axis
        /// </summary>
        public double Z { get; protected set; }

        /// <summary>
        /// Angle of rotation (radians) around X axis.
        /// </summary>
        public double RX { get; protected set; }

        /// <summary>
        /// Angle of rotation (radians) around Y axis.
        /// </summary>
        public double RY { get; protected set; }

        /// <summary>
        /// Angle of rotation (radians) around Z axis.
        /// </summary>
        public double RZ { get; protected set; }

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
            JSInterop.Invoke("SetPrimitivePosition", this.ID, this.X, this.Y, this.Z);
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
            JSInterop.Invoke("SetPrimitiveRotation", this.ID, this.RX, this.RY, this.RZ);
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
        public void SetPositionFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneProvider.PositionShortName);
            var translation = valueSet.ParseIValueToPosition();
            this.SetTranslation(translation[0], translation[1], translation[2]);        
        }

        /// <summary>
        /// Set the orientation of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public void SetOrientationFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneProvider.OrientationShortName);
            var angles = valueSet.ParseIValueToEulerAngles();
            this.SetRotation(angles[0], angles[1], angles[2]);
        }

        /// <summary>
        /// Get the <see cref="ParameterBase"/> that translates this <see cref="Primitive"/>
        /// </summary>
        /// <returns>the related parameter</returns>
        public ParameterBase? GetTranslationParameter()
        {
            var param = this.ElementUsage.GetParametersInUse();
            return param.FirstOrDefault(x => x.ParameterType.ShortName == SceneProvider.PositionShortName);
        }

        /// <summary>
        /// Get the <see cref="ParameterBase"/> that orients this <see cref="Primitive"/>
        /// </summary>
        /// <returns>the related parameter</returns>
        public ParameterBase? GetOrientationParameter()
        {
            var param = this.ElementUsage.GetParametersInUse();
            return param.FirstOrDefault(x => x.ParameterType.ShortName == SceneProvider.OrientationShortName);
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
                case SceneProvider.PositionShortName:
                    var translation = newValue.ParseIValueToPosition();
                    this.SetTranslation(translation[0], translation[1], translation[2]);
                    break;

                case SceneProvider.OrientationShortName:
                    var angles = newValue.ParseIValueToEulerAngles();
                    this.SetRotation(angles[0], angles[1], angles[2]);
                    break;

            }
        }

        /// <summary>
        /// Set the dimensions of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public abstract void SetDimensionsFromElementUsageParameters();
    }
}
