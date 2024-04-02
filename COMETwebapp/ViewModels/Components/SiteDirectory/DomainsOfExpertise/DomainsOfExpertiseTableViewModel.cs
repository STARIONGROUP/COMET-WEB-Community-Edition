// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainsOfExpertiseTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.DomainsOfExpertise
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="DomainOfExpertise" />
    /// </summary>
    public class DomainsOfExpertiseTableViewModel : DeprecatableDataItemTableViewModel<DomainOfExpertise, DomainOfExpertiseRowViewModel>, IDomainsOfExpertiseTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public DomainsOfExpertiseTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, ILogger<DomainsOfExpertiseTableViewModel> logger)
            : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
            this.Thing = new DomainOfExpertise();
        }

        /// <summary>
        /// Creates or edits a <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="DomainOfExpertise"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditDomainOfExpertise(bool shouldCreate)
        {
            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
            var thingsToCreate = new List<Thing>();

            if (shouldCreate)
            {
                siteDirectoryClone.Domain.Add(this.Thing);
                thingsToCreate.Add(siteDirectoryClone);
            }

            thingsToCreate.Add(this.Thing);
            await this.SessionService.UpdateThings(siteDirectoryClone, thingsToCreate);
            await this.SessionService.RefreshSession();
        }
    }
}
