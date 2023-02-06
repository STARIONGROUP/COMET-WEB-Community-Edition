// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Extensions
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Static class for extension methods for <see cref="Iteration"/>
    /// </summary>
    public static class IterationExtensions
    {
        /// <summary>
        /// Gets the <see cref="ElementBase"/> from this iteration
        /// </summary>
        /// <param name="iteration">the iteration used for retrieving the elements</param>
        /// <returns>an <see cref="IEnumerable{ElementBase}"/></returns>
        /// <exception cref="ArgumentNullException">if the iteration is null</exception>
        public static IEnumerable<ElementBase> GetElementsOfIteration(this Iteration iteration)
        {
            if (iteration is null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var elements = new List<ElementBase>();

            if (iteration.TopElement is not null)
            {
                elements.Add(iteration.TopElement);
            }

            iteration.Element.ForEach(e => elements.AddRange(e.ContainedElement));

            return elements;
        }
    }
}
