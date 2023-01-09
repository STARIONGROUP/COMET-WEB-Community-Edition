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
    using COMETwebapp.Primitives;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Represents a object in the 3D Scene
    /// </summary>
    public class SceneObject
    {
        /// <summary>
        /// Gets the ID of the <see cref="SceneObject"/>
        /// </summary>
        public Guid ID { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the <see cref="Primitives.Primitive"/> related to this <see cref="SceneObject"/>
        /// </summary>
        public Primitive? Primitive { get; set; }

        /// <summary>
        /// Gets or sets the factory used to create the <see cref="Primitive"/>
        /// </summary>
        [Inject]
        public IShapeFactory ShapeFactory { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="SceneObject"/>
        /// </summary>
        /// <param name="shapeFactory">the factory used</param>
        public SceneObject(IShapeFactory shapeFactory)
        {
            this.ShapeFactory = shapeFactory;
        }

        /// <summary>
        /// Creates the <see cref="Primitive"/> for this <see cref="SceneObject"/>
        /// </summary>
        /// <param name="elementUsage">the element usage with the data of the type of primitive</param>
        /// <param name="option">the current selected option</param>
        /// <param name="states">the states for this option</param>
        public void CreatePrimitive(ElementUsage elementUsage, Option option, IEnumerable<ActualFiniteState> states)
        {
            this.Primitive = this.ShapeFactory.CreatePrimitiveFromElementUsage(elementUsage, option, states.ToList());
        }
    }
}
