// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ResultExtensions.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Extensions
{
    using System.Text.RegularExpressions;

    using AntDesign;

    using FluentResults;

    /// <summary>
    /// Extension class for Result
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Gets the toast description, from <see cref="NotificationService" />
        /// </summary>
        /// <param name="result">The validation result base</param>
        /// <returns>A html string containing the description to be displayed</returns>
        public static string GetHtmlErrorsDescription(this IResultBase result)
        {
            return $"""
                       {string.Join("<br>", result.Errors.Select(GetErrorString))}
                       <br>If the error persists, check <a href='https://github.com/STARIONGROUP/COMET-WEB-Community-Edition/issues' target='_blank'>our issues page</a>
                    """;
        }

        /// <summary>
        /// Gets the error string for display in the description of the <see cref="NotificationService" /> toast
        /// </summary>
        /// <param name="error">The error to be displayed</param>
        /// <returns>A html string containing the error</returns>
        private static string GetErrorString(this IReason error)
        {
            string errorLine;
            var errorId = Guid.NewGuid();

            if (error is IExceptionalError exceptionalError)
            {
                errorLine = $"""
                             <a href="#errorDetails{errorId}" data-bs-toggle="collapse" aria-expanded="false" class="text-reset">{exceptionalError.Message}</a>
                             <div class="collapse opacity-75" id="errorDetails{errorId}">{Regex.Unescape(exceptionalError.Exception.ToString())}</div>
                             """;
            }
            else
            {
                errorLine = error.Message;
            }

            return errorLine;
        }
    }
}
