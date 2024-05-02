// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOrganizationalParticipantsTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="OrganizationalParticipant" />
    /// </summary>
    public interface IOrganizationalParticipantsTableViewModel : IDeletableDataItemTableViewModel<OrganizationalParticipant, OrganizationalParticipantRowViewModel>
    {
        /// <summary>
        /// Filters the current Rows, keeping only the organizational participants associated with the given engineering model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its participants</param>
        void SetEngineeringModel(EngineeringModelSetup model);

        /// <summary>
        /// Gets a collection of all the available <see cref="Organization"/>s
        /// </summary>
        IEnumerable<Organization> Organizations { get; }

        /// <summary>
        /// Gets or sets a collection of all the participating <see cref="Organization"/>s for the organizational participant creation
        /// </summary>
        IEnumerable<Organization> ParticipatingOrganizations { get; set; }
    }
}
