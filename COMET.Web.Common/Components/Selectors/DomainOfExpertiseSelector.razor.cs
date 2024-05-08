// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSelector.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.Selectors
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component used to select a <see cref="DomainOfExpertise" />
    /// </summary>
    public partial class DomainOfExpertiseSelector
    {
        /// <summary>
        /// Gets or sets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        [Parameter]
        public IDomainOfExpertiseSelectorViewModel ViewModel { get; set; }

        /// <summary>
        /// Text to be displayed when the selector is shown
        /// </summary>
        [Parameter]
        public string DisplayText { get; set; } = "Select a Domain:";

        /// <summary>
        /// The css class used to apply custom styles to the selector
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets the domain and shortname to display, in the following format: name [shortname]
        /// </summary>
        /// <param name="domainOfExpertise">The domain of expertise to get the name and shortname</param>
        /// <returns>A string that contains name and shortname</returns>
        private static string GetDomainNameAndShortnameToDisplay(DomainOfExpertise domainOfExpertise)
        {
            return $"{domainOfExpertise.Name} [{domainOfExpertise.ShortName}]";
        }
    }
}
