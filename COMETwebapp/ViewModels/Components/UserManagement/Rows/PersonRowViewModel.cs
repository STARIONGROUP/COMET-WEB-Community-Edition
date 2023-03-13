// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.UserManagement.Rows
{
    using COMETwebapp.Components.UserManagement;
    
    using CDP4Common.SiteDirectoryData;


    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="Person" />
    /// </summary>
    public class PersonRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel" /> class.
        /// </summary>
        public PersonRowViewModel(Person person)
        {
            this.Person = person;
            this.PersonName = Person.Name;
            this.PersonShortName = Person.ShortName;
            this.PersonEmailAddress = Person.DefaultEmailAddress?.Value;
            this.PersonTelephoneNumber = Person.DefaultTelephoneNumber?.Value;
            this.IsActive = Person.IsActive;
            this.IsDeprecated = Person.IsDeprecated;
        }

        /// <summary>
        /// The <see cref="Person" /> that is represented by this row
        /// </summary>
        public Person Person;

        /// <summary>
        /// Backing field for <see cref="PersonName" />
        /// </summary>
        private string personName;

        /// <summary>
        ///     The name of the <see cref="Person" />
        /// </summary>
        public string PersonName
        {
            get => this.personName;
            set => this.RaiseAndSetIfChanged(ref this.personName, value);
        }

        /// <summary>
        /// Backing field for <see cref="PersonShortName" />
        /// </summary>
        private string personShortName;

        /// <summary>
        /// The short name of the <see cref="Person" />
        /// </summary>
        public string PersonShortName
        {
            get => this.personShortName;
            set => this.RaiseAndSetIfChanged(ref this.personShortName, value);
        }

        /// <summary>
        /// Backing field for <see cref="PersonEmailAddress" />
        /// </summary>
        private string personEmailAddress;

        /// <summary>
        /// The default <see cref="EmailAddress" /> of the <see cref="Person" />
        /// </summary>
        public string PersonEmailAddress
        {
            get => this.personEmailAddress;
            set => this.RaiseAndSetIfChanged(ref this.personEmailAddress, value);
        }

        /// <summary>
        /// Backing field for <see cref="PersonTelephoneNumber" />
        /// </summary>
        private string personTelephoneNumber;

        /// <summary>
        /// The default <see cref="TelephoneNumber" /> of the <see cref="Person" />
        /// </summary>
        public string PersonTelephoneNumber
        {
            get => this.personTelephoneNumber;
            set => this.RaiseAndSetIfChanged(ref this.personTelephoneNumber, value);
        }

        /// <summary>
        /// Backing field for <see cref="Role" />
        /// </summary>
        private string role;

        /// <summary>
        /// The <see cref="PersonRole" /> of the <see cref="Person" />
        /// </summary>  
        public string Role
        {
           get => this.role;
            set => this.RaiseAndSetIfChanged(ref this.role, value);
        }

        /// <summary>
        /// Backing field for <see cref="IsActive" />
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Value indicating if the <see cref="Person" /> is active
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;
            set => this.RaiseAndSetIfChanged(ref this.isActive, value);
        }

        /// <summary>
        /// Backing field for <see cref="IsDeprecated" />
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Value indicating if the <see cref="Person" /> is deprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="person">The <see cref="PersonRowViewModel" /> to use for updating</param>
        public void UpdateProperties(PersonRowViewModel person)
        {
            this.PersonName = person.PersonName;
            this.PersonShortName = person.PersonShortName;
            this.PersonEmailAddress = person.PersonEmailAddress;
            this.PersonTelephoneNumber = person.PersonTelephoneNumber;
            this.Role = person.Role;
            this.IsActive = person.IsActive;
            this.IsDeprecated = person.IsDeprecated;
        }
    }
}
