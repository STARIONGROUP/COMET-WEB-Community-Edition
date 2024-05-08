// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.UserManagement
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model used to manage <see cref="Person" />
    /// </summary>
    public class UserManagementTableViewModel : DeprecatableDataItemTableViewModel<Person, PersonRowViewModel>, IUserManagementTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public UserManagementTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, 
            ILogger<UserManagementTableViewModel> logger) : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
            this.Thing = new Person();

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner =>
                {
                    this.Thing.DefaultDomain = selectedOwner;
                })
            };
        }

        /// <summary>
        /// The <see cref="EmailAddress" /> to create
        /// </summary>
        public EmailAddress EmailAddress { get; set; } = new();

        /// <summary>
        /// The <see cref="TelephoneNumber" /> to create
        /// </summary>
        public TelephoneNumber TelephoneNumber { get; set; } = new();

        /// <summary>
        /// Available <see cref="Organization" />s
        /// </summary>
        public IEnumerable<Organization> AvailableOrganizations { get; set; }

        /// <summary>
        /// Available <see cref="PersonRole" />s
        /// </summary>
        public IEnumerable<PersonRole> AvailablePersonRoles { get; set; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

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
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        public override void InitializeViewModel()
        {
            var siteDirectory = this.SessionService.Session.RetrieveSiteDirectory();
            this.AvailableOrganizations = siteDirectory.Organization;
            this.AvailablePersonRoles = siteDirectory.PersonRole;

            base.InitializeViewModel();
        }

        /// <summary>
        /// Selects the current <see cref="Person" />
        /// </summary>
        /// <param name="person">The person to be set</param>
        public void SelectPerson(Person person)
        {
            this.Thing = person.Clone(true);
            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(person.Iid == Guid.Empty, person.DefaultDomain);
        }

        /// <summary>
        /// Tries to create or edit an existing <see cref="Thing"/>
        /// </summary>
        /// <param name="shouldCreate">Value to check if the current Person should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditPerson(bool shouldCreate)
        {
            var thingsToCreate = new List<Thing>();

            if (!string.IsNullOrWhiteSpace(this.EmailAddress.Value))
            {
                this.Thing.EmailAddress.Add(this.EmailAddress);
                thingsToCreate.Add(this.EmailAddress);
            }

            if (!string.IsNullOrWhiteSpace(this.TelephoneNumber.Value))
            {
                this.Thing.TelephoneNumber.Add(this.TelephoneNumber);
                thingsToCreate.Add(this.TelephoneNumber);
            }

            if (this.IsDefaultEmail)
            {
                this.Thing.DefaultEmailAddress = this.EmailAddress;
            }

            if (this.IsDefaultTelephoneNumber)
            {
                this.Thing.DefaultTelephoneNumber = this.TelephoneNumber;
            }

            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);

            if (shouldCreate)
            {
                siteDirectoryClone.Person.Add(this.Thing);
                thingsToCreate.Add(siteDirectoryClone);
            }

            thingsToCreate.Add(this.Thing);
            await this.SessionService.CreateOrUpdateThings(siteDirectoryClone, thingsToCreate);
            await this.SessionService.RefreshSession();
            this.ResetFields();
        }

        /// <summary>
        /// Tries to activate or disactivate a <see cref="Thing" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ActivateOrDeactivatePerson(GridDataColumnCellDisplayTemplateContext context, bool value)
        {
            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
            var personRow = (PersonRowViewModel)context.DataItem;
            var personToUpdate = personRow.Thing;
            var clonedPerson = personToUpdate.Clone(false);
            clonedPerson.IsActive = value;

            await this.SessionService.CreateOrUpdateThings(siteDirectoryClone, [clonedPerson]);
            await this.SessionService.RefreshSession();
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        protected override void RefreshAccessRight()
        {
            this.IsLoading = true;

            foreach (var row in this.Rows.Items)
            {
                row.IsAllowedToWrite = row.Thing.Iid != this.SessionService.Session.ActivePerson.Iid
                                       && this.PermissionService.CanWrite(ClassKind.Person, this.SessionService.GetSiteDirectory());
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Method that resets all form fields
        /// </summary>
        private void ResetFields()
        {
            this.EmailAddress = new EmailAddress();
            this.TelephoneNumber = new TelephoneNumber();
            this.IsDefaultEmail = false;
            this.IsDefaultTelephoneNumber = false;
        }
    }
}
