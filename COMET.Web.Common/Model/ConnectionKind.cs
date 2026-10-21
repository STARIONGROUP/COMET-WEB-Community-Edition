// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConnectionKind.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Model
{
    /// <summary>
    /// Represents a way of connecting offered on the landing page, which decides whether the server login or the
    /// ECSS-E-TM-10-25 Annex C3 archive login is presented
    /// </summary>
    /// <param name="Value">The value that identifies the option, which is what gets remembered in session storage</param>
    /// <param name="Name">The text presented to the user</param>
    public sealed record ConnectionKind(string Value, string Name)
    {
        /// <summary>
        /// Gets the option that connects to a CDP4-COMET server, which is the default
        /// </summary>
        public static ConnectionKind Server { get; } = new("server", "Connect to a server");

        /// <summary>
        /// Gets the option that opens an ECSS-E-TM-10-25 Annex C3 archive read-only
        /// </summary>
        public static ConnectionKind Archive { get; } = new("archive", "Open a model archive (read-only)");

        /// <summary>
        /// Gets the ways a user can connect, in the order they are offered
        /// </summary>
        public static IReadOnlyList<ConnectionKind> All { get; } = [Server, Archive];
    }
}
