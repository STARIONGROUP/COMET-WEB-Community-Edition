// --------------------------------------------------------------------------------------------------------------------
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
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;
    using DevExpress.Blazor;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interface for the <see cref="ElementDefinitionTableViewModel"/>
    /// </summary>
    public interface IElementDefinitionTableViewModel : ISingleIterationApplicationBaseViewModel
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
		///     Value indicating the user is currently creating a new <see cref="ElementDefinition" />
		/// </summary>
		bool IsOnCreationMode { get; set; }

		/// <summary>
		/// Represents the selected ElementDefinitionRowViewModel
		/// </summary>
		object SelectedElementDefinition { get; set; }

		/// <summary>
		/// The <see cref="IElementDefinitionDetailsViewModel" />
		/// </summary>
		IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

		/// <summary>
		///     Gets the <see cref="IElementDefinitionCreationViewModel" />
		/// </summary>
		IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

		/// <summary>
		///     Opens the <see cref="ElementDefinitionCreation" /> popup
		/// </summary>
		void OpenCreateElementDefinitionCreationPopup();

		/// <summary>
		/// set the selected <see cref="SystemNodeViewModel" />
		/// </summary>
		/// <param name="selectedNode">The selected <see cref="SystemNodeViewModel" /></param>
		/// <returns>A <see cref="Task" /></returns>
		void SelectElement(GridRowClickEventArgs args);
	}
}
