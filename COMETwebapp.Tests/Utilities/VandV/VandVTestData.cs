// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVTestData.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Utilities.VandV
{
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Utilities.VandV;

    /// <summary>
    /// Builds V&amp;V test models from <see cref="VandVManifest" /> itself, so every fixture uses exactly the short names and
    /// enumeration values production code reads, and writes things the way the COMET IME writes them. Categories share one
    /// cache, because a plain <c>new Category { ... }</c> throws from <c>AllSuperCategories()</c>.
    /// </summary>
    internal sealed class VandVTestData
    {
        /// <summary>
        /// The short name of the model's requirement category created by every instance.
        /// </summary>
        internal const string RequirementCategoryShortName = "REQ";

        /// <summary>
        /// The cache shared by every category.
        /// </summary>
        private readonly ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVTestData" /> class: a model library holding every manifest
        /// parameter type and category plus a REQ requirement category, and an engineering model with one iteration.
        /// </summary>
        public VandVTestData()
        {
            foreach (var definition in VandVManifest.ParameterTypes)
            {
                var parameterType = CreateParameterType(definition);
                this.ParameterTypes[definition.ShortName] = parameterType;
                this.Library.ParameterType.Add(parameterType);
            }

            foreach (var definition in VandVManifest.Categories)
            {
                this.Library.DefinedCategory.Add(this.CreateCategory(definition.ShortName, definition.Name, [..definition.PermissibleClass]));
            }

            foreach (var definition in VandVManifest.Categories.Where(x => x.SuperCategoryShortName != null))
            {
                this.Categories[definition.ShortName].SuperCategory.Add(this.Categories[definition.SuperCategoryShortName]);
            }

            this.Library.DefinedCategory.Add(this.CreateCategory(RequirementCategoryShortName, "Requirement", [ClassKind.Requirement]));
            this.EngineeringModel.Iteration.Add(this.Iteration);
        }

        /// <summary>
        /// Gets the model reference data library holding the V&amp;V reference data.
        /// </summary>
        public ModelReferenceDataLibrary Library { get; } = new() { Iid = Guid.NewGuid(), ShortName = "MRDL", Name = "Model library" };

        /// <summary>
        /// Gets the engineering model containing <see cref="Iteration" /> and the annotations.
        /// </summary>
        public EngineeringModel EngineeringModel { get; } = new() { Iid = Guid.NewGuid() };

        /// <summary>
        /// Gets the iteration.
        /// </summary>
        public Iteration Iteration { get; } = new() { Iid = Guid.NewGuid() };

        /// <summary>
        /// Gets the owner of every created thing.
        /// </summary>
        public DomainOfExpertise Owner { get; } = new() { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };

        /// <summary>
        /// Gets the parameter types, keyed by short name.
        /// </summary>
        public Dictionary<string, ParameterType> ParameterTypes { get; } = [];

        /// <summary>
        /// Gets the categories, keyed by short name.
        /// </summary>
        public Dictionary<string, Category> Categories { get; } = [];

        /// <summary>
        /// Creates a category registered in the shared cache and in <see cref="Categories" />.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="name">The name.</param>
        /// <param name="permissibleClasses">The permissible classes.</param>
        /// <returns>The new <see cref="Category" />.</returns>
        public Category CreateCategory(string shortName, string name, List<ClassKind> permissibleClasses)
        {
            var iid = Guid.NewGuid();
            var category = new Category(iid, this.cache, new Uri("https://test.example")) { ShortName = shortName, Name = name };
            category.PermissibleClass.AddRange(permissibleClasses);
            this.cache.TryAdd(new CacheKey(iid, null), new Lazy<Thing>(() => category));
            this.Categories[shortName] = category;
            return category;
        }

        /// <summary>
        /// Adds a specification to the iteration.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="categoryShortNames">The short names of the categories to apply.</param>
        /// <returns>The new <see cref="RequirementsSpecification" />.</returns>
        public RequirementsSpecification AddSpecification(string shortName, params string[] categoryShortNames)
        {
            var specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = shortName, Name = shortName, Owner = this.Owner };
            specification.Category.AddRange(categoryShortNames.Select(x => this.Categories[x]));
            this.Iteration.RequirementsSpecification.Add(specification);
            return specification;
        }

        /// <summary>
        /// Adds a requirement to the given specification.
        /// </summary>
        /// <param name="specification">The containing <see cref="RequirementsSpecification" />.</param>
        /// <param name="shortName">The short name.</param>
        /// <param name="categoryShortNames">The short names of the categories to apply.</param>
        /// <returns>The new <see cref="Requirement" />.</returns>
        public Requirement AddRequirement(RequirementsSpecification specification, string shortName, params string[] categoryShortNames)
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = shortName, Name = shortName, Owner = this.Owner };
            requirement.Category.AddRange(categoryShortNames.Select(x => this.Categories[x]));
            specification.Requirement.Add(requirement);
            return requirement;
        }

        /// <summary>
        /// Adds a binary relationship to the iteration.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="categoryShortName">The short name of the relationship category.</param>
        /// <returns>The new <see cref="BinaryRelationship" />.</returns>
        public BinaryRelationship Link(Thing source, Thing target, string categoryShortName)
        {
            var relationship = new BinaryRelationship { Iid = Guid.NewGuid(), Source = source, Target = target, Owner = this.Owner };
            relationship.Category.Add(this.Categories[categoryShortName]);
            this.Iteration.Relationship.Add(relationship);
            return relationship;
        }

        /// <summary>
        /// Sets an attribute of a requirement, replacing any existing value.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <param name="value">The raw value to store.</param>
        public void SetValue(Requirement requirement, string parameterTypeShortName, string value)
        {
            var parameterType = this.ParameterTypes[parameterTypeShortName];
            requirement.ParameterValue.RemoveAll(x => x.ParameterType == parameterType);
            requirement.ParameterValue.Add(new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = parameterType, Value = new ValueArray<string>([value]) });
        }

        /// <summary>
        /// Raises an annotation against the given thing, with the same reference as primary and related thing, as the COMET
        /// IME writes it.
        /// </summary>
        /// <typeparam name="T">The annotation type.</typeparam>
        /// <param name="thing">The annotated thing.</param>
        /// <param name="status">The <see cref="AnnotationStatusKind" />.</param>
        /// <returns>The new annotation.</returns>
        public T Annotate<T>(Thing thing, AnnotationStatusKind status) where T : ModellingAnnotationItem, new()
        {
            var reference = new ModellingThingReference(thing) { Iid = Guid.NewGuid() };
            var annotation = new T { Iid = Guid.NewGuid(), ShortName = $"A_{this.EngineeringModel.ModellingAnnotation.Count + 1}", Status = status, PrimaryAnnotatedThing = reference };
            annotation.RelatedThing.Add(reference);
            this.EngineeringModel.ModellingAnnotation.Add(annotation);
            return annotation;
        }

        /// <summary>
        /// Builds the <see cref="VandVModel" /> of the iteration.
        /// </summary>
        /// <returns>The snapshot.</returns>
        public VandVModel BuildModel()
        {
            return VandVModel.Build(this.Iteration, [this.Library]);
        }

        /// <summary>
        /// Creates the parameter type declared by the given definition, with value definitions named and short-named as the
        /// COMET IME seeds them.
        /// </summary>
        /// <param name="definition">The <see cref="VandVParameterTypeDefinition" />.</param>
        /// <returns>The new <see cref="ParameterType" />.</returns>
        private static ParameterType CreateParameterType(VandVParameterTypeDefinition definition)
        {
            return definition.Kind switch
            {
                VandVParameterKind.Text => new TextParameterType { Iid = Guid.NewGuid(), ShortName = definition.ShortName, Name = definition.Name },
                VandVParameterKind.Date => new DateParameterType { Iid = Guid.NewGuid(), ShortName = definition.ShortName, Name = definition.Name },
                VandVParameterKind.Boolean => new BooleanParameterType { Iid = Guid.NewGuid(), ShortName = definition.ShortName, Name = definition.Name },
                VandVParameterKind.Enumeration => CreateEnumerationParameterType(definition),
                _ => throw new NotSupportedException($"No test parameter type for {definition.Kind}")
            };
        }

        /// <summary>
        /// Creates the enumeration parameter type declared by the given definition.
        /// </summary>
        /// <param name="definition">The <see cref="VandVParameterTypeDefinition" />.</param>
        /// <returns>The new <see cref="EnumerationParameterType" />.</returns>
        private static EnumerationParameterType CreateEnumerationParameterType(VandVParameterTypeDefinition definition)
        {
            var enumeration = new EnumerationParameterType { Iid = Guid.NewGuid(), ShortName = definition.ShortName, Name = definition.Name };

            enumeration.ValueDefinition.AddRange(definition.Values.Select(value => new EnumerationValueDefinition
            {
                Iid = Guid.NewGuid(), Name = value, ShortName = VandVConstants.ToShortName(value)
            }));

            return enumeration;
        }
    }
}
