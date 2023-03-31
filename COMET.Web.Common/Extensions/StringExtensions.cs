// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StringExtensions.cs" company="RHEA System S.A.">
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
    using System.Drawing;

    using Microsoft.AspNetCore.WebUtilities;

    /// <summary>
    /// Extension class for <see cref="string" />
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets the parameter from an url
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}" /> containing the parameters</returns>
        public static Dictionary<string, string> GetParametersFromUrl(this string url)
        {
            var parametersSection = url.Contains('?') ? url.Split("?")[1] : url;
            var parameters = new Dictionary<string, string>();

            foreach (var parsedQuery in QueryHelpers.ParseQuery(parametersSection).Where(x => !string.IsNullOrEmpty(x.Value.ToString())))
            {
                parameters[parsedQuery.Key] = parsedQuery.Value;
            }

            return parameters;
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
            else if (text.Contains(':'))
            {
                var textSplitted = text.Split(':');

                if (int.TryParse(textSplitted[0], out var r) && int.TryParse(textSplitted[1], out var g) && int.TryParse(textSplitted[2], out var b))
                {
                    color = Color.FromArgb(r, g, b);
                }
                else
                {
                    color = Color.White;
                }
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
            var colorRgb = Color.FromArgb(color.R, color.G, color.B);
            return ColorTranslator.ToHtml(colorRgb);
        }
    }
}
