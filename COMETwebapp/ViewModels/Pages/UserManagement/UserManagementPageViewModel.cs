// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementPageViewModel.cs" company="RHEA System S.A.">
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
namespace COMETwebapp.ViewModels.Pages.UserManagement
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using COMETwebapp.Pages.UserManagement;
    using COMETwebapp.SessionManagement;
    
    using DevExpress.Blazor;
    using DevExpress.Blazor.Internal;
    using DynamicData;
    
    using ReactiveUI;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    ///     View model for the <see cref="UserManagementPage" /> page
    /// </summary>
    public class UserManagementPageViewModel : ReactiveObject, IUserManagementPageViewModel, IDisposable
    {
        /// <summary>
        ///     The <see cref="Person" /> to create
        /// </summary>
        public Person Person { get; set; } = new();

        /// <summary>
        ///     The <see cref="EmailAddress" /> to create
        /// </summary>
        public EmailAddress EmailAddress { get; set; } = new();

        /// <summary>
        ///     The <see cref="TelephoneNumber" /> to create
        /// </summary>
        public TelephoneNumber TelephoneNumber { get; set; } = new();

        /// <summary>
        ///     Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<Person> DataSource { get; } = new();

        /// <summary>
        ///    Available <see cref="Organization"/>s
        /// </summary>
        public IEnumerable<Organization> AvailableOrganizations { get; set; }

        /// <summary>
        ///    Available <see cref="PersonRole"/>s
        /// </summary>
        public IEnumerable<PersonRole> AvailablePersonRoles { get; set; }

        /// <summary>
        ///    Available <see cref="DomainOfExpertise"/>s
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

        /// <summary>
        ///    Available <see cref="VcardEmailAddressKind"/>s
        /// </summary>
        public IEnumerable<VcardEmailAddressKind> EmailAddressKinds { get; set; } = Enum.GetValues(typeof(VcardEmailAddressKind)).Cast<VcardEmailAddressKind>();

        /// <summary>
        ///    Available <see cref="VcardTelephoneNumberKind"/>s
        /// </summary>
        public IEnumerable<VcardTelephoneNumberKind> TelephoneNumberKinds { get; set; } = Enum.GetValues(typeof(VcardTelephoneNumberKind)).Cast<VcardTelephoneNumberKind>();

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        private readonly ISessionAnchor SessionAnchor;

        /// <summary>
        ///     A collection of <see cref="IDisposable" />
        /// </summary>
        private readonly List<IDisposable> disposables = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserManagementPageViewModel" /> class.
        /// </summary>
        /// <param name="sessionAnchor">The <see cref="ISessionAnchor" /></param>
        public UserManagementPageViewModel(ISessionAnchor sessionAnchor)
        {
            this.SessionAnchor = sessionAnchor;

            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Person))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.SessionAnchor.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Person)
                    .Subscribe(person => this.DataSource.Add(person));
            this.disposables.Add(addListener);

            var updateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Person))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.SessionAnchor.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Person)
                    .Subscribe(person => this.DataSource.Edit(innerList =>
                    {
                        var index = innerList.FindIndex(x => x.Iid == person.Iid);
                        innerList[index] = person;
                    }));
            this.disposables.Add(updateListener);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.disposables.ForEach(x => x.Dispose());
            this.disposables.Clear();
        }

        /// <summary>
        ///     Tries to create a new <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task AddingPerson()
        {
            var thingsToCreate = new List<Person>();
            this.Person.EmailAddress.Add(this.EmailAddress);
            this.Person.TelephoneNumber.Add(this.TelephoneNumber);
            thingsToCreate.Add(this.Person);
            
            try
            {
                await this.SessionAnchor.CreateThingsSiteDirectory(thingsToCreate);
            }
            catch (Exception exception)
            {
                throw;
            }        
        }

        /// <summary>
        ///     Tries to deprecate a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeprecatingPerson(GridDataItemDeletingEventArgs e)
        {
            var personToDeprecate = new List<Person>();
            var deprecatedPerson = (Person)e.DataItem;
            var clonedPerson = deprecatedPerson.Clone(false);
            clonedPerson.IsDeprecated = true;
            personToDeprecate.Add(clonedPerson);
            try
            {
                await this.SessionAnchor.UpdateThingsSiteDirectory(personToDeprecate);
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public void OnInitializedAsync()
        {
            this.DataSource.Clear();
            this.DataSource.AddRange(this.SessionAnchor.Session.RetrieveSiteDirectory().Person);
            this.AvailableOrganizations = this.SessionAnchor.Session.RetrieveSiteDirectory().Organization;
            this.AvailablePersonRoles = this.SessionAnchor.Session.RetrieveSiteDirectory().PersonRole;
            this.AvailableDomains = this.SessionAnchor.Session.RetrieveSiteDirectory().Domain;
        }
    }
}
