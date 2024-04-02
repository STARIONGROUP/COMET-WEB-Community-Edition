// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.Rows
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for <see cref="CDP4Common.SiteDirectoryData.Participant" />s
    /// </summary>
    public class ParticipantRowViewModel : BaseDataItemRowViewModel<Participant>
    {
        /// <summary>
        /// Backing field for <see cref="Organization" />
        /// </summary>
        private string organization;

        /// <summary>
        /// Backing field for <see cref="Role" />
        /// </summary>
        private string role;

        /// <summary>
        /// Backing field for <see cref="AssignedDomains" />
        /// </summary>
        private string assignedDomains;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRowViewModel" /> class.
        /// </summary>
        /// <param name="participant">The associated <see cref="Participant" /></param>
        public ParticipantRowViewModel(Participant participant) : base(participant)
        {
            this.Name = participant.Person.Name;
            this.Organization = participant.Person.Organization.Name;
            this.Role = participant.Role.Name;
            this.AssignedDomains = string.Join(Separator, participant.Domain.Select(x => x.Name));
        }

        /// <summary>
        /// Gets the separator used to join the participant domains of expertise
        /// </summary>
        public const string Separator = ",";

        /// <summary>
        /// The organization value for the current <see cref="Participant"/>
        /// </summary>
        public string Organization
        {
            get => this.organization;
            set => this.RaiseAndSetIfChanged(ref this.organization, value);
        }

        /// <summary>
        /// The role value for the current <see cref="Participant"/>
        /// </summary>
        public string Role
        {
            get => this.role;
            set => this.RaiseAndSetIfChanged(ref this.role, value);
        }

        /// <summary>
        /// The assigned domains value for the current <see cref="Participant"/>
        /// </summary>
        public string AssignedDomains
        {
            get => this.assignedDomains;
            set => this.RaiseAndSetIfChanged(ref this.assignedDomains, value);
        }
    }
}
