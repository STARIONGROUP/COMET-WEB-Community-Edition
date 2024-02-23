// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionService.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Enumerations;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SessionService" /> is to provide access to
    /// an instance of <see cref="ISession" />
    /// </summary>
    public class SessionService : ReactiveObject, ISessionService
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
        public SessionService(ILogger<SessionService> logger, ICDPMessageBus messageBus)
        {
            this.logger = logger;
            this.messageBus = messageBus;
        }

        /// <summary>
        /// Gets a readonly collection of open <see cref="EngineeringModel" />
        /// </summary>
        public IReadOnlyCollection<EngineeringModel> OpenEngineeringModels => this.QueryOpenEngineeringModels();

        /// <summary>
        /// Gets or sets the <see cref="ISession" />
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// A reactive collection of opened <see cref="Iteration" />
        /// </summary>
        public SourceList<Iteration> OpenIterations { get; private set; } = new();

        /// <summary>
        /// True if the <see cref="ISession" /> is opened
        /// </summary>
        public bool IsSessionOpen { get; set; }

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory" /> that is loaded in the <see cref="ISession" />
        /// </summary>
        /// <returns>The <see cref="SiteDirectory" /></returns>
        public SiteDirectory GetSiteDirectory()
        {
            return this.Session.RetrieveSiteDirectory();
        }

        /// <summary>
        /// Close the ISession
        /// </summary>
        /// <returns>a <see cref="Task" /></returns>
        public async Task Close()
        {
            if (!this.IsSessionOpen)
            {
                return;
            }

            await this.Session.Close();
            this.IsSessionOpen = false;
            this.CloseIterations();
        }

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
        /// Close all the opened <see cref="Iteration" />
        /// </summary>
        public void CloseIterations()
        {
            this.logger.LogInformation("Closing all the opened iterations");

            foreach (var iteration in this.OpenIterations.Items.ToList())
            {
                this.CloseIteration(iteration);
            }
        }

        /// <summary>
        /// Closes an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        public void CloseIteration(Iteration iteration)
        {
            this.logger.LogInformation("Closing iteration with id {iterationId}", iteration.Iid);
            this.Session.CloseIterationSetup(iteration.IterationSetup);
            this.OpenIterations.Remove(this.OpenIterations.Items.First(x => x.Iid == iteration.Iid));
        }

        /// <summary>
        /// Get <see cref="EngineeringModelSetup" /> available for the ActivePerson
        /// </summary>
        /// <returns>
        /// A container of <see cref="EngineeringModelSetup" />
        /// </returns>
        public IEnumerable<EngineeringModelSetup> GetParticipantModels()
        {
            if (this.IsSessionOpen)
            {
                return this.GetSiteDirectory().Model
                    .Where(m => m.Participant.Exists(p => p.Person.Name.Equals(this.Session.ActivePerson.Name)));
            }

            return Enumerable.Empty<EngineeringModelSetup>();
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
        /// Refresh the ISession object
        /// </summary>
        public async Task RefreshSession()
        {
            var sw = Stopwatch.StartNew();
            this.messageBus.SendMessage(SessionStateKind.Refreshing);

            await this.Session.Refresh();

            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.logger.LogInformation("Session refreshed in {ElapsedMilliseconds} [ms]", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Switches the current domain for an opened iteration
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="domainOfExpertise">The domain</param>
        public void SwitchDomain(Iteration iteration, DomainOfExpertise domainOfExpertise)
        {
            this.Session.SwitchDomain(iteration.Iid, domainOfExpertise);
        }

        /// <summary>
        /// Write a new Thing in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">the <see cref="Thing" /> container where the <paramref name="thingToCreate" /> should be created </param>
        /// <param name="thingToCreate">the thing to create in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public Task<Result> CreateThing(Thing container, Thing thingToCreate)
        {
            return this.CreateThings(container, new List<Thing> { thingToCreate });
        }

        /// <summary>
        /// Write new Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">
        /// the <see cref="Thing" /> container where the
        /// <paramref name="thingsToCreate" />
        /// should be created
        /// </param>
        /// <param name="thingsToCreate">the things to create in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public Task<Result> CreateThings(Thing container, params Thing[] thingsToCreate)
        {
            return this.CreateThings(container, thingsToCreate.ToList());
        }

        /// <summary>
        /// Write new Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be created</param>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public async Task<Result> CreateThings(Thing container, IEnumerable<Thing> thingsToCreate)
        {
            var result = new Result();

            if (thingsToCreate == null)
            {
                result.Errors.Add(new Error("The things to create can't be null"));
                return result;
            }

            var thingClone = container;

            if (container.Original == null)
            {
                thingClone = container.Clone(false);
            }

            // set the context of the transaction to the thing changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(thingClone);
            var transaction = new ThingTransaction(context);

            // register new Things and the container Thing (clone) with the transaction.
            thingsToCreate.ToList().ForEach(x => { transaction.Create(x, thingClone); });

            // finalize the transaction, the result is an OperationContainer that the session class uses to write the changes
            // to the Thing object.
            var operationContainer = transaction.FinalizeTransaction();

            try
            {
                await this.Session.Write(operationContainer);
                this.logger.LogInformation("Writing done!");
                result.Successes.Add(new Success("Writing done!"));
            }
            catch (DalWriteException ex)
            {
                this.logger.LogError("The create operation failed: {exMessage}", ex.Message);
                result.Errors.Add(new Error($"The create operation failed: {ex.Message}"));
            }

            return result;
        }

        /// <summary>
        /// Write updated Thing in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingToUpdate">the thing to update in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public Task<Result> UpdateThing(Thing container, Thing thingToUpdate)
        {
            return this.UpdateThings(container, new List<Thing> { thingToUpdate });
        }

        /// <summary>
        /// Write updated Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public Task<Result> UpdateThings(Thing container, params Thing[] thingsToUpdate)
        {
            return this.UpdateThings(container, thingsToUpdate.ToList());
        }

        /// <summary>
        /// Write updated Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="container">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public async Task<Result> UpdateThings(Thing container, IEnumerable<Thing> thingsToUpdate)
        {
            var result = new Result();

            if (thingsToUpdate == null)
            {
                result.Errors.Add(new Error("The things to update can't be null"));
                return result;
            }

            var sw = Stopwatch.StartNew();

            // CreateThings a shallow clone of the thing. The cached Thing object should not be changed, so we record the change on a clone.
            var thingClone = container;

            if (container.Original == null)
            {
                thingClone = container.Clone(false);
            }

            // set the context of the transaction to the thing changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(thingClone);
            var transaction = new ThingTransaction(context);

            // register all updates with the transaction.
            thingsToUpdate.ToList().ForEach(transaction.CreateOrUpdate);

            // finalize the transaction, the result is an OperationContainer that the session class uses to write the changes
            // to the Thing object.
            var operationContainer = transaction.FinalizeTransaction();

            try
            {
                await this.Session.Write(operationContainer);
                this.logger.LogInformation("Update writing done in {swElapsedMilliseconds} [ms]", sw.ElapsedMilliseconds);
                result.Successes.Add(new Success($"Update writing done in {sw.ElapsedMilliseconds} [ms]"));
            }
            catch (Exception ex)
            {
                this.logger.LogError("The update operation failed: {exMessage}", ex.Message);
                result.Errors.Add(new Error($"The update operation failed: {ex.Message}"));
            }
            finally
            {
                sw.Stop();
            }

            return result;
        }

        /// <summary>
        /// Deletes a <see cref="Thing" /> from it's container
        /// </summary>
        /// <param name="containerClone">the container clone of the thing to delete</param>
        /// <param name="thingToDelete">the cloned thing to delete in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public Task<Result> DeleteThing(Thing containerClone, Thing thingToDelete)
        {
            return this.DeleteThings(containerClone, new List<Thing> { thingToDelete });
        }

        /// <summary>
        /// Deletes a collection of <see cref="Thing" /> from it's container
        /// </summary>
        /// <param name="containerClone">the container clone of the thing to delete</param>
        /// <param name="thingsToDelete">the cloned things to delete in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public Task<Result> DeleteThings(Thing containerClone, params Thing[] thingsToDelete)
        {
            return this.DeleteThings(containerClone, thingsToDelete.ToList());
        }

        /// <summary>
        /// Deletes a collection <see cref="Thing" /> from it's container
        /// </summary>
        /// <param name="containerClone">the container clone of the thing to delete</param>
        /// <param name="thingsToDelete">the cloned things to delete in the session</param>
        /// <returns>An asynchronous operation with a <see cref="Result" /></returns>
        public async Task<Result> DeleteThings(Thing containerClone, IEnumerable<Thing> thingsToDelete)
        {
            var result = new Result();

            if (thingsToDelete == null)
            {
                result.Errors.Add(new Error("The things to delete can't be null"));
                return result;
            }

            var sw = Stopwatch.StartNew();

            // CreateThings a shallow clone of the thing. The cached Thing object should not be changed, so we record the change on a clone.
            var thingClone = containerClone;

            if (containerClone.Original == null)
            {
                thingClone = containerClone.Clone(false);
            }

            // set the context of the transaction to the thing changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(thingClone);
            var transaction = new ThingTransaction(context);

            // register all deletes with the transaction.
            foreach (var thingToDelete in thingsToDelete)
            {
                var thingToDeleteClone = thingToDelete.Clone(false);
                transaction.Delete(thingToDeleteClone, containerClone);
            }

            // finalize the transaction, the result is an OperationContainer that the session class uses to write the changes
            // to the Thing object.
            var operationContainer = transaction.FinalizeTransaction();

            try
            {
                await this.Session.Write(operationContainer);
                this.logger.LogInformation("Delete done in {ElapsedMilliseconds} [ms]", sw.ElapsedMilliseconds);
                result.Successes.Add(new Success($"Delete done in {sw.ElapsedMilliseconds} [ms]"));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"The delete operation failed");
                result.Errors.Add(new Error($"The delete operation failed: {ex.Message}"));
            }
            finally
            {
                sw.Stop();
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="ParticipantRole" /> inside an iteration
        /// </summary>
        public Participant GetParticipant(Iteration iteration)
        {
            return this.GetSiteDirectory().Model.Find(m => m.IterationSetup.Contains(iteration.IterationSetup))?
                .Participant.Find(p => p.Person.Iid == this.Session.ActivePerson.Iid);
        }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> for an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The <see cref="DomainOfExpertise" /></returns>
        /// <exception cref="ArgumentException">If the <see cref="Iteration" /> is not opened</exception>
        public DomainOfExpertise GetDomainOfExpertise(Iteration iteration)
        {
            if (!this.Session.OpenIterations.TryGetValue(iteration, out var participantInformation))
            {
                throw new ArgumentException("The requested iteration is not opened");
            }

            return participantInformation.Item1;
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
