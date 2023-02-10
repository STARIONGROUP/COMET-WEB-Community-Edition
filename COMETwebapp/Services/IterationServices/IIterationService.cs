// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIterationService.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.IterationServices
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model;

    /// <summary>
    /// Service to access iteration data
    /// </summary>
    public interface IIterationService
    {
        /// <summary>
        /// Save updates changes to avoid highlights after validation
        /// Save changes for each domain available in the opened session
        /// </summary>
        Dictionary<DomainOfExpertise, List<ParameterSubscriptionViewModel>> ValidatedUpdates { get; set; }

        /// <summary>
        /// Save Thing Iid with edit changes in the web application
        /// </summary>
        List<Guid> NewUpdates { get; set; }

        /// <summary>
        /// Get all <see cref="ParameterSubscription" /> of the given domain and for the given element
        /// </summary>
        /// <param name="element">The <see cref="ElementBase"> to get the subscriptions</param>
        /// <param name="currentDomainOfExpertise">The current <see cref="DomainOfExpertise" /> of the iteration</param>
        /// <returns>List of all <see cref="ParameterSubscription" /> for this element </returns>
        List<ParameterSubscription> GetParameterSubscriptionsByElement(ElementBase element, DomainOfExpertise? currentDomainOfExpertise);

        /// <summary>
        /// Gets number of updates in the iteration after a session refresh
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to get number of updates</param>
        /// <param name="currentDomainOfExpertise">The <see cref="DomainOfExpertise" /></param>
        /// <returns></returns>
        int GetNumberUpdates(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise);

        /// <summary>
        /// Gets list of parameter types used in the given iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> for which the <see cref="ParameterType" />s list is created</param>
        /// <returns>All <see cref="ParameterType" />s used in the iteration</returns>
        List<ParameterType> GetParameterTypes(Iteration? iteration);
    }
}
