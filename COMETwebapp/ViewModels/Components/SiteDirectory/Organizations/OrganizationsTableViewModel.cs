// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationsTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.Organizations
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="Organization" />
    /// </summary>
    public class OrganizationsTableViewModel : DeprecatableDataItemTableViewModel<Organization, OrganizationRowViewModel>, IOrganizationsTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public OrganizationsTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, ILogger<OrganizationsTableViewModel> logger)
            : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
            this.Thing = new Organization();
        }

        /// <summary>
        /// Creates or edits an <see cref="Organization"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Organization"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditOrganization(bool shouldCreate)
        {
            this.IsLoading = true;

            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
            var thingsToCreate = new List<Thing>();

            if (shouldCreate)
            {
                siteDirectoryClone.Organization.Add(this.Thing);
                thingsToCreate.Add(siteDirectoryClone);
            }

            thingsToCreate.Add(this.Thing);
            await this.SessionService.CreateOrUpdateThingsWithNotification(siteDirectoryClone, thingsToCreate);

            this.IsLoading = false;
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<Organization> QueryListOfThings()
        {
            return this.SessionService.GetSiteDirectory().Organization;
        }
    }
}
