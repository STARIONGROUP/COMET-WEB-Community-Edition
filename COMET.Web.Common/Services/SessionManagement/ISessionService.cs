// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISessionService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Dal.Operations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.NotificationService;

    using DynamicData;

    using FluentResults;

    /// <summary>
    /// The <see cref="ISessionService" /> interface provides access to an <see cref="ISession" />
    /// </summary>
    public interface ISessionService : CDP4Web.Services.SessionService.ISessionService
    {
        /// <summary>
        /// A reactive collection of opened <see cref="Iteration" />
        /// </summary>
        SourceList<Iteration> OpenIterations { get; }

        /// <summary>
        /// Gets a readonly collection of open <see cref="EngineeringModel" />
        /// </summary>
        IReadOnlyCollection<EngineeringModel> OpenEngineeringModels { get; }

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup" /> and <see cref="IterationSetup" />
        /// </summary>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        /// <returns>An asynchronous operation with a <see cref="Result" /> that contains the iteration, if succeeded</returns>
        Task<Result<Iteration>> ReadIteration(IterationSetup iterationSetup, DomainOfExpertise domain);

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
        /// Creates or updates <see cref="Thing" />s
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="files">A <see cref="IReadOnlyCollection{T}"/> of the file paths as <see cref="string"/> to create or update</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        Task<Result> CreateOrUpdateThings(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, IReadOnlyCollection<string> files);

        /// <summary>
        /// Reads the <see cref="EngineeringModel" /> instances from the data-source
        /// </summary>
        /// <param name="engineeringModelIds">
        /// The unique identifiers of the <see cref="EngineeringModel" />s that needs to be read from the data-source, in case the list is empty
        /// all the <see cref="EngineeringModel" />s will be read
        /// </param>
        /// <returns>
        /// A <see cref="Task" />
        /// </returns>
        /// <remarks>
        /// Only those <see cref="EngineeringModel" />s are returned that the <see cref="Person" /> is a <see cref="Participant" />
        /// in.
        /// </remarks>
        Task<Result> ReadEngineeringModels(IEnumerable<Guid> engineeringModelIds);

        /// <summary>
        /// Reads the <see cref="EngineeringModel" /> instances from the data-source
        /// </summary>
        /// <param name="engineeringModelSetups">
        /// A collection of <see cref="EngineeringModelSetup" /> where the <see cref="EngineeringModel" /> is tied to
        /// </param>
        /// <returns>
        /// A <see cref="Task" />
        /// </returns>
        /// <remarks>
        /// Only those <see cref="EngineeringModel" />s are returned that the <see cref="Person" /> is a <see cref="Participant" />
        /// in.
        /// </remarks>
        Task<Result> ReadEngineeringModels(IEnumerable<EngineeringModelSetup> engineeringModelSetups);

        /// <summary>
        /// Writes an <see cref="OperationContainer" /> to the <see cref="ISession" />
        /// </summary>
        /// <param name="operationContainer">The <see cref="OperationContainer" /> to write</param>
        /// <param name="files">A <see cref="IReadOnlyCollection{T}"/> of the file paths as <see cref="string"/> to create or update</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        Task<Result> WriteTransaction(OperationContainer operationContainer, IReadOnlyCollection<string> files);

        /// <summary>
        /// Creates or updates things, add new notifications to the <see cref="INotificationService"/>
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        Task<Result> CreateOrUpdateThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, NotificationDescription notificationDescription = null);

        /// <summary>
        /// Creates or updates things, add new notifications to the <see cref="INotificationService"/>
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="files">A <see cref="IReadOnlyCollection{T}"/> of the file paths as <see cref="string"/> to create or update</param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        Task<Result> CreateOrUpdateThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, IReadOnlyCollection<string> files, NotificationDescription notificationDescription = null);

        /// <summary>
        /// Deletes <see cref="Thing" />s
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toDelete">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        Task<Result> DeleteThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toDelete, NotificationDescription notificationDescription = null);
    }
}
