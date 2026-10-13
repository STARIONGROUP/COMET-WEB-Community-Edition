// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceExtensions.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Extensions
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    /// <summary>
    /// Static class containing extension methods for <see cref="ISessionService" />.
    /// </summary>
    public static class SessionServiceExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseDataItemRowViewModel{T}.IsAllowedToWrite" /> on each of the provided rows from the active
        /// user's permission on the <see cref="Thing" /> it represents.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="rows">The rows to refresh, which may be null.</param>
        /// <typeparam name="T">The type of <see cref="Thing" /> the rows represent.</typeparam>
        /// <remarks>
        /// Uses the per-thing <c>CanWrite</c> overload rather than the <c>(ClassKind, container)</c> one, because the
        /// latter is the <i>creation</i> overload and answers true under MODIFY_IF_OWNER regardless of ownership. For a
        /// row that already exists the ownership has to be honoured, which only the per-thing overload does.
        /// </remarks>
        public static void RefreshWritePermissions<T>(this ISessionService sessionService, IEnumerable<BaseDataItemRowViewModel<T>> rows) where T : Thing
        {
            if (rows == null)
            {
                return;
            }

            var permissionService = sessionService.Session.PermissionService;

            foreach (var row in rows)
            {
                row.IsAllowedToWrite = permissionService.CanWrite(row.Thing);
            }
        }

        /// <summary>
        /// Asserts whether the active user may add a new <paramref name="classKind" /> to <paramref name="container" />.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="classKind">The <see cref="ClassKind" /> that would be created.</param>
        /// <param name="container">The <see cref="Thing" /> that would contain it, which may be null.</param>
        /// <returns>True if a new <paramref name="classKind" /> may be created inside <paramref name="container" />.</returns>
        /// <remarks>
        /// Centralises the two guards that every create gate needs. A read-only session refuses outright, so a control
        /// bound to this needs no separate <c>IsReadOnly</c> check. A container that is itself still being created has no
        /// <see cref="Thing.Container" /> of its own yet; asking <c>CanWrite</c> about it would resolve
        /// SAME_AS_CONTAINER against a null container, so it is allowed here - the create permission for the aggregate
        /// was already asserted by the table whose add button opened the form.
        /// </remarks>
        public static bool IsAllowedToCreateIn(this ISessionService sessionService, ClassKind classKind, Thing container)
        {
            if (sessionService.IsReadOnly || container == null)
            {
                return false;
            }

            return container.Container == null || sessionService.Session.PermissionService.CanWrite(classKind, container);
        }

        /// <summary>
        /// Gets the available <see cref="NaturalLanguage" />s offered for a definition: the default IME language set
        /// (<see cref="DefaultNaturalLanguages.All" />) merged with any languages defined on the model's
        /// <see cref="SiteDirectory" />,
        /// the model ones taking precedence, de-duplicated by language code and ordered by name.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <returns>A list of available <see cref="NaturalLanguage" />s.</returns>
        public static List<NaturalLanguage> GetAvailableNaturalLanguages(this ISessionService sessionService)
        {
            var languagesByCode = new Dictionary<string, NaturalLanguage>(StringComparer.OrdinalIgnoreCase);

            foreach (var language in DefaultNaturalLanguages.All)
            {
                languagesByCode[language.LanguageCode] = language;
            }

            var siteDirectory = sessionService?.GetSiteDirectory();

            if (siteDirectory?.NaturalLanguage == null)
            {
                return languagesByCode.Values
                    .OrderBy(x => string.IsNullOrWhiteSpace(x.NativeName) ? x.Name : x.NativeName, StringComparer.InvariantCultureIgnoreCase)
                    .ToList();
            }

            foreach (var language in siteDirectory.NaturalLanguage)
            {
                languagesByCode[language.LanguageCode] = language;
            }

            return languagesByCode.Values
                .OrderBy(x => string.IsNullOrWhiteSpace(x.NativeName) ? x.Name : x.NativeName, StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }
    }
}
