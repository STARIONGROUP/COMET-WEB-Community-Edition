// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JSInterop.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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
namespace COMETwebapp
{
    using Microsoft.JSInterop;

    /// <summary>
    /// Class to invoke the JS interoperability 
    /// </summary>
    public static class JSInterop
    {
        /// <summary>
        /// Field for the <see cref="JsRuntime"/> property
        /// </summary>
        private static IJSRuntime jsruntime;

        /// <summary>
        /// Used for the invoke of JS methods
        /// </summary>
        public static IJSRuntime JsRuntime
        {
            get => jsruntime;
            set
            {
                if (jsruntime == null)
                    jsruntime = value;
            }
        }

        /// <summary>
        /// Invoke a void method from javascript
        /// </summary>
        /// <param name="methodName">The name of the method in the javascript file</param>
        public static async Task Invoke(string methodName)
        {
            await JsRuntime.InvokeVoidAsync(methodName);
        }

        /// <summary>
        /// Invoke a void method from javascript with the specified parameters
        /// </summary>
        /// <param name="methodName">The name of the method in the javascript file</param>
        /// <param name="args">The arguments expected for the method</param>
        public static async Task Invoke(string methodName, params object[] args)
        {
            await JsRuntime.InvokeVoidAsync(methodName, args);
        }

        /// <summary>
        /// Invoke a method from javascript
        /// </summary>
        /// <typeparam name="T">The type of the spected return value</typeparam>
        /// <param name="methodName">The name of the method in the javascript file</param>
        /// <returns>A task of type of the spected return value</returns>
        public static async Task<T> Invoke<T>(string methodName)
        {
            return await JsRuntime.InvokeAsync<T>(methodName);
        }

        /// <summary>
        /// Invoke a method from javascript with the specified parameters
        /// </summary>
        /// <typeparam name="T">The type of the spected return value</typeparam>
        /// <param name="methodName">The name of the method in the javascript file</param>
        /// <param name="args">The arguments expected for the method</param>
        /// <returns>A task of the type spected return value</returns>
        public static async Task<T> Invoke<T>(string methodName, params object[] args)
        {
            return await JsRuntime.InvokeAsync<T>(methodName, args);
        }
    }
}
