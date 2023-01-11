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
    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.Primitives;
    using COMETwebapp.Utilities;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a object in the 3D Scene
    /// </summary>
    public class SceneObject
    {
        /// <summary>
        /// Gets or sets the <see cref="Primitives.Primitive"/> related to this <see cref="SceneObject"/>
        /// </summary>
        public Primitive? Primitive { get; set; }

        public bool PrimitiveCanBeCreated { get; private set; }
        private ElementUsage ElementUsage { get; set; }
        private Option Option { get; set; }
        private List<ActualFiniteState> States { get; set; }
        private IShapeFactory ShapeFactory { get; set; }
        public IReadOnlyList<ParameterBase> ParametersAsociated { get; set; } 

        /// <summary>
        /// Creates a new instance of type <see cref="SceneObject"/>
        /// </summary>
        private SceneObject() { }

        /// <summary>
        /// Creates a new empty instance of type <see cref="SceneObject"/>. Used only for drawing the temporary scene objects and testing. 
        /// </summary>
        /// <param name="primitive">the primitive that contains</param>
        public SceneObject(Primitive primitive)
        {
            this.Primitive = primitive;
        }

        /// <summary>
        /// Creates a new full <see cref="SceneObject"/>. Used for drawing normal scene objects in scene.
        /// </summary>
        /// <param name="shapeFactory">the factory used to create the primitives</param>
        /// <param name="elementUsage">the <see cref="ElementUsage"/> that contains the data for creating the <see cref="Primitive"/></param>
        /// <param name="option">the selected option</param>
        /// <param name="states">the possible actual finite states</param>
        /// <returns></returns>
        public static SceneObject Create(IShapeFactory shapeFactory, ElementUsage elementUsage, Option option, List<ActualFiniteState> states)
        {
            var sceneObj = new SceneObject() { ShapeFactory = shapeFactory, ElementUsage = elementUsage, Option = option, States = states };
            sceneObj.Primitive =  sceneObj.ShapeFactory.CreatePrimitiveFromElementUsage(elementUsage, option, states);
            sceneObj.ParametersAsociated = elementUsage.GetParametersInUse().ToList();
            sceneObj.CheckIfPrimitiveCanBeCreatedWithAvailableParameters();
            return sceneObj;
        }

        /// <summary>
        /// Checks if the <see cref="SceneObject"/> contains the necessary parameters to create the <see cref="Primitive"/>
        /// </summary>
        public void CheckIfPrimitiveCanBeCreatedWithAvailableParameters()
        {
            if(this.Primitive is not null)
            {
                if (this.ShapeAndParametersRelation.TryGetValue(this.Primitive.GetType(), out var namesOfNeededParameters))
                {
                    var namesOfActualParameters = this.ParametersAsociated.Select(x => x.ParameterType.ShortName).ToList();
                    this.PrimitiveCanBeCreated = true;

                    foreach(var name in namesOfNeededParameters)
                    {
                        if (!namesOfActualParameters.Contains(name))
                        {
                            this.PrimitiveCanBeCreated = false;
                            break;
                        }
                    }
                }
            }
        }

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
    }
}
