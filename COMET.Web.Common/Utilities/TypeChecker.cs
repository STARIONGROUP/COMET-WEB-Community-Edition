// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TypeChecker.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//     Annex A and Annex C.
// 
//     Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
// 
//         http://www.apache.org/licenses/LICENSE-2.0
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
// 
//   </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Utilities
{
    /// <summary>
    /// The <see cref="TypeChecker" /> provides <see cref="Type" /> check utilities
    /// </summary>
    public static class TypeChecker
    {
        /// <summary>
        /// Asserts that the <paramref name="toCheck" /> <see cref="Type" /> is a subclass of a generic <see cref="Type" />
        /// </summary>
        /// <param name="toCheck">The <see cref="Type" /> to checks</param>
        /// <param name="generic">The generic <see cref="Type" /></param>
        /// <param name="genericTypeArgument">The generic argument of the <paramref name="generic" /> <see cref="Type" /></param>
        /// <returns>True if the <paramref name="toCheck" /> inherits from the <paramref name="generic" /> one</returns>
        public static bool IsSubclassOfRawGeneric(Type toCheck, Type generic, out Type genericTypeArgument)
        {
            genericTypeArgument = null;

            if (toCheck == null)
            {
                return false;
            }

            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (generic == cur)
                {
                    genericTypeArgument = toCheck.GetGenericArguments()[0];
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Asserts that the <paramref name="toCheck" /> <see cref="Type" /> implements the generic interface
        /// <paramref name="genericInterfaceType" />
        /// </summary>
        /// <param name="toCheck">The <see cref="Type" /> to checks</param>
        /// <param name="genericInterfaceType">The generic <see cref="Type" /> interface</param>
        /// <param name="genericTypeArgument">
        /// The generic argument of the <paramref name="genericInterfaceType" />
        /// <see cref="Type" />
        /// </param>
        /// <returns>True if the <paramref name="toCheck" /> implements from the <paramref name="genericInterfaceType" /> one</returns>
        public static bool ImplementsGenericInterface(Type toCheck, Type genericInterfaceType, out Type genericTypeArgument)
        {
            genericTypeArgument = null;

            if (toCheck == null)
            {
                return false;
            }

            var interfaces = toCheck.GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericInterfaceType)
                {
                    genericTypeArgument = iface.GetGenericArguments()[0];
                    return true;
                }
            }

            return false;
        }
    }
}
