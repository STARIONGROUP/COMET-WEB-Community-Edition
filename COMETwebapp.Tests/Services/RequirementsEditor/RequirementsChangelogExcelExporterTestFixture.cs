// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogExcelExporterTestFixture.cs" company="Starion Group S.A.">
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
    using ClosedXML.Excel;

    using COMETwebapp.Services.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsChangelogExcelExporterTestFixture
    {
        /// <summary>
        /// The expected header row of every per-specification worksheet.
        /// </summary>
        private static readonly string[] ExpectedHeaders = ["ClassKind", "ShortName", "Change", "Field", "Old value", "New value", "Owner"];

        private RequirementChange createdChange;
        private RequirementChange modifiedChange;
        private RequirementChange changeWithoutSpecification;

        [SetUp]
        public void SetUp()
        {
            this.createdChange = new RequirementChange
            {
                Kind = RequirementChangeKind.Created,
                ElementKind = "Requirement",
                ElementName = "New requirement",
                ElementShortName = "R01",
                SpecificationId = Guid.NewGuid(),
                SpecificationName = "Key-User Requirements",
                SpecificationShortName = "KUR",
                Field = string.Empty,
                OldValue = string.Empty,
                NewValue = "Name: New requirement\nOwner: SYS",
                Owner = "SYS"
            };

            this.modifiedChange = new RequirementChange
            {
                Kind = RequirementChangeKind.Modified,
                ElementKind = "Requirement",
                ElementName = "Other requirement",
                ElementShortName = "R05",
                SpecificationId = Guid.NewGuid(),
                SpecificationName = "System Requirements",
                SpecificationShortName = "SYS",
                Field = "Name",
                OldValue = "Old name",
                NewValue = "New name",
                Owner = "SYS"
            };

            this.changeWithoutSpecification = new RequirementChange
            {
                Kind = RequirementChangeKind.Modified,
                ElementKind = "Binary Relationship",
                ElementName = "Trace",
                ElementShortName = "TRC01",
                SpecificationId = Guid.Empty,
                Field = "Source",
                OldValue = "R01",
                NewValue = "R02",
                Owner = "THE"
            };
        }

        [Test]
        public void VerifyExport()
        {
            var exporter = new RequirementsChangelogExcelExporter([this.createdChange, this.modifiedChange, this.changeWithoutSpecification], null, null);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var kurWorksheet = workbook.Worksheet(this.createdChange.SpecificationShortName);
            var sysWorksheet = workbook.Worksheet(this.modifiedChange.SpecificationShortName);
            var generalWorksheet = workbook.Worksheet("General");
            var kurHeaders = kurWorksheet.Row(1).Cells(1, 7).Select(cell => cell.GetString()).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(3), "One worksheet must be written per specification, plus one for changes without a specification.");
                Assert.That(kurWorksheet, Is.Not.Null);
                Assert.That(sysWorksheet, Is.Not.Null);
                Assert.That(generalWorksheet, Is.Not.Null);
                Assert.That(kurHeaders, Is.EqualTo(ExpectedHeaders));
                Assert.That(kurWorksheet?.Cell(2, 1).GetString(), Is.EqualTo(this.createdChange.ElementKind), "The class kind must be its own column.");
                Assert.That(kurWorksheet?.Cell(2, 2).GetString(), Is.EqualTo(this.createdChange.ElementShortName), "The short name must be its own column.");
                Assert.That(kurWorksheet?.Cell(2, 3).GetString(), Is.EqualTo(this.createdChange.Kind.ToString()));
                Assert.That(kurWorksheet?.Cell(2, 6).GetString(), Does.Contain("Name:"), "A created row's New value cell must hold the element's full snapshot.");
                Assert.That(sysWorksheet?.Cell(2, 2).GetString(), Is.EqualTo(this.modifiedChange.ElementShortName));
                Assert.That(generalWorksheet?.Cell(2, 1).GetString(), Is.EqualTo(this.changeWithoutSpecification.ElementKind));
            });
        }

        [Test]
        public void VerifyFileName()
        {
            var withIterationNumbers = new RequirementsChangelogExcelExporter([], 3, 2);
            var withoutIterationNumbers = new RequirementsChangelogExcelExporter([], null, 2);

            Assert.Multiple(() =>
            {
                Assert.That(withIterationNumbers.FileName, Is.EqualTo("Requirements changelog iteration 3 vs 2.xlsx"));
                Assert.That(withoutIterationNumbers.FileName, Is.EqualTo("Requirements changelog.xlsx"));
            });
        }

        [Test]
        public void VerifyExportWithNoChangesProducesHeaderOnly()
        {
            var exporter = new RequirementsChangelogExcelExporter([], null, null);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet("Changelog");

            Assert.Multiple(() =>
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
                Assert.That(worksheet, Is.Not.Null);
                Assert.That(worksheet?.LastRowUsed()?.RowNumber(), Is.EqualTo(1));
            });
        }
    }
}
