// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefaultNaturalLanguages.cs" company="Starion Group S.A.">
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
    using System.Reflection;
    using System.Resources;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Provides the default set of <see cref="NaturalLanguage" />s (the specific cultures the desktop IME offers) as a
    /// bundled list. The list is embedded because the application runs with <c>InvariantGlobalization</c> enabled, under
    /// which <see cref="System.Globalization.CultureInfo.GetCultures" /> returns only the invariant culture at runtime.
    /// </summary>
    public static class DefaultNaturalLanguages
    {
        /// <summary>
        /// The fully-qualified name of the embedded language resource (one <c>code|englishName|nativeName</c> line each).
        /// </summary>
        private const string ResourceName = "COMETwebapp.Resources.NaturalLanguages.txt";

        /// <summary>
        /// The lazily-parsed list of default languages.
        /// </summary>
        private static readonly Lazy<IReadOnlyList<NaturalLanguage>> LazyLanguages = new(Load);

        /// <summary>
        /// Gets the default <see cref="NaturalLanguage" />s.
        /// </summary>
        public static IReadOnlyList<NaturalLanguage> All => LazyLanguages.Value;

        /// <summary>
        /// Parses the embedded resource into <see cref="NaturalLanguage" />s.
        /// </summary>
        /// <returns>The default languages.</returns>
        private static List<NaturalLanguage> Load()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName) ?? throw new MissingManifestResourceException($"The embedded resource '{ResourceName}' was not found.");
            using var reader = new StreamReader(stream);

            var languages = new List<NaturalLanguage>();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split('|');

                if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    continue;
                }

                languages.Add(new NaturalLanguage
                {
                    LanguageCode = parts[0],
                    Name = parts[1],
                    NativeName = parts[2]
                });
            }

            return languages;
        }
    }
}
