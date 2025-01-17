﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IBatchParameterEditorViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    using DynamicData;

    /// <summary>
    /// ViewModel used to apply batch operations for a parameter
    /// </summary>
    public interface IBatchParameterEditorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// Gets the <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        IParameterTypeEditorSelectorViewModel ParameterTypeEditorSelectorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        IParameterTypeSelectorViewModel ParameterTypeSelectorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" />
        /// </summary>
        IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; }

        /// <summary>
        /// Gets or sets the visibility of the current component
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the loading value
        /// </summary>
        bool IsLoading { get; set; }

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        IOptionSelectorViewModel OptionSelectorViewModel { get; }

        /// <summary>
        /// Gets the list of <see cref="ParameterValueSetBaseRowViewModel" />s
        /// </summary>
        SourceList<ParameterValueSetBaseRowViewModel> Rows { get; }

        /// <summary>
        /// Gets the <see cref="IFiniteStateSelectorViewModel" />
        /// </summary>
        IFiniteStateSelectorViewModel FiniteStateSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the collection of selected rows
        /// </summary>
        IReadOnlyList<object> SelectedValueSetsRowsToUpdate { get; set; }

        /// <summary>
        /// Gets a collection of all available categories
        /// </summary>
        IEnumerable<Category> AvailableCategories { get; }

        /// <summary>
        /// Gets or sets the selected category
        /// </summary>
        Category SelectedCategory { get; set; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Method invoked for opening the batch update popup
        /// </summary>
        void OpenPopup();
    }
}
