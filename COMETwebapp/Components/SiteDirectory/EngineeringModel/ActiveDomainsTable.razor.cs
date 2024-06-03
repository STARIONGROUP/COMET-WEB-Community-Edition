// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ActiveDomainsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ActiveDomainsTable" />
    /// </summary>
    public partial class ActiveDomainsTable : SelectedDataItemBase<DomainOfExpertise, DomainOfExpertiseRowViewModel>
    {
        /// <summary>
        /// Gets or sets the <see cref="EngineeringModelSetup" />
        /// </summary>
        [Parameter]
        public EngineeringModelSetup EngineeringModelSetup { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="DomainOfExpertise" />
        /// </summary>
        [Parameter]
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; set; }

        /// <summary>
        /// Sets the edit active domains popup visibility
        /// </summary>
        /// <param name="visible">The visibility of the popup</param>
        private void SetEditPopupVisibility(bool visible)
        {
            this.IsOnEditMode = visible;
        }
    }
}
