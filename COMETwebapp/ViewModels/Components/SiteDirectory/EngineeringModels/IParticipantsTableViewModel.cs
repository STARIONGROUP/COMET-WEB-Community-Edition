﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParticipantsTableViewModel.cs" company="Starion Group S.A.">
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

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="Participant" />
    /// </summary>
    public interface IParticipantsTableViewModel : IDeletableDataItemTableViewModel<Participant, ParticipantRowViewModel>
    {
        /// <summary>
        /// Gets a collection of all the available <see cref="Person"/>s
        /// </summary>
        IEnumerable<Person> Persons { get; }

        /// <summary>
        /// Gets a collection of all the available <see cref="ParticipantRole"/>s
        /// </summary>
        IEnumerable<ParticipantRole> ParticipantRoles { get; }

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise"/>s
        /// </summary>
        IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; }

        /// <summary>
        /// Gets or sets a collection of all the selected <see cref="DomainOfExpertise"/>s for the participant creation
        /// </summary>
        IEnumerable<DomainOfExpertise> SelectedDomains { get; set; }

        /// <summary>
        /// Creates or edits the current participant
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Participant"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditParticipant(bool shouldCreate);

        /// <summary>
        /// Updates the current participant domains with the <see cref="ParticipantsTableViewModel.SelectedDomains"/>
        /// </summary>
        void UpdateSelectedDomains();

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its participants</param>
        void InitializeViewModel(EngineeringModelSetup model);
    }
}
