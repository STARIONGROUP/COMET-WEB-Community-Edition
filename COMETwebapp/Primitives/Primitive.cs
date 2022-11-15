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
    using COMETwebapp.Utilities;
    
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an <see cref="CDP4Common.EngineeringModelData.ElementUsage"/> on the Scene from the selected <see cref="Option"/> and <see cref="ActualFiniteState"/>
    /// </summary>
    public abstract class Primitive
    {
        /// <summary>
        /// Backing field for the property <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected = false;

        /// <summary>
        /// Backing field for the property <see cref="IsVisible"/>
        /// </summary>
        private bool isVisible = true;

        /// <summary>
        /// The <see cref="ElementUsage"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public ElementUsage ElementUsage { get; set; }

        /// <summary>
        /// The <see cref="Option"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public Option SelectedOption { get; set; }

        /// <summary>
        /// The <see cref="ActualFiniteState"/> for which the <see cref="Primitive"/> was created.
        /// </summary>
        [JsonIgnore]
        public List<ActualFiniteState> States { get; set; }

        /// <summary>
        /// The default color if the <see cref="Color"/> has not been defined.
        /// </summary>
        public static Vector3 DefaultColor { get; } = new Vector3(210, 210, 210);

        /// <summary>
        /// Gets or sets if the <see cref="Primitive"/> is selected or not
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                JSInterop.Invoke("SetSelection", this.ID, value);
            }
        }

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
        /// The base color of the primitive
        /// </summary>
        public Vector3 Color { get; private set; }

        /// <summary>
        /// Property that defined the exact type of pritimive. Used in JS.
        /// </summary>
        public abstract string Type { get; protected set; }

        /// <summary>
        /// ID of the property. Used to identify the primitive between the interop C#-JS
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

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
        /// Gets a list of the parameters that this <see cref="Primitive"/> contains
        /// </summary>
        /// <returns></returns>
        public List<ParameterBase> GetParameters()
        {
            var parameters = new List<ParameterBase>();
    
            parameters.AddRange(this.ElementUsage.ParameterOverride);

            this.ElementUsage.ElementDefinition.Parameter.ForEach(x =>
            {
                if(!parameters.Any(par=>par.ParameterType.ShortName == x.ParameterType.ShortName))
                {
                    parameters.Add(x);
                }
            });
                        
            return parameters.OrderBy(x=>x.ParameterType.ShortName).ToList();
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
            }
        }
    }
}
