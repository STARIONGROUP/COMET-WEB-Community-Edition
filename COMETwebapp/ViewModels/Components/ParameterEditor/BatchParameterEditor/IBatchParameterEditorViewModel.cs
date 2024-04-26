// --------------------------------------------------------------------------------------------------------------------
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
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

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
    }
}
