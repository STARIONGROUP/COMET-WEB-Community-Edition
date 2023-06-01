﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IElementDefinitionTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.JSInterop;

    using System.Collections.ObjectModel;

    /// <summary>
    /// Interface for the <see cref="ElementDefinitionTableViewModel"/>
    /// </summary>
    public interface IElementDefinitionTableViewModel
    {
        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel"/>
        /// </summary>
        ObservableCollection<ElementDefinitionRowViewModel> RowsTarget { get; }

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel"/>
        /// </summary>
        ObservableCollection<ElementDefinitionRowViewModel> RowsSource { get; }

        /// <summary>
        /// Set the dotnet helper
        /// </summary>
        /// <param name="dotNetHelper">the dotnet helper</param>
        Task LoadDotNetHelper(DotNetObjectReference<ElementDefinitionTable> dotNetHelper);

        /// <summary>
        ///  Method used to initialize the draggable grids
        /// </summary>
        /// <param name="firstGrid">the first grid</param>
        /// <param name="secondGrid">the second grid</param>
        Task InitDraggableGrids(string firstGrid, string secondGrid);
    }
}