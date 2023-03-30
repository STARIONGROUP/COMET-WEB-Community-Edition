// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ShowHideDeprecatedThingsService.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Services.ShowHideDeprecatedThingsService
{
    using DevExpress.Blazor;

    using ReactiveUI;

    /// <summary>
    /// Service to manage the "Show/Hide Deprecated Items" 
    /// </summary>
    public class ShowHideDeprecatedThingsService : ReactiveObject, IShowHideDeprecatedThingsService
    {
        /// <summary>
        /// Backing field for <see cref="ShowDeprecatedThings" />
        /// </summary>
        private bool showDeprecatedThings;

        /// <summary>
        ///    Value indicating whether to display deprecated items.
        /// </summary>
        public bool ShowDeprecatedThings
        {
            get => this.showDeprecatedThings;
            set => this.RaiseAndSetIfChanged(ref this.showDeprecatedThings, value);
        }
    }
}
