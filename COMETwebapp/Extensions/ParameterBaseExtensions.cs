﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseExtensions.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Extensions
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// static extension methods for <see cref="ParameterBase"/>
    /// </summary>
    public static class ParameterBaseExtensions
    {
        /// <summary>
        /// Gets a <see cref="IValueSet"/> for a <see cref="Option"/> and a <see cref="IEnumerable{T}"/> of <see cref="ActualFiniteState"/>
        /// </summary>
        /// <param name="parameterBase">the <see cref="ParameterBase"/> asociated to the <see cref="IValueSet"/></param>
        /// <param name="option">the <see cref="Option"/> for that <see cref="ParameterBase"/></param>
        /// <param name="states">the states for that <see cref="ParameterBase"/></param>
        /// <returns>the value set if exists, null otherwise</returns>
        public static IValueSet GetValueSetFromOptionAndStates(this ParameterBase parameterBase, Option option, IEnumerable<ActualFiniteState> states)
        {
            IValueSet valueSet = null;
            states = states?.ToList();

            if (states is not null && states.Any() && parameterBase.StateDependence != null)
            {
                var actualFiniteState = states.FirstOrDefault(x => parameterBase.StateDependence.ActualState.Any(a => x.Iid == a.Iid));
                valueSet = parameterBase.QueryParameterBaseValueSet(option, actualFiniteState);
            }
            else
            {
                valueSet = parameterBase.QueryParameterBaseValueSet(option, null);
            }
            
            return valueSet;
        }
    }
}
