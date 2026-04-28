// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ICometHasStartedService.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Tracks whether the CDP4-COMET WEB application has finished its startup bootstrap and is
    /// ready to accept traffic. Backs the Startup and Readiness health probes.
    /// </summary>
    public interface ICometHasStartedService
    {
        /// <summary>
        /// Gets a value indicating whether startup bootstrap has completed.
        /// </summary>
        bool HasStarted { get; }

        /// <summary>
        /// Gets the timestamp at which <see cref="MarkStarted"/> was first called, or
        /// <see cref="DateTime.MinValue"/> if startup has not yet completed.
        /// </summary>
        DateTime StartedAt { get; }

        /// <summary>
        /// Marks the application as started. Idempotent — subsequent calls are ignored and the
        /// original <see cref="StartedAt"/> timestamp is preserved.
        /// </summary>
        void MarkStarted();
    }
}
