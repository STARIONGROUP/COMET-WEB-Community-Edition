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
        /// The element usage the primitive was created from
        /// </summary>
        [JsonIgnore]
        public ElementUsage ElementUsage { get; set; }

        /// <summary>
        /// If the primitive is selected or not.
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
        /// If the primitive is visible or not
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
        public Vector3 Color { get; set; }

        /// <summary>
        /// Property that defined the exact type of pritimive. Used in JS.
        /// </summary>
        public abstract string Type { get; protected set; }

        /// <summary>
        /// ID of the property. Used to identify the primitive between the interop C#-JS
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

        /// <summary>
        /// Sets the diffuse color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveDiffuseColor(Vector3 color)
        {
            await JSInterop.Invoke("SetDiffuseColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Sets the specular color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveSpecularColor(Vector3 color)
        {
            await JSInterop.Invoke("SetSpecularColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Sets the emissive color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveEmissiveColor(Vector3 color)
        {
            await JSInterop.Invoke("SetEmissiveColor", this.ID, color.X, color.Y, color.Z);
        }

        /// <summary>
        /// Sets the ambient color of this primitive.
        /// </summary>
        /// <param name="color">The color in rgb format with values range [0,1]</param>
        public async Task SetPrimitiveAmbientColor(Vector3 color)
        {
            await JSInterop.Invoke("SetAmbientColor", this.ID, color.X, color.Y, color.Z);
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
        /// Gets the value sets asociated to an element usage depending on the selected option and the available states
        /// </summary>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> used for query the value set</param>
        /// <param name="selectedOption">the current <see cref="Option"/> selected</param>
        /// <param name="states">The available states</param>
        /// <returns></returns>
        protected IValueSet? GetElementUsageValueSet(Option selectedOption, List<ActualFiniteState> states, string parameterTypeShortName)
        {
            ParameterBase? parameterBase = null;
            IValueSet? valueSet = null;
            Type parameterType = SceneProvider.ParameterShortNameToTypeDictionary[parameterTypeShortName];

            if (this.ElementUsage.ParameterOverride.Count > 0)
            {
                parameterBase = this.ElementUsage.ParameterOverride.FirstOrDefault(x => x.ParameterType.ShortName == parameterTypeShortName
                                                                                   && x.ParameterType.GetType() == parameterType);
            }

            if (parameterBase is null)
            {
                parameterBase = this.ElementUsage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.ShortName == parameterTypeShortName
                                                                                             && x.ParameterType.GetType() == parameterType);
            }

            if (parameterBase is not null)
            {
                if (states.Count > 0)
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
    }
}
