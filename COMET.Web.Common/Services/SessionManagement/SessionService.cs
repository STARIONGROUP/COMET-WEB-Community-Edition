// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using System.Diagnostics;
    using System.Net;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;
    using CDP4Dal.Utilities;

    using CDP4Web.Extensions;

    using COMET.Web.Common.Enumerations;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SessionService" /> is to provide access to
    /// an instance of <see cref="ISession" />
    /// </summary>
    public class SessionService : CDP4Web.Services.SessionService.SessionService, ISessionService
    {
        /// <summary>
        /// The <see cref="ILogger{T}" />
        /// </summary>
        private readonly ILogger<SessionService> logger;

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus" />
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Creates a new instance of type <see cref="SessionService" />
        /// </summary>
        /// <param name="logger">the <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="messageBus">The <see cref="IMessageBus" /></param>
        public SessionService(ILogger<SessionService> logger, ICDPMessageBus messageBus) : base(logger, messageBus)
        {
            this.logger = logger;
            this.messageBus = messageBus;
        }

        /// <summary>
        /// Gets a readonly collection of open <see cref="EngineeringModel" />
        /// </summary>
        public IReadOnlyCollection<EngineeringModel> OpenEngineeringModels => this.QueryOpenEngineeringModels();

        /// <summary>
        /// A reactive collection of opened <see cref="Iteration" />
        /// </summary>
        public SourceList<Iteration> OpenIterations { get; private set; } = new();

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup" /> and <see cref="IterationSetup" />
        /// </summary>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public async Task<Result> ReadIteration(IterationSetup iterationSetup, DomainOfExpertise domain)
        {
            var result = new Result();

            this.logger.LogInformation("Opening iteration");
            var modelSetup = (EngineeringModelSetup)iterationSetup.Container;
            var model = new EngineeringModel(modelSetup.EngineeringModelIid, this.Session.Assembler.Cache, this.Session.Credentials.Uri);

            var iteration = new Iteration(iterationSetup.IterationIid, this.Session.Assembler.Cache, this.Session.Credentials.Uri)
            {
                Container = model
            };

            try
            {
                await this.Session.Read(iteration, domain);

                if (this.Session.OpenIterations.All(x => x.Key.Iid != iterationSetup.IterationIid))
                {
                    throw new InvalidOperationException("The requested iteration could not be opened");
                }

                var openedIteration = this.Session.OpenIterations.FirstOrDefault(x => x.Key.Iid == iterationSetup.IterationIid).Key;
                this.OpenIterations.Add(openedIteration);

                this.messageBus.SendMessage(SessionStateKind.IterationOpened);
                this.logger.LogInformation("Iteration opened successfully");
                result.Successes.Add(new Success("Iteration opened successfully"));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "During read operation an error has occured");
                result.Errors.Add(new Error($"During read operation an error has occured: {exception.Message}"));
                throw;
            }

            return result;
        }

        /// <summary>
        /// Get <see cref="DomainOfExpertise" /> available for the active person in the selected
        /// <see cref="EngineeringModelSetup" />
        /// </summary>
        /// <param name="modelSetup">The selected <see cref="EngineeringModelSetup" /></param>
        /// <returns>
        /// A container of <see cref="DomainOfExpertise" /> accessible for the active person
        /// </returns>
        public IEnumerable<DomainOfExpertise> GetModelDomains(EngineeringModelSetup modelSetup)
        {
            var domains = new List<DomainOfExpertise>();
            modelSetup?.Participant.FindAll(p => p.Person.Iid.Equals(this.Session.ActivePerson.Iid)).ForEach(p => p.Domain.ForEach(d => domains.Add(d)));
            return domains.DistinctBy(d => d.Name).OrderBy(d => d.Name);
        }

        /// <summary>
        /// Creates or updates <see cref="Thing" />s
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="files">A <see cref="IReadOnlyCollection{T}"/> of the file paths as <see cref="string"/> to create or update</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        public Task<Result> CreateOrUpdateThings(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, IReadOnlyCollection<string> files)
        {
            Guard.ThrowIfNotValidForTransaction(topContainer);
            Guard.ThrowIfNullOrEmpty(toUpdateOrCreate, nameof(toUpdateOrCreate));

            if (!this.IsSessionOpen)
            {
                this.logger.LogError("Trying to Create or update Thing(s) while the Session is not open");
                throw new InvalidOperationException("Cannot Create or update Thing(s) while the Session is not open");
            }

            var context = TransactionContextResolver.ResolveContext(topContainer);
            var transaction = new ThingTransaction(context);

            foreach (var thing in toUpdateOrCreate)
            {
                transaction.CreateOrUpdate(thing);
            }

            var operationContainer = transaction.FinalizeTransaction();
            return this.WriteTransaction(operationContainer, files);
        }

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
        public async Task<Result> ReadEngineeringModels(IEnumerable<Guid> engineeringModelIds)
        {
            var result = new Result();

            try
            {
                await this.Session.Read(engineeringModelIds);
            }
            catch (Exception exception)
            {
                this.logger.LogError("During reading EnngineeringModel an error has occured: {exception}", exception.Message);
                result.Reasons.Add(new Error($"During reading EnngineeringModel an error has occured: {exception.Message}"));
            }

            return result;
        }

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
        public Task<Result> ReadEngineeringModels(IEnumerable<EngineeringModelSetup> engineeringModelSetups)
        {
            return this.ReadEngineeringModels(engineeringModelSetups.Select(x => x.EngineeringModelIid));
        }

        /// <summary>
        /// Writes an <see cref="OperationContainer" /> to the <see cref="ISession" />
        /// </summary>
        /// <param name="operationContainer">The <see cref="OperationContainer" /> to write</param>
        /// <param name="files">A <see cref="IReadOnlyCollection{T}"/> of the file paths as <see cref="string"/> to create or update</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        public async Task<Result> WriteTransaction(OperationContainer operationContainer, IReadOnlyCollection<string> files)
        {
            Guard.ThrowIfNull(operationContainer, nameof(operationContainer));

            if (!this.IsSessionOpen)
            {
                this.logger.LogError("Trying to write a transaction while the Session is not open");
                throw new InvalidOperationException("Cannot write a transaction while the Session is not open");
            }

            var stopWatch = Stopwatch.StartNew();

            try
            {
                await this.Session.Write(operationContainer, files);
                this.logger.LogInformation("Transaction done in {swElapsedMilliseconds} [ms]", stopWatch.ElapsedMilliseconds);
                return Result.Ok();
            }
            catch (InvalidOperationException ex)
            {
                this.logger.LogError("Transaction failed: {exception}", ex.Message);
                return Result.Fail(new ExceptionalError("Transaction failed", ex).AddReasonIdentifier(HttpStatusCode.Unauthorized));
            }
            catch (DalWriteException ex)
            {
                this.logger.LogError("Transaction failed: {exception}", ex.Message);
                return Result.Fail(new ExceptionalError("Transaction failed", ex).AddReasonIdentifier(HttpStatusCode.BadRequest));
            }
            finally
            {
                stopWatch.Stop();
            }
        }

        /// <summary>
        /// Queries all open <see cref="EngineeringModel" />
        /// </summary>
        /// <returns>A collection of <see cref="EngineeringModel" /></returns>
        private List<EngineeringModel> QueryOpenEngineeringModels()
        {
            return this.OpenIterations.Items.Select(x => (EngineeringModel)x.Container)
                .DistinctBy(x => x.Iid).ToList();
        }
    }
}
