// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemNodeViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// ViewModel for a node in the SystemRepresentation tree
    /// </summary>
    public class SystemNodeViewModel : BaseNodeViewModel<SystemNodeViewModel>
    {
        /// <summary>
        /// Creates a new instance of type <see cref="SystemNodeViewModel" />
        /// </summary>
        /// <param name="elementBase">the system node view model</param>
        public SystemNodeViewModel(ElementBase elementBase) : base(elementBase)
        {
        }

        /// <summary>
        /// The <see cref="EventCallback" /> to call on baseNode selection
        /// </summary>
        public EventCallback<SystemNodeViewModel> OnSelect { get; set; }

        /// <summary>
        /// Callback method for when a node is selected
        /// </summary>
        public override async void RaiseTreeSelectionChanged()
        {
            this.GetRootNode().GetFlatListOfDescendants(true).ForEach(x => x.IsSelected = false);
            await this.OnSelect.InvokeAsync(this);
        }
    }
}
