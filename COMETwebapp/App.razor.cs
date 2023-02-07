// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="App.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;

    /// <summary>
    /// Main application component
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// The <see cref="NavigationManager"/>
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Redirect to the home page with the redirect parameter set
        /// </summary>
        private void RedirectToHomePage()
        {
            var requestedUrl = this.NavigationManager.Uri.Replace(this.NavigationManager.BaseUri, string.Empty);
            var targetUrl = QueryHelpers.AddQueryString("/", "redirect", requestedUrl);
            this.NavigationManager.NavigateTo(targetUrl);
        }
    }
}
