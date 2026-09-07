// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionService.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Services.SessionManagement
{
    using System.Diagnostics;
    using System.Net;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;
    using CDP4Dal.Utilities;

    using CDP4JsonFileDal;

    using CDP4Web.Extensions;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.NotificationService;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SessionService" /> is to provide access to
    /// an instance of <see cref="ISession" />
    /// </summary>
    public sealed class SessionService : CDP4Web.Services.SessionService.SessionService, ISessionService
    {
        /// <summary>
        /// The <see cref="ILogger{T}" />
        /// </summary>
        private readonly ILogger<SessionService> logger;

        /// <summary>
        /// The <see cref="ICDPMessageBus" /> that a manually constructed <see cref="ISession" /> has to be built with.
        /// The base class keeps its own copy private, so it is held here as well
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The <see cref="INotificationService" />
        /// </summary>
        private readonly INotificationService notificationService;

        /// <summary>
        /// Creates a new instance of type <see cref="SessionService" />
        /// </summary>
        /// <param name="logger">the <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="messageBus">The <see cref="IMessageBus" /></param>
        /// <param name="notificationService">The <see cref="INotificationService" /></param>
        public SessionService(ILogger<SessionService> logger, ICDPMessageBus messageBus, INotificationService notificationService) : base(logger, messageBus)
        {
            this.logger = logger;
            this.messageBus = messageBus;
            this.notificationService = notificationService;
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
        /// Gets a value indicating whether the current <see cref="ISession" /> is backed by a read-only data source,
        /// which is the case for a session opened from an ECSS-E-TM-10-25 Annex C3 archive. No <see cref="Thing" /> may
        /// be created, updated or deleted on such a session
        /// </summary>
        public bool IsReadOnly => this.Session?.Dal?.IsReadOnly == true;

        /// <summary>
        /// Opens an <see cref="ISession" /> against an ECSS-E-TM-10-25 Annex C3 archive. The resulting session is
        /// read-only, see <see cref="IsReadOnly" />
        /// </summary>
        /// <param name="archivePath">The full path of the Annex C3 archive to open</param>
        /// <param name="userName">
        /// The short name of the <see cref="Person" /> contained by the archive that the session should be opened as
        /// </param>
        /// <param name="password">The password that the archive is encrypted with</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="archivePath" /> or <paramref name="userName" /> is null or empty</exception>
        public async Task<Result> OpenArchiveSession(string archivePath, string userName, string password)
        {
            Guard.ThrowIfNullOrEmpty(archivePath, nameof(archivePath));
            Guard.ThrowIfNullOrEmpty(userName, nameof(userName));

            if (this.IsSessionOpen)
            {
                await this.CloseSession();
            }

            var stopWatch = Stopwatch.StartNew();

            try
            {
                var credentials = new Credentials(userName, password, new Uri(archivePath));
                var session = new Session(new JsonFileDal(), credentials, this.messageBus);
                AssignSession(this, session);
                await session.Open();
                stopWatch.Stop();
                var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                this.logger.LogInformation("Annex C3 session opened in {Time} [ms]", elapsedMilliseconds);
                return Result.Ok();
            }
            catch (UnauthorizedAccessException exception)
            {
                this.logger.LogError(exception, "The archive does not contain the requested person");
                AssignSession(this, null);
                return Result.Fail(new Error($"The archive does not contain a person with short name '{userName}'").AddReasonIdentifier(HttpStatusCode.Unauthorized));
            }
            catch (FileLoadException exception)
            {
                this.logger.LogError(exception, "The archive could not be read");
                AssignSession(this, null);
                return Result.Fail(new Error("The file could not be read as an Annex C3 archive. Verify that the file is a valid archive and that the password is correct").AddReasonIdentifier(HttpStatusCode.BadRequest));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Failed to open the Annex C3 archive");
                AssignSession(this, null);
                return Result.Fail(new Error("The file could not be opened as an Annex C3 archive").AddReasonIdentifier(HttpStatusCode.BadRequest));
            }
            finally
            {
                stopWatch.Stop();
            }
        }

        /// <summary>
        /// Closes an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> that needs to be closed</param>
        /// <returns>A <see cref="Task" /></returns>
        public new async Task CloseIteration(Iteration iteration)
        {
            await base.CloseIteration(iteration);
            this.OpenIterations.Remove(iteration);
        }

        /// <summary>
        /// Closes the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public new async Task CloseSession()
        {
            await base.CloseSession();
            this.OpenIterations.Clear();
        }

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup" /> and <see cref="IterationSetup" />
        /// </summary>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup" /></param>
        /// <param name="domain">The <see cref="DomainOfExpertise" /></param>
        /// <returns>An asynchronous operation with a <see cref="Result" /> that contains the iteration, if succeeded</returns>
        public async Task<Result<Iteration>> ReadIteration(IterationSetup iterationSetup, DomainOfExpertise domain)
        {
            var result = await this.OpenIteration(iterationSetup, domain);

            if (result.IsSuccess)
            {
                this.OpenIterations.Add(result.Value);
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
        /// <param name="files">
        /// A <see cref="IReadOnlyCollection{T}" /> of the file paths as <see cref="string" /> to create or
        /// update
        /// </param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        public Task<Result> CreateOrUpdateThings(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, IReadOnlyCollection<string> files)
        {
            if (this.IsReadOnly)
            {
                return Task.FromResult(this.RefuseReadOnlyWrite());
            }

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
        /// Creates or updates <see cref="Thing" />s
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>
        /// Hides the CDP4Web base implementation, which writes through its own <c>WriteTransaction</c> and would
        /// therefore escape the read-only check. Every write has to route through the local overloads
        /// </remarks>
        public new Task<Result> CreateOrUpdateThings(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate)
        {
            return this.CreateOrUpdateThings(topContainer, toUpdateOrCreate, []);
        }

        /// <summary>
        /// Deletes <see cref="Thing" />s
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toDelete">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to delete</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>
        /// Hides the CDP4Web base implementation, which writes through its own <c>WriteTransaction</c> and would
        /// therefore escape the read-only check
        /// </remarks>
        public new Task<Result> DeleteThings(Thing topContainer, IReadOnlyCollection<Thing> toDelete)
        {
            return this.IsReadOnly ? Task.FromResult(this.RefuseReadOnlyWrite()) : base.DeleteThings(topContainer, toDelete);
        }

        /// <summary>
        /// Writes an <see cref="OperationContainer" /> to the <see cref="ISession" />
        /// </summary>
        /// <param name="operationContainer">The <see cref="OperationContainer" /> to write</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>
        /// Hides the CDP4Web base implementation so that a caller using the single-argument overload is subject to the
        /// same read-only check as the local one
        /// </remarks>
        public new Task<Result> WriteTransaction(OperationContainer operationContainer)
        {
            return this.WriteTransaction(operationContainer, []);
        }

        /// <summary>
        /// Creates or updates things, add new notifications to the <see cref="INotificationService" />
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        public async Task<Result> CreateOrUpdateThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, NotificationDescription notificationDescription = null)
        {
            var result = await this.CreateOrUpdateThings(topContainer, toUpdateOrCreate);
            this.notificationService.Results.Add(new ResultNotification(result, notificationDescription));
            return result;
        }

        /// <summary>
        /// Creates or updates things, add new notifications to the <see cref="INotificationService" />
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="files">
        /// A <see cref="IReadOnlyCollection{T}" /> of the file paths as <see cref="string" /> to create or
        /// update
        /// </param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        public async Task<Result> CreateOrUpdateThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, IReadOnlyCollection<string> files, NotificationDescription notificationDescription = null)
        {
            var result = await this.CreateOrUpdateThings(topContainer, toUpdateOrCreate, files);
            this.notificationService.Results.Add(new ResultNotification(result, notificationDescription));
            return result;
        }

        /// <summary>
        /// Creates or updates <see cref="Thing" />s and deletes <see cref="Thing" />s within a single transaction, and adds a
        /// new notification to the <see cref="INotificationService" />
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toUpdateOrCreate">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="toDelete">
        /// A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to delete, each with its
        /// <see cref="Thing.Container" /> set to the cloned container it is removed from
        /// </param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        /// <exception cref="InvalidOperationException">When the <see cref="ISession" /> is not open</exception>
        public async Task<Result> CreateUpdateAndDeleteThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toUpdateOrCreate, IReadOnlyCollection<Thing> toDelete, NotificationDescription notificationDescription = null)
        {
            Guard.ThrowIfNotValidForTransaction(topContainer);
            Guard.ThrowIfNullOrEmpty(toUpdateOrCreate, nameof(toUpdateOrCreate));

            if (!this.IsSessionOpen)
            {
                this.logger.LogError("Trying to Create, update or delete Thing(s) while the Session is not open");
                throw new InvalidOperationException("Cannot Create, update or delete Thing(s) while the Session is not open");
            }

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(topContainer));

            foreach (var thing in toUpdateOrCreate)
            {
                transaction.CreateOrUpdate(thing);
            }

            foreach (var thing in toDelete)
            {
                transaction.Delete(thing, thing.Container);
            }

            var result = await this.WriteTransaction(transaction.FinalizeTransaction(), []);
            this.notificationService.Results.Add(new ResultNotification(result, notificationDescription));
            return result;
        }

        /// <summary>
        /// Deletes <see cref="Thing" />s
        /// </summary>
        /// <param name="topContainer">The <see cref="Thing" /> top container to use for the transaction</param>
        /// <param name="toDelete">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" /> to create or update</param>
        /// <param name="notificationDescription">The notification description to be displayed</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        /// <remarks>The <paramref name="topContainer" /> have to be a cloned <see cref="Thing" /></remarks>
        public async Task<Result> DeleteThingsWithNotification(Thing topContainer, IReadOnlyCollection<Thing> toDelete, NotificationDescription notificationDescription = null)
        {
            var result = await this.DeleteThings(topContainer, toDelete);
            this.notificationService.Results.Add(new ResultNotification(result, notificationDescription));
            return result;
        }

        /// <summary>
        /// Reads the <see cref="EngineeringModel" /> instances from the data-source
        /// </summary>
        /// <param name="engineeringModelIds">
        /// The unique identifiers of the <see cref="EngineeringModel" />s that needs to be read from the data-source, in case the
        /// list is empty
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
                this.logger.LogError(exception, "During reading EnngineeringModel an error has occured");
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
        /// <param name="files">
        /// A <see cref="IReadOnlyCollection{T}" /> of the file paths as <see cref="string" /> to create or
        /// update
        /// </param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        public async Task<Result> WriteTransaction(OperationContainer operationContainer, IReadOnlyCollection<string> files)
        {
            Guard.ThrowIfNull(operationContainer, nameof(operationContainer));

            if (!this.IsSessionOpen)
            {
                this.logger.LogError("Trying to write a transaction while the Session is not open");
                throw new InvalidOperationException("Cannot write a transaction while the Session is not open");
            }

            if (this.IsReadOnly)
            {
                return this.RefuseReadOnlyWrite();
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
                this.logger.LogError(ex, "Transaction failed");
                return Result.Fail(new ExceptionalError("Transaction failed", ex).AddReasonIdentifier(HttpStatusCode.Unauthorized));
            }
            catch (DalWriteException ex)
            {
                this.logger.LogError(ex, "Transaction failed");
                return Result.Fail(new ExceptionalError("Transaction failed", ex).AddReasonIdentifier(HttpStatusCode.BadRequest));
            }
            finally
            {
                stopWatch.Stop();
            }
        }

        /// <summary>
        /// Builds the failed <see cref="Result" /> returned by every write entry point when the data source is
        /// read-only, and logs the attempt
        /// </summary>
        /// <returns>A failed <see cref="Result" /> carrying a message suitable for display to the user</returns>
        private Result RefuseReadOnlyWrite()
        {
            this.logger.LogWarning("Trying to write against a read-only data source");
            return Result.Fail(new Error("This model was opened from an archive and cannot be modified").AddReasonIdentifier(HttpStatusCode.Forbidden));
        }

        /// <summary>
        /// Assigns the provided <see cref="ISession" /> onto the CDP4Web <c>SessionService</c> base class
        /// </summary>
        /// <param name="sessionService">The <see cref="SessionService" /> to assign the session on</param>
        /// <param name="session">The <see cref="ISession" /> to assign, may be null to clear a failed session</param>
        /// <exception cref="InvalidOperationException">
        /// If the CDP4-COMET-SDK no longer exposes a settable <c>Session</c> property, which means this workaround has to
        /// be revisited against the new SDK version
        /// </exception>
        /// <remarks>
        /// ponytail: the CDP4-COMET-SDK exposes <c>SessionService.Session</c> with an <c>internal</c> setter and its
        /// <c>OpenSession</c> hard-wires <c>CdpServicesDal</c>, so there is no supported way to open a session against a
        /// <see cref="JsonFileDal" />. Replace this with the supported call once the SDK offers an
        /// <c>OpenSession(IDal, Credentials)</c> overload
        /// </remarks>
        private static void AssignSession(SessionService sessionService, ISession session)
        {
            var property = typeof(CDP4Web.Services.SessionService.SessionService)
                .GetProperty(nameof(CDP4Web.Services.SessionService.SessionService.Session));

            if (property?.SetMethod == null)
            {
                throw new InvalidOperationException("The CDP4Web SessionService no longer exposes a settable Session property, opening an archive has to be reworked against this SDK version");
            }

            property.SetValue(sessionService, session);
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
