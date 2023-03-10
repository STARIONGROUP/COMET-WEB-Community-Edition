// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISessionService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace COMETwebapp.Services.SessionManagement
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using DynamicData;

    /// <summary>
    /// The <see cref="ISessionService"/> interface provides access to an <see cref="ISession"/>
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Gets or sets the <see cref="ISession" />
        /// </summary>
        ISession Session { get; set; }

        /// <summary>
        /// A reactive collection of opened <see cref="Iteration" />
        /// </summary>
        SourceList<Iteration> OpenIterations { get; }

        /// <summary>
        /// True if the <see cref="ISession" /> is opened
        /// </summary>
        bool IsSessionOpen { get; set; }

        /// <summary>
        /// Event for when the session has been refreshed.
        /// </summary>
        event EventHandler OnSessionRefreshed;

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory" /> that is loaded in the <see cref="ISession" />
        /// </summary>
        /// <returns>The <see cref="SiteDirectory" /></returns>
        SiteDirectory GetSiteDirectory();

        /// <summary>
        /// Close the ISession
        /// </summary>
        /// <returns>a <see cref="Task" /></returns>
        Task Close();

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup" /> and <see cref="IterationSetup" />
        /// </summary>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        Task ReadIteration(IterationSetup iterationSetup, DomainOfExpertise domain);

        /// <summary>
        /// Close all the opened <see cref="Iteration" />
        /// </summary>
        void CloseIterations();

        /// <summary>
        /// Closes an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        void CloseIteration(Iteration iteration);

        /// <summary>
        /// Get <see cref="EngineeringModelSetup" /> available for the ActivePerson
        /// </summary>
        /// <returns>
        /// A container of <see cref="EngineeringModelSetup" />
        /// </returns>
        IEnumerable<EngineeringModelSetup> GetParticipantModels();

        /// <summary>
        /// Get <see cref="DomainOfExpertise" /> available for the active person in the selected
        /// <see cref="EngineeringModelSetup" />
        /// </summary>
        /// <param name="modelSetup">The selected <see cref="EngineeringModelSetup" /></param>
        /// <returns>
        /// A container of <see cref="DomainOfExpertise" /> accessible for the active person
        /// </returns>
        IEnumerable<DomainOfExpertise> GetModelDomains(EngineeringModelSetup modelSetup);

        /// <summary>
        /// Refresh the ISession object
        /// </summary>
        Task RefreshSession();

        /// <summary>
        /// Switches the current domain for an opened iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="domainOfExpertise">The domain</param>
        void SwitchDomain(Iteration iteration, DomainOfExpertise domainOfExpertise);

        /// <summary>
        /// Write new Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> where the <see cref="Thing" />s should be created</param>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        Task CreateThings(Iteration iteration, IEnumerable<Thing> thingsToCreate);

        /// <summary>
        /// Write updated Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        Task UpdateThings(Iteration iteration, IEnumerable<Thing> thingsToUpdate);

        /// <summary>
        /// Gets the <see cref="ParticipantRole" /> inside an iteration
        /// </summary>
        Participant GetParticipant(Iteration iteration);

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> for an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The <see cref="DomainOfExpertise" /></returns>
        /// <exception cref="ArgumentException">If the <see cref="Iteration" /> is not opened</exception>
        DomainOfExpertise GetDomainOfExpertise(Iteration iteration);

        /// <summary>
        ///     Write new Things in the session
        /// </summary>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        Task CreateThingsSiteDirectory(IEnumerable<Thing> thingsToCreate);

        /// <summary>
        ///     Update Things in the session
        /// </summary>
        /// <param name="thingsToDelete">List of Things to update in the session</param>
        Task UpdateThingsSiteDirectory(IEnumerable<Thing> thingsToDelete);
    }
}
