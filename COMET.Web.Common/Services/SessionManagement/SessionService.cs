// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionService.cs" company="RHEA System S.A.">
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
    using System.Diagnostics;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Enumerations;

    using DynamicData;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SessionService" /> is to provide access to
    /// an instance of <see cref="ISession" />
    /// </summary>
    public class SessionService : ReactiveObject, ISessionService
    {
        /// <summary>
        /// The current class <see cref="Logger" />
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        public async Task ReadIteration(IterationSetup iterationSetup, DomainOfExpertise domain)
        {
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

                CDPMessageBus.Current.SendMessage(SessionStateKind.IterationOpened);
            }
            catch (Exception exception)
            {
                this.logger.Error($"During read operation an error has occured: {exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// Close all the opened <see cref="Iteration" />
        /// </summary>
        public void CloseIterations()
        {
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
            return this.GetSiteDirectory().Model
                .Where(m => m.Participant.Any(p => p.Person.Name.Equals(this.Session.ActivePerson.Name)));
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
            CDPMessageBus.Current.SendMessage(SessionStateKind.Refreshing);

            await this.Session.Refresh();

            Console.WriteLine($"Session refreshed in {sw.ElapsedMilliseconds} [ms]");
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
        /// Write new Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> where the <see cref="Thing" />s should be created</param>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        public async Task CreateThings(Thing thing, IEnumerable<Thing> thingsToCreate)
        {
            if (thingsToCreate == null)
            {
                return;
            }

            var thingClone = thing;

            if (thing.Original == null)
            {
                thingClone = thing.Clone(false);
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
                Console.WriteLine("Writing done !");
            }
            catch (DalWriteException ex)
            {
                Console.WriteLine($"The create operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Write updated Things in an <see cref="Iteration" />
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> where the <see cref="Thing" />s should be updated</param>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        public async Task UpdateThings(Thing thing, IEnumerable<Thing> thingsToUpdate)
        {
            if (thingsToUpdate == null)
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            // CreateThings a shallow clone of the thing. The cached Thing object should not be changed, so we record the change on a clone.
            var thingClone = thing.Clone(false);

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
                Console.WriteLine($"Update writing done in {sw.ElapsedMilliseconds} [ms]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The update operation failed: {ex.Message}");
            }
            finally
            {
                sw.Stop();
            }
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
    }
}
