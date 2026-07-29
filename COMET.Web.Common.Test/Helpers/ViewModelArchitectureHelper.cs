// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewModelArchitectureHelper.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Test.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helper class for enforcing ViewModel architecture purity rules across assemblies.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ViewModelArchitectureHelper
    {
        /// <summary>
        /// The list of prohibited UI component library namespace prefixes.
        /// </summary>
        private static readonly string[] ProhibitedUiNamespacePrefixes =
        [
            "DevExpress",
            "AntDesign",
            "BlazorStrap",
            "Feather",
            "Radzen",
            "MudBlazor",
            "Blazor.Diagrams",
            "Z.Blazor.Diagrams"
        ];

        /// <summary>
        /// Inspects all types in ViewModel namespaces in the given assembly and returns any violations.
        /// </summary>
        /// <param name="assembly">The assembly to inspect.</param>
        /// <returns>A list of violation messages.</returns>
        public static List<string> GetViewModelPurityViolations(Assembly assembly)
        {
            var viewModelTypes = assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.Split('.').Contains("ViewModels"))
                .ToList();

            var violations = new List<string>();

            foreach (var type in viewModelTypes)
            {
                InspectType(type, violations);
            }

            return violations;
        }

        /// <summary>
        /// Inspects a type for references to UI component library types across base types, interfaces, fields, properties, constructors, and methods.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="violations">The list of violation messages to accumulate into.</param>
        public static void InspectType(Type type, List<string> violations)
        {
            if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            {
                var offending = GetOffendingUiType(type.BaseType);

                if (offending != null)
                {
                    violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in base type '{type.BaseType.Name}'.");
                }
            }

            foreach (var iface in type.GetInterfaces())
            {
                var offending = GetOffendingUiType(iface);

                if (offending != null)
                {
                    violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in interface '{iface.Name}'.");
                }
            }

            if (type.IsGenericType)
            {
                foreach (var genericArg in type.GetGenericArguments())
                {
                    var offending = GetOffendingUiType(genericArg);

                    if (offending != null)
                    {
                        violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in generic argument '{genericArg.Name}'.");
                    }
                }
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var field in type.GetFields(flags))
            {
                if (field.Name.Contains("k__BackingField"))
                {
                    continue;
                }

                var offending = GetOffendingUiType(field.FieldType);

                if (offending != null)
                {
                    violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in field '{field.Name}'.");
                }
            }

            foreach (var property in type.GetProperties(flags))
            {
                var offending = GetOffendingUiType(property.PropertyType);

                if (offending != null)
                {
                    violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in property '{property.Name}'.");
                }
            }

            foreach (var constructor in type.GetConstructors(flags))
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    var offending = GetOffendingUiType(parameter.ParameterType);

                    if (offending != null)
                    {
                        violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in constructor parameter '{parameter.Name}'.");
                    }
                }
            }

            foreach (var method in type.GetMethods(flags))
            {
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                if (method.ReturnType != typeof(void))
                {
                    var offending = GetOffendingUiType(method.ReturnType);

                    if (offending != null)
                    {
                        violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in method '{method.Name}' return type.");
                    }
                }

                foreach (var parameter in method.GetParameters())
                {
                    var offending = GetOffendingUiType(parameter.ParameterType);

                    if (offending != null)
                    {
                        violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in method '{method.Name}' parameter '{parameter.Name}'.");
                    }
                }

                if (!method.IsGenericMethod)
                {
                    continue;
                }

                foreach (var genericArg in method.GetGenericArguments())
                {
                    var offending = GetOffendingUiType(genericArg);

                    if (offending != null)
                    {
                        violations.Add($"ViewModel '{type.FullName}' references UI library type '{offending.FullName}' in method '{method.Name}' generic argument '{genericArg.Name}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Recursively unwraps a type and returns the first prohibited UI component library type referenced, or null if clean.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The offending UI type, or null.</returns>
        private static Type GetOffendingUiType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            foreach (var unwrapped in UnwrapTypes(type))
            {
                if (IsProhibitedUiComponentLibraryType(unwrapped))
                {
                    return unwrapped;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively unwraps element types, generic arguments, and underlying nullable types.
        /// </summary>
        /// <param name="type">The root type.</param>
        /// <returns>An enumerable of all constituent types.</returns>
        private static IEnumerable<Type> UnwrapTypes(Type type)
        {
            if (type == null)
            {
                yield break;
            }

            yield return type;

            var underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType != null)
            {
                foreach (var inner in UnwrapTypes(underlyingType))
                {
                    yield return inner;
                }
            }

            if (type.HasElementType)
            {
                foreach (var inner in UnwrapTypes(type.GetElementType()))
                {
                    yield return inner;
                }
            }

            if (!type.IsGenericType)
            {
                yield break;
            }

            foreach (var genericArgument in type.GetGenericArguments())
            {
                foreach (var inner in UnwrapTypes(genericArgument))
                {
                    yield return inner;
                }
            }
        }

        /// <summary>
        /// Checks whether the specified type belongs to a prohibited UI component library namespace.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is in a prohibited UI namespace; otherwise, false.</returns>
        private static bool IsProhibitedUiComponentLibraryType(Type type)
        {
            var typeNamespace = type?.Namespace;

            if (string.IsNullOrEmpty(typeNamespace))
            {
                return false;
            }

            foreach (var prefix in ProhibitedUiNamespacePrefixes)
            {
                if (typeNamespace == prefix || typeNamespace.StartsWith(prefix + ".", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
