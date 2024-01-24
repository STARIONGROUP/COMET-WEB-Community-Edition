// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISessionService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Services.SessionManagement
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using DynamicData;

    using FluentResults;

    /// <summary>
    /// The <see cref="ISessionService" /> interface provides access to an <see cref="ISession" />
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
        /// Gets a readonly collection of open <see cref="EngineeringModel" />
        /// </summary>
        IReadOnlyCollection<EngineeringModel> OpenEngineeringModels { get; }

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
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> ReadIteration(IterationSetup iterationSetup, DomainOfExpertise domain);

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
        /// <returns>An asynchronous operation</returns>
        Task RefreshSession();

        /// <summary>
        /// Switches the current domain for an opened iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="domainOfExpertise">The domain</param>
        void SwitchDomain(Iteration iteration, DomainOfExpertise domainOfExpertise);

        /// <summary>
        /// Write a new Thing in an <see cref="Iteration"/>
        /// </summary>
        /// <param name="container">the <see cref="Thing"/> container where the <param name="thingToCreate"></param> should be created</param>
        /// <param name="thingToCreate">the thing to create in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> CreateThing(Thing container, Thing thingToCreate);

        /// <summary>
        /// Write new Things in an <see cref="Iteration"/>
        /// </summary>
        /// <param name="container">the <see cref="Thing"/> container where the <param name="thingsToCreate"></param> should be created</param>
        /// <param name="thingsToCreate">the things to create in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> CreateThings(Thing container, params Thing[] thingsToCreate);

        /// <summary>
        /// Write new Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be created</param>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> CreateThings(Thing container, IEnumerable<Thing> thingsToCreate);

        /// <summary>
        /// Write updated Thing in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingToUpdate">the thing to update in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> UpdateThing(Thing container, Thing thingToUpdate);

        /// <summary>
        /// Write updated Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> UpdateThings(Thing container, params Thing[] thingsToUpdate);

        /// <summary>
        /// Write updated Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> UpdateThings(Thing container, IEnumerable<Thing> thingsToUpdate);

        /// <summary>
        /// Deletes a <see cref="Thing"/> from it's container
        /// </summary>
        /// <param name="containerClone">the container clone of the thing to delete</param>
        /// <param name="thingToDelete">the thing to delete in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> DeleteThing(Thing containerClone, Thing thingToDelete);

        /// <summary>
        /// Deletes a collection of <see cref="Thing"/> from it's container
        /// </summary>
        /// <param name="containerClone">the container clone of the thing to delete</param>
        /// <param name="thingsToDelete">the things to delete in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> DeleteThings(Thing containerClone, params Thing[] thingsToDelete);

        /// <summary>
        /// Deletes a collection <see cref="Thing"/> from it's container
        /// </summary>
        /// <param name="containerClone">the container clone of the thing to delete</param>
        /// <param name="thingsToDelete">the things to delete in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result"/></returns>
        Task<Result> DeleteThings(Thing containerClone, IEnumerable<Thing> thingsToDelete);

        /// <summary>
        /// Gets the <see cref="ParticipantRole" /> inside an iteration
        /// </summary>
        /// <returns>the <see cref="Participant"/></returns>
        Participant GetParticipant(Iteration iteration);

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> for an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The <see cref="DomainOfExpertise" /></returns>
        /// <exception cref="ArgumentException">If the <see cref="Iteration" /> is not opened</exception>
        /// <returns>The <see cref="DomainOfExpertise"/></returns>
        DomainOfExpertise GetDomainOfExpertise(Iteration iteration);
    }
}
