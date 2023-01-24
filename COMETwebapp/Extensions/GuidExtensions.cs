// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuidExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// static extension methods for <see cref="Guid"/>
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts a <see cref="Guid"/> string representation to a ShortGuid
        /// </summary>
        /// <param name="value">
        /// a string representation of a <see cref="Guid"/>
        /// </param>
        /// <returns>
        /// a ShortGuid representation of the provided <see cref="Guid"/>
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri"/> friendly)
        /// </remarks>
        public static string ToShortGuid(this string value)
        {
            var guid = new Guid(value);
            return guid.ToShortGuid();
        }

        /// <summary>
        /// converts a <see cref="Guid"/> to its base64 encoded short form
        /// </summary>
        /// <param name="guid">
        /// an instance of <see cref="Guid"/>
        /// </param>
        /// <returns>
        /// a shortGuid representation of the provided <see cref="Guid"/>
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri"/> friendly)
        /// </remarks>
        public static string ToShortGuid(this Guid guid)
        {
            var enc = Convert.ToBase64String(guid.ToByteArray());
            return enc.Replace("/", "_").Replace("+", "-").Substring(0, 22);
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{String}"/> <see cref="Guid"/> string representation to a ShortGuid
        /// </summary>
        /// <param name="values">
        /// an <see cref="IEnumerable{String}"/> representation of a <see cref="Guid"/>s
        /// </param>
        /// <returns>
        /// a <see cref="IEnumerable{String}"/> ShortGuid representation of the provided <see cref="Guid"/>s
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri"/> friendly)
        /// </remarks>
        public static IEnumerable<string> ToShortGuids(this IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                var guid = new Guid(value);
                yield return guid.ToShortGuid();
            }
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{Guid}"/> to an <see cref="IEnumerable{String}"/> ShortGuid
        /// </summary>
        /// <param name="guids">
        /// an <see cref="IEnumerable{Guid}"/>
        /// </param>
        /// <returns>
        /// a <see cref="IEnumerable{String}"/> ShortGuid representation of the provided <see cref="Guid"/>s
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri"/> friendly)
        /// </remarks>
        public static IEnumerable<string> ToShortGuids(this IEnumerable<Guid> guids)
        {
            foreach (var guid in guids)
            {
                yield return guid.ToShortGuid();
            }
        }

        /// <summary>
        /// Creates a <see cref="Guid"/> based the ShortGuid representation
        /// </summary>
        /// <param name="shortGuid">
        /// a shortGuid string
        /// </param>
        /// <returns>
        /// an instance of <see cref=".Guid"/>
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref=".Uri"/> friendly)
        /// </remarks>
        public static Guid FromShortGuid(this string shortGuid)
        {
            var buffer = Convert.FromBase64String(shortGuid.Replace("_", "/").Replace("-", "+") + "==");
            return new Guid(buffer);
        }

        /// <summary>
        /// Creates a <see cref="IEnumerable{Guid}"/> based the ShortGuid Array representation -> 
        /// </summary>
        /// <param name="shortGuids">
        /// an <see cref="IEnumerable{String}"/> shortGuid 
        /// </param>
        /// <returns>
        /// an <see cref="IEnumerable{Guid}"/>
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref=".Uri"/> friendly)
        /// A ShortGuid Array is a string that starts with "[", ends with "]" and contains a number of ShortGuid separated by a ";"
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="shortGuids"/> does not start with '[' or ends with ']'
        /// </exception>
        public static IEnumerable<Guid> FromShortGuidArray(this string shortGuids)
        {
            if (!shortGuids.StartsWith("["))
            {
                throw new ArgumentException("Invalid ShortGuid Array, must start with [", nameof(shortGuids));
            }

            if (!shortGuids.EndsWith("]"))
            {
                throw new ArgumentException("Invalid ShortGuid Array, must end with ]", nameof(shortGuids));
            }

            var listOfShortGuids = shortGuids.TrimStart('[').TrimEnd(']').Split(';');

            foreach (var shortGuid in listOfShortGuids)
            {
                yield return shortGuid.FromShortGuid();
            }
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{Guid}"/> to an Array of ShortGuid
        /// </summary>
        /// <param name="guids">
        /// an <see cref="IEnumerable{Guid}"/>
        /// </param>
        /// <returns>
        /// a <see cref="IEnumerable{String}"/> ShortGuid representation of the provided <see cref="Guid"/>s
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri"/> friendly)
        /// A ShortGuid Array is a string that starts with "[", ends with "]" and contains a number of ShortGuid separated by a ";"
        /// </remarks>
        public static string ToShortGuidArray(this IEnumerable<Guid> guids)
        {
            var shortGuids = new List<string>();

            foreach (var guid in guids)
            {
                shortGuids.Add(guid.ToShortGuid());
            }

            return "[" + string.Join(";", shortGuids) + "]";
        }
    }
}
