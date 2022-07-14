// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionAnchor.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using NLog;
    using System.Diagnostics;
    using CDP4Common.CommonData;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Dal.Exceptions;

    /// <summary>
    /// The purpose of the <see cref="SessionAnchor"/> is to provide access to
    /// an instance of <see cref="ISession"/>
    /// </summary>
    public class SessionAnchor : ISessionAnchor
    {
        /// <summary>
        /// The current class <see cref="NLog.Logger"/>
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// True if the <see cref="ISession"/> is opened
        /// </summary>
        public bool IsSessionOpen { get; set; }

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> selected to open a model
        /// </summary>
        public DomainOfExpertise? CurrentDomainOfExpertise { get; set; }

        /// <summary>
        /// Name of the opened Engineering Model
        /// </summary>
        public string? CurrentEngineeringModelName { get; set; }

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory"/> that is loaded in the <see cref="ISession"/>
        /// </summary>
        /// <returns>The <see cref="SiteDirectory"/></returns>
        public SiteDirectory GetSiteDirectory() => this.Session.RetrieveSiteDirectory();

        /// <summary>
        /// Close the ISession
        /// </summary>
        /// <returns>a <see cref="Task"/></returns>
        public async Task Close()
        {
            await this.Session.Close();
            this.IsSessionOpen = false;
            this.CloseIteration();
        }

        /// <summary>
        /// Returns the opened <see cref="Iteration"/> in the Session
        /// </summary>
        /// <returns>An <see cref="Iteration"/></returns>
        public Iteration? GetIteration()
        {
            if (this.IsSessionOpen && this.Session.OpenIterations.Any())
            {
                return this.Session.OpenIterations.First().Key;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup"/> and <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="modelSetup"> The selected <see cref="EngineeringModelSetup"/> </param>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup"/></param>
        public async Task SetOpenIteration(EngineeringModelSetup? modelSetup, IterationSetup? iterationSetup)
        {
            if (modelSetup != null && iterationSetup != null)
            {
                var model = new EngineeringModel(modelSetup.EngineeringModelIid, this.Session.Assembler.Cache, this.Session.Credentials.Uri);
                var iteration = new Iteration(iterationSetup.IterationIid, this.Session.Assembler.Cache, this.Session.Credentials.Uri);
                iteration.Container = model;

                try
                {
                    await this.Session.Read(iteration, this.CurrentDomainOfExpertise);
                }
                catch (Exception exception)
                {
                    this.logger.Error($"During read operation an error has occured: {exception.Message}");
                }

                CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.IterationOpened);
            }
        }

        /// <summary>
        /// Close the <see cref="OpenIteration"/>
        /// </summary>
        public void CloseIteration()
        {
            this.Session.CloseIterationSetup(this.GetIteration()?.IterationSetup);
            this.CurrentDomainOfExpertise = null;
            this.CurrentEngineeringModelName = null;

            CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.IterationClosed);
        }

        /// <summary>
        /// Get <see cref="EngineeringModelSetup"/> available for the ActivePerson
        /// </summary>
        /// <returns>
        /// A container of <see cref="EngineeringModelSetup"/>
        /// </returns>
        public IEnumerable<EngineeringModelSetup> GetParticipantModels()
        {
            foreach (var model in this.GetSiteDirectory().Model)
            {
                foreach (var participant in model.Participant)
                {
                    if (participant.Person.Name.Equals(Session.ActivePerson.Name))
                    {
                        yield return model;
                    }
                }
            }
        }

        /// <summary>
        /// Get <see cref="DomainOfExpertise"/> available for the active person in the selected <see cref="EngineeringModelSetup"/>
        /// </summary>
        /// <param name="modelSetup">The selected <see cref="EngineeringModelSetup"/></param>
        /// <returns>
        /// A container of <see cref="DomainOfExpertise"/> accessible for the active person
        /// </returns>
        public IEnumerable<DomainOfExpertise> GetModelDomains(EngineeringModelSetup? modelSetup)
        {
            var domains = new List<DomainOfExpertise>();
            modelSetup?.Participant.FindAll(p => p.Person.Name.Equals(this.Session.ActivePerson.Name)).ForEach(p => p.Domain.ForEach(d => domains.Add(d)));
            return domains.DistinctBy(d => d.Name).OrderBy(d => d.Name);
        }

        /// <summary>
        /// Refresh the ISession object
        /// </summary>
        public async Task RefreshSession()
        {
            var sw = Stopwatch.StartNew();
            CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.Refreshing);

            await this.Session.Refresh();

            CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.UpToDate);
            Console.WriteLine($"Session refreshed in {sw.ElapsedMilliseconds} [ms]");
        }

        /// <summary>
        /// Switches the current domain for the opened iteration
        /// </summary>
        /// <param name="DomainOfExpertise">The domain</param>
        public void SwitchDomain(DomainOfExpertise? DomainOfExpertise)
        {
            var iteration = this.GetIteration();
            if (iteration != null)
            {
                this.CurrentDomainOfExpertise = DomainOfExpertise;
                this.Session.SwitchDomain(iteration.Iid, DomainOfExpertise);
            }
        }

        /// <summary>
        /// Write new Things in the session
        /// </summary>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        public async Task CreateThings(IEnumerable<Thing> thingsToCreate)
        {
            var openedIteration = this.GetIteration();
            if (openedIteration == null)
            {
                throw new InvalidOperationException("At first an iteration should be opened");
            }
            if (thingsToCreate == null)
            {
                throw new ArgumentException("Please add at least one Thing to be created");
            }

            // CreateThings a shallow clone of the iteration. The cached Iteration object should not be changed, so we record the change on a clone.
            var iterationClone = openedIteration.Clone(false);

            // set the context of the transaction to the iteration changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(iterationClone);
            var transaction = new ThingTransaction(context);

            // register new Things and the container Iteration (clone) with the transaction.
            thingsToCreate.ToList().ForEach(thing =>
            {
                transaction.Create(thing, iterationClone);
            });

            // finalize the transaction, the result is an OperationContainer that the session class uses to write the changes
            // to the Iteration object (the list of contained elements is updated) and and the new ElementDefinition.
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
        /// Write updated Things in the session
        /// </summary>
        /// <param name="thingsToCreate">List of Things to update in the session</param>
        public async Task UpdateThings(IEnumerable<Thing> thingsToUpdate)
        {
            var openedIteration = this.GetIteration();
            if (openedIteration == null)
            {
                throw new InvalidOperationException("At first an iteration should be opened");
            }
            if (thingsToUpdate == null)
            {
                throw new ArgumentException("Please add at least one Thing to be updated");
            }

            // CreateThings a shallow clone of the iteration. The cached Iteration object should not be changed, so we record the change on a clone.
            var iterationClone = openedIteration.Clone(false);

            // set the context of the transaction to the iteration changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(iterationClone);
            var transaction = new ThingTransaction(context);

            // register all updates with the transaction.
            thingsToUpdate.ToList().ForEach(thing =>
            {
                transaction.CreateOrUpdate(thing);
            });

            // finalize the transaction, the result is an OperationContainer that the session class uses to write the changes
            // to the Iteration object (the list of contained elements is updated) and and the new ElementDefinition.
            var operationContainer = transaction.FinalizeTransaction();
            try
            {
                await this.Session.Write(operationContainer);
                Console.WriteLine("Update writing done !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The update operation failed: {ex.Message}");
            }
        }
    }
}