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

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an <see cref="CDP4Common.EngineeringModelData.ElementUsage"/> on the Scene from the selected <see cref="Option"/> and <see cref="ActualFiniteState"/>
    /// </summary>
    public abstract class Primitive
    {        
        /// <summary>
        /// Backing field for the property <see cref="IsVisible"/>
        /// </summary>
        private bool isVisible = true;

        /// <summary>
        /// Backing field for the property <see cref="Alpha"/>
        /// </summary>
        private double alpha = 1;

        /// <summary>
        /// Rendering group of this <see cref="Primitive"/>. Default is 0. Range [0,4]
        /// </summary>
        public int RenderingGroup { get; set; } = 0;

        /// <summary>
        /// The <see cref="ElementUsage"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public ElementUsage? ElementUsage { get; set; } 

        /// <summary>
        /// The <see cref="Option"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public Option? SelectedOption { get; set; }

        /// <summary>
        /// The <see cref="ActualFiniteState"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public List<ActualFiniteState>? States { get; set; }

        /// <summary>
        /// The default color if the <see cref="Color"/> has not been defined.
        /// </summary>
        public static Vector3 DefaultColor { get; } = new Vector3(210, 210, 210);

        /// <summary>
        /// Gets or sets if the <see cref="Primitive"/> has an halo. When changed the <see cref="Primitive"/> needs a regeneration.
        /// </summary>
        public bool HasHalo { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="Primitive"/> is visible or not
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                JSInterop.Invoke("SetMeshVisibility", this.ID, value);
            }
        }

        /// <summary>
        /// Gets or sets the base color of the primitive
        /// </summary>
        public Vector3 Color { get; private set; }

        /// <summary>
        /// Gets or sets the alpha transparency for this primitive. Range [0,1]
        /// </summary>
        public double Alpha
        {
            get => alpha;
            set
            {
                alpha = Math.Clamp(value,0,1.0);
                JSInterop.Invoke("SetMaterialAlpha", this.ID, value);
            }
        }

        /// <summary>
        /// Property that defined the exact type of pritimive. Used in JS.
        /// </summary>
        public abstract string Type { get; protected set; }

        /// <summary>
        /// Gets ID of the property. Used to identify the primitive between the interop C#-JS
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();


        protected Dictionary<string, Action> Actions = new Dictionary<string, Action>();


        public Primitive()
        {
            this.Actions.Add(SceneProvider.ColorShortName, this.SetColorFromElementUsageParameters);
            this.Actions.Add(SceneProvider.TransparencyShortName, this.SetTransparencyFromElementUsageParameters);
        }

        public void FireAction(string parameterTypeShortName)
        {
            if (this.Actions.ContainsKey(parameterTypeShortName))
            {
                this.Actions[parameterTypeShortName].Invoke();
            }
        }

        /// <summary>
        /// Regenerates the <see cref="Primitive"/>. This updates the scene with the data of the the <see cref="Primitive"/>
        /// </summary>
        public void Regenerate()
        {
            string jsonPrimitive = JsonConvert.SerializeObject(this, Formatting.Indented);
            JSInterop.Invoke("RegenMesh", jsonPrimitive);
        }

        /// <summary>
        /// Sets the color of this <see cref="Primitive"/>.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,255]</param>
        public void SetColor(Vector3 color)
        {
            this.Color = color;
            JSInterop.Invoke("SetMeshColor", this.ID, color.X, color.Y, color.Z);
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
            this.SetColor(new Vector3(r, g, b));
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <returns>A collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/></returns>
        public Dictionary<ParameterBase, IValueSet> GetValueSets()
        {
            var collection = new Dictionary<ParameterBase, IValueSet>();
            var parameters = this.ElementUsage?.GetParametersInUse();
            IValueSet? valueSet = null;
            
            if(parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    valueSet = parameter.GetValueSetFromOptionAndStates(this.SelectedOption, this.States);

                    if (valueSet is not null)
                    {
                        collection.Add(parameter, valueSet);
                    }
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
            var parameters = this.ElementUsage?.GetParametersInUse();
            var parameter = parameters?.FirstOrDefault(x => x.ParameterType.ShortName == parameterTypeShortName, null);

            if(parameter is not null)
            {
                return this.GetValueSet(parameter);
            }

            return null;
        }

        /// <summary>
        /// Set the color of the <see cref="Primitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public void SetColorFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneProvider.ColorShortName);

            if(valueSet is not null)
            {
                string textColor = valueSet.ActualValue.First();
                Vector3 color = textColor.ParseToColorVector();
                this.SetColor(color);
            }
            else
            {
                this.SetColor(Primitive.DefaultColor);
            }
        }

        /// <summary>
        /// Set the alpha of the <see cref="Primitive"/> from the <see cref="ElementUsage"/> parameters
        /// </summary>
        public void SetTransparencyFromElementUsageParameters()
        {
            IValueSet? valueSet = this.GetValueSet(SceneProvider.TransparencyShortName);

            if (valueSet is not null)
            {
                string textTransparency = valueSet.ActualValue.First();
                if (double.TryParse(textTransparency, out double a))
                {
                    this.Alpha = a;
                }
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
                case SceneProvider.ColorShortName:
                    string textColor = newValue.ActualValue.First();
                    Vector3 color = textColor.ParseToColorVector();
                    this.SetColor(color);
                    break;

                case SceneProvider.TransparencyShortName:
                    string textTransparency = newValue.ActualValue.First();
                    if(double.TryParse(textTransparency, out double a))
                    {
                        this.Alpha = a;
                    }
                    break;
            }
        }
    }
}
