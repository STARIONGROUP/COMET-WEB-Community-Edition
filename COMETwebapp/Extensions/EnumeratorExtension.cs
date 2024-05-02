// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumeratorExtensions.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
namespace COMETwebapp.Extensions
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Extension class for Enumerator
    /// </summary>
    public static class EnumeratorExtension
    {
        /// <summary>
        ///     A <see cref="Dictionary{TKey,TValue}" /> to cache the name correspondance
        /// </summary>
        private static readonly Dictionary<Enum, string> displayName = new();

        /// <summary>
        ///     Retrieve the <see cref="DisplayAttribute" /> of an <see cref="Enum" /> value
        /// </summary>
        /// <param name="value">The <see cref="Enum" /> value</param>
        /// <returns>The <see cref="DisplayAttribute" /> value if found</returns>
        public static string GetEnumDisplayName(this Enum value)
        {
            if (displayName.TryGetValue(value, out string enumDisplayName))
            {
                return enumDisplayName;
            }

            var fi = value.GetType().GetField(value.ToString());

            if (fi == null)
            {
                return value.ToString();
            }

            var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);
            enumDisplayName = attributes is { Length: > 0 } ? attributes[0].Name : value.ToString();
            displayName[value] = enumDisplayName;
            return enumDisplayName;
        }
    }
}
