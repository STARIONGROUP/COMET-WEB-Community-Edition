// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ActualFiniteStateSelectorViewModel.cs" company="RHEA System S.A.">
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

    using ReactiveUI;

    /// <summary>
    /// View Model for the allow to select multiple existing <see cref="ActualFiniteState"/>
    /// </summary>
    public class ActualFiniteStateSelectorViewModel : ReactiveObject, IActualFiniteStateSelectorViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="ActualFiniteStateListsCollection"/>
        /// </summary>
        private List<ActualFiniteStateList> actualFiniteStateListsCollection;

        /// <summary>
        /// Gets or sets the collection of <see cref="ActualFiniteStateList"/>
        /// </summary>
        public List<ActualFiniteStateList> ActualFiniteStateListsCollection
        {
            get => this.actualFiniteStateListsCollection;
            set => this.RaiseAndSetIfChanged(ref this.actualFiniteStateListsCollection, value);
        }

        /// <summary>
        /// Field for keeping track of the selection state of the <see cref="ActualFiniteState"/>
        /// </summary>
        public Dictionary<ActualFiniteState, bool> SelectionStateActualFiniteState { get; set; } = new();

        /// <summary>
        /// Backing field for the <see cref="SelectedStates"/>
        /// </summary>
        private List<ActualFiniteState> selectedStates;

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState"/>
        /// </summary>
        public List<ActualFiniteState> SelectedStates
        {
            get => this.selectedStates;
            set => this.RaiseAndSetIfChanged(ref this.selectedStates, value);
        }

        /// <summary>
        /// Initializes this view model
        /// </summary>
        public void InitializeViewModel()
        {
            this.ResetDictionary();
            this.WhenAnyValue(x => x.ActualFiniteStateListsCollection).Subscribe(_ => this.ResetDictionary());
            this.SelectedStates = this.ActualFiniteStateListsCollection.SelectMany(x => x.ActualState.Where(x => x.IsDefault)).ToList();
        }

        /// <summary>
        /// Resets the dictionary and fills it with the default states
        /// </summary>
        private void ResetDictionary()
        {
            this.SelectionStateActualFiniteState.Clear();
            var totalActualFiniteStates = this.ActualFiniteStateListsCollection.SelectMany(x => x.ActualState).ToList();
  
            foreach (var actualFS in totalActualFiniteStates)
            {
                this.SelectionStateActualFiniteState.TryAdd(actualFS, actualFS.IsDefault);
            }
        }

        /// <summary>
        /// Event for when a <see cref="ActualFiniteState"/> has been selected
        /// </summary>
        /// <param name="actualFiniteState">the selected <see cref="ActualFiniteState"/></param>
        public void OnActualFiniteStateSelected(ActualFiniteState actualFiniteState)
        {
            if (actualFiniteState.Container is ActualFiniteStateList AFSlist)
            {
                foreach (var finiteState in AFSlist.ActualState)
                {
                    if (this.SelectionStateActualFiniteState.ContainsKey(finiteState))
                    {
                        this.SelectionStateActualFiniteState[finiteState] = false;
                    }
                }
            }

            if (this.SelectionStateActualFiniteState.ContainsKey(actualFiniteState))
            {
                this.SelectionStateActualFiniteState[actualFiniteState] = true;
            }

            this.SelectedStates = this.SelectionStateActualFiniteState.Where(x => x.Value).Select(x => x.Key).ToList();
        }
    }
}
