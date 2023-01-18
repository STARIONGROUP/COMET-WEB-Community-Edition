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
    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Model;
    
    using Microsoft.AspNetCore.Components;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Class for the state selector component
    /// </summary>
    public partial class ActualFiniteStateSelectorComponent
    {
        [Parameter]
        public List<ActualFiniteStateList> ListActualFiniteStateList { get; set; } = new();

        private Dictionary<ActualFiniteState, bool> SelectedActualFiniteStates = new();

        private List<ActualFiniteState> TotalActualFiniteStates = new();

        [Parameter]
        public EventCallback<List<ActualFiniteState>> OnActualFiniteStateChanged { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                this.TotalActualFiniteStates = this.ListActualFiniteStateList.SelectMany(x => x.ActualState).ToList();
                this.ResetDictionary();
            }
        }

        private void ResetDictionary()
        {
            this.SelectedActualFiniteStates.Clear();

            foreach (var actualFS in this.TotalActualFiniteStates)
            {
                this.SelectedActualFiniteStates.TryAdd(actualFS, actualFS.IsDefault);
            }
        }

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
