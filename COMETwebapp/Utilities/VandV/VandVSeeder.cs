// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVSeeder.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Utilities.VandV
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    /// <summary>
    /// Builds the reference data the <see cref="VandVManifest" /> declares and a <see cref="VandVReferenceDataCheck" /> reports
    /// missing, exactly as the COMET IME seeds it, as plain functions so a single transaction can write it and tests need no
    /// session. Anything already defined anywhere in the chain is referenced, never created again.
    /// </summary>
    public static class VandVSeeder
    {
        /// <summary>
        /// The SDK validation rules, the ones the IME applies when a value definition is edited there.
        /// </summary>
        private static readonly IValidationService Validation = new ValidationService();

        /// <summary>
        /// Gets the chain of reference data libraries starting at the given library: the library, the library it requires,
        /// and so on.
        /// </summary>
        /// <param name="library">The first <see cref="ReferenceDataLibrary" />, typically the model's library.</param>
        /// <returns>The chain, empty for a null library.</returns>
        public static IReadOnlyList<ReferenceDataLibrary> GetChain(ReferenceDataLibrary library)
        {
            var chain = new List<ReferenceDataLibrary>();

            for (var current = library; current != null && !chain.Contains(current); current = current.RequiredRdl)
            {
                chain.Add(current);
            }

            return chain;
        }

        /// <summary>
        /// Normalises the stage gates the user entered: trimmed, blanks dropped, order kept.
        /// </summary>
        /// <param name="stageGates">The stage gates as entered.</param>
        /// <returns>The stage gates to seed.</returns>
        public static IReadOnlyList<string> ParseStageGates(IEnumerable<string> stageGates)
        {
            return (stageGates ?? []).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        /// <summary>
        /// Validates the stage gates the user entered.
        /// </summary>
        /// <param name="stageGates">The stage gates as entered.</param>
        /// <returns>The error to show, or null when they can be seeded.</returns>
        public static string GetStageGateError(IEnumerable<string> stageGates)
        {
            var parsed = ParseStageGates(stageGates);

            if (parsed.Count == 0)
            {
                return "Define at least one stage gate.";
            }

            var unnamed = parsed.FirstOrDefault(x => !x.Any(char.IsLetterOrDigit));

            if (unnamed != null)
            {
                return $"The stage gate '{unnamed}' needs at least one letter or digit.";
            }

            var duplicate = parsed.GroupBy(VandVConstants.ToShortName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);

            if (duplicate != null)
            {
                return $"The stage gate '{duplicate.First()}' is listed more than once.";
            }

            // a value definition the SDK rules reject could not be edited in the IME afterwards
            return parsed
                .Select(x => (Gate: x, Error: Validation.ValidateProperty(nameof(DefinedThing.Name), x) ?? Validation.ValidateProperty(nameof(DefinedThing.ShortName), VandVConstants.ToShortName(x))))
                .Where(x => x.Error != null)
                .Select(x => $"The stage gate '{x.Gate}' is not a valid name: {x.Error}")
                .FirstOrDefault();
        }

        /// <summary>
        /// Describes what will be created, one line per item, for the set-up dialog.
        /// </summary>
        /// <param name="check">The <see cref="VandVReferenceDataCheck" />.</param>
        /// <returns>The lines.</returns>
        public static IReadOnlyList<string> DescribeMissing(VandVReferenceDataCheck check)
        {
            var lines = new List<string>();

            lines.AddRange(check.MissingParameterTypes.Select(x => $"Parameter type {x.ShortName} ({x.Name}): {DescribeKind(x)}"));
            lines.AddRange(check.MissingCategories.Select(x => $"Category {x.ShortName} ({x.Name}) on {string.Join(", ", x.PermissibleClass)}"));
            lines.AddRange(check.MissingParameterizedCategoryRules.Select(x => $"Rule {x.ShortName} ({x.Name})"));

            if (check.RequirementCategory != null)
            {
                lines.AddRange(check.MissingBinaryRelationshipRules.Select(x => $"Rule {x.ShortName} ({x.SourceCategoryShortName} {x.ForwardRelationshipName} {check.RequirementCategory.ShortName})"));
            }

            return lines;
        }

        /// <summary>
        /// Gets the concrete class kinds the current user must be allowed to create in the target library to seed what is
        /// missing. Permissions are per concrete class, so an abstract kind such as ParameterType has no entry.
        /// </summary>
        /// <param name="check">The <see cref="VandVReferenceDataCheck" />.</param>
        /// <returns>The distinct class kinds.</returns>
        public static IReadOnlyList<ClassKind> GetRequiredClassKinds(VandVReferenceDataCheck check)
        {
            var kinds = new List<ClassKind>();
            kinds.AddRange(check.MissingParameterTypes.Select(x => GetClassKind(x.Kind)));

            if (check.MissingCategories.Count > 0)
            {
                kinds.Add(ClassKind.Category);
            }

            if (check.MissingParameterizedCategoryRules.Count > 0)
            {
                kinds.Add(ClassKind.ParameterizedCategoryRule);
            }

            if (check.RequirementCategory != null && check.MissingBinaryRelationshipRules.Count > 0)
            {
                kinds.Add(ClassKind.BinaryRelationshipRule);
            }

            return kinds.Distinct().ToList();
        }

        /// <summary>
        /// Builds every missing thing and adds it to the given library clone, so one transaction with the clone as top
        /// container writes them all: parameter types with their enumeration values, categories with their super categories,
        /// the parameterized category rules and, when a requirement category was resolved, the binary relationship rules.
        /// </summary>
        /// <param name="libraryClone">The clone of the target library the things are added to.</param>
        /// <param name="check">The <see cref="VandVReferenceDataCheck" /> computed right before.</param>
        /// <param name="stageGates">The stage gates, in project order, that become the values of the stage gate type.</param>
        /// <param name="chain">The chain of libraries, used to reference what already exists.</param>
        /// <returns>The things to create, excluding the clone itself.</returns>
        public static List<Thing> BuildThingsToCreate(ReferenceDataLibrary libraryClone, VandVReferenceDataCheck check, IReadOnlyList<string> stageGates, IEnumerable<ReferenceDataLibrary> chain)
        {
            var libraries = chain.ToList();
            var parameterTypes = ByShortName(libraries.SelectMany(x => x.ParameterType));
            var categories = ByShortName(libraries.SelectMany(x => x.DefinedCategory));
            var created = new List<Thing>();

            foreach (var definition in check.MissingParameterTypes)
            {
                var parameterType = CreateParameterType(definition, definition.ShortName == VandVConstants.Stage ? stageGates : definition.Values);
                libraryClone.ParameterType.Add(parameterType);
                parameterTypes[definition.ShortName] = parameterType;
                created.Add(parameterType);

                if (parameterType is EnumerationParameterType enumeration)
                {
                    created.AddRange(enumeration.ValueDefinition);
                }
            }

            foreach (var definition in check.MissingCategories)
            {
                var category = new Category { Iid = Guid.NewGuid(), ShortName = definition.ShortName, Name = definition.Name };
                category.PermissibleClass.AddRange(definition.PermissibleClass);
                libraryClone.DefinedCategory.Add(category);
                categories[definition.ShortName] = category;
                created.Add(category);
            }

            // super categories are wired once every category exists, whether found in the chain or created above
            foreach (var definition in check.MissingCategories.Where(x => x.SuperCategoryShortName != null))
            {
                if (categories.TryGetValue(definition.SuperCategoryShortName, out var superCategory))
                {
                    categories[definition.ShortName].SuperCategory.Add(superCategory);
                }
            }

            foreach (var definition in check.MissingParameterizedCategoryRules)
            {
                if (!categories.TryGetValue(definition.CategoryShortName, out var category))
                {
                    continue;
                }

                var rule = new ParameterizedCategoryRule { Iid = Guid.NewGuid(), ShortName = definition.ShortName, Name = definition.Name, Category = category };
                rule.ParameterType.AddRange(definition.ParameterTypeShortNames.Select(parameterTypes.GetValueOrDefault).Where(x => x != null));
                libraryClone.Rule.Add(rule);
                created.Add(rule);
            }

            if (check.RequirementCategory == null)
            {
                return created;
            }

            foreach (var definition in check.MissingBinaryRelationshipRules)
            {
                if (!categories.TryGetValue(definition.RelationshipCategoryShortName, out var relationshipCategory)
                    || !categories.TryGetValue(definition.SourceCategoryShortName, out var sourceCategory))
                {
                    continue;
                }

                var rule = new BinaryRelationshipRule
                {
                    Iid = Guid.NewGuid(),
                    ShortName = definition.ShortName,
                    Name = definition.Name,
                    RelationshipCategory = relationshipCategory,
                    SourceCategory = sourceCategory,
                    TargetCategory = check.RequirementCategory,
                    ForwardRelationshipName = definition.ForwardRelationshipName,
                    InverseRelationshipName = definition.InverseRelationshipName
                };

                libraryClone.Rule.Add(rule);
                created.Add(rule);
            }

            return created;
        }

        /// <summary>
        /// Indexes the given things by short name, the first one winning when a short name occurs more than once in the chain.
        /// </summary>
        /// <typeparam name="T">The parameter type or category type.</typeparam>
        /// <param name="things">The parameter types or categories.</param>
        /// <returns>The index.</returns>
        private static Dictionary<string, T> ByShortName<T>(IEnumerable<T> things) where T : DefinedThing
        {
            return things.GroupBy(x => x.ShortName, StringComparer.Ordinal).ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        }

        /// <summary>
        /// Describes the kind of a parameter type for the set-up dialog.
        /// </summary>
        /// <param name="definition">The <see cref="VandVParameterTypeDefinition" />.</param>
        /// <returns>The description.</returns>
        private static string DescribeKind(VandVParameterTypeDefinition definition)
        {
            return definition switch
            {
                { ShortName: VandVConstants.Stage } => "enumeration of the stage gates defined below",
                { Kind: VandVParameterKind.Enumeration } => $"enumeration of {string.Join(", ", definition.Values)}",
                _ => definition.Kind.ToString().ToLowerInvariant()
            };
        }

        /// <summary>
        /// Gets the concrete class kind of a parameter type kind.
        /// </summary>
        /// <param name="kind">The <see cref="VandVParameterKind" />.</param>
        /// <returns>The <see cref="ClassKind" />.</returns>
        /// <exception cref="NotSupportedException">The kind has no parameter type.</exception>
        private static ClassKind GetClassKind(VandVParameterKind kind)
        {
            return kind switch
            {
                VandVParameterKind.Text => ClassKind.TextParameterType,
                VandVParameterKind.Date => ClassKind.DateParameterType,
                VandVParameterKind.Boolean => ClassKind.BooleanParameterType,
                VandVParameterKind.Enumeration => ClassKind.EnumerationParameterType,
                _ => throw new NotSupportedException($"No parameter type for {kind}")
            };
        }

        /// <summary>
        /// Creates the parameter type declared by the given definition; its symbol is its short name, as the IME seeds it.
        /// </summary>
        /// <param name="definition">The <see cref="VandVParameterTypeDefinition" />.</param>
        /// <param name="values">The enumeration value names, ignored for any other kind.</param>
        /// <returns>The new <see cref="ParameterType" />.</returns>
        /// <exception cref="NotSupportedException">The kind has no parameter type.</exception>
        private static ParameterType CreateParameterType(VandVParameterTypeDefinition definition, IReadOnlyList<string> values)
        {
            ParameterType parameterType = definition.Kind switch
            {
                VandVParameterKind.Text => new TextParameterType(),
                VandVParameterKind.Date => new DateParameterType(),
                VandVParameterKind.Boolean => new BooleanParameterType(),
                VandVParameterKind.Enumeration => CreateEnumeration(values),
                _ => throw new NotSupportedException($"No parameter type for {definition.Kind}")
            };

            parameterType.Iid = Guid.NewGuid();
            parameterType.ShortName = definition.ShortName;
            parameterType.Name = definition.Name;
            parameterType.Symbol = definition.ShortName;
            return parameterType;
        }

        /// <summary>
        /// Creates an enumeration parameter type with one value definition per value, named and short-named as the IME does.
        /// </summary>
        /// <param name="values">The value names, in order.</param>
        /// <returns>The new <see cref="EnumerationParameterType" />.</returns>
        private static EnumerationParameterType CreateEnumeration(IReadOnlyList<string> values)
        {
            var enumeration = new EnumerationParameterType { AllowMultiSelect = false };

            enumeration.ValueDefinition.AddRange(values.Select(x => new EnumerationValueDefinition
            {
                Iid = Guid.NewGuid(), Name = x, ShortName = VandVConstants.ToShortName(x)
            }));

            return enumeration;
        }
    }
}
