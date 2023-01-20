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

namespace COMETwebapp.Components.Canvas
{
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using Microsoft.AspNetCore.Components;
    
    /// <summary>
    /// Class for the state selector component
    /// </summary>
    public partial class ActualFiniteStateSelectorComponent
    {
        /// <summary>
        /// Gets or sets the list of <see cref="ActualFiniteStateList"/> used by the <see cref="ActualFiniteStateSelectorComponent"/>
        /// </summary>
        [Parameter]
        public List<ActualFiniteStateList> ListActualFiniteStateList { get; set; } = new();

        /// <summary>
        /// Field for keeping track of the selection state of the <see cref="ActualFiniteState"/>
        /// </summary>
        private Dictionary<ActualFiniteState, bool> SelectedActualFiniteStates = new();

        /// <summary>
        /// All the <see cref="ActualFiniteState"/> that the <see cref="ActualFiniteStateSelectorComponent"/> contains
        /// </summary>
        private List<ActualFiniteState> TotalActualFiniteStates = new();

        /// <summary>
        /// Event callback for when an <see cref="ActualFiniteState"/> has been selected
        /// </summary>
        [Parameter]
        public EventCallback<List<ActualFiniteState>> OnActualFiniteStateChanged { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRender(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                this.TotalActualFiniteStates = this.ListActualFiniteStateList.SelectMany(x => x.ActualState).ToList();
                this.ResetDictionary();
            }
        }

        /// <summary>
        /// Resets the dictionary and fills it with the default states
        /// </summary>
        private void ResetDictionary()
        {
            this.SelectedActualFiniteStates.Clear();

            foreach (var actualFS in this.TotalActualFiniteStates)
            {
                this.SelectedActualFiniteStates.TryAdd(actualFS, actualFS.IsDefault);
            }
        }

        /// <summary>
        /// Event for when a <see cref="ActualFiniteState"/> has been selected
        /// </summary>
        /// <param name="actualFiniteState">the selected <see cref="ActualFiniteState"/></param>
        public void OnActualFiniteStateSelected(ActualFiniteState actualFiniteState)
        {
            if(actualFiniteState.Container is ActualFiniteStateList AFSlist)
            {
                foreach(var finiteState in AFSlist.ActualState)
                {
                    if (this.SelectedActualFiniteStates.ContainsKey(finiteState))
                    {
                        this.SelectedActualFiniteStates[finiteState] = false;
                    }
                }
            }

            this.SelectedActualFiniteStates[actualFiniteState] = true;

            var selectedStates = this.SelectedActualFiniteStates.Where(x => x.Value).Select(x => x.Key).ToList();
            this.OnActualFiniteStateChanged.InvokeAsync(selectedStates);
        }
    }
}
