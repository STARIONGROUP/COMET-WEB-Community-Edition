// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Primitive.cs" company="RHEA System S.A.">
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
    using System.Numerics;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.Utilities;
    
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an <see cref="CDP4Common.EngineeringModelData.ElementUsage"/> on the Scene from the selected <see cref="Option"/> and <see cref="ActualFiniteState"/>
    /// </summary>
    public abstract class Primitive
    {        
        /// <summary>
        /// Rendering group of this <see cref="Primitive"/>. Default is 0. Valid Range[0,4].
        /// </summary>
        public int RenderingGroup { get; set; } 

        /// <summary>
        /// The <see cref="ElementUsage"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public ElementUsage ElementUsage { get; set; } = default!;

        /// <summary>
        /// The <see cref="Option"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public Option SelectedOption { get; set; } = default!;

        /// <summary>
        /// The <see cref="ActualFiniteState"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public List<ActualFiniteState> States { get; set; } = default!;

        /// <summary>
        /// The default color if the <see cref="Color"/> has not been defined.
        /// </summary>
        public static Vector3 DefaultColor { get; } = new Vector3(210, 210, 210);

        /// <summary>
        /// Property that defined the exact type of pritimive. Used in JS.
        /// </summary>
        public abstract string Type { get; protected set; }

        /// <summary>
        /// ID of the property. Used to identify the primitive between the interop C#-JS
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets if the <see cref="Primitive"/> is selected or not
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="Primitive"/> is visible or not
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// The base color of the primitive
        /// </summary>
        public Vector3 Color { get; set; }

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
        public double RX { get; set; }

        /// <summary>
        /// Angle of rotation (radians) around Y axis.
        /// </summary>
        public double RY { get; set; }

        /// <summary>
        /// Angle of rotation (radians) around Z axis.
        /// </summary>
        public double RZ { get; set; }

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
        /// Sets the color of this <see cref="Primitive"/>.
        /// </summary>
        /// <param name="r">red component of the color in range [0,255]</param>
        /// <param name="g">green component of the color in range [0,255]</param>
        /// <param name="b">blue component of the color in range [0,255]</param>
        /// <returns></returns>
        public void SetColor(float r, float g, float b)
        {
            this.Color = new Vector3(r, g, b);
        }

        /// <summary>
        /// Gets info of the entity that can be used to show the user
        /// </summary>
        /// <returns>A string containing the info</returns>
        public virtual string GetInfo()
        {
            return "Type: " + this.Type.ToString();
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <returns>A collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/></returns>
        public Dictionary<ParameterBase, IValueSet> GetValueSets()
        {
            var collection = new Dictionary<ParameterBase, IValueSet>();
            var parameters = this.ElementUsage.GetParametersInUse();
            IValueSet? valueSet = null;

            foreach(var parameter in parameters)
            {
                valueSet = parameter.GetValueSetFromOptionAndStates(this.SelectedOption, this.States);
                                
                if(valueSet is not null)
                {
                    collection.Add(parameter, valueSet);
                }
            }

            return collection;
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterBase">the parameter asociated to the value set</param>
        /// <returns>A value set if exists, null otherwise</returns>
        protected IValueSet? GetValueSet(ParameterBase parameterBase)
        {
            var collection = this.GetValueSets();

            if (collection.ContainsKey(parameterBase))
            {
                return collection[parameterBase];
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterTypeShortName">the short name of the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> asociated to the <see cref="ParameterBase"/></param>
        /// <returns>A value set if exists, null otherwise</returns>
        protected IValueSet? GetValueSet(string parameterTypeShortName)
        {
            var parameters = this.ElementUsage.GetParametersInUse();
            var parameter = parameters.FirstOrDefault(x => x.ParameterType.ShortName == parameterTypeShortName, null);

            if(parameter is not null)
            {
                return this.GetValueSet(parameter);
            }

            return null;
        }

        /// <summary>
        /// Get the <see cref="ParameterBase"/> that translates this <see cref="Primitive"/>
        /// </summary>
        /// <returns>the related parameter</returns>
        public ParameterBase? GetTranslationParameter()
        {
            var param = this.ElementUsage.GetParametersInUse();
            return param.FirstOrDefault(x => x.ParameterType.ShortName == SceneSettings.PositionShortName);
        }

        /// <summary>
        /// Get the <see cref="ParameterBase"/> that orients this <see cref="Primitive"/>
        /// </summary>
        /// <returns>the related parameter</returns>
        public ParameterBase? GetOrientationParameter()
        {
            var param = this.ElementUsage.GetParametersInUse();
            return param.FirstOrDefault(x => x.ParameterType.ShortName == SceneSettings.OrientationShortName);
        }

        /// <summary>
        /// Set the color of the <see cref="Primitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public void SetColorFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneSettings.ColorShortName);

            if(valueSet is not null)
            {
                string textColor = valueSet.ActualValue.First();
                this.Color = textColor.ParseToColorVector();
            }
            else
            {
                this.Color = Primitive.DefaultColor;
            }
        }

        /// <summary>
        /// Set the position of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public void SetPositionFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneSettings.PositionShortName);
            var translation = valueSet.ParseIValueToPosition();
            if(translation is not null)
            {
                this.X = translation[0];
                this.Y = translation[1];
                this.Z = translation[2];
            }
        }

        /// <summary>
        /// Set the orientation of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public void SetOrientationFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneSettings.OrientationShortName);
            var orientation = valueSet.ParseIValueToOrientation(Enumerations.AngleFormat.Radians);
            if(orientation is not null)
            {
                this.RX = orientation.X;
                this.RY = orientation.Y;
                this.RZ = orientation.Z;
            }
        }

        /// <summary>
        /// Creates a clone of this <see cref="Primitive"/>
        /// </summary>
        /// <returns></returns>
        public Primitive Clone()
        {
            var shapeFactory = new ShapeFactory();
            return shapeFactory.CreatePrimitiveFromElementUsage(this.ElementUsage, this.SelectedOption, this.States)!;
        }

        /// <summary>
        /// Updates a property of the <see cref="Primitive"/> with the data of the <see cref="IValueSet"/>
        /// </summary>
        /// <param name="parameterTypeShortName">the short name for the parameter type that needs an update</param>
        /// <param name="newValue">the new value set</param>
        public virtual void UpdatePropertyWithParameterData(string parameterTypeShortName, IValueSet newValue)
        {
            switch (parameterTypeShortName)
            {
                case SceneSettings.ColorShortName:
                    string textColor = newValue.ActualValue.First();
                    this.Color = textColor.ParseToColorVector();
                    break;

                case SceneSettings.PositionShortName:
                    var translation = newValue.ParseIValueToPosition();
                    this.X = translation[0];
                    this.Y = translation[1];
                    this.Z = translation[2];
                    break;

                case SceneSettings.OrientationShortName:
                    var orientation = newValue.ParseIValueToOrientation(Enumerations.AngleFormat.Radians);
                    if (orientation is not null)
                    {
                        this.RX = orientation.X;
                        this.RY = orientation.Y;
                        this.RZ = orientation.Z;
                    }
                    break;
            }
        }

        /// <summary>
        /// Set the dimensions of the <see cref="BasicPrimitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public virtual void SetDimensionsFromElementUsageParameters() { }
    }
}
