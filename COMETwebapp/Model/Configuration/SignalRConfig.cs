// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SignalRConfig.cs" company="Starion Group S.A.">
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Model.Configuration
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Configuration options bound from the <c>SignalR</c> section of <c>appsettings.json</c> for SignalR connection timeouts.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-10.0"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SignalRConfig
    {
        /// <summary>
        /// Gets or sets the SignalR keep-alive interval in seconds (default is 15s; configured to 5s for faster drop detection).
        /// </summary>
        public int KeepAliveSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the SignalR client timeout interval in seconds (default is 30s; recommended value is double <see cref="KeepAliveSeconds"/>).
        /// </summary>
        public int ClientTimeoutSeconds { get; set; } = 10;
    }
}
