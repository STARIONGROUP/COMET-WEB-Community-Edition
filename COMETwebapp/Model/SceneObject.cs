// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SceneObject.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace COMETwebapp.Model
{
    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Primitives;
    using COMETwebapp.Utilities;
    
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a object in the 3D Scene
    /// </summary>
    public class SceneObject
    {
        /// <summary>
        /// Backing field for the <see cref="Primitive"/> property
        /// </summary>
        private Primitive primitive;

        /// <summary>
        /// Gets the ID of the <see cref="SceneObject"/>. Used to identify the <see cref="SceneObject"/> between JS and C# interop.
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the <see cref="Primitives.Primitive"/> related to this <see cref="SceneObject"/>
        /// </summary>
        public Primitive? Primitive
        {
            get => this.primitive;
            private set
            {
                this.primitive = value;
                //TODO: Use the current selected SceneObj so the rest of the parameters are updated. If not uses the data in the server!
                if(this.primitive != null && this.ParametersAsociated != null)
                {
                    var restOfParameters = this.ParametersAsociated.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);

                    foreach (var parameter in restOfParameters)
                    {
                        this.ParseParameter(parameter);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets if the <see cref="Primitive"/> have enough parameters to be created.
        /// </summary>
        public bool PrimitiveCanBeCreated { get; private set; }

        /// <summary>
        /// Gets or sets the element usage that contains the data for creating the <see cref="Primitive"/>
        /// </summary>
        [JsonIgnore]
        public ElementBase ElementBase { get; private set; }

        /// <summary>
        /// Gets or sets the selected option for this <see cref="SceneObject"/>
        /// </summary>
        [JsonIgnore]
        public Option Option { get; private set; }

        /// <summary>
        /// Gets or sets the possible actual finite states for this <see cref="SceneObjects"/>
        /// </summary>
        [JsonIgnore]
        public List<ActualFiniteState> States { get; private set; }

        /// <summary>
        /// Gets or sets the asociated <see cref="ParameterBase"/> of the <see cref="ElementBase"/>
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<ParameterBase> ParametersAsociated
        {
            get
            {
                if(this.ElementBase is null)
                {
                    return null;
                }
                return this.ElementBase.GetParametersInUse().ToList();
            }
        }

        /// <summary>
        /// Collection that handles the relation between a primitive and the necessary parameters for creating that primitive.
        /// </summary>
        private Dictionary<Type, List<string>> ShapeAndParametersRelation = new Dictionary<Type, List<string>>()
        {
            { typeof(Cone), new List<string>(){ SceneSettings.DiameterShortName, SceneSettings. HeightShortName } },
            { typeof(Cube), new List<string>(){ SceneSettings.WidthShortName, SceneSettings.HeightShortName, SceneSettings.LengthShortName } },
            { typeof(Cylinder), new List<string>(){ SceneSettings.DiameterShortName, SceneSettings.HeightShortName } },
            { typeof(Disc), new List<string>(){ SceneSettings.DiameterShortName } },
            { typeof(EquilateralTriangle), new List<string>(){ SceneSettings.DiameterShortName } },
            { typeof(HexagonalPrism), new List<string>(){ SceneSettings.DiameterShortName, SceneSettings.HeightShortName } },
            { typeof(Rectangle), new List<string>(){ SceneSettings.WidthShortName, SceneSettings.HeightShortName } },
            { typeof(Sphere), new List<string>(){ SceneSettings.DiameterShortName } },
            { typeof(Torus), new List<string>(){ SceneSettings.DiameterShortName, SceneSettings.ThicknessShortName} },
            { typeof(TriangularPrism), new List<string>(){ SceneSettings.DiameterShortName, SceneSettings.HeightShortName } },
        };

        /// <summary>
        /// Creates a new instance of type <see cref="SceneObject"/>
        /// </summary>
        private SceneObject() { }

        /// <summary>
        /// Creates a new empty instance of type <see cref="SceneObject"/>. Used only for testing. 
        /// </summary>
        /// <param name="primitive">the primitive that contains</param>
        public SceneObject(Primitive? primitive)
        {
            this.Primitive = primitive;
        }

        /// <summary>
        /// Creates a new full <see cref="SceneObject"/>. Used for drawing normal scene objects in scene.
        /// </summary>
        /// <param name="elementBase">the <see cref="ElementBase"/> that contains the data for creating the <see cref="Primitive"/></param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns></returns>
        public static SceneObject Create(ElementBase elementBase, Option option, List<ActualFiniteState> states)
        {
            var sceneObj = new SceneObject() { ElementBase = elementBase, Option = option, States = states };

            //TODO: this needs a review of how to do it properly.
            var shapeKindParameter = sceneObj.ParametersAsociated.FirstOrDefault(x => x.ParameterType.ShortName == SceneSettings.ShapeKindShortName);
            if(shapeKindParameter is not null)
            {
                sceneObj.ParseParameter(shapeKindParameter);

                var restOfParameters = sceneObj.ParametersAsociated.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);

                foreach (var parameter in restOfParameters)
                {
                    sceneObj.ParseParameter(parameter);
                }

                sceneObj.CheckIfPrimitiveCanBeCreatedWithAvailableParameters();
            }

            return sceneObj;
        }

        /// <summary>
        /// Checks if the <see cref="SceneObject"/> contains the necessary parameters to create the <see cref="Primitive"/>
        /// </summary>
        public void CheckIfPrimitiveCanBeCreatedWithAvailableParameters()
        {
            if (this.Primitive is not null && this.ShapeAndParametersRelation.TryGetValue(this.Primitive.GetType(), out var namesOfNeededParameters))
            {
                var namesOfActualParameters = this.ParametersAsociated.Select(x => x.ParameterType.ShortName).ToList();
                this.PrimitiveCanBeCreated = true;

                foreach (var name in namesOfNeededParameters)
                {
                    if (!namesOfActualParameters.Contains(name))
                    {
                        this.PrimitiveCanBeCreated = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Parses the value of the <see cref="ParameterBase"/> into the corresponding property of this <see cref="SceneObject"/>
        /// </summary>
        /// <param name="parameterBase">the parameter used for the parse</param>
        public void ParseParameter(ParameterBase parameterBase)
        {
            var valueSet = parameterBase.GetValueSetFromOptionAndStates(this.Option, this.States);
            UpdateParameter(parameterBase, valueSet);
        }

        /// <summary>
        /// Updates the properties of <see cref="SceneObject"/> that are related to the <paramref name="parameterBase"/>
        /// </summary>
        /// <param name="parameterBase">the parameter base used for updating the values</param>
        /// <param name="valueSet">the new values set used to update</param>
        public void UpdateParameter(ParameterBase parameterBase, IValueSet valueSet)
        {
            var parameterTypeShortName = parameterBase.ParameterType.ShortName;

            switch (parameterTypeShortName)
            {
                case SceneSettings.ShapeKindShortName:
                    this.Primitive = ParameterParser.ShapeKindParser(valueSet);
                    break;
                default:
                    if (this.Primitive is not null)
                    {
                        this.Primitive.ParseParameter(parameterBase, valueSet);
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the value sets for this <see cref="SceneObject"/>
        /// </summary>
        /// <returns>a collection of <see cref="ParameterBase"/> and its related <see cref="IValueSet"/></returns>
        public Dictionary<ParameterBase,IValueSet> GetValueSets()
        {
            var collection = new Dictionary<ParameterBase, IValueSet>();
            IValueSet? valueSet = null;

            foreach (var parameter in this.ParametersAsociated)
            {
                valueSet = parameter.GetValueSetFromOptionAndStates(this.Option, this.States);

                if (valueSet is not null)
                {
                    collection.Add(parameter, valueSet);
                }
            }

            return collection;
        }

        /// <summary>
        /// Creates a clone of this <see cref="SceneObject"/>
        /// </summary>
        /// <returns>the clone</returns>
        public SceneObject Clone()
        {
            return Create(this.ElementBase, this.Option, this.States);
        }
    }
}
