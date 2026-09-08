// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogCalculatorTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsChangelogCalculatorTestFixture
    {
        private DomainOfExpertise systemDomain;
        private DomainOfExpertise thermalDomain;
        private Category keyUserCategory;
        private SimpleQuantityKind massParameterType;
        private RequirementsGroup baseGroup;
        private RequirementsGroup currentGroup;
        private Guid unchangedSpecificationIid;
        private Guid unchangedRequirementIid;
        private Guid deletedRequirementIid;
        private Guid createdRequirementIid;
        private Guid deprecatedRequirementIid;
        private Person author;
        private Iteration baseIteration;
        private Iteration currentIteration;

        [SetUp]
        public void SetUp()
        {
            this.systemDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System Engineering" };
            this.thermalDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "THE", Name = "Thermal" };
            this.keyUserCategory = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };
            this.massParameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            this.author = new Person { Iid = Guid.NewGuid(), ShortName = "jdoe", GivenName = "John", Surname = "Doe" };

            this.unchangedSpecificationIid = Guid.NewGuid();
            this.unchangedRequirementIid = Guid.NewGuid();
            this.deletedRequirementIid = Guid.NewGuid();
            this.createdRequirementIid = Guid.NewGuid();
            this.deprecatedRequirementIid = Guid.NewGuid();

            this.baseGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "OPERATE", Name = "Operate", Owner = this.systemDomain };
            this.currentGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "C4I", Name = "C4I", Owner = this.systemDomain };

            this.baseIteration = new Iteration { Iid = Guid.NewGuid() };
            this.currentIteration = new Iteration { Iid = Guid.NewGuid() };

            var baseRequirement = new Requirement
            {
                Iid = this.unchangedRequirementIid, ShortName = "R01", Name = "Old name", Owner = this.thermalDomain,
                Group = this.baseGroup,
                Definition = { new Definition { LanguageCode = "en", Content = "Old definition." } }
            };

            var deletedRequirement = new Requirement { Iid = this.deletedRequirementIid, ShortName = "R02", Name = "Retired requirement", Owner = this.systemDomain };

            var baseSpecification = new RequirementsSpecification { Iid = this.unchangedSpecificationIid, ShortName = "KUR", Name = "Key-User Requirements", Owner = this.systemDomain };
            baseSpecification.Requirement.AddRange([baseRequirement, deletedRequirement]);
            this.baseIteration.RequirementsSpecification.Add(baseSpecification);

            var currentRequirement = new Requirement
            {
                Iid = this.unchangedRequirementIid, ShortName = "R01", Name = "New name", Owner = this.systemDomain,
                Group = this.currentGroup, Category = { this.keyUserCategory }, Actor = this.author,
                Definition = { new Definition { LanguageCode = "en", Content = "New definition." } },
                ParameterValue = { new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, Value = new ValueArray<string>(["5"]) } },
                ParametricConstraint = { new ParametricConstraint { Iid = Guid.NewGuid() } }
            };

            var createdRequirement = new Requirement { Iid = this.createdRequirementIid, ShortName = "R03", Name = "Newly added requirement", Owner = this.systemDomain };

            var deprecatedRequirement = new Requirement { Iid = this.deprecatedRequirementIid, ShortName = "R04", Name = "About to be deprecated", Owner = this.systemDomain, IsDeprecated = false };
            var baseDeprecationHolder = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "DEP", Name = "Deprecation holder", Owner = this.systemDomain };
            baseDeprecationHolder.Requirement.Add(deprecatedRequirement);
            this.baseIteration.RequirementsSpecification.Add(baseDeprecationHolder);

            var currentSpecification = new RequirementsSpecification { Iid = this.unchangedSpecificationIid, ShortName = "KUR", Name = "Key-User Requirements", Owner = this.systemDomain };
            currentSpecification.Requirement.AddRange([currentRequirement, createdRequirement]);
            this.currentIteration.RequirementsSpecification.Add(currentSpecification);

            var currentDeprecationHolder = new RequirementsSpecification { Iid = baseDeprecationHolder.Iid, ShortName = "DEP", Name = "Deprecation holder", Owner = this.systemDomain };
            currentDeprecationHolder.Requirement.Add(new Requirement { Iid = this.deprecatedRequirementIid, ShortName = "R04", Name = "About to be deprecated", Owner = this.systemDomain, IsDeprecated = true });
            this.currentIteration.RequirementsSpecification.Add(currentDeprecationHolder);

            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            this.currentIteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = currentRequirement, Target = relationalExpression });
        }

        [Test]
        public void VerifyCompare()
        {
            var noChanges = RequirementsChangelogCalculator.Compare(this.baseIteration, this.baseIteration);

            var emptyBase = new Iteration { Iid = Guid.NewGuid() };
            var emptyCurrent = new Iteration { Iid = Guid.NewGuid() };
            var noRequirementChanges = RequirementsChangelogCalculator.Compare(emptyBase, emptyCurrent);

            var changes = RequirementsChangelogCalculator.Compare(this.baseIteration, this.currentIteration);

            var nameChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Modified && c.ElementShortName == "R01" && c.Field == "Name");
            var ownerChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Modified && c.ElementShortName == "R01" && c.Field == "Owner");
            var definitionChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Modified && c.ElementShortName == "R01" && c.Field == "Definition");
            var categoryChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Modified && c.ElementShortName == "R01" && c.Field == "Category");
            var groupChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Modified && c.ElementShortName == "R01" && c.Field == "Group");
            var valueChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Created && c.ElementShortName == "R01" && c.Field == $"Value: {this.massParameterType.ShortName}");
            var constraintChange = changes.SingleOrDefault(c => c.Kind == RequirementChangeKind.Created && c.ElementShortName == "R01" && c.Field == "Constraint");

            Assert.Multiple(() =>
            {
                Assert.That(noChanges, Is.Empty, "Comparing an iteration against itself must yield no changes.");
                Assert.That(noRequirementChanges, Is.Empty, "Two iterations without requirements yield no changes.");
                Assert.That(changes, Is.Not.Empty);

                Assert.That(changes.Any(c => c.Kind == RequirementChangeKind.Created && c.ElementShortName == "R03"), Is.True,
                    "A requirement only present in the current iteration must be reported as Created.");

                Assert.That(changes.Any(c => c.Kind == RequirementChangeKind.Deleted && c.ElementShortName == "R02"), Is.True,
                    "A requirement only present in the base iteration must be reported as Deleted.");

                Assert.That(changes.Any(c => c.Kind == RequirementChangeKind.Deprecated && c.ElementShortName == "R04"), Is.True,
                    "A requirement whose IsDeprecated flipped false to true must be reported as Deprecated.");

                Assert.That(nameChange, Is.Not.Null, "A changed Name must produce one Modified row with Field 'Name'.");
                Assert.That(nameChange?.OldValue, Is.EqualTo("Old name"));
                Assert.That(nameChange?.NewValue, Is.EqualTo("New name"));
                Assert.That(nameChange?.Author, Is.EqualTo(this.author.Name), "Author must be the changed requirement's Actor name.");

                Assert.That(ownerChange, Is.Not.Null, "A changed Owner must produce one Modified row with Field 'Owner'.");
                Assert.That(ownerChange?.OldValue, Is.EqualTo(this.thermalDomain.ShortName));
                Assert.That(ownerChange?.NewValue, Is.EqualTo(this.systemDomain.ShortName));

                Assert.That(definitionChange, Is.Not.Null, "A changed Definition content must produce one Modified row with Field 'Definition'.");
                Assert.That(definitionChange?.OldValue, Is.EqualTo("Old definition."));
                Assert.That(definitionChange?.NewValue, Is.EqualTo("New definition."));

                Assert.That(categoryChange, Is.Not.Null, "A changed Category set must produce one Modified row with Field 'Category'.");
                Assert.That(categoryChange?.NewValue, Is.EqualTo(this.keyUserCategory.ShortName));

                Assert.That(groupChange, Is.Not.Null, "A changed parent Group must produce one Modified row with Field 'Group'.");
                Assert.That(groupChange?.OldValue, Is.EqualTo(this.baseGroup.ShortName));
                Assert.That(groupChange?.NewValue, Is.EqualTo(this.currentGroup.ShortName));

                Assert.That(valueChange, Is.Not.Null, "A newly-added SimpleParameterValue must produce a Created row named after its parameter type.");
                Assert.That(valueChange?.NewValue, Is.EqualTo("5"));

                Assert.That(constraintChange, Is.Not.Null, "A newly-added ParametricConstraint must produce one Created row with Field 'Constraint'.");

                Assert.That(changes.Any(c => c.ElementKind == "Binary Relationship" && c.Kind == RequirementChangeKind.Created), Is.True,
                    "A BinaryRelationship added between two current iterations that involves a requirement must be reported as a Created Binary Relationship row.");

                Assert.That(nameChange?.SpecificationShortName, Is.EqualTo("KUR"), "A requirement change row must carry its owning specification's short name.");
                Assert.That(nameChange?.SpecificationId, Is.Not.EqualTo(Guid.Empty), "A requirement change row must carry its owning specification's id.");
            });
        }
    }
}
