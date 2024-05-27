// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserManagementTableViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    /// <summary>
    /// View model used to manage <see cref="Person" />
    /// </summary>
    public interface IUserManagementTableViewModel : IDeprecatableDataItemTableViewModel<Person, PersonRowViewModel>
    {
        /// <summary>
        /// The <see cref="EmailAddress" /> to create
        /// </summary>
        EmailAddress EmailAddress { get; set; }

        /// <summary>
        /// The <see cref="TelephoneNumber" /> to create
        /// </summary>
        TelephoneNumber TelephoneNumber { get; set; }

        /// <summary>
        /// Available <see cref="Organization" />s
        /// </summary>
        IEnumerable<Organization> AvailableOrganizations { get; set; }

        /// <summary>
        /// Available <see cref="PersonRole" />s
        /// </summary>
        IEnumerable<PersonRole> AvailablePersonRoles { get; set; }

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
        /// Indicates if the <see cref="TelephoneNumber" /> is the default telephone number
        /// </summary>
        bool IsDefaultTelephoneNumber { get; set; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Tries to activate or disactivate a <see cref="Thing" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ActivateOrDeactivatePerson(GridDataColumnCellDisplayTemplateContext context, bool value);

        /// <summary>
        /// Tries to create or edit an existing <see cref="Thing"/>
        /// </summary>
        /// <param name="shouldCreate">Value to check if the current Person should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        Task CreateOrEditPerson(bool shouldCreate);
    }
}
