// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISessionAnchor.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.SessionManagement
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// The <see cref="ISessionAnchor"/> interface provides access to an <see cref="ISession"/>
    /// </summary>
    public interface ISessionAnchor
    {
        /// <summary>
        /// Gets or sets the <see cref="ISession"/>
        /// </summary>
        ISession Session { get; set; }

        /// <summary>
        /// True if the <see cref="ISession"/> is opened
        /// </summary>
        bool IsSessionOpen { get; set; }

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> selected to open a model
        /// </summary>
        DomainOfExpertise? CurrentDomainOfExpertise { get; set; }

        /// <summary>
        /// Close the ISession
        /// </summary>
        /// <returns>a <see cref="Task"/></returns>
        Task Close();

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory"/> that is loaded in the <see cref="ISession"/>
        /// </summary>
        /// <returns>The <see cref="SiteDirectory"/></returns>
        SiteDirectory GetSiteDirectory();

        /// <summary>
        /// Returns the opened <see cref="Iteration"/> in the Session
        /// </summary>
        /// <returns>An <see cref="Iteration"/></returns>
        Iteration? GetIteration();

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup"/> and <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="modelSetup"> The selected <see cref="EngineeringModelSetup"/> </param>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup"/></param>
        Task SetOpenIteration(EngineeringModelSetup? modelSetup, IterationSetup? iterationSetup);

        /// <summary>
        /// Close the <see cref="OpenIteration"/>
        /// </summary>
        void CloseIteration();

        /// <summary>
        /// Get <see cref="EngineeringModelSetup"/> available for the ActivePerson
        /// </summary>
        /// <returns>
        /// A container of <see cref="EngineeringModelSetup"/>
        /// </returns>
        IEnumerable<EngineeringModelSetup> GetParticipantModels();

        /// <summary>
        /// Get <see cref="DomainOfExpertise"/> available in the selected <see cref="EngineeringModelSetup"/>
        /// </summary>
        /// <param name="modelSetup">The selected <see cref="EngineeringModelSetup"/></param>
        /// <returns>
        /// A container of <see cref="DomainOfExpertise"/>
        /// </returns>
        IEnumerable<DomainOfExpertise> GetModelDomains(EngineeringModelSetup? modelSetup);

        /// <summary>
        /// Refresh the ISession object
        /// </summary>
        Task RefreshSession();

        /// <summary>
        /// Switches the current domain for the opened iteration
        /// </summary>
        /// <param name="DomainOfExpertise">The domain</param>
        void SwitchDomain(DomainOfExpertise? DomainOfExpertise);
    }
}
