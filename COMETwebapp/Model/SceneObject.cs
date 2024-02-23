// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SceneObject.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace COMETwebapp.Model
{
    using CDP4Common.EngineeringModelData;
    using COMETwebapp.Extensions;
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Utilities;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents a object in the 3D Scene
    /// </summary>
    public class SceneObject
    {
        /// <summary>
        /// Collection that handles the relation between a primitive and the necessary parameters for creating that primitive.
        /// </summary>
        private readonly Dictionary<Type, List<string>> shapeAndParametersRelation = new()
        {
            { typeof(Cone), new List<string> { SceneSettings.DiameterShortName, SceneSettings.HeightShortName } },
            { typeof(Cube), new List<string> { SceneSettings.WidthShortName, SceneSettings.HeightShortName, SceneSettings.LengthShortName } },
            { typeof(Cylinder), new List<string> { SceneSettings.DiameterShortName, SceneSettings.HeightShortName } },
            { typeof(Disc), new List<string> { SceneSettings.DiameterShortName } },
            { typeof(EquilateralTriangle), new List<string> { SceneSettings.DiameterShortName } },
            { typeof(HexagonalPrism), new List<string> { SceneSettings.DiameterShortName, SceneSettings.HeightShortName } },
            { typeof(Rectangle), new List<string> { SceneSettings.WidthShortName, SceneSettings.HeightShortName } },
            { typeof(Sphere), new List<string> { SceneSettings.DiameterShortName } },
            { typeof(Torus), new List<string> { SceneSettings.DiameterShortName, SceneSettings.ThicknessShortName } },
            { typeof(TriangularPrism), new List<string> { SceneSettings.DiameterShortName, SceneSettings.HeightShortName } }
        };

        /// <summary>
        /// Creates a new instance of type <see cref="SceneObject" />
        /// </summary>
        private SceneObject()
        {
        }

        /// <summary>
        /// Creates a new empty instance of type <see cref="SceneObject" />. Used only for testing.
        /// </summary>
        /// <param name="primitive">the primitive that contains</param>
        public SceneObject(Primitive primitive)
        {
            this.Primitive = primitive;
        }

        /// <summary>
        /// Gets the ID of the <see cref="SceneObject" />. Used to identify the <see cref="SceneObject" /> between JS and C# interop.
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the <see cref="Viewer.Primitives.Primitive" /> related to this <see cref="SceneObject" />.
        /// Can be NULL is a <see cref="SceneObject" /> don't have a <see cref="Primitive" /> asociated
        /// </summary>
        public Primitive Primitive { get; private set; }

        /// <summary>
        /// Gets or sets if this SceneObject is a clone of other <see cref="SceneObject" />
        /// </summary>
        public bool IsClone { get; private init; }

        /// <summary>
        /// Gets or sets if the <see cref="Primitive" /> have enough parameters to be created.
        /// </summary>
        public bool PrimitiveCanBeCreated { get; private set; }

        /// <summary>
        /// Gets or sets the element usage that contains the data for creating the <see cref="Primitive" />
        /// </summary>
        [JsonIgnore]
        public ElementBase ElementBase { get; private init; }

        /// <summary>
        /// Gets or sets the selected option for this <see cref="SceneObject" />
        /// </summary>
        [JsonIgnore]
        public Option Option { get; private init; }

        /// <summary>
        /// Gets or sets the possible actual finite states for this <see cref="SceneObject" />
        /// </summary>
        [JsonIgnore]
        public List<ActualFiniteState> States { get; private init; }

        /// <summary>
        /// Gets or sets the asociated <see cref="ParameterBase" /> of the <see cref="ElementBase" />
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<ParameterBase> ParametersAsociated => this.QueryParametersAssociated();

        /// <summary>
        /// Creates a new full <see cref="SceneObject" />. Used for drawing normal scene objects in scene.
        /// </summary>
        /// <param name="elementBase">
        /// the <see cref="ElementBase" /> that contains the data for creating the
        /// <see cref="Primitive" />
        /// </param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns>the <see cref="SceneObject" /></returns>
        public static SceneObject Create(ElementBase elementBase, Option option, List<ActualFiniteState> states)
        {
            var sceneObj = new SceneObject { ElementBase = elementBase, Option = option, States = states };
            var shapeKindParameter = sceneObj.ParametersAsociated.FirstOrDefault(x => x.ParameterType.ShortName == SceneSettings.ShapeKindShortName);

            if (shapeKindParameter is not null)
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
        /// Checks if the <see cref="SceneObject" /> contains the necessary parameters to create the <see cref="Primitive" />
        /// </summary>
        public void CheckIfPrimitiveCanBeCreatedWithAvailableParameters()
        {
            if (this.Primitive is not null && this.shapeAndParametersRelation.TryGetValue(this.Primitive.GetType(), out var namesOfNeededParameters))
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
        /// Parses the value of the <see cref="ParameterBase" /> into the corresponding property of this <see cref="SceneObject" />
        /// </summary>
        /// <param name="parameterBase">the parameter used for the parse</param>
        public void ParseParameter(ParameterBase parameterBase)
        {
            var valueSet = parameterBase.GetValueSetFromOptionAndStates(this.Option, this.States);
            this.UpdateParameter(parameterBase, valueSet);
        }

        /// <summary>
        /// Updates the properties of <see cref="SceneObject" /> that are related to the <paramref name="parameterBase" />
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
        /// Gets the value sets for this <see cref="SceneObject" />
        /// </summary>
        /// <returns>a collection of <see cref="ParameterBase" /> and its related <see cref="IValueSet" /></returns>
        public Dictionary<ParameterBase, IValueSet> GetParameterValueSetRelations()
        {
            var collection = new Dictionary<ParameterBase, IValueSet>();

            if (this.ParametersAsociated is null)
            {
                return new Dictionary<ParameterBase, IValueSet>();
            }

            foreach (var parameter in this.ParametersAsociated)
            {
                var valueSet = parameter.GetValueSetFromOptionAndStates(this.Option, this.States);

                if (valueSet is not null)
                {
                    collection.Add(parameter, valueSet);
                }
            }

            return collection;
        }

        /// <summary>
        /// Sets the current <see cref="Primitive" /> for this <see cref="SceneObject" />
        /// </summary>
        /// <param name="primitive">the new primitive</param>
        public void SetPrimitive(Primitive primitive)
        {
            if (primitive != null && this.ParametersAsociated != null)
            {
                var restOfParameters = this.ParametersAsociated.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);

                foreach (var parameter in restOfParameters)
                {
                    this.ParseParameter(parameter);
                }
            }
        }

        /// <summary>
        /// Creates a clone of this <see cref="SceneObject" />
        /// </summary>
        /// <returns>the clone</returns>
        public SceneObject Clone()
        {
            return new SceneObject
            {
                ElementBase = this.ElementBase,
                Option = this.Option,
                States = this.States,
                IsClone = true,
                PrimitiveCanBeCreated = this.PrimitiveCanBeCreated,
                Primitive = this.Primitive
            };
        }

        /// <summary>
        /// Query the associated <see cref="ParameterBase" />
        /// </summary>
        /// <returns>An <see cref="IReadOnlyList{T}" /> of associated <see cref="ParameterBase" /></returns>
        private IReadOnlyList<ParameterBase> QueryParametersAssociated()
        {
            return this.ElementBase.GetParametersInUse().ToList();
        }
    }
}
