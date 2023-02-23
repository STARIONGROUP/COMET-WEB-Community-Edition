// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IActualFiniteStateSelectorViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer.Canvas
{
    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.ViewModels.Components.Shared.Selectors;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.Canvas.ActualFiniteStateSelector"/>
    /// </summary>
    public interface IActualFiniteStateSelectorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteStates"/> of this selector
        /// </summary>
        IEnumerable<ActualFiniteState> ActualFiniteStates { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState"/>
        /// </summary>
        ActualFiniteState SelectedFiniteState { get; set; }

        /// <summary>
        /// Selects the current state and triggers the corresponding event
        /// </summary>
        /// <param name="finiteState">the new selected finite state</param>
        void SelectActualFiniteState(ActualFiniteState finiteState);
    }
}
