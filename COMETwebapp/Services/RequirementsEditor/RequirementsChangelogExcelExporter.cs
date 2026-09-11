// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogExcelExporter.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RequirementsEditor
{
    using System.Text.RegularExpressions;

    using ClosedXML.Excel;

    using COMETwebapp.Services.Export;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    /// <summary>
    /// Exports the rows of the requirements changelog (see <see cref="RequirementsChangelogViewModel" />) to an Excel
    /// workbook with one worksheet per <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" />, each
    /// sheet an Excel table with one row per <see cref="RequirementChange" /> of that specification.
    /// </summary>
    public partial class RequirementsChangelogExcelExporter : IExporter
    {
        /// <summary>
        /// Matches the characters Excel forbids in a worksheet name.
        /// </summary>
        /// <returns>The compiled, source-generated <see cref="Regex" /></returns>
        [GeneratedRegex(@"[\[\]\*/\\\?:]")]
        private static partial Regex InvalidSheetNameCharacters();

        /// <summary>
        /// Matches the characters not allowed in the downloaded file name.
        /// </summary>
        /// <returns>The compiled, source-generated <see cref="Regex" /></returns>
        [GeneratedRegex(@"[<>:""/\\|\?\*\x00-\x1F]")]
        private static partial Regex InvalidFileNameCharacters();

        /// <summary>
        /// The fixed width, in characters, given to the wide free-text columns.
        /// </summary>
        private const double WideColumnWidth = 45;

        /// <summary>
        /// The changes to export, one worksheet per specification, one row per change.
        /// </summary>
        private readonly IReadOnlyList<RequirementChange> changes;

        /// <summary>
        /// The iteration number of the current iteration, used to build the file name, or null when unknown.
        /// </summary>
        private readonly int? currentIterationNumber;

        /// <summary>
        /// The iteration number of the baseline iteration, used to build the file name, or null when unknown.
        /// </summary>
        private readonly int? baselineIterationNumber;

        /// <summary>
        /// The worksheet names already used, so two specifications sharing a short name still get unique sheets.
        /// </summary>
        private readonly HashSet<string> usedSheetNames = [];

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsChangelogExcelExporter" />
        /// </summary>
        /// <param name="changes">The <see cref="RequirementChange" />s to export</param>
        /// <param name="currentIterationNumber">The iteration number of the current iteration, or null when unknown</param>
        /// <param name="baselineIterationNumber">The iteration number of the baseline iteration, or null when unknown</param>
        public RequirementsChangelogExcelExporter(IReadOnlyList<RequirementChange> changes, int? currentIterationNumber, int? baselineIterationNumber)
        {
            this.changes = changes;
            this.currentIterationNumber = currentIterationNumber;
            this.baselineIterationNumber = baselineIterationNumber;
        }

        /// <summary>
        /// Gets the name of the exported file.
        /// </summary>
        public string FileName => this.ResolveFileName();

        /// <summary>
        /// Builds the Excel workbook and returns it as a readable <see cref="Stream" />.
        /// </summary>
        /// <returns>The <see cref="Stream" /> holding the workbook bytes</returns>
        public Stream Export()
        {
            using var workbook = new XLWorkbook();

            foreach (var group in this.changes.GroupBy(x => x.SpecificationId))
            {
                var worksheet = workbook.Worksheets.Add(this.SafeSheetName(ResolveSheetName(group)));
                this.WriteHeaderRow(worksheet);
                var row = 2;

                foreach (var change in group)
                {
                    WriteRow(worksheet, row, change);
                    row++;
                }

                this.FinalizeWorksheet(worksheet, row - 1);
            }

            if (workbook.Worksheets.Count == 0)
            {
                var worksheet = workbook.Worksheets.Add("Changelog");
                this.WriteHeaderRow(worksheet);
                this.FinalizeWorksheet(worksheet, 1);
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Resolves the worksheet name for the given <paramref name="group" /> of changes: the owning specification's
        /// short name, or "General" for changes without an owning specification.
        /// </summary>
        /// <param name="group">The changes grouped by <see cref="RequirementChange.SpecificationId" /></param>
        /// <returns>The unsanitized worksheet name</returns>
        private static string ResolveSheetName(IGrouping<Guid, RequirementChange> group)
        {
            return group.Key == Guid.Empty ? "General" : group.First().SpecificationShortName;
        }

        /// <summary>
        /// Writes the header row, one cell per column.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        private void WriteHeaderRow(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "ClassKind";
            worksheet.Cell(1, 2).Value = "ShortName";
            worksheet.Cell(1, 3).Value = "Change";
            worksheet.Cell(1, 4).Value = "Field";
            worksheet.Cell(1, 5).Value = "Old value";
            worksheet.Cell(1, 6).Value = "New value";
            worksheet.Cell(1, 7).Value = "Owner";
        }

        /// <summary>
        /// Writes one <paramref name="change" /> as the given <paramref name="row" /> of the <paramref name="worksheet" />.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        /// <param name="row">The one-based row number</param>
        /// <param name="change">The <see cref="RequirementChange" /> to write</param>
        private static void WriteRow(IXLWorksheet worksheet, int row, RequirementChange change)
        {
            worksheet.Cell(row, 1).Value = change.ElementKind;
            worksheet.Cell(row, 2).Value = change.ElementShortName;
            worksheet.Cell(row, 3).Value = change.Kind.ToString();
            worksheet.Cell(row, 4).Value = change.Field;
            worksheet.Cell(row, 5).Value = change.OldValue;
            worksheet.Cell(row, 6).Value = change.NewValue;
            worksheet.Cell(row, 7).Value = change.Owner;
        }

        /// <summary>
        /// Turns the written rows of the <paramref name="worksheet" /> into an Excel table (or, when it only holds the
        /// header, bolds it instead), and sizes its columns.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to finalize</param>
        /// <param name="lastRow">The one-based number of the last written row</param>
        private void FinalizeWorksheet(IXLWorksheet worksheet, int lastRow)
        {
            if (lastRow >= 2)
            {
                worksheet.Range(1, 1, lastRow, 7).CreateTable();
            }
            else
            {
                worksheet.Row(1).Style.Font.Bold = true;
            }

            worksheet.Columns().AdjustToContents();

            int[] wideColumns = [4, 5, 6];

            foreach (var column in wideColumns)
            {
                worksheet.Column(column).Width = WideColumnWidth;
                worksheet.Column(column).Style.Alignment.WrapText = true;
            }
        }

        /// <summary>
        /// Sanitizes the given specification <paramref name="name" /> into a worksheet name that Excel accepts and that
        /// has not already been used, appending a numbered suffix on collision.
        /// </summary>
        /// <param name="name">The specification's short name, or "General"</param>
        /// <returns>The unique, sanitized worksheet name</returns>
        private string SafeSheetName(string name)
        {
            var sanitized = InvalidSheetNameCharacters().Replace(name ?? string.Empty, " ").Trim();

            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = "Specification";
            }

            if (sanitized.Length > 31)
            {
                sanitized = sanitized[..31];
            }

            var candidate = sanitized;
            var suffix = 1;

            while (!this.usedSheetNames.Add(candidate))
            {
                var tag = $" ({suffix++})";
                candidate = sanitized.Length + tag.Length > 31 ? sanitized[..(31 - tag.Length)] + tag : sanitized + tag;
            }

            return candidate;
        }

        /// <summary>
        /// Resolves the exported file name from the iteration numbers, sanitized for the characters a downloaded file
        /// name forbids, falling back to "Requirements changelog" when the numbers are unknown.
        /// </summary>
        /// <returns>The file name, including its ".xlsx" extension</returns>
        private string ResolveFileName()
        {
            var name = this.currentIterationNumber == null || this.baselineIterationNumber == null
                ? "Requirements changelog"
                : $"Requirements changelog iteration {this.currentIterationNumber} vs {this.baselineIterationNumber}";

            return $"{InvalidFileNameCharacters().Replace(name, string.Empty)}.xlsx";
        }
    }
}
