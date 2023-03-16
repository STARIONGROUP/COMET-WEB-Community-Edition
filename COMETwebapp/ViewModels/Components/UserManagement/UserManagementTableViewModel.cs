// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.UserManagement
{
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.UserManagement.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// View model used to manage <see cref="Person" />
    /// </summary>
    public class UserManagementTableViewModel : DisposableObject, IUserManagementTableViewModel
    {
        /// <summary>
        /// Injected property to get access to <see cref="IPermissionService" />
        /// </summary>
        private readonly IPermissionService permissionService;

        /// <summary>
        /// Injected property to get access to <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// A collection of all <see cref="PersonRowViewModel" />
        /// </summary>
        private IEnumerable<PersonRowViewModel> allRows = new List<PersonRowViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public UserManagementTableViewModel(ISessionService sessionService)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Person))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Person)
                    .Subscribe(this.AddNewPerson));

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Person))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Person)
                    .Subscribe(this.UpdatePerson));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(PersonRole))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as PersonRole)
                .Subscribe(_ => this.RefreshAccessRight()));
        }

        /// <summary>
        /// The <see cref="Person" /> to create or edit
        /// </summary>
        public Person Person { get; set; } = new();

        /// <summary>
        /// A reactive collection of <see cref="PersonRowViewModel" />
        /// </summary>
        public SourceList<PersonRowViewModel> Rows { get; } = new();

        /// <summary>
        /// The <see cref="EmailAddress" /> to create
        /// </summary>
        public EmailAddress EmailAddress { get; set; } = new();

        /// <summary>
        /// The <see cref="TelephoneNumber" /> to create
        /// </summary>
        public TelephoneNumber TelephoneNumber { get; set; } = new();

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<Person> DataSource { get; } = new();

        /// <summary>
        /// Available <see cref="Organization" />s
        /// </summary>
        public IEnumerable<Organization> AvailableOrganizations { get; set; }

        /// <summary>
        /// Available <see cref="PersonRole" />s
        /// </summary>
        public IEnumerable<PersonRole> AvailablePersonRoles { get; set; }

        /// <summary>
        /// Available <see cref="DomainOfExpertise" />s
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

        /// <summary>
        /// Available <see cref="VcardEmailAddressKind" />s
        /// </summary>
        public IEnumerable<VcardEmailAddressKind> EmailAddressKinds { get; set; } = Enum.GetValues(typeof(VcardEmailAddressKind)).Cast<VcardEmailAddressKind>();

        /// <summary>
        /// Indicates if the <see cref="EmailAddress" /> is the default email address
        /// </summary>
        public bool IsDefaultEmail { get; set; }

        /// <summary>
        /// Available <see cref="VcardTelephoneNumberKind" />s
        /// </summary>
        public IEnumerable<VcardTelephoneNumberKind> TelephoneNumberKinds { get; set; } = Enum.GetValues(typeof(VcardTelephoneNumberKind)).Cast<VcardTelephoneNumberKind>();

        /// <summary>
        /// Indicates if the <see cref="TelephoneNumber" /> is the default telephone number
        /// </summary>
        public bool IsDefaultTelephoneNumber { get; set; }

        /// <summary>
        ///     Backing field for <see cref="IsOnDeprecationMode" />
        /// </summary>
        private bool isOnDeprecationMode;

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnDeprecationMode
        {
            get => this.isOnDeprecationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeprecationMode, value);
        }

        /// <summary>
        /// popum message dialog
        /// </summary>
        public string PopupDialog { get; set; }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnConfirmButtonClick()
        {
            if (this.Person.IsDeprecated)
            {
                await this.UnDeprecatingPerson();
            }
            else
            {
                await this.DeprecatingPerson();
            }

            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="Person" />
        /// </summary>
        public void OnCancelButtonClick()
        {
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Tries to create a new <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task AddingPerson()
        {
            var thingsToCreate = new List<Thing>();

            if (this.IsDefaultEmail)
            {
                this.Person.DefaultEmailAddress = this.EmailAddress;
            }

            if (this.IsDefaultTelephoneNumber)
            {
                this.Person.DefaultTelephoneNumber = this.TelephoneNumber;
            }

            this.Person.EmailAddress.Add(this.EmailAddress);
            this.Person.TelephoneNumber.Add(this.TelephoneNumber);
            thingsToCreate.Add(this.Person);
            await this.sessionService.CreateThings(this.sessionService.GetSiteDirectory(), thingsToCreate);
        }

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="personRow">A <see cref="PersonRowViewModel" /> that represents the person to deprecate or undeprecate </param>
        public void OnDeprecateUnDeprecateButtonClick(PersonRowViewModel personRow)
        {
            this.Person = new Person();
            this.Person = personRow.Person;
            this.PopupDialog = this.Person.IsDeprecated ? "You are about to un-deprecate the user: " + personRow.PersonName : "You are about to deprecate the user: " + personRow.PersonName;
            this.IsOnDeprecationMode = true;
        }

        /// <summary>
        /// Tries to activate or disactivate a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ActivatingOrDisactivatingPerson(GridDataColumnCellDisplayTemplateContext context, bool value)
        {
            var personRow = (PersonRowViewModel)context.DataItem;
            var personToActivateOrDesactivate = new List<Person>();
            var personToUpdate = personRow.Person;
            var clonedPerson = personToUpdate.Clone(false);
            clonedPerson.IsActive = value;
            personToActivateOrDesactivate.Add(clonedPerson);
            await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), personToActivateOrDesactivate);
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        public void OnInitialized()
        {
            this.DataSource.AddRange(this.sessionService.Session.RetrieveSiteDirectory().Person);
            this.UpdateProperties(this.DataSource.Items);
            this.AvailableOrganizations = this.sessionService.Session.RetrieveSiteDirectory().Organization;
            this.AvailablePersonRoles = this.sessionService.Session.RetrieveSiteDirectory().PersonRole;
            this.AvailableDomains = this.sessionService.Session.RetrieveSiteDirectory().Domain;
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Adds a new <see cref="Person" /> to the <see cref="DataSource" />
        /// </summary>
        public void AddNewPerson(Person person)
        {
            var newRows = new List<PersonRowViewModel>(this.allRows)
            {
                new(person)
            };

            this.UpdateRows(newRows);
        }

        /// <summary>
        /// Updates the <see cref="Person" /> in the <see cref="DataSource" />
        /// </summary>
        public void UpdatePerson(Person person)
        {
            var updatedRows = new List<PersonRowViewModel>(this.allRows);
            var index = updatedRows.FindIndex(x => x.Person.Iid == person.Iid);
            updatedRows[index] = new PersonRowViewModel(person);
            this.UpdateRows(updatedRows);
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="persons">A collection of <see cref="Person" /></param>
        public void UpdateProperties(IEnumerable<Person> persons)
        {
            this.allRows = persons.Select(x => new PersonRowViewModel(x));
            this.UpdateRows(this.allRows);
        }

        /// <summary>
        /// Tries to undeprecate a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task UnDeprecatingPerson()
        {
            var personToUnDeprecate = new List<Person>();
            var clonedPerson = this.Person.Clone(false);
            clonedPerson.IsDeprecated = false;
            personToUnDeprecate.Add(clonedPerson);
            await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), personToUnDeprecate);
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Tries to deprecate a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeprecatingPerson()
        {
            var personToDeprecate = new List<Person>();
            var clonedPerson = this.Person.Clone(false);
            clonedPerson.IsDeprecated = true;
            personToDeprecate.Add(clonedPerson);
            await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), personToDeprecate);
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        private void RefreshAccessRight()
        {
            foreach (var row in Rows.Items)
            {
                if(row.Person == this.sessionService.Session.ActivePerson)
                {
                    row.IsAllowedToWrite = false;
                }
                else
                {
                    row.IsAllowedToWrite = this.permissionService.CanWrite(ClassKind.Person, this.sessionService.GetSiteDirectory());
                }
            }
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on a collection of
        /// <see cref="PersonRowViewModel" /> to display.
        /// </summary>
        /// <param name="rowsToDisplay">A collection of <see cref="PersonRowViewModel" /></param>
        private void UpdateRows(IEnumerable<PersonRowViewModel> rowsToDisplay)
        {
            rowsToDisplay = rowsToDisplay.ToList();

            var deletedRows = this.Rows.Items.Where(x => rowsToDisplay.All(r => r.Person.Iid != x.Person.Iid)).ToList();
            var addedRows = rowsToDisplay.Where(x => this.Rows.Items.All(r => r.Person.Iid != x.Person.Iid)).ToList();
            var existingRows = rowsToDisplay.Where(x => this.Rows.Items.Any(r => r.Person.Iid == x.Person.Iid)).ToList();

            this.Rows.RemoveMany(deletedRows);
            this.Rows.AddRange(addedRows);

            foreach (var existingRow in existingRows)
            {
                this.Rows.Items.First(x => x.Person.Iid == existingRow.Person.Iid).UpdateProperties(existingRow);
            }
        }
    }
}
