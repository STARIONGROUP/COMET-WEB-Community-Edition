// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserManagementPageViewModel.cs" company="RHEA System S.A.">
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
    
    using DevExpress.Blazor;
    
    using DynamicData;

    /// <summary>
    ///     Interface definition for <see cref="UserManagementPageViewModel" />
    /// </summary>
    public interface IUserManagementPageViewModel
    {
        /// <summary>
        ///     The <see cref="Person" /> to create
        /// </summary>
        Person Person { get; set; }

        /// <summary>
        ///     The <see cref="EmailAddress" /> to create
        /// </summary>
        EmailAddress EmailAddress { get; set; }

        /// <summary>
        ///     The <see cref="TelephoneNumber" /> to create
        /// </summary>
        TelephoneNumber TelephoneNumber { get; set; } 

        /// <summary>
        ///     Gets or sets the data source for the grid control.
        /// </summary>
        SourceList<Person> DataSource { get; }

        /// <summary>
        ///    Available <see cref="Organization"/>s
        /// </summary>
        IEnumerable<Organization> AvailableOrganizations { get; set; }

        /// <summary>
        ///    Available <see cref="PersonRole"/>s
        /// </summary>
        IEnumerable<PersonRole> AvailablePersonRoles { get; set; }

        /// <summary>
        ///    Available <see cref="DomainOfExpertise"/>s
        /// </summary>
        IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

        /// <summary>
        ///    Available <see cref="VcardEmailAddressKind"/>s
        /// </summary>
        IEnumerable<VcardEmailAddressKind> EmailAddressKinds { get; set; }

        /// <summary>
        ///    Available <see cref="VcardTelephoneNumberKind"/>s
        /// </summary>
        IEnumerable<VcardTelephoneNumberKind> TelephoneNumberKinds { get; set; }

        /// <summary>
        ///     Tries to create a new <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task Grid_ModelSaving();

        /// <summary>
        ///     Tries to deprecate a <see cref="Person" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task Grid_DataItemDeprecating(GridDataItemDeletingEventArgs e);

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        void OnInitializedAsync();
    }
}
