// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Utilities
{
    using System;
    using System.Drawing;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;

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
            Color color = text.ParseToColor();
            return new Vector3(color.R, color.G, color.B);
        }

        /// <summary>
        /// Tries to parse this string into a color in format R,G,B
        /// </summary>
        /// <param name="text">the text to parse into color</param>
        /// <returns>the parsed color</returns>
        public static Color ParseToColor(this string text)
        {
            Color color;

            if (text.StartsWith("#"))
            {
                color = ColorTranslator.FromHtml(text);
            }
            else if (text.Contains(":"))
            {
                var textSplitted = text.Split(':');

                _ = int.TryParse(textSplitted[0], out var r);
                _ = int.TryParse(textSplitted[1], out var g);
                _ = int.TryParse(textSplitted[2], out var b);

                color = Color.FromArgb(r, g, b);
            }
            else
            {
                color = Color.FromName(text);
            }

            return color;
        }

        /// <summary>
        /// Tries to parse this string into a color in hexadecimal format
        /// </summary>
        /// <param name="text">the text to parse into color</param>
        /// <returns>a string of the color in hexadecimal format</returns>
        public static string ParseToHexColor(this string text)
        {
            var color = text.ParseToColor();
            var colorRGB = Color.FromArgb(color.R, color.G, color.B);
            return ColorTranslator.ToHtml(colorRGB);
        }
    }
}
