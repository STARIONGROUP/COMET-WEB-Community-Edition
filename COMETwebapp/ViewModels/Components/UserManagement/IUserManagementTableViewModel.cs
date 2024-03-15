// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserManagementTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.UserManagement
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.UserManagement.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    /// <summary>
    /// View model used to manage <see cref="Person" />
    /// </summary>
    public interface IUserManagementTableViewModel : IApplicationBaseViewModel, IHaveReusableRows
    {
        /// <summary>
        /// The <see cref="Person" /> to create
        /// </summary>
        Person Person { get; set; }

        /// <summary>
        /// The <see cref="EmailAddress" /> to create
        /// </summary>
        EmailAddress EmailAddress { get; set; }

        /// <summary>
        /// The <see cref="TelephoneNumber" /> to create
        /// </summary>
        TelephoneNumber TelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        SourceList<Person> DataSource { get; }

        /// <summary>
        /// A reactive collection of <see cref="PersonRowViewModel" />
        /// </summary>
        SourceList<PersonRowViewModel> Rows { get; }

        /// <summary>
        /// Available <see cref="Organization" />s
        /// </summary>
        IEnumerable<Organization> AvailableOrganizations { get; set; }

        /// <summary>
        /// Available <see cref="PersonRole" />s
        /// </summary>
        IEnumerable<PersonRole> AvailablePersonRoles { get; set; }

        /// <summary>
        /// Available <see cref="DomainOfExpertise" />s
        /// </summary>
        IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

        /// <summary>
        /// Available <see cref="VcardEmailAddressKind" />s
        /// </summary>
        IEnumerable<VcardEmailAddressKind> EmailAddressKinds { get; set; }

        /// <summary>
        /// Indicates if the <see cref="EmailAddress" /> is the default email address
        /// </summary>
        bool IsDefaultEmail { get; set; }

        /// <summary>
        /// Available <see cref="VcardTelephoneNumberKind" />s
        /// </summary>
        IEnumerable<VcardTelephoneNumberKind> TelephoneNumberKinds { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Indicates if the <see cref="TelephoneNumber" /> is the default telephone number
        /// </summary>
        bool IsDefaultTelephoneNumber { get; set; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnDeprecationMode { get; set; }

        /// <summary>
        /// Popup message dialog
        /// </summary>
        string PopupDialog { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a person should be created
        /// </summary>
        bool ShouldCreatePerson { get; set; }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task OnConfirmButtonClick();

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="Person" />
        /// </summary>
        void OnCancelButtonClick();

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="personRow">A <see cref="PersonRowViewModel" /> that represents the person to deprecate or undeprecate </param>
        void OnDeprecateUnDeprecateButtonClick(PersonRowViewModel personRow);

        /// <summary>
        /// Tries to activate or disactivate a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ActivatingOrDisactivatingPerson(GridDataColumnCellDisplayTemplateContext context, bool value);

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        void OnInitialized();

        /// <summary>
        /// Tries to create or edit an existing <see cref="UserManagementTableViewModel.Person"/>, based on the <see cref="UserManagementTableViewModel.ShouldCreatePerson"/> property
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task CreatingOrEditingPerson();
    }
}
