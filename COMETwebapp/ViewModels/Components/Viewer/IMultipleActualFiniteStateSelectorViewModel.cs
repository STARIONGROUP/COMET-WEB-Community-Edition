﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMultipleActualFiniteStateSelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Viewer
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DynamicData;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.ActualFiniteStateSelector"/>
    /// </summary>
    public interface IMultipleActualFiniteStateSelectorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="ActualFiniteStateList"/>
        /// </summary>
        SourceList<ActualFiniteStateList> ActualFiniteStateListsCollection { get; }

        /// <summary>
        /// Gets or sets the view models for the child selectors
        /// </summary>
        IEnumerable<IActualFiniteStateSelectorViewModel> ActualFiniteStateSelectorViewModels { get; set; }

        /// <summary>
        /// Gets or sets the current selected states
        /// </summary>
        IEnumerable<ActualFiniteState> SelectedFiniteStates { get; }

        /// <summary>
        /// Initializes this viewmodel with the collection of <see cref="ActualFiniteStateList"/>
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Event for when a <see cref="ActualFiniteState"/> has been selected
        /// </summary>
        /// <param name="stateSelector">the selector that raised the event</param>
        void OnActualFiniteStateSelectionChanged(IActualFiniteStateSelectorViewModel stateSelector);
    }
}
