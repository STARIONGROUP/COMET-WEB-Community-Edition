// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IPersonEditViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Utilities.DisposableObject;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model driving the self-service "Edit my profile" dialog. Wraps the active
    /// <see cref="Person" /> and exposes the editable surface (identity, contact information, password)
    /// plus a save pipeline that writes the changes back through the existing session-service write API.
    /// </summary>
    public interface IPersonEditViewModel : IDisposableObject
    {
        /// <summary>
        /// Gets the live <see cref="Person" /> currently bound to the form, captured by
        /// <see cref="Initialize" />. <c>null</c> until <see cref="Initialize" /> is called.
        /// </summary>
        Person CurrentPerson { get; }

        /// <summary>
        /// Gets or sets the editable Given Name. Initialized from <see cref="CurrentPerson" /> by
        /// <see cref="Initialize" /> and applied to the cloned person on save.
        /// </summary>
        string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the editable Surname. Initialized from <see cref="CurrentPerson" /> by
        /// <see cref="Initialize" /> and applied to the cloned person on save.
        /// </summary>
        string Surname { get; set; }

        /// <summary>
        /// Gets or sets the editable Organizational Unit. Initialized from <see cref="CurrentPerson" />
        /// by <see cref="Initialize" /> and applied to the cloned person on save.
        /// </summary>
        string OrganizationalUnit { get; set; }

        /// <summary>
        /// Gets the editable e-mail rows. Each row preserves a reference to its underlying SDK
        /// <see cref="EmailAddress" /> when one exists; rows added in the form have <c>null</c>
        /// originals and are persisted as new entries.
        /// </summary>
        ObservableCollection<EmailAddressRowViewModel> EmailAddresses { get; }

        /// <summary>
        /// Gets the editable telephone rows.
        /// </summary>
        ObservableCollection<TelephoneNumberRowViewModel> TelephoneNumbers { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has opted in to changing the password during
        /// this save. When <c>false</c>, the cloned person's <see cref="Person.Password" /> is left
        /// untouched.
        /// </summary>
        bool IsPasswordEditEnabled { get; set; }

        /// <summary>
        /// Gets or sets the new password to apply when <see cref="IsPasswordEditEnabled" /> is <c>true</c>.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets the password confirmation. Must equal <see cref="Password" /> for
        /// <see cref="IsValid" /> to be <c>true</c> when the password is being changed.
        /// </summary>
        string PasswordConfirmation { get; set; }

        /// <summary>
        /// Gets a value indicating whether the form's combined state is acceptable for persistence:
        /// <see cref="GivenName" /> and <see cref="Surname" /> non-empty, no contact row has an empty
        /// value, and — if the password is being changed — the password and confirmation match.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether a save operation is currently in flight. Bound by the form's
        /// loading indicator and used to disable the Save button while the SDK call runs.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Gets or sets the callback fired after a successful save so the host (the SessionMenu popup)
        /// can close itself.
        /// </summary>
        EventCallback OnSaved { get; set; }

        /// <summary>
        /// Captures the supplied <see cref="Person" /> as <see cref="CurrentPerson" />, projects its
        /// identity / contact fields into the editable properties, resets the password state, and
        /// repopulates the row collections from the person's existing emails and telephone numbers.
        /// </summary>
        /// <param name="person">The active session's <see cref="Person" /> the form should edit.</param>
        void Initialize(Person person);

        /// <summary>
        /// Adds an empty editable e-mail row so the user can fill in a new entry. The row's
        /// <see cref="EmailAddressRowViewModel.Original" /> is <c>null</c>, marking it as a create on save.
        /// </summary>
        void AddEmail();

        /// <summary>
        /// Removes the supplied row. New rows (those with no <see cref="EmailAddressRowViewModel.Original" />)
        /// are simply dropped from the collection; existing rows are remembered so the save pipeline can
        /// also delete them on the server.
        /// </summary>
        /// <param name="row">The row to remove.</param>
        void RemoveEmail(EmailAddressRowViewModel row);

        /// <summary>
        /// Telephone counterpart of <see cref="AddEmail" />.
        /// </summary>
        void AddTelephone();

        /// <summary>
        /// Telephone counterpart of <see cref="RemoveEmail" />.
        /// </summary>
        /// <param name="row">The row to remove.</param>
        void RemoveTelephone(TelephoneNumberRowViewModel row);

        /// <summary>
        /// Persists the edits via <c>ISessionService.CreateOrUpdateThingsWithNotification</c>. On success
        /// fires <see cref="OnSaved" /> and clears the password fields. Exceptions are logged and never
        /// propagated so the host popup is always restored to a sane state.
        /// </summary>
        /// <returns>A <see cref="Task" /> that completes when the save attempt has finished.</returns>
        Task SaveAsync();
    }
}
