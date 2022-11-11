// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShapeFactory.cs" company="RHEA System S.A.">
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
    /// Factory for creating different basic shapes of type <see cref="Primitive"/>
    /// </summary>
    public interface IShapeFactory
    {
        /// <summary>
        /// Tries to create a <see cref="Primitive"/> from the data of a <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> used for creating a <see cref="Primitive"/></param>
        /// <param name="selectedOption">The current <see cref="Option"/> selected</param>
        /// <param name="states">The list of <see cref="ActualFiniteState"/> that are active</param>
        /// <returns>The created <see cref="Primitive"/></returns>
        Primitive? CreatePrimitiveFromElementUsage(ElementUsage elementUsage, Option selectedOption, List<ActualFiniteState> states);
    }
}
