// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantRolesTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SiteDirectory.Roles
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="ParticipantRole" />
    /// </summary>
    public class ParticipantRolesTableViewModel : DeprecatableDataItemTableViewModel<ParticipantRole, ParticipantRoleRowViewModel>, IParticipantRolesTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRolesTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public ParticipantRolesTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, ILogger<ParticipantRolesTableViewModel> logger)
            : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
            this.CurrentThing = new ParticipantRole();
        }

        /// <summary>
        /// Gets the participant permission access kinds
        /// </summary>
        public IEnumerable<ParticipantAccessRightKind> ParticipantAccessKinds { get; private set; } = [ParticipantAccessRightKind.NONE, ParticipantAccessRightKind.MODIFY, ParticipantAccessRightKind.MODIFY_IF_OWNER, ParticipantAccessRightKind.READ];

        /// <summary>
        /// Creates or edits a <see cref="ParticipantRole" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="ParticipantRole" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditParticipantRole(bool shouldCreate)
        {
            try
            {
                this.IsLoading = true;

                var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
                var thingsToCreate = new List<Thing>();

                thingsToCreate.AddRange(this.CurrentThing.ParticipantPermission);

                if (shouldCreate)
                {
                    siteDirectoryClone.ParticipantRole.Add(this.CurrentThing);
                    thingsToCreate.Add(siteDirectoryClone);
                }

                thingsToCreate.Add(this.CurrentThing);
                await this.SessionService.CreateOrUpdateThingsWithNotification(siteDirectoryClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Create or Update ParticipantRole failed");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<ParticipantRole> QueryListOfThings()
        {
            return this.SessionService.GetSiteDirectory().ParticipantRole;
        }
    }
}
