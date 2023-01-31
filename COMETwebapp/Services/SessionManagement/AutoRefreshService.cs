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

namespace COMETwebapp.Services.SessionManagement
{
    using System.Timers;

    using CDP4Dal;

    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// Service to enable auto-refresh of the opened session
    /// </summary>
    public sealed class AutoRefreshService : ReactiveObject, IAutoRefreshService, IDisposable
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to get access to the <see cref="ISession" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="Timer" />
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// Backing field for <see cref="AutoRefreshInterval" />
        /// </summary>
        private int autoRefreshInterval = 60;

        /// <summary>
        /// Define seconds left in the timer before the next refresh
        /// </summary>
        private int autoRefreshSecondsLeft;

        /// <summary>
        /// A collection of <see cref="IDisposable" />
        /// </summary>
        private readonly List<IDisposable> disposables = new();

        /// <summary>
        /// Backing field for <see cref="IsAutoRefreshEnabled" />
        /// </summary>
        private bool isAutoRefreshEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRefreshService" />
        /// </summary>
        /// <param name="sessionService">
        /// The (injected) <see cref="ISessionService" /> to automatically refresh
        /// </param>
        public AutoRefreshService(ISessionService sessionService)
        {
            this.sessionService = sessionService;
            this.timer = new Timer(1000);
            this.timer.Elapsed += this.OnTimerElapsed;

            this.disposables.Add(this.WhenAnyPropertyChanged(nameof(this.IsAutoRefreshEnabled),
                nameof(this.AutoRefreshInterval)).Subscribe(_ => this.SetTimer()));
        }

        /// <summary>
        /// Enable / disable auto-refresh for the ISession
        /// </summary>
        public bool IsAutoRefreshEnabled
        {
            get => this.isAutoRefreshEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isAutoRefreshEnabled, value);
        }

        /// <summary>
        /// Define the interval in sec to auto-refresh the session
        /// Set to 60s by default
        /// </summary>
        public int AutoRefreshInterval
        {
            get => this.autoRefreshInterval;
            set => this.RaiseAndSetIfChanged(ref this.autoRefreshInterval, value);
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.timer.Elapsed -= this.OnTimerElapsed;
                this.timer.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// The eventhandler to handle elapse of one second.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments.</param>
        private async void OnTimerElapsed(object sender, EventArgs e)
        {
            this.autoRefreshSecondsLeft -= 1;

            if (this.autoRefreshSecondsLeft == 0)
            {
                this.timer.Stop();
                await this.sessionService.RefreshSession();

                this.autoRefreshSecondsLeft = this.AutoRefreshInterval;
                this.timer.Start();
            }
        }
    }
}
