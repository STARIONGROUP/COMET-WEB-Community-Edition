// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ShowHideDeprecatedThingsViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Shared.TopMenuEntry
{
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;

    /// <summary>
    /// View model that handles the display of deprecated items.
    /// </summary>
    public class ShowHideDeprecatedThingsViewModel : IShowHideDeprecatedThingsViewModel
    {
        /// <summary>
        /// Initializes a <see cref="ShowHideDeprecatedThingsViewModel" />
        /// </summary>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        public ShowHideDeprecatedThingsViewModel(IShowHideDeprecatedThingsService showHideDeprecatedThingsService)
        {
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
        }

        /// <summary>
        /// Gets the <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Method invoked when the user clicks on the Show/Hide Deprecated Items button.
        /// </summary>
        public void OnShowHideDeprecatedItems()
        {
            this.ShowHideDeprecatedThingsService.ShowDeprecatedThings = !this.ShowHideDeprecatedThingsService.ShowDeprecatedThings;
        }
    }
}
