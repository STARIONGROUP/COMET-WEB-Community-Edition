// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationDescriptionBuilder.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
//
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Utilities
{
    using COMET.Web.Common.Model;

    /// <summary>
    /// Builds the success and failure messages shown after a write about one named thing, shared by the Requirements
    /// Editor and the annotations so they word them the same way: "Change Request 'CR-001' deleted" and "Could not delete
    /// the Change Request 'CR-001'".
    /// </summary>
    public static class NotificationDescriptionBuilder
    {
        /// <summary>
        /// Builds the <see cref="NotificationDescription" /> of a write about one thing.
        /// </summary>
        /// <param name="kind">The human-readable kind of the thing, e.g. "Change Request"</param>
        /// <param name="label">The name the thing is known by, e.g. its short name</param>
        /// <param name="pastVerb">The past-tense phrase for the success message, e.g. "deleted"</param>
        /// <param name="actionVerb">The infinitive phrase for the failure message, e.g. "delete"</param>
        /// <returns>The <see cref="NotificationDescription" /></returns>
        public static NotificationDescription Build(string kind, string label, string pastVerb, string actionVerb)
        {
            return new NotificationDescription
            {
                OnSuccess = $"{kind} '{label}' {pastVerb}",
                OnError = $"Could not {actionVerb} the {kind} '{label}'"
            };
        }
    }
}
