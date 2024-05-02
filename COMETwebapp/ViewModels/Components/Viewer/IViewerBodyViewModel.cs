﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IViewerBodyViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.ViewerBody"/>
    /// </summary>
    public interface IViewerBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IOptionSelectorViewModel"/>
        /// </summary>
        IOptionSelectorViewModel OptionSelector { get; }

        /// <summary>
        /// Gets or sets the <see cref="IMultipleActualFiniteStateSelectorViewModel"/>
        /// </summary>
        IMultipleActualFiniteStateSelectorViewModel MultipleFiniteStateSelector { get; }

        /// <summary>
        /// Gets or sets the <see cref="ViewerProductTreeViewModel"/>
        /// </summary>
        ViewerProductTreeViewModel ProductTreeViewModel { get; }

        /// <summary>
        /// Gets or sets the <see cref="ICanvasViewModel"/>
        /// </summary>
        ICanvasViewModel CanvasViewModel { get; }

        /// <summary>
        /// Gets or sets the <see cref="IPropertiesComponentViewModel"/>
        /// </summary>
        IPropertiesComponentViewModel PropertiesViewModel { get; }
        
        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration
        /// </summary>
        List<ElementBase> Elements { get; set; }

        /// <summary>
        /// Initializes this <see cref="IViewerBodyViewModel"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task InitializeViewModel();

        /// <summary>
        /// Create the <see cref="ElementBase"/> based on the current <see cref="Iteration"/>
        /// </summary>
        IEnumerable<ElementBase> InitializeElements();

        /// <summary>
        /// Initializes the elements and creates the tree based on that elements
        /// </summary>
        void InitializeElementsAndCreateTree();
    }
}
