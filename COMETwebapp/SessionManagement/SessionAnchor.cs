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
        /// Define the interval in sec to auto-refresh the session
        /// Set to 60s by default
        /// </summary>
        public uint AutoRefreshInterval { get; set; } = 60;

        /// <summary>
        /// True if the <see cref="ISession"/> is opened
        /// </summary>
        public bool IsSessionOpen { get; set; }

        /// <summary>
        /// The opened <see cref="Iteration"/>
        /// </summary>
        public Iteration? OpenIteration { get; set; }

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> selected to open a model
        /// </summary>
        public DomainOfExpertise? CurrentDomainOfExpertise { get; set; }

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
            this.Session.Close().GetAwaiter().GetResult();
            this.IsSessionOpen = false;
            this.CloseIteration();
        }

        /// <summary>
        /// Reads an <see cref="Iteration"/> and set the active <see cref="DomainOfExpertise"/> for the Iteration
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{T,T}"/> of <see cref="Iteration"/> and <see cref="Tuple{T,T}"/> of <see cref="DomainOfExpertise"/> and <see cref="Participant"/></returns>
        public IReadOnlyDictionary<Iteration, Tuple<DomainOfExpertise, Participant>> GetIteration() => this.Session.OpenIterations;

        /// <summary>
        /// Open the iteration with the selected <see cref="EngineeringModelSetup"/> and <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="modelSetup"> The selected <see cref="EngineeringModelSetup"/> </param>
        /// <param name="iterationSetup">The selected <see cref="IterationSetup"/></param>
        public async Task GetIteration(EngineeringModelSetup? modelSetup, IterationSetup? iterationSetup)
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
                if (this.GetIteration() != null)
                {
                    this.OpenIteration = this.GetIteration().First().Key;
                }
            }
        }

        /// <summary>
        /// Close the <see cref="OpenIteration"/>
        /// </summary>
        public void CloseIteration()
        {
            this.Session.CloseIterationSetup(this.OpenIteration?.IterationSetup);
            this.OpenIteration = null;
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
            CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.Refreshing);
            var actualIterationSetup = this.OpenIteration?.IterationSetup;
            var actualEngineeringModelSetup = this.GetSiteDirectory().Model.Find(m => m.IterationSetup.Contains(actualIterationSetup));
            await this.Session.Refresh();
            await this.GetIteration(actualEngineeringModelSetup, actualIterationSetup);
            CDPMessageBus.Current.SendMessage<SessionStateKind>(SessionStateKind.UpToDate);
            Console.WriteLine("Session refreshed !");
        }
    }
}
