// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IElementDefinitionTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using System.Collections.ObjectModel;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    /// <summary>
    /// Interface for the <see cref="ElementDefinitionTableViewModel" />
    /// </summary>
    public interface IElementDefinitionTableViewModel : ISingleIterationApplicationBaseViewModel, IHaveReusableRows
    {
        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" />
        /// </summary>
        ObservableCollection<ElementDefinitionRowViewModel> RowsTarget { get; }

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" />
        /// </summary>
        ObservableCollection<ElementDefinitionRowViewModel> RowsSource { get; }

        /// <summary>
        /// Value indicating the user is currently creating a new <see cref="ElementDefinition" />
        /// </summary>
        bool IsOnCreationMode { get; set; }

        /// <summary>
        /// Represents the selected ElementDefinitionRowViewModel
        /// </summary>
        ElementDefinition SelectedElementDefinition { get; set; }

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IElementDefinitionCreationViewModel" />
        /// </summary>
        IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IAddParameterViewModel" />
        /// </summary>
        IAddParameterViewModel AddParameterViewModel { get; set; }

        /// <summary>
        /// Value indicating the user is currently adding a new <see cref="Parameter" /> to a <see cref="ElementDefinition" />
        /// </summary>
        bool IsOnAddingParameterMode { get; set; }

        /// <summary>
        /// Opens the <see cref="ElementDefinitionCreation" /> popup
        /// </summary>
        void OpenCreateElementDefinitionCreationPopup();

        /// <summary>
        /// Set the selected <see cref="ElementDefinition" />
        /// </summary>
        /// <param name="selectedElementBase">The selected <see cref="ElementBase" /></param>
        void SelectElement(ElementBase selectedElementBase);

        /// <summary>
        /// Opens the <see cref="AddParameter" /> popup
        /// </summary>
        void OpenAddParameterPopup();
    }
}
