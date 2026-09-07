// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExcelExporterTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Services.RequirementsEditor
{
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using ClosedXML.Excel;

    using COMETwebapp.Model.RequirementsEditor.Export;
    using COMETwebapp.Services.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsExcelExporterTestFixture
    {
        private DomainOfExpertise owner;
        private Category category;
        private SimpleQuantityKind parameterType;
        private RequirementsGroup group;
        private Requirement ungroupedRequirement;
        private Requirement groupedRequirement;
        private ParametricConstraint constraint;
        private RequirementsSpecification specification;

        [SetUp]
        public void SetUp()
        {
            this.owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System Engineering" };
            this.category = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };
            this.parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            this.group = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "GRP", Name = "Group one" };

            this.constraint = new ParametricConstraint { Iid = Guid.NewGuid() };

            this.ungroupedRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R02", Name = "Top requirement", Owner = this.owner,
                Category = { this.category },
                Definition = { new Definition { LanguageCode = "en", Content = "the definition" } },
                ParameterValue = { new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.parameterType, Value = new ValueArray<string>(["42"]) } },
                ParametricConstraint = { this.constraint }
            };

            this.groupedRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R01", Name = "Grouped requirement", Owner = this.owner,
                Group = this.group
            };

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "SPEC", Name = "Specification" };
            this.specification.Group.Add(this.group);
            this.specification.Requirement.AddRange([this.ungroupedRequirement, this.groupedRequirement]);
        }

        private RequirementsExportPayload BuildPayload(RequirementsExportConfiguration configuration)
        {
            return new RequirementsExportPayload(
                [this.specification],
                configuration,
                _ => [],
                _ => string.Empty);
        }

        [Test]
        public void VerifyExportProducesHeadersAndGroupedBody()
        {
            var exporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration()));

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(exporter.FileName, Is.EqualTo("Requirements.xlsx"));
                Assert.That(worksheet.Name, Is.EqualTo("Specification"));
                Assert.That(worksheet.Tables.Count(), Is.EqualTo(1), "the sheet must be an excel table");
                Assert.That(headers, Is.EqualTo(new[] { "Short Name", "Name", "Definition (en)", "Owner", "Categories", "Group", "mass (m)", "Parametric Constraints" }));
                Assert.That(worksheet.CellsUsed().Any(cell => cell.GetString() == "R01"), Is.True, "the grouped requirement must render");
                Assert.That(worksheet.CellsUsed().Any(cell => cell.GetString() == "R02"), Is.True, "the ungrouped requirement must render");
                Assert.That(worksheet.CellsUsed().Any(cell => cell.GetString() == "42"), Is.True, "the simple parameter value must render");

                var groupColumn = headers.IndexOf("Group") + 1;
                var groupedRow = worksheet.RowsUsed().Single(row => row.Cell(1).GetString() == "R01").RowNumber();
                var groupCell = worksheet.Cell(groupedRow, groupColumn).GetString();
                Assert.That(groupCell, Is.EqualTo("Group one (GRP)"));
                Assert.That(groupCell, Does.Not.Contain("/"), "the group cell is the immediate group label, not a breadcrumb");
            });
        }

        [Test]
        public void VerifyNamingModeTogglesIdentityColumns()
        {
            var shortNameOnly = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { NamingMode = RequirementsExportNamingMode.ShortName }));
            var nameOnly = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { NamingMode = RequirementsExportNamingMode.Name }));

            using var shortNameStream = shortNameOnly.Export();
            using var shortNameWorkbook = new XLWorkbook(shortNameStream);
            var shortNameHeaders = shortNameWorkbook.Worksheet(1).Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();

            using var nameStream = nameOnly.Export();
            using var nameWorkbook = new XLWorkbook(nameStream);
            var nameHeaders = nameWorkbook.Worksheet(1).Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(shortNameHeaders, Does.Contain("Short Name").And.Not.Contain("Name"));
                Assert.That(nameHeaders, Does.Contain("Name").And.Not.Contain("Short Name"));
            });
        }

        [Test]
        public void VerifySectionTogglesRemoveColumns()
        {
            var configuration = new RequirementsExportConfiguration
            {
                IncludeOwner = false,
                IncludeCategories = false,
                IncludeDefinitions = false,
                IncludeParametricConstraints = false,
                GroupRequirements = false,
                Relationships = RequirementsExportSelectionMode.None,
                SimpleParameterValues = RequirementsExportSelectionMode.None
            };

            var exporter = new RequirementsExcelExporter(this.BuildPayload(configuration));

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var headers = workbook.Worksheet(1).Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();

            Assert.That(headers, Is.EqualTo(new[] { "Short Name", "Name" }));
        }

        [Test]
        public void VerifyIncludeDeprecatedToggle()
        {
            var deprecatedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R99", Name = "Retired", Owner = this.owner, IsDeprecated = true };
            this.specification.Requirement.Add(deprecatedRequirement);

            var excluded = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration()));
            var included = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { IncludeDeprecated = true }));

            using var excludedStream = excluded.Export();
            using var excludedWorkbook = new XLWorkbook(excludedStream);
            var excludedWorksheet = excludedWorkbook.Worksheet(1);

            using var includedStream = included.Export();
            using var includedWorkbook = new XLWorkbook(includedStream);
            var includedWorksheet = includedWorkbook.Worksheet(1);
            var includedHeaders = includedWorksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var deprecatedColumn = includedHeaders.IndexOf("Deprecated") + 1;
            var deprecatedRow = includedWorksheet.RowsUsed().Single(row => row.Cell(1).GetString() == "R99").RowNumber();
            var nonDeprecatedRow = includedWorksheet.RowsUsed().Single(row => row.Cell(1).GetString() == "R02").RowNumber();

            Assert.Multiple(() =>
            {
                Assert.That(excludedWorksheet.Row(1).CellsUsed().Select(cell => cell.GetString()), Does.Not.Contain("Deprecated"));
                Assert.That(excludedWorksheet.CellsUsed().Any(cell => cell.GetString() == "R99"), Is.False, "a deprecated requirement is excluded by default");
                Assert.That(includedHeaders, Does.Contain("Deprecated"));
                Assert.That(includedWorksheet.Cell(deprecatedRow, deprecatedColumn).GetString(), Is.EqualTo("Yes"));
                Assert.That(includedWorksheet.Cell(nonDeprecatedRow, deprecatedColumn).GetString(), Is.Empty);
            });
        }

        [Test]
        public void VerifyDefinitionsPerLanguage()
        {
            this.ungroupedRequirement.Definition.Add(new Definition { LanguageCode = "fr", Content = "la definition" });

            var allLanguages = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration()));
            var frenchOnly = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { DefinitionLanguageCode = "fr" }));

            using var allStream = allLanguages.Export();
            using var allWorkbook = new XLWorkbook(allStream);
            var allWorksheet = allWorkbook.Worksheet(1);
            var allHeaders = allWorksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var row = allWorksheet.RowsUsed().Single(r => r.Cell(1).GetString() == "R02").RowNumber();
            var englishColumn = allHeaders.IndexOf("Definition (en)") + 1;
            var frenchColumn = allHeaders.IndexOf("Definition (fr)") + 1;

            using var frenchStream = frenchOnly.Export();
            using var frenchWorkbook = new XLWorkbook(frenchStream);
            var frenchHeaders = frenchWorkbook.Worksheet(1).Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(allHeaders, Does.Contain("Definition (en)").And.Contain("Definition (fr)"));
                Assert.That(allWorksheet.Cell(row, englishColumn).GetString(), Is.EqualTo("the definition"));
                Assert.That(allWorksheet.Cell(row, frenchColumn).GetString(), Is.EqualTo("la definition"));
                Assert.That(frenchHeaders, Does.Contain("Definition (fr)").And.Not.Contain("Definition (en)"));
            });
        }

        [Test]
        public void VerifyRelationshipsDirectional()
        {
            var relatedElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            var rule = new RelationshipRuleReference { Iid = Guid.NewGuid(), Name = "trace", ForwardName = "verifies", InverseName = "is verified by" };

            var detail = new RequirementRelationshipDetail
            {
                RelatedThings = [relatedElement],
                Direction = RelationshipDirection.Outgoing,
                Rule = rule,
                Categories = []
            };

            var payload = new RequirementsExportPayload(
                [this.specification],
                new RequirementsExportConfiguration(),
                requirement => requirement.Iid == this.ungroupedRequirement.Iid ? [detail] : [],
                _ => string.Empty);

            var exporter = new RequirementsExcelExporter(payload);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var forwardColumn = headers.IndexOf("verifies") + 1;
            var inverseColumn = headers.IndexOf("is verified by") + 1;
            var row = worksheet.RowsUsed().Single(x => x.Cell(1).GetString() == "R02").RowNumber();

            Assert.Multiple(() =>
            {
                Assert.That(headers, Does.Contain("verifies").And.Contain("is verified by").And.Not.Contain("Relationships"));
                Assert.That(worksheet.Cell(row, forwardColumn).GetString(), Does.Contain("Satellite"));
                Assert.That(worksheet.Cell(row, inverseColumn).GetString(), Is.Empty);
            });
        }

        [Test]
        public void VerifyRelationshipsGeneric()
        {
            var relatedElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };

            var detail = new RequirementRelationshipDetail
            {
                RelatedThings = [relatedElement],
                Direction = RelationshipDirection.Bidirectional,
                Rule = null,
                Categories = []
            };

            var payload = new RequirementsExportPayload(
                [this.specification],
                new RequirementsExportConfiguration(),
                requirement => requirement.Iid == this.ungroupedRequirement.Iid ? [detail] : [],
                _ => string.Empty);

            var exporter = new RequirementsExcelExporter(payload);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var relationshipsColumn = headers.IndexOf("Relationships") + 1;
            var row = worksheet.RowsUsed().Single(x => x.Cell(1).GetString() == "R02").RowNumber();

            Assert.Multiple(() =>
            {
                Assert.That(headers, Does.Contain("Relationships"));
                Assert.That(worksheet.Cell(row, relationshipsColumn).GetString(), Does.Contain("Satellite"));
            });
        }

        [Test]
        public void VerifyRelationshipsSelectionFilter()
        {
            var relatedElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            var detailCategory = new Category { Iid = Guid.NewGuid(), ShortName = "verifies", Name = "verifies" };
            var otherCategory = new Category { Iid = Guid.NewGuid(), ShortName = "other", Name = "other" };

            var detail = new RequirementRelationshipDetail
            {
                RelatedThings = [relatedElement],
                Direction = RelationshipDirection.Bidirectional,
                Rule = null,
                Categories = [detailCategory]
            };

            var payload = new RequirementsExportPayload(
                [this.specification],
                new RequirementsExportConfiguration { Relationships = RequirementsExportSelectionMode.Selection, SelectedRelationshipCategories = [otherCategory] },
                requirement => requirement.Iid == this.ungroupedRequirement.Iid ? [detail] : [],
                _ => string.Empty);

            var exporter = new RequirementsExcelExporter(payload);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();

            Assert.That(headers, Does.Not.Contain("Relationships"), "a relationship whose categories do not match the selection is filtered out");
        }

        [Test]
        public void VerifyParametricConstraintsCellUsesDelegate()
        {
            var payload = new RequirementsExportPayload(
                [this.specification],
                new RequirementsExportConfiguration(),
                _ => [],
                constraint => constraint.Iid == this.constraint.Iid ? "the constraint text" : string.Empty);

            var exporter = new RequirementsExcelExporter(payload);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var constraintsColumn = headers.IndexOf("Parametric Constraints") + 1;
            var row = worksheet.RowsUsed().Single(x => x.Cell(1).GetString() == "R02");

            Assert.That(row.Cell(constraintsColumn).GetString(), Is.EqualTo("the constraint text"));
        }

        [Test]
        public void VerifyGroupHeaderRowShowsOwnName()
        {
            var exporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration()));

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var groupColumn = headers.IndexOf("Group") + 1;
            var groupedRow = worksheet.RowsUsed().Single(row => row.Cell(1).GetString() == "R01").RowNumber();
            var groupCell = worksheet.Cell(groupedRow, groupColumn).GetString();

            Assert.Multiple(() =>
            {
                Assert.That(worksheet.CellsUsed().Any(cell => cell.GetString() == "Group one (GRP)"), Is.True, "the group header row must carry the group's own name");
                Assert.That(groupCell, Is.EqualTo("Group one (GRP)"), "a single-level group's path equals its own name");
                Assert.That(worksheet.CellsUsed().Any(cell => cell.GetString().Contains('/')), Is.False, "a single-level group has no breadcrumb");
            });
        }

        [Test]
        public void VerifyNestedGroupPathInColumn()
        {
            var child = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "CHILD", Name = "Child" };
            this.group.Group.Add(child);
            this.groupedRequirement.Group = child;

            var exporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration()));

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var groupColumn = headers.IndexOf("Group") + 1;
            var groupedRow = worksheet.RowsUsed().Single(row => row.Cell(1).GetString() == "R01").RowNumber();
            var groupCell = worksheet.Cell(groupedRow, groupColumn).GetString();
            var childHeaderRow = worksheet.RowsUsed().Single(row => row.Cell(1).GetString() == "Child (CHILD)").RowNumber();
            var childHeaderGroupCell = worksheet.Cell(childHeaderRow, groupColumn).GetString();

            Assert.Multiple(() =>
            {
                Assert.That(groupCell, Is.EqualTo("Child (CHILD)"), "a requirement's group cell is its direct parent, not a breadcrumb");
                Assert.That(groupCell, Does.Not.Contain(" / "));
                Assert.That(childHeaderGroupCell, Is.EqualTo("Group one (GRP) / Child (CHILD)"), "the group's own header row carries the full breadcrumb path");
            });
        }

        [Test]
        public void VerifyFileNameIsConfigurable()
        {
            var defaultExporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration()));
            var namedExporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { FileName = "Quarterly Export" }));
            var namedWithExtensionExporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { FileName = "Quarterly Export.xlsx" }));
            var forbiddenCharactersExporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { FileName = "a/b:c" }));

            Assert.Multiple(() =>
            {
                Assert.That(defaultExporter.FileName, Is.EqualTo("Requirements.xlsx"));
                Assert.That(namedExporter.FileName, Is.EqualTo("Quarterly Export.xlsx"));
                Assert.That(namedWithExtensionExporter.FileName, Is.EqualTo("Quarterly Export.xlsx"));
                Assert.That(forbiddenCharactersExporter.FileName, Does.Not.Contain("/").And.Not.Contain(":"));
                Assert.That(forbiddenCharactersExporter.FileName, Does.EndWith(".xlsx"));
            });
        }

        [Test]
        public void VerifyMultipleRelationshipsSeparatedByComma()
        {
            var firstRelatedElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT1", Name = "Satellite 1" };
            var secondRelatedElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT2", Name = "Satellite 2" };
            var rule = new RelationshipRuleReference { Iid = Guid.NewGuid(), Name = "trace", ForwardName = "verifies", InverseName = "is verified by" };

            var multipleDetail = new RequirementRelationshipDetail
            {
                RelatedThings = [firstRelatedElement, secondRelatedElement],
                Direction = RelationshipDirection.Outgoing,
                Rule = rule,
                Categories = []
            };

            var multiplePayload = new RequirementsExportPayload(
                [this.specification],
                new RequirementsExportConfiguration(),
                requirement => requirement.Iid == this.ungroupedRequirement.Iid ? [multipleDetail] : [],
                _ => string.Empty);

            var multipleExporter = new RequirementsExcelExporter(multiplePayload);

            using var multipleStream = multipleExporter.Export();
            using var multipleWorkbook = new XLWorkbook(multipleStream);
            var multipleWorksheet = multipleWorkbook.Worksheet(1);
            var multipleHeaders = multipleWorksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var multipleColumn = multipleHeaders.IndexOf("verifies") + 1;
            var multipleRow = multipleWorksheet.RowsUsed().Single(x => x.Cell(1).GetString() == "R02").RowNumber();
            var multipleCell = multipleWorksheet.Cell(multipleRow, multipleColumn).GetString();

            var singleDetail = new RequirementRelationshipDetail
            {
                RelatedThings = [firstRelatedElement],
                Direction = RelationshipDirection.Outgoing,
                Rule = rule,
                Categories = []
            };

            var singlePayload = new RequirementsExportPayload(
                [this.specification],
                new RequirementsExportConfiguration(),
                requirement => requirement.Iid == this.ungroupedRequirement.Iid ? [singleDetail] : [],
                _ => string.Empty);

            var singleExporter = new RequirementsExcelExporter(singlePayload);

            using var singleStream = singleExporter.Export();
            using var singleWorkbook = new XLWorkbook(singleStream);
            var singleWorksheet = singleWorkbook.Worksheet(1);
            var singleHeaders = singleWorksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var singleColumn = singleHeaders.IndexOf("verifies") + 1;
            var singleRow = singleWorksheet.RowsUsed().Single(x => x.Cell(1).GetString() == "R02").RowNumber();
            var singleCell = singleWorksheet.Cell(singleRow, singleColumn).GetString();

            Assert.Multiple(() =>
            {
                Assert.That(multipleCell, Does.Contain(","));
                Assert.That(multipleCell, Does.Contain("Satellite 1").And.Contain("Satellite 2"));
                Assert.That(singleCell, Does.Not.EndWith(","));
            });
        }

        [Test]
        public void VerifySpecificationPerSheetFalseUsesOneSheetWithSpecificationColumn()
        {
            var exporter = new RequirementsExcelExporter(this.BuildPayload(new RequirementsExportConfiguration { SpecificationPerSheet = false }));

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var headers = worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToList();
            var specificationColumn = headers.IndexOf("Specification") + 1;
            var row = worksheet.RowsUsed().Single(x => x.Cell(2).GetString() == "R02").RowNumber();

            Assert.Multiple(() =>
            {
                Assert.That(workbook.Worksheets.Select(x => x.Name), Is.EqualTo(new[] { "Requirements" }));
                Assert.That(headers, Does.Contain("Specification"));
                Assert.That(worksheet.Cell(row, specificationColumn).GetString(), Is.EqualTo("Specification (SPEC)"));
            });
        }
    }
}
