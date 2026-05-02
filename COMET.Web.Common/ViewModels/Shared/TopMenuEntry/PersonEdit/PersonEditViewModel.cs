// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonEditViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit
{
    using System.Collections.ObjectModel;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model implementation backing the self-service "Edit my profile" dialog. Persists changes
    /// through <see cref="ISessionService.CreateOrUpdateThingsWithNotification(Thing, IReadOnlyCollection{Thing}, NotificationDescription)" />
    /// using the same clone-then-write pattern as <c>UserManagementTableViewModel</c> in the admin flow.
    /// </summary>
    public class PersonEditViewModel : DisposableObject, IPersonEditViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to load the SiteDirectory and submit writes.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="ILogger{T}" /> used to record any exception thrown by the save pipeline so the
        /// host popup is always restored to a sane state.
        /// </summary>
        private readonly ILogger<PersonEditViewModel> logger;

        /// <summary>
        /// Existing <see cref="EmailAddress" /> entries the user has marked for removal during the
        /// current edit session. Cleared on every <see cref="Initialize" /> call.
        /// </summary>
        private readonly List<EmailAddress> removedEmails = new();

        /// <summary>
        /// Existing <see cref="TelephoneNumber" /> entries the user has marked for removal during the
        /// current edit session. Cleared on every <see cref="Initialize" /> call.
        /// </summary>
        private readonly List<TelephoneNumber> removedTelephones = new();

        /// <summary>
        /// Backing field for <see cref="GivenName" />.
        /// </summary>
        private string givenName;

        /// <summary>
        /// Backing field for <see cref="Surname" />.
        /// </summary>
        private string surname;

        /// <summary>
        /// Backing field for <see cref="OrganizationalUnit" />.
        /// </summary>
        private string organizationalUnit;

        /// <summary>
        /// Backing field for <see cref="IsPasswordEditEnabled" />.
        /// </summary>
        private bool isPasswordEditEnabled;

        /// <summary>
        /// Backing field for <see cref="Password" />.
        /// </summary>
        private string password = string.Empty;

        /// <summary>
        /// Backing field for <see cref="PasswordConfirmation" />.
        /// </summary>
        private string passwordConfirmation = string.Empty;

        /// <summary>
        /// Backing field for <see cref="CurrentPerson" />.
        /// </summary>
        private Person currentPerson;

        /// <summary>
        /// Backing field for <see cref="IsLoading" />.
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Initializes a new <see cref="PersonEditViewModel" />.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /> used to load and persist.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> used to log save-pipeline exceptions.</param>
        public PersonEditViewModel(ISessionService sessionService, ILogger<PersonEditViewModel> logger)
        {
            this.sessionService = sessionService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the live <see cref="Person" /> currently bound to the form.
        /// </summary>
        public Person CurrentPerson
        {
            get => this.currentPerson;
            private set => this.RaiseAndSetIfChanged(ref this.currentPerson, value);
        }

        /// <summary>
        /// Gets or sets the editable Given Name.
        /// </summary>
        public string GivenName
        {
            get => this.givenName;
            set => this.RaiseAndSetIfChanged(ref this.givenName, value);
        }

        /// <summary>
        /// Gets or sets the editable Surname.
        /// </summary>
        public string Surname
        {
            get => this.surname;
            set => this.RaiseAndSetIfChanged(ref this.surname, value);
        }

        /// <summary>
        /// Gets or sets the editable Organizational Unit.
        /// </summary>
        public string OrganizationalUnit
        {
            get => this.organizationalUnit;
            set => this.RaiseAndSetIfChanged(ref this.organizationalUnit, value);
        }

        /// <summary>
        /// Gets the editable e-mail rows.
        /// </summary>
        public ObservableCollection<EmailAddressRowViewModel> EmailAddresses { get; } = new();

        /// <summary>
        /// Gets the editable telephone rows.
        /// </summary>
        public ObservableCollection<TelephoneNumberRowViewModel> TelephoneNumbers { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the password should be changed during this save.
        /// </summary>
        public bool IsPasswordEditEnabled
        {
            get => this.isPasswordEditEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isPasswordEditEnabled, value);
        }

        /// <summary>
        /// Gets or sets the new password to apply when <see cref="IsPasswordEditEnabled" /> is <c>true</c>.
        /// </summary>
        public string Password
        {
            get => this.password;
            set => this.RaiseAndSetIfChanged(ref this.password, value);
        }

        /// <summary>
        /// Gets or sets the password confirmation.
        /// </summary>
        public string PasswordConfirmation
        {
            get => this.passwordConfirmation;
            set => this.RaiseAndSetIfChanged(ref this.passwordConfirmation, value);
        }

        /// <summary>
        /// Gets a value indicating whether a save operation is currently in flight.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets a value indicating whether the form is in a saveable state.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.GivenName) || string.IsNullOrWhiteSpace(this.Surname))
                {
                    return false;
                }

                if (this.IsPasswordEditEnabled && (string.IsNullOrEmpty(this.Password) || this.Password != this.PasswordConfirmation))
                {
                    return false;
                }

                return this.EmailAddresses.All(x => !string.IsNullOrWhiteSpace(x.Value))
                       && this.TelephoneNumbers.All(x => !string.IsNullOrWhiteSpace(x.Value));
            }
        }

        /// <summary>
        /// Gets or sets the callback fired after a successful save.
        /// </summary>
        public EventCallback OnSaved { get; set; }

        /// <summary>
        /// Captures the supplied <see cref="Person" /> and projects its fields into the form state.
        /// </summary>
        /// <param name="person">The <see cref="Person" /> to edit.</param>
        public void Initialize(Person person)
        {
            this.CurrentPerson = person;

            this.GivenName = person?.GivenName;
            this.Surname = person?.Surname;
            this.OrganizationalUnit = person?.OrganizationalUnit;

            this.removedEmails.Clear();
            this.removedTelephones.Clear();

            this.EmailAddresses.Clear();
            this.TelephoneNumbers.Clear();

            if (person is null)
            {
                this.IsPasswordEditEnabled = false;
                this.Password = string.Empty;
                this.PasswordConfirmation = string.Empty;
                return;
            }

            foreach (var email in person.EmailAddress)
            {
                this.EmailAddresses.Add(new EmailAddressRowViewModel(email, person.DefaultEmailAddress == email));
            }

            foreach (var telephone in person.TelephoneNumber)
            {
                this.TelephoneNumbers.Add(new TelephoneNumberRowViewModel(telephone, person.DefaultTelephoneNumber == telephone));
            }

            this.IsPasswordEditEnabled = false;
            this.Password = string.Empty;
            this.PasswordConfirmation = string.Empty;
        }

        /// <summary>
        /// Adds an empty editable e-mail row.
        /// </summary>
        public void AddEmail()
        {
            this.EmailAddresses.Add(new EmailAddressRowViewModel());
        }

        /// <summary>
        /// Removes the supplied e-mail row, marking the original entry for deletion when applicable.
        /// </summary>
        /// <param name="row">The row to remove.</param>
        public void RemoveEmail(EmailAddressRowViewModel row)
        {
            if (row is null)
            {
                return;
            }

            if (row.Original is not null)
            {
                this.removedEmails.Add(row.Original);
            }

            this.EmailAddresses.Remove(row);
        }

        /// <summary>
        /// Adds an empty editable telephone row.
        /// </summary>
        public void AddTelephone()
        {
            this.TelephoneNumbers.Add(new TelephoneNumberRowViewModel());
        }

        /// <summary>
        /// Removes the supplied telephone row, marking the original entry for deletion when applicable.
        /// </summary>
        /// <param name="row">The row to remove.</param>
        public void RemoveTelephone(TelephoneNumberRowViewModel row)
        {
            if (row is null)
            {
                return;
            }

            if (row.Original is not null)
            {
                this.removedTelephones.Add(row.Original);
            }

            this.TelephoneNumbers.Remove(row);
        }

        /// <summary>
        /// Persists the edits.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous save.</returns>
        public async Task SaveAsync()
        {
            if (!this.IsValid || this.CurrentPerson is null)
            {
                return;
            }

            try
            {
                this.IsLoading = true;

                var siteDirectoryClone = this.sessionService.GetSiteDirectory().Clone(false);
                var personClone = this.CurrentPerson.Clone(false);

                personClone.GivenName = this.GivenName;
                personClone.Surname = this.Surname;
                personClone.OrganizationalUnit = this.OrganizationalUnit;

                if (this.IsPasswordEditEnabled)
                {
                    personClone.Password = this.Password;
                }

                var thingsToWrite = new List<Thing> { personClone };

                this.ApplyEmailEdits(personClone, thingsToWrite);
                this.ApplyTelephoneEdits(personClone, thingsToWrite);

                var result = await this.sessionService.CreateOrUpdateThingsWithNotification(siteDirectoryClone, thingsToWrite, GetNotificationDescription());

                if (result.IsSuccess)
                {
                    this.IsPasswordEditEnabled = false;
                    this.Password = string.Empty;
                    this.PasswordConfirmation = string.Empty;
                    await this.OnSaved.InvokeAsync();
                }
            }
            catch (Exception exception)
            {
                this.logger?.LogError(exception, "An error occurred while updating the active Person with iid {Iid}", this.CurrentPerson.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Applies the editable e-mail rows to the supplied <paramref name="personClone" />, adding new
        /// entries and edited clones to <paramref name="thingsToWrite" />, dropping removed originals from
        /// the cloned collection, and restoring <see cref="Person.DefaultEmailAddress" /> when one row is
        /// flagged as the default.
        /// </summary>
        /// <param name="personClone">The cloned <see cref="Person" /> being mutated for write.</param>
        /// <param name="thingsToWrite">The growing list of Things passed to the session-service write.</param>
        private void ApplyEmailEdits(Person personClone, ICollection<Thing> thingsToWrite)
        {
            foreach (var removed in this.removedEmails)
            {
                personClone.EmailAddress.Remove(removed);
                if (personClone.DefaultEmailAddress == removed)
                {
                    personClone.DefaultEmailAddress = null;
                }
            }

            EmailAddress newDefault = null;

            foreach (var row in this.EmailAddresses)
            {
                EmailAddress targetForDefault;

                if (row.Original is null)
                {
                    var created = new EmailAddress
                    {
                        Iid = Guid.NewGuid(),
                        VcardType = row.VcardType,
                        Value = row.Value
                    };

                    personClone.EmailAddress.Add(created);
                    thingsToWrite.Add(created);
                    targetForDefault = created;
                }
                else
                {
                    var changed = row.Original.VcardType != row.VcardType || row.Original.Value != row.Value;

                    if (changed)
                    {
                        var clone = row.Original.Clone(false);
                        clone.VcardType = row.VcardType;
                        clone.Value = row.Value;

                        var existing = personClone.EmailAddress.FirstOrDefault(x => x.Iid == row.Original.Iid);
                        if (existing is not null)
                        {
                            personClone.EmailAddress.Remove(existing);
                        }

                        personClone.EmailAddress.Add(clone);
                        thingsToWrite.Add(clone);
                        targetForDefault = clone;
                    }
                    else
                    {
                        targetForDefault = row.Original;
                    }
                }

                if (row.IsDefault)
                {
                    newDefault = targetForDefault;
                }
            }

            if (newDefault is not null)
            {
                personClone.DefaultEmailAddress = newDefault;
            }
        }

        /// <summary>
        /// Telephone counterpart of <see cref="ApplyEmailEdits" />.
        /// </summary>
        /// <param name="personClone">The cloned <see cref="Person" /> being mutated for write.</param>
        /// <param name="thingsToWrite">The growing list of Things passed to the session-service write.</param>
        private void ApplyTelephoneEdits(Person personClone, ICollection<Thing> thingsToWrite)
        {
            foreach (var removed in this.removedTelephones)
            {
                personClone.TelephoneNumber.Remove(removed);
                if (personClone.DefaultTelephoneNumber == removed)
                {
                    personClone.DefaultTelephoneNumber = null;
                }
            }

            TelephoneNumber newDefault = null;

            foreach (var row in this.TelephoneNumbers)
            {
                TelephoneNumber targetForDefault;

                if (row.Original is null)
                {
                    var created = new TelephoneNumber
                    {
                        Iid = Guid.NewGuid(),
                        Value = row.Value
                    };

                    created.VcardType.AddRange(row.VcardType);
                    personClone.TelephoneNumber.Add(created);
                    thingsToWrite.Add(created);
                    targetForDefault = created;
                }
                else
                {
                    var changed = !row.Original.VcardType.SequenceEqual(row.VcardType) || row.Original.Value != row.Value;

                    if (changed)
                    {
                        var clone = row.Original.Clone(false);
                        clone.VcardType.Clear();
                        clone.VcardType.AddRange(row.VcardType);
                        clone.Value = row.Value;

                        var existing = personClone.TelephoneNumber.FirstOrDefault(x => x.Iid == row.Original.Iid);
                        if (existing is not null)
                        {
                            personClone.TelephoneNumber.Remove(existing);
                        }

                        personClone.TelephoneNumber.Add(clone);
                        thingsToWrite.Add(clone);
                        targetForDefault = clone;
                    }
                    else
                    {
                        targetForDefault = row.Original;
                    }
                }

                if (row.IsDefault)
                {
                    newDefault = targetForDefault;
                }
            }

            if (newDefault is not null)
            {
                personClone.DefaultTelephoneNumber = newDefault;
            }
        }

        /// <summary>
        /// Builds the success / error <see cref="NotificationDescription" /> shown after the save attempt.
        /// </summary>
        /// <returns>The <see cref="NotificationDescription" /> describing the outcome.</returns>
        private static NotificationDescription GetNotificationDescription()
        {
            return new NotificationDescription
            {
                OnSuccess = "Profile updated",
                OnError = "Failed to update profile"
            };
        }
    }
}
