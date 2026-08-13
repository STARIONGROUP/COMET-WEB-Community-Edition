// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Application.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Model
{
    using COMET.Web.Common.Enumerations;

    /// <summary>
    /// Define application information
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Name of the application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A little description of the application
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The color of the icon to represent the application
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// The glyph of the consolidated icon system used to represent the application on its card, tab and
        /// side-bar entry.
        /// </summary>
        public IconName Icon { get; set; }

        /// <summary>
        /// The navigation url for the current application
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Value asserting that the current <see cref="Application" /> is currently disabled
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// A one-sentence summary of what the user can accomplish on this application's page.
        /// </summary>
        public string PageIntroSummary { get; set; }

        /// <summary>
        /// Short, verb-led statements of what the user can do on this application's page.
        /// </summary>
        /// <remarks>
        /// A maximum of four bullet points is suggested.
        /// </remarks>
        public IEnumerable<string> PageIntroPoints { get; set; } = [];

        /// <summary>
        /// Gets the user preference key used to persist the introduction box dismissal state for this application.
        /// </summary>
        /// <returns>The string key for the user preference.</returns>
        public string GetPageIntroUserPreferenceKey()
        {
            return $"comet.intro.dismissed.{this.Url}";
        }
    }
}
