// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="GuidExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Extensions
{
    /// <summary>
    /// static extension methods for <see cref="Guid" />
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts a <see cref="Guid" /> string representation to a ShortGuid
        /// </summary>
        /// <param name="value">
        /// a string representation of a <see cref="Guid" />
        /// </param>
        /// <returns>
        /// a ShortGuid representation of the provided <see cref="Guid" />
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// </remarks>
        public static string ToShortGuid(this string value)
        {
            var guid = new Guid(value);
            return guid.ToShortGuid();
        }

        /// <summary>
        /// converts a <see cref="Guid" /> to its base64 encoded short form
        /// </summary>
        /// <param name="guid">
        /// an instance of <see cref="Guid" />
        /// </param>
        /// <returns>
        /// a shortGuid representation of the provided <see cref="Guid" />
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// </remarks>
        public static string ToShortGuid(this Guid guid)
        {
            var enc = Convert.ToBase64String(guid.ToByteArray());
            return enc.Replace("/", "_").Replace("+", "-")[..22];
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{String}" /> <see cref="Guid" /> string representation to a ShortGuid
        /// </summary>
        /// <param name="values">
        /// an <see cref="IEnumerable{String}" /> representation of a <see cref="Guid" />s
        /// </param>
        /// <returns>
        /// a <see cref="IEnumerable{String}" /> ShortGuid representation of the provided <see cref="Guid" />s
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// </remarks>
        public static IEnumerable<string> ToShortGuids(this IEnumerable<string> values)
        {
            return values.Select(value => new Guid(value)).Select(guid => guid.ToShortGuid());
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{Guid}" /> to an <see cref="IEnumerable{String}" /> ShortGuid
        /// </summary>
        /// <param name="guids">
        /// an <see cref="IEnumerable{Guid}" />
        /// </param>
        /// <returns>
        /// a <see cref="IEnumerable{String}" /> ShortGuid representation of the provided <see cref="Guid" />s
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// </remarks>
        public static IEnumerable<string> ToShortGuids(this IEnumerable<Guid> guids)
        {
            return guids.Select(guid => guid.ToShortGuid());
        }

        /// <summary>
        /// Creates a <see cref="Guid" /> based the ShortGuid representation
        /// </summary>
        /// <param name="shortGuid">
        /// a shortGuid string
        /// </param>
        /// <returns>
        /// an instance of <see cref="Guid" />
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// </remarks>
        public static Guid FromShortGuid(this string shortGuid)
        {
            var buffer = Convert.FromBase64String(shortGuid.Replace("_", "/").Replace("-", "+") + "==");
            return new Guid(buffer);
        }

        /// <summary>
        /// Creates a <see cref="IEnumerable{Guid}" /> based the ShortGuid Array representation ->
        /// </summary>
        /// <param name="shortGuids">
        /// an <see cref="IEnumerable{String}" /> shortGuid
        /// </param>
        /// <returns>
        /// an <see cref="IEnumerable{Guid}" />
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// A ShortGuid Array is a string that starts with "[", ends with "]" and contains a number of ShortGuid separated by a ";"
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="shortGuids" /> does not start with '[' or ends with ']'
        /// </exception>
        public static IEnumerable<Guid> FromShortGuidArray(this string shortGuids)
        {
            ValidateShortGuidArray(shortGuids);

            var listOfShortGuids = shortGuids.TrimStart('[').TrimEnd(']').Split(';');

            foreach (var shortGuid in listOfShortGuids)
            {
                yield return shortGuid.FromShortGuid();
            }
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{Guid}" /> to an Array of ShortGuid
        /// </summary>
        /// <param name="guids">
        /// an <see cref="IEnumerable{Guid}" />
        /// </param>
        /// <returns>
        /// a <see cref="IEnumerable{String}" /> ShortGuid representation of the provided <see cref="Guid" />s
        /// </returns>
        /// <remarks>
        /// A ShortGuid is a base64 encoded guid-string representation where any "/" has been replaced with a "_"
        /// and any "+" has been replaced with a "-" (to make the string representation <see cref="Uri" /> friendly)
        /// A ShortGuid Array is a string that starts with "[", ends with "]" and contains a number of ShortGuid separated by a ";"
        /// </remarks>
        public static string ToShortGuidArray(this IEnumerable<Guid> guids)
        {
            var shortGuids = guids.Select(guid => guid.ToShortGuid()).ToList();

            return "[" + string.Join(";", shortGuids) + "]";
        }

        /// <summary>
        /// Verifies that the provided <paramref name="shortGuidArray" /> is a Valid one. A valid short guid array
        /// should starts with a '[' and ends with ']'
        /// </summary>
        /// <param name="shortGuidArray">The array of short guid to verify</param>
        /// <exception cref="ArgumentException">If the provided short guid array does not start with a '[' or not end with ']'</exception>
        private static void ValidateShortGuidArray(string shortGuidArray)
        {
            if (!shortGuidArray.StartsWith('['))
            {
                throw new ArgumentException("Invalid ShortGuid Array, must start with [", nameof(shortGuidArray));
            }

            if (!shortGuidArray.EndsWith(']'))
            {
                throw new ArgumentException("Invalid ShortGuid Array, must end with ]", nameof(shortGuidArray));
            }
        }
    }
}
