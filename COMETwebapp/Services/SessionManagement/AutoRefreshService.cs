// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoRefreshService.cs" company="RHEA System S.A.">
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
    using System.Timers;

    /// <summary>
    /// Service to enable auto-refresh of the opened session
    /// </summary>
    public class AutoRefreshService : IAutoRefreshService, IDisposable
    {
        /// <summary>
        /// The <see cref="ISessionAnchor"/> used to get access to the <see cref="ISession"/>
        /// </summary>
        private readonly ISessionAnchor sessionAnchor;

        /// <summary>
        /// Define seconds left in the timer before the next refresh
        /// </summary>
        private int autoRefreshSecondsLeft;

        /// <summary>
        /// The timer
        /// </summary>
        private Timer timer { get; set; }

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
        /// Initializes a new instance of the <see cref="AutoRefreshService"/>
        /// </summary>
        /// <param name="sessionAnchor">
        /// The (injected) <see cref="ISessionAnchor"/> to automatically refresh
        /// </param>
        public AutoRefreshService(ISessionAnchor sessionAnchor)
        {
            this.sessionAnchor = sessionAnchor;
            this.timer = new Timer(1000);
            this.timer.Elapsed += this.OntTimerElapsed;
        }

        /// <summary>
        /// Sets the timer according to the appropriate setting
        /// </summary>
        public void SetTimer()
        {
            if (this.IsAutoRefreshEnabled)
            {
                this.autoRefreshSecondsLeft = this.AutoRefreshInterval;
                this.timer.Start();
            }
            else 
            {
                this.timer.Stop();
            }
        }

        /// <summary>
        /// The eventhandler to handle elapse of one second.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments.</param>
        private async void OntTimerElapsed(object? sender, EventArgs? e)
        {
            this.autoRefreshSecondsLeft -= 1;

            if (this.autoRefreshSecondsLeft == 0)
            {
                this.timer.Stop();
                await this.sessionAnchor.RefreshSession();

                this.autoRefreshSecondsLeft = this.AutoRefreshInterval;
                this.timer.Start();
            }
        }

        public void Dispose()
        {
            try
            {
                this.timer.Elapsed -= this.OntTimerElapsed;
                this.timer.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
