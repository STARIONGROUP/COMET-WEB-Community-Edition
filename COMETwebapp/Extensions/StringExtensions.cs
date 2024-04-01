// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using System.Numerics;
    using System.Text;

    using COMET.Web.Common.Extensions;

    /// <summary>
    /// static extension methods for <see cref="string"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// BASE64 encodes a string
        /// </summary>
        /// <param name="text">
        /// the string that is to be encoded
        /// </param>
        /// <returns>
        /// a BASE64 encoded string
        /// </returns>
        public static string Base64Encode(this string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString;
        }

        /// <summary>
        /// BASE64 decodes a string
        /// </summary>
        /// <param name="text">
        /// the BASE64 encoded string that is to be decoded
        /// </param>
        /// <returns>
        /// a BASE64 decoded string
        /// </returns>
        public static string Base64Decode(this string text)
        {
            var bytes = Convert.FromBase64String(text);

            var decodedString = Encoding.UTF8.GetString(bytes);

            return decodedString;
        }

        /// <summary>
        /// Tries to parse this string into a vector that represents a color in format R,G,B
        /// </summary>
        /// <param name="text">the text to parse into color</param>
        /// <returns>the parsed color</returns>
        public static Vector3 ParseToColorVector(this string text)
        {
            var color = text.ParseToColor();
            return new Vector3(color.R, color.G, color.B);
        }

        /// <summary>
        /// Queries the name of the body component of a page
        /// </summary>
        /// <param name="pageName">The page name</param>
        /// <returns>The compute name for the body component</returns>
        public static string QueryPageBodyName(this string pageName)
        {
            return $"{pageName}-body".ToLower();
        }

        /// <summary>
        /// Takes the first <see cref="numberOfCharacters"/> characters from a string
        /// </summary>
        /// <param name="input">The string to take characters of</param>
        /// <param name="numberOfCharacters">The number of characters to take</param>
        /// <returns>A string containing the characters taken</returns>
        public static string TakeString(this string input, int numberOfCharacters)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.Length <= numberOfCharacters ? input : input[..numberOfCharacters];
        }
    }
}
