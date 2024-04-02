// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParticipantsTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    /// View model used to manage <see cref="Participant" />
    /// </summary>
    public interface IParticipantsTableViewModel : IDeletableDataItemTableViewModel<Participant, ParticipantRowViewModel>
    {
        /// <summary>
        /// Filters the current Rows, keeping only the participants associated with the given engineering model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its participants</param>
        void SetEngineeringModel(EngineeringModelSetup model);

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
        /// Selects the current participant
        /// </summary>
        /// <param name="participant">The <see cref="Participant"/> to select</param>
        void SelectThing(Participant participant);
    }
}
