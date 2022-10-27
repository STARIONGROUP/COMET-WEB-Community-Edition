// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveStateData.cs" company="RHEA System S.A.">
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
namespace COMETwebapp.Model
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Class that contains the data of a filter of type <see cref="ActualFiniteStateList"/>
    /// </summary>
    public class ActualFiniteStateListFilterData
    {
        /// <summary>
        /// If the filter is active or not
        /// </summary>
        public bool IsFilterActive { get; set; }

        /// <summary>
        /// The <see cref="ActualFiniteState"/> that is active inside the <see cref="ActualFiniteStateList"/>
        /// </summary>
        public ActualFiniteState ActiveState { get; set; }

        /// <summary>
        /// The default <see cref="ActualFiniteState"/> that needs to be used when <see cref="IsFilterActive"/> is false
        /// </summary>
        public ActualFiniteState DefaultState { get;}

        /// <summary>
        /// Creates a new instance of type <see cref="ActualFiniteStateListFilterData"/>
        /// </summary>
        /// <param name="defaultState">the <see cref="ActualFiniteState"/> used for initializing the <see cref="DefaultState"/> and the <see cref="ActiveState"/></param>
        public ActualFiniteStateListFilterData(ActualFiniteState defaultState)
        {
            this.DefaultState = defaultState;
            this.ActiveState = defaultState;
        }

        /// <summary>
        /// Gets the state that needs to be used.
        /// </summary>
        /// <returns>the <see cref="ActiveState"/> if <see cref="IsFilterActive"/> is true, the <see cref="DefaultState"/> otherwise</returns>
        public ActualFiniteState GetStateToUse()
        {
            return IsFilterActive ? ActiveState : DefaultState;
        }
    }
}
