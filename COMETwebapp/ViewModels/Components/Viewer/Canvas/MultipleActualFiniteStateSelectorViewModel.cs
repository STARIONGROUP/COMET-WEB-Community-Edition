// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipleActualFiniteStateSelectorViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Viewer.Canvas
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Shared.Selectors;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.Canvas.ActualFiniteStateSelector"/>
    /// </summary>
    public class MultipleActualFiniteStateSelectorViewModel : BelongsToIterationSelectorViewModel, IMultipleActualFiniteStateSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="ActualFiniteStateList"/>
        /// </summary>
        public SourceList<ActualFiniteStateList> ActualFiniteStateListsCollection { get; } = new();

        /// <summary>
        /// Backing field for the <see cref="ActualFiniteStateSelectorViewModels"/> property
        /// </summary>
        private IEnumerable<IActualFiniteStateSelectorViewModel> actualFiniteStateSelectorViewModels;

        /// <summary>
        /// Gets or sets the view models for the child selectors
        /// </summary>
        public IEnumerable<IActualFiniteStateSelectorViewModel> ActualFiniteStateSelectorViewModels
        {
            get => this.actualFiniteStateSelectorViewModels;
            set => this.RaiseAndSetIfChanged(ref this.actualFiniteStateSelectorViewModels, value);
        }
        
        /// <summary>
        /// Backing field for the <see cref="SelectionStates"/> property
        /// </summary>
        private IEnumerable<ActualFiniteState> selectedFiniteStates = Enumerable.Empty<ActualFiniteState>();

        /// <summary>
        /// Gets or sets the current selected states
        /// </summary>
        public IEnumerable<ActualFiniteState> SelectedFiniteStates
        {
            get => this.selectedFiniteStates;
            private set => this.RaiseAndSetIfChanged(ref this.selectedFiniteStates, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteState"/> and its selection state.
        /// </summary>
        private Dictionary<ActualFiniteState, bool> SelectionStates { get; set; } = new();
        
        /// <summary>
        /// Creates a new instance of type <see cref="MultipleActualFiniteStateSelectorViewModel"/>
        /// </summary>
        public MultipleActualFiniteStateSelectorViewModel()
        {
            this.Disposables.Add(this.ActualFiniteStateListsCollection.CountChanged.Subscribe(_ =>
            {
                this.InitializeViewModel();
            }));
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.ActualFiniteStateListsCollection.Clear();
            var items = this.CurrentIteration?.ActualFiniteStateList.ToList() ?? new List<ActualFiniteStateList>();
            this.ActualFiniteStateListsCollection.AddRange(items);
        }

        /// <summary>
        /// Initializes this viewmodel with the collection of <see cref="ActualFiniteStateList"/>
        /// </summary>
        public void InitializeViewModel()
        {
            this.SelectionStates.Clear();
            var viewModels = new List<IActualFiniteStateSelectorViewModel>();

            var eventCallback = new EventCallbackFactory().Create(this, (ActualFiniteStateSelectorViewModel stateSelector) =>
            {
                this.OnActualFiniteStateSelectionChanged(stateSelector);
            });

            foreach (var finiteStateList in this.ActualFiniteStateListsCollection.Items)
            {
                viewModels.Add(new ActualFiniteStateSelectorViewModel(finiteStateList, eventCallback));

                foreach (var finiteState in finiteStateList.ActualState)
                {
                    this.SelectionStates.TryAdd(finiteState, finiteState.IsDefault);
                }
            }

            this.ActualFiniteStateSelectorViewModels = viewModels;
            this.SelectedFiniteStates = this.SelectionStates.Where(x => x.Value).Select(x => x.Key);
        }

        /// <summary>
        /// Event for when a <see cref="ActualFiniteState"/> has been selected
        /// </summary>
        /// <param name="stateSelector">the selector that raised the event</param>
        public void OnActualFiniteStateSelectionChanged(IActualFiniteStateSelectorViewModel stateSelector)
        {
            foreach (var finiteState in stateSelector.ActualFiniteStates)
            {
                if (this.SelectionStates.ContainsKey(finiteState))
                {
                    this.SelectionStates[finiteState] = false;
                }
            }

            if (this.SelectionStates.ContainsKey(stateSelector.SelectedFiniteState))
            {
                this.SelectionStates[stateSelector.SelectedFiniteState] = true;
            }
            
            this.SelectedFiniteStates = this.SelectionStates.Where(x => x.Value).Select(x=>x.Key);
        }
    }
}
