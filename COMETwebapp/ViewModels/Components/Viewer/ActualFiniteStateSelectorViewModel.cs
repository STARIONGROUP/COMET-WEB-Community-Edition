﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ActualFiniteStateSelectorViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.ActualFiniteStateSelector"/>
    /// </summary>
    public class ActualFiniteStateSelectorViewModel : BelongsToIterationSelectorViewModel, IActualFiniteStateSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteStates"/> of this selector
        /// </summary>
        public IEnumerable<ActualFiniteState> ActualFiniteStates { get; set; }

        /// <summary>
        /// Backing field for the <see cref="SelectedFiniteState"/>
        /// </summary>
        private ActualFiniteState selectedFiniteState;

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState"/>
        /// </summary>
        public ActualFiniteState SelectedFiniteState
        {
            get => this.selectedFiniteState;
            set => this.RaiseAndSetIfChanged(ref this.selectedFiniteState, value);
        }

        /// <summary>
        /// Eventcallback for when an <see cref="ActualFiniteState"/> has been selected
        /// </summary>
        private EventCallback<ActualFiniteStateSelectorViewModel> OnActualFiniteStateSelected { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ActualFiniteStateSelectorViewModel"/>
        /// </summary>
        /// <param name="actualFiniteStateList">the <see cref="ActualFiniteStateList"/> that contains the states to select</param>
        /// <param name="onActualFiniteStateSelected">the event used to update the parent data</param>
        public ActualFiniteStateSelectorViewModel(ActualFiniteStateList actualFiniteStateList, 
            EventCallback<ActualFiniteStateSelectorViewModel> onActualFiniteStateSelected)
        {
            this.ActualFiniteStates = actualFiniteStateList.ActualState;
            this.SelectedFiniteState = this.ActualFiniteStates.FirstOrDefault(x => x.IsDefault);
            this.OnActualFiniteStateSelected = onActualFiniteStateSelected;
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.ActualFiniteStates = this.CurrentIteration?.ActualFiniteStateList.OrderBy(x => x.Name).SelectMany(x => x.ActualState)
                .OrderBy(x => x.Name) ?? Enumerable.Empty<ActualFiniteState>();

            this.SelectedFiniteState = this.ActualFiniteStates.FirstOrDefault(x => x.IsDefault);
        }

        /// <summary>
        /// Selects the current state and triggers the corresponding event
        /// </summary>
        /// <param name="finiteState">the new selected finite state</param>
        public void SelectActualFiniteState(ActualFiniteState finiteState)
        {
            if (this.ActualFiniteStates.Contains(finiteState))
            {
                this.SelectedFiniteState = finiteState;
            }

            this.OnActualFiniteStateSelected.InvokeAsync(this);
        }
    }
}
