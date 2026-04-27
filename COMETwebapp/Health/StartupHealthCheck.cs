// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StartupHealthCheck.cs" company="Starion Group S.A.">
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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Diagnostics.HealthChecks;

    /// <summary>
    /// Health check that reports Healthy once <see cref="ICometHasStartedService.HasStarted"/> is
    /// <c>true</c>. Used by both the Startup and Readiness probe endpoints.
    /// </summary>
    public class StartupHealthCheck : IHealthCheck
    {
        /// <summary>
        /// The (injected) <see cref="ICometHasStartedService"/> that holds the shared
        /// startup-completion flag observed by this check.
        /// </summary>
        private readonly ICometHasStartedService cometHasStartedService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupHealthCheck"/> class.
        /// </summary>
        /// <param name="cometHasStartedService">
        /// The (injected) <see cref="ICometHasStartedService"/> whose <see cref="ICometHasStartedService.HasStarted"/>
        /// flag determines the result of this health check.
        /// </param>
        public StartupHealthCheck(ICometHasStartedService cometHasStartedService)
        {
            this.cometHasStartedService = cometHasStartedService;
        }

        /// <summary>
        /// Runs the health check, returning <see cref="HealthStatus.Healthy"/> once the
        /// application has finished startup bootstrap and <see cref="HealthStatus.Unhealthy"/>
        /// while startup is still in progress.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HealthCheckContext"/> describing the registration. Not inspected here;
        /// the same check answers both the Startup and Readiness probe endpoints.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> propagated by the host. Unused — the check is a
        /// non-blocking flag read.
        /// </param>
        /// <returns>
        /// A completed <see cref="Task{TResult}"/> wrapping a <see cref="HealthCheckResult"/>:
        /// <see cref="HealthCheckResult.Healthy(string, System.Collections.Generic.IReadOnlyDictionary{string, object})"/>
        /// once started (description includes the start timestamp in ISO 8601 format), otherwise
        /// <see cref="HealthCheckResult.Unhealthy(string, System.Exception, System.Collections.Generic.IReadOnlyDictionary{string, object})"/>
        /// with description <c>"Startup in progress"</c>.
        /// </returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (this.cometHasStartedService.HasStarted)
            {
                var description = string.Format(CultureInfo.InvariantCulture, "Started since {0:O}", this.cometHasStartedService.StartedAt);
                return Task.FromResult(HealthCheckResult.Healthy(description));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Startup in progress"));
        }
    }
}
