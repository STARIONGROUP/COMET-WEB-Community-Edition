// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIterationService.cs" company="RHEA System S.A.">
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
        /// Get all <see cref="ParameterValueSet"/> of the given iteration
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ParameterValueSet"/>s list is created
        /// </param>
        /// <returns>All <see cref="ParameterValueSet"/></returns>
        List<ParameterValueSet> GetParameterValueSets(Iteration? iteration);

        /// <summary>
        /// Get all <see cref="NestedElement"/> of the given iteration for all options
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="NestedElement"/>s list is created
        /// </param>
        /// <returns>All <see cref="NestedElement"/></returns>
        List<NestedElement> GetNestedElements(Iteration? iteration);

        /// <summary>
        /// Get the nested parameters from the given option
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="NestedParameter"/>s list is created
        /// </param>
        /// <param name="optionIid">
        /// The Iid of the option for which the <see cref="NestedParameter"/>s list is created
        /// </param>
        /// <returns>All<see cref="NestedParameter"/> of the given option</returns>
        List<NestedParameter> GetNestedParameters(Iteration? iteration, Guid? optionIid);

        /// <summary>
        /// Get unused elements defintion of the opened iteration
        /// An unused element is an element not used in an option
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ElementDefinition"/>s list is created
        /// </param>
        /// <returns>All unused <see cref="ElementDefinition"/></returns>
        List<ElementDefinition> GetUnusedElementDefinitions(Iteration? iteration);

        /// <summary>
        /// Get all the unreferenced element definitions in the opened iteration
        /// An unreferenced element is an element with no associated ElementUsage
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the <see cref="ElementDefinition"/>s list is created
        /// </param>
        /// <returns>All unreferenced <see cref="ElementDefinition"/></returns>
        List<ElementDefinition> GetUnreferencedElements(Iteration? iteration);

        /// <summary>
        /// Get all <see cref="ParameterSubscription"/> by the given domain in the given iteration 
        /// </summary>
        /// <param name="iteration">The opened <see cref="Iteration"/></param>
        /// <param name="currentDomainOfExpertise">The current <see cref="DomainOfExpertise"/> of the iteration</param>
        /// <returns>List of all <see cref="ParameterSubscription"/></returns>
        List<ParameterSubscription> GetParameterSubscriptions(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise);

        /// <summary>
        /// Gets all <see cref="Parameter"/> owned by the given <see cref="DomainOfExpertise"/> and subscribed by other <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to get <see cref="Parameter"/></param>
        /// <param name="currentDomainOfExpertise">The <see cref="DomainOfExpertise"/></param>
        /// <returns>Subscribed <see cref="Parameter"/> owned by the given <see cref="DomainOfExpertise"/></returns>
        List<Parameter> GetCurrentDomainSubscribedParameters(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise);

        /// <summary>
        /// Gets number of updates in the iteration after a session refresh
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to get number of updates</param>
        /// <param name="currentDomainOfExpertise">The <see cref="DomainOfExpertise"/></param>
        /// <returns></returns>
        int GetNumberUpdates(Iteration? iteration, DomainOfExpertise? currentDomainOfExpertise);
    }
}
