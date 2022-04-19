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
        /// The opened <see cref="Iteration"/>
        /// </summary>
        Iteration OpenIteration { get; set; }

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> selected to open a model
        /// </summary>
        DomainOfExpertise CurrentDomainOfExpertise { get; set; }

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory"/> that is loaded in the <see cref="ISession"/>
        /// </summary>
        /// <returns>The <see cref="SiteDirectory"/></returns>
        SiteDirectory GetSiteDirectory();

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup"/> and <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="modelSetup"> The selected <see cref="EngineeringModelSetup"/> </param>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup"/></param>
        void GetIteration(EngineeringModelSetup modelSetup, IterationSetup iterationSetup);

        /// <summary>
        /// Close the <see cref="OpenIteration"/>
        /// </summary>
        void CloseIteration();
    }
}
