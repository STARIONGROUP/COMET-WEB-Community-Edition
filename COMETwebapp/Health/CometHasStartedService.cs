// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometHasStartedService.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
//
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Health
{
    using System;
    using System.Threading;

    /// <summary>
    /// Default <see cref="ICometHasStartedService"/> backed by a single in-process flag.
    /// Registered as a singleton so all probe checks observe the same state.
    /// </summary>
    public class CometHasStartedService : ICometHasStartedService
    {
        /// <summary>
        /// Backing field for <see cref="StartedAt"/>, stored as UTC ticks so that
        /// <see cref="Interlocked.CompareExchange(ref long, long, long)"/> can be used to publish
        /// the timestamp atomically.
        /// </summary>
        private long startedAtTicks;

        /// <summary>
        /// Backing field for <see cref="HasStarted"/>. Marked <c>volatile</c> so that probe
        /// requests on other threads observe the flip immediately.
        /// </summary>
        private volatile bool hasStarted;

        /// <summary>
        /// Gets a value indicating whether startup bootstrap has completed.
        /// </summary>
        public bool HasStarted => this.hasStarted;

        /// <summary>
        /// Gets the timestamp at which <see cref="MarkStarted"/> was first called, or
        /// <see cref="DateTime.MinValue"/> if startup has not yet completed.
        /// </summary>
        public DateTime StartedAt
        {
            get
            {
                var ticks = Interlocked.Read(ref this.startedAtTicks);
                return ticks == 0 ? DateTime.MinValue : new DateTime(ticks, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Marks the application as started. Idempotent — subsequent calls are ignored and the
        /// original <see cref="StartedAt"/> timestamp is preserved.
        /// </summary>
        public void MarkStarted()
        {
            if (this.hasStarted)
            {
                return;
            }

            Interlocked.CompareExchange(ref this.startedAtTicks, DateTime.UtcNow.Ticks, 0);
            this.hasStarted = true;
        }
    }
}
