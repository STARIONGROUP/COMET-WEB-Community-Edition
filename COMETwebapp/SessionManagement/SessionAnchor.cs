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
        /// Enable / disable auto-refresh for the ISession
        /// </summary>
        public bool IsAutoRefreshEnabled { get; set; }

        /// <summary>
        /// Define the interval in sec to auto-refresh the session
        /// Set to 60s by default
        /// </summary>
        public int AutoRefreshInterval { get; set; } = 60;

        /// <summary>
        /// True if the <see cref="ISession"/> is opened
        /// </summary>
        public bool IsSessionOpen { get; set; }

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
            } else
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
            }
        }

        /// <summary>
        /// Close the <see cref="OpenIteration"/>
        /// </summary>
        public void CloseIteration()
        {
            this.Session.CloseIterationSetup(this.GetIteration()?.IterationSetup);
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
            if(this.GetIteration() != null)
            {
                this.CurrentDomainOfExpertise = DomainOfExpertise;
                this.Session.SwitchDomain(this.GetIteration().Iid, DomainOfExpertise);
            }
        }
    }
}
