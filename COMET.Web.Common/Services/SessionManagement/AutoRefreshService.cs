// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AutoRefreshService.cs" company="RHEA System S.A.">
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
    using System.Timers;

    using CDP4Dal;

    using COMET.Web.Common.Utilities.DisposableObject;

    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// Service to enable auto-refresh of the opened session
    /// </summary>
    public sealed class AutoRefreshService : DisposableObject, IAutoRefreshService
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

            this.Disposables.Add(this.WhenAnyPropertyChanged(nameof(this.IsAutoRefreshEnabled),
                nameof(this.AutoRefreshInterval)).Subscribe(_ => this.SetTimer()));
        }

        /// <summary>
        /// Define seconds left in the timer before the next refresh
        /// </summary>
        public int AutoRefreshSecondsLeft { get; private set; }

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
                this.AutoRefreshSecondsLeft = this.AutoRefreshInterval;
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
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

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
            this.AutoRefreshSecondsLeft -= 1;

            if (this.AutoRefreshSecondsLeft != 0)
            {
                return;
            }

            this.timer.Stop();
            await this.sessionService.RefreshSession();

            this.AutoRefreshSecondsLeft = this.AutoRefreshInterval;
            this.timer.Start();
        }
    }
}
