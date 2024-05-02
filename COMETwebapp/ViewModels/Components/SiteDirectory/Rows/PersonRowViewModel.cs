// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonRowViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.Rows
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="CDP4Common.SiteDirectoryData.Person" />
    /// </summary>
    public class PersonRowViewModel : DeprecatableDataItemRowViewModel<Person>
    {
        /// <summary>
        /// Backing field for <see cref="IsActive" />
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="PersonEmailAddress" />
        /// </summary>
        private string personEmailAddress;

        /// <summary>
        /// Backing field for <see cref="PersonTelephoneNumber" />
        /// </summary>
        private string personTelephoneNumber;

        /// <summary>
        /// Backing field for <see cref="Role" />
        /// </summary>
        private string role;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel" /> class.
        /// </summary>
        /// <param name="thing">The associated <see cref="CDP4Common.SiteDirectoryData.Person" /></param>
        public PersonRowViewModel(Person thing) : base(thing)
        {
            this.Role = thing.Role?.Name;
            this.PersonEmailAddress = thing.DefaultEmailAddress?.Value;
            this.PersonTelephoneNumber = thing.DefaultTelephoneNumber?.Value;
            this.IsActive = thing.IsActive;
        }

        /// <summary>
        /// The default <see cref="EmailAddress" /> of the <see cref="CDP4Common.SiteDirectoryData.Person" />
        /// </summary>
        public string PersonEmailAddress
        {
            get => this.personEmailAddress;
            set => this.RaiseAndSetIfChanged(ref this.personEmailAddress, value);
        }

        /// <summary>
        /// The default <see cref="TelephoneNumber" /> of the <see cref="CDP4Common.SiteDirectoryData.Person" />
        /// </summary>
        public string PersonTelephoneNumber
        {
            get => this.personTelephoneNumber;
            set => this.RaiseAndSetIfChanged(ref this.personTelephoneNumber, value);
        }

        /// <summary>
        /// The <see cref="PersonRole" /> of the <see cref="CDP4Common.SiteDirectoryData.Person" />
        /// </summary>
        public string Role
        {
            get => this.role;
            set => this.RaiseAndSetIfChanged(ref this.role, value);
        }

        /// <summary>
        /// Value indicating if the <see cref="CDP4Common.SiteDirectoryData.Person" /> is active
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;
            set => this.RaiseAndSetIfChanged(ref this.isActive, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="person">The <see cref="PersonRowViewModel" /> to use for updating</param>
        public void UpdateProperties(PersonRowViewModel person)
        {
            base.UpdateProperties(person);

            this.PersonEmailAddress = person.PersonEmailAddress;
            this.PersonTelephoneNumber = person.PersonTelephoneNumber;
            this.Role = person.Role;
            this.IsActive = person.IsActive;
        }
    }
}
