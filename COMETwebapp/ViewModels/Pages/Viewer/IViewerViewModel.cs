// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IViewerViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Pages.Viewer
{
    using CDP4Common.EngineeringModelData;
    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;

    /// <summary>
    /// Interface for the <see cref="ViewerViewModel"/>
    /// </summary>
    public interface IViewerViewModel
    {
        /// <summary>
        /// Gets or sets the selected <see cref="Option"/>
        /// </summary>
        Option SelectedOption { get; set; }

        /// <summary>
        /// Gets or sets the list of the available <see cref="Option"/>
        /// </summary>
        List<Option> TotalOptions { get; set; }

        /// <summary>
        /// Gets or sets the root VM of the <see cref="COMETwebapp.Components.Viewer.Canvas.ProductTree"/>
        /// </summary>
        INodeComponentViewModel RootNodeViewModel { get; set; }

        /// <summary>
        /// List of the of <see cref="ActualFiniteStateList"/> 
        /// </summary>        
        List<ActualFiniteStateList> ListActualFiniteStateLists { get; set; }

        /// <summary>
        /// Event raised when an actual finite state has changed
        /// </summary>
        /// <param name="selectedActiveFiniteStates"></param>
        void ActualFiniteStateChanged(List<ActualFiniteState> selectedActiveFiniteStates);
    }
}
