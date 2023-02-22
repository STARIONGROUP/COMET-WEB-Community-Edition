// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionAnchor.cs" company="RHEA System S.A.">
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
        /// Event for when the session has been refreshed.
        /// </summary>
        public event EventHandler OnSessionRefreshed;

        /// <summary>
        /// Gets or sets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// The opened <see cref="Iteration"/>
        /// </summary>
        public Iteration? OpenIteration { get; private set; }

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
        /// Open the iteration with the selected <see cref="EngineeringModelSetup"/> and <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="modelSetup"> The selected <see cref="EngineeringModelSetup"/> </param>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup"/></param>
        public async Task ReadIteration(IterationSetup? iterationSetup)
        {
            if(iterationSetup == null)
            {
                throw new ArgumentNullException(nameof(iterationSetup));
            }

            var modelSetup = (EngineeringModelSetup)iterationSetup.Container;

            var model = new EngineeringModel(modelSetup.EngineeringModelIid, this.Session.Assembler.Cache, this.Session.Credentials.Uri);
            var iteration = new Iteration(iterationSetup.IterationIid, this.Session.Assembler.Cache, this.Session.Credentials.Uri);
            iteration.Container = model;

            try
            {
                await this.Session.Read(iteration, this.CurrentDomainOfExpertise);
                if (!this.Session.OpenIterations.Any())
                {
                    throw new InvalidOperationException("At first an Iteration should be opened");
                }
                this.OpenIteration = this.Session.OpenIterations.FirstOrDefault().Key;

                CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.IterationOpened);
            }
            catch (Exception exception)
            {
                this.logger.Error($"During read operation an error has occured: {exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// Close the ReadIteration
        /// </summary>
        public void CloseIteration()
        {
            this.Session.CloseIterationSetup(this.OpenIteration?.IterationSetup);
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
            modelSetup?.Participant.FindAll(p => p.Person.Iid.Equals(this.Session.ActivePerson.Iid)).ForEach(p => p.Domain.ForEach(d => domains.Add(d)));
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

            this.OnSessionRefreshed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Switches the current domain for the opened iteration
        /// </summary>
        /// <param name="DomainOfExpertise">The domain</param>
        public void SwitchDomain(DomainOfExpertise? DomainOfExpertise)
        {
            var iteration = this.OpenIteration;
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
            var openedIteration = this.OpenIteration;
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
        /// <param name="thingsToUpdate">List of Things to update in the session</param>
        public async Task UpdateThings(IEnumerable<Thing> thingsToUpdate)
        {
            var sw = Stopwatch.StartNew();
            var openedIteration = this.OpenIteration;
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
                Console.WriteLine($"Update writing done in {sw.ElapsedMilliseconds} [ms]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The update operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Write new Things in the session
        /// </summary>
        /// <param name="thingsToCreate">List of Things to create in the session</param>
        public async Task CreateThingsSiteDirectory(IEnumerable<Thing> thingsToCreate)
        {
            var openedSiteDirectory = this.GetSiteDirectory();
            if (openedSiteDirectory == null)
            {
                throw new InvalidOperationException("At first a SiteDirectory should be opened");
            }
            if (thingsToCreate == null)
            {
                throw new ArgumentException("Please add at least one Thing to be created");
            }

            // CreateThings a shallow clone of the site directory. The cached site directory object should not be changed, so we record the change on a clone.
            var siteDirectoryClone = openedSiteDirectory.Clone(false);

            // set the context of the transaction to the site directory changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(siteDirectoryClone);
            var transaction = new ThingTransaction(context);

            // register new Things and the container site directory (clone) with the transaction.
            thingsToCreate.ToList().ForEach(thing =>
            {
                transaction.Create(thing, siteDirectoryClone);
            });

            // finalize the transaction, the result is an OperationContainer that the session class uses to write the changes
            // to the site directory (the list of contained elements is updated).
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
        ///     Update Things in the session
        /// </summary>
        /// <param name="thingsToUpdate">List of Things to update in the session</param>  
        public async Task UpdateThingsSiteDirectory(IEnumerable<Thing> thingsToUpdate)
        {
            var openedSiteDirectory = this.GetSiteDirectory();
            if (openedSiteDirectory == null)
            {
                throw new InvalidOperationException("At first a SiteDirectory should be opened");
            }
            if (thingsToUpdate == null)
            {
                throw new ArgumentException("Please add at least one Thing to be deleted");
            }

            // CreateThings a shallow clone of the iteration. The cached Iteration object should not be changed, so we record the change on a clone.
            var siteDirectoryClone = openedSiteDirectory.Clone(false);

            // set the context of the transaction to the iteration changes need to be added to.
            var context = TransactionContextResolver.ResolveContext(siteDirectoryClone);
            var transaction = new ThingTransaction(context);

            // register new Things and the container Iteration (clone) with the transaction.
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
            catch (DalWriteException ex)
            {
                Console.WriteLine($"The update operation failed: {ex.Message}");
            }
        }   

        /// <summary>
        /// Gets the <see cref="ParticipantRole"/> in the opened iteration
        /// </summary>
        public Participant? GetParticipant()
        {
            return this.GetSiteDirectory().Model.Find(m => m.IterationSetup.Contains(this.OpenIteration?.IterationSetup))?
                .Participant.Find(p => p.Person.Iid == this.Session.ActivePerson.Iid);
        }

        /// <summary>
        /// Gets the <see cref="Person"/>s 
        /// </summary>
        public IEnumerable<Person> GetPersons()
        {
            return this.GetSiteDirectory().Person;
        }

        /// <summary>
        /// Gets the <see cref="Organization"/>s
        /// </summary>
        public IEnumerable<Organization> GetAvailableOrganizations()
        {
            return this.GetSiteDirectory().Organization;
        }

        /// <summary>
        /// Gets the <see cref="PersonRole"/>s 
        /// </summary>
        public IEnumerable<PersonRole> GetAvailablePersonRoles()
        {
            return this.GetSiteDirectory().PersonRole;
        }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/>s 
        /// </summary>
        public IEnumerable<DomainOfExpertise> GetAvailableDomains()
        {
            return this.GetSiteDirectory().Domain;
        }
    }
}