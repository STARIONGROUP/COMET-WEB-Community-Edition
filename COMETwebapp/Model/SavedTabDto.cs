// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SavedTabDto.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Model
{
    /// <summary>
    /// Represents serializable information of a tab saved in session storage
    /// </summary>
    public class SavedTabDto
    {
        /// <summary>
        /// Gets or sets the name of the application
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the object of interest
        /// </summary>
        public Guid ObjectOfInterestId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the iteration setup
        /// </summary>
        public Guid IterationSetupId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the domain of expertise
        /// </summary>
        public Guid DomainId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tab belongs to the side panel
        /// </summary>
        public bool IsSidePanel { get; set; }
    }
}
