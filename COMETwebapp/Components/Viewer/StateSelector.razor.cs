// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateSelector.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.Viewer
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Partial class for the <see cref="StateSelector"/> component
    /// </summary>
    public partial class StateSelector
    {
        /// <summary>
        /// List of the of <see cref="ActualFiniteStateList"/>
        /// </summary>
        [Parameter]
        public List<ActualFiniteStateList> ListActualFiniteStateLists { get; set; }

        /// <summary>
        /// Event for when an <see cref="ActualFiniteStateList"/> checked state has changed
        /// </summary>
        [Parameter]
        public EventCallback<CustomChangeEventArgs> OnActualFiniteStateList_SelectionChanged { get; set; }

        /// <summary>
        /// Event for when an <see cref="ActualFiniteState"/> checked state has changed
        /// </summary>
        [Parameter]
        public EventCallback<CustomChangeEventArgs> OnActualFiniteState_SelectionChanged { get; set; }

        /// <summary>
        /// Method to invoke the <see cref="OnActualFiniteStateList_SelectionChanged"/> event
        /// </summary>
        private void ActualFiniteStateList_SelectionChanged(ActualFiniteStateList actualFiniteStateList, ChangeEventArgs e)
        {
            this.OnActualFiniteStateList_SelectionChanged.InvokeAsync(new CustomChangeEventArgs(actualFiniteStateList, e.Value));
        }

        /// <summary>
        /// Method to invoke the <see cref="OnActualFiniteState_SelectionChanged"/> event
        /// </summary>
        private void ActualFiniteState_SelectionChanged(ActualFiniteState actualFiniteState, ChangeEventArgs e)
        {
            this.OnActualFiniteState_SelectionChanged.InvokeAsync(new CustomChangeEventArgs(actualFiniteState, e.Value));
        }
    }
}
