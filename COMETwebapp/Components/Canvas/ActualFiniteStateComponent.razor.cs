﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateComponent.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.Canvas
{
    using CDP4Common.EngineeringModelData;
    using COMETwebapp.Components.Canvas;
    using Microsoft.AspNetCore.Components;
    using System.Threading.Tasks;

    /// <summary>
    /// partial class for the <see cref="ActualFiniteStateComponent"/>
    /// </summary>
    public partial class ActualFiniteStateComponent
    {
        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteState"/> that this component represents
        /// </summary>
        [Parameter]
        public ActualFiniteState ActualFiniteState { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteStateListComponent"/> parent of this component
        /// </summary>
        [Parameter]
        public ActualFiniteStateListComponent ActualFiniteStateListComponent { get; set; }

        /// <summary>
        /// Event when a <see cref="ActualFiniteStateComponent"/> it's been clicked
        /// </summary>
        /// <param name="actualFiniteState">the actual finite state clicked</param>
        /// <returns></returns>
        public async Task OnClick(ActualFiniteState actualFiniteState)
        {
            await this.ActualFiniteStateListComponent.SetSelectedState(actualFiniteState);
        }
    }
}