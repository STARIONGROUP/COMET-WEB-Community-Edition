// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixExporter.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RelationshipMatrix
{
    using System.Globalization;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using ClosedXML.Excel;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.Export;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    /// <summary>
    /// Exports a Relationship Matrix to an Excel workbook (a Matrix, a Relationships and a Configuration worksheet,
    /// mirroring the COMET-IME export). Built from an immutable <see cref="RelationshipMatrixExportPayload" /> so it is
    /// decoupled from the view model and unit-testable.
    /// </summary>
    public class RelationshipMatrixExporter : IExporter
    {
        /// <summary>
        /// The immutable snapshot of the matrix to export.
        /// </summary>
        private readonly RelationshipMatrixExportPayload payload;

        /// <summary>
        /// Creates a new instance of <see cref="RelationshipMatrixExporter" />
        /// </summary>
        /// <param name="payload">The <see cref="RelationshipMatrixExportPayload" /> to export</param>
        public RelationshipMatrixExporter(RelationshipMatrixExportPayload payload)
        {
            this.payload = payload;
        }

        /// <summary>
        /// Gets the name of the exported file.
        /// </summary>
        public string FileName => "RelationshipMatrix.xlsx";

        /// <summary>
        /// Builds the Excel workbook and returns it as a readable <see cref="Stream" />.
        /// </summary>
        /// <returns>The <see cref="Stream" /> holding the workbook bytes</returns>
        public Stream Export()
        {
            using var workbook = new XLWorkbook();

            this.WriteMatrixWorksheet(workbook);
            this.WriteRelationshipsWorksheet(workbook);
            this.WriteConfigurationWorksheet(workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Writes the "Matrix" worksheet: the row/column grid of direction glyphs with a "Traces" totals row, mirroring
        /// the COMET-IME Matrix sheet.
        /// </summary>
        /// <param name="workbook">The <see cref="XLWorkbook" /> to add the worksheet to</param>
        private void WriteMatrixWorksheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Matrix");
            worksheet.TabColor = XLColor.FromHtml("#8FBC8B");

            var tracesColumn = this.payload.Columns.Count + 2;
            var tracesRow = this.payload.Rows.Count + 2;
            var tracesFill = XLColor.FromHtml("#ACE1AF");
            var noTraceFill = XLColor.FromHtml("#FFE4E1");

            var cornerCell = worksheet.Cell(1, 1);
            cornerCell.Value = new string(' ', 17) + this.payload.ColumnConfiguration.SelectedClassKind + new string('\n', 8) + this.payload.RowConfiguration.SelectedClassKind + "\n";
            cornerCell.Style.Alignment.WrapText = true;
            cornerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            cornerCell.Style.Border.DiagonalDown = true;
            cornerCell.Style.Border.DiagonalBorder = XLBorderStyleValues.Thin;

            for (var columnIndex = 0; columnIndex < this.payload.Columns.Count; columnIndex++)
            {
                var headerCell = worksheet.Cell(1, columnIndex + 2);
                headerCell.Value = this.payload.Columns[columnIndex].Name;
                headerCell.Style.Alignment.TextRotation = 90;
            }

            for (var rowIndex = 0; rowIndex < this.payload.Rows.Count; rowIndex++)
            {
                var row = this.payload.Rows[rowIndex];
                worksheet.Cell(rowIndex + 2, 1).Value = row.Name;

                for (var columnIndex = 0; columnIndex < this.payload.Columns.Count; columnIndex++)
                {
                    var direction = this.payload.DirectionOf(row, this.payload.Columns[columnIndex]);
                    var bodyCell = worksheet.Cell(rowIndex + 2, columnIndex + 2);

                    bodyCell.Value = direction switch
                    {
                        RelationshipDirectionKind.RowToColumn => "↑",
                        RelationshipDirectionKind.ColumnToRow => "←",
                        RelationshipDirectionKind.Bidirectional => "↔",
                        _ => string.Empty
                    };

                    bodyCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (this.payload.ShowNonRelatedBackgroundColor && direction == RelationshipDirectionKind.None)
                    {
                        bodyCell.Style.Fill.SetBackgroundColor(noTraceFill);
                    }
                }

                var rowTraces = this.payload.Columns.Count(column => this.payload.DirectionOf(row, column) != RelationshipDirectionKind.None);
                ApplyTraceCell(worksheet.Cell(rowIndex + 2, tracesColumn), rowTraces, tracesFill, noTraceFill);
            }

            var tracesColumnHeader = worksheet.Cell(1, tracesColumn);
            tracesColumnHeader.Value = "Traces";
            tracesColumnHeader.Style.Alignment.TextRotation = 90;
            tracesColumnHeader.Style.Fill.SetBackgroundColor(tracesFill);

            var tracesRowHeader = worksheet.Cell(tracesRow, 1);
            tracesRowHeader.Value = "Traces";
            tracesRowHeader.Style.Font.Italic = true;
            tracesRowHeader.Style.Fill.SetBackgroundColor(tracesFill);

            for (var columnIndex = 0; columnIndex < this.payload.Columns.Count; columnIndex++)
            {
                var column = this.payload.Columns[columnIndex];
                var columnTraces = this.payload.Rows.Count(row => this.payload.DirectionOf(row, column) != RelationshipDirectionKind.None);
                ApplyTraceCell(worksheet.Cell(tracesRow, columnIndex + 2), columnTraces, tracesFill, noTraceFill);
            }

            var totalTraces = this.payload.Rows.Sum(row => this.payload.Columns.Count(column => this.payload.DirectionOf(row, column) != RelationshipDirectionKind.None));
            ApplyTraceCell(worksheet.Cell(tracesRow, tracesColumn), totalTraces, tracesFill, noTraceFill);

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Column(1).Style.Font.Bold = true;
            worksheet.SheetView.FreezeRows(1);
            worksheet.SheetView.FreezeColumns(1);
            worksheet.Row(1).Height = 140;
            worksheet.Columns().AdjustToContents();
            worksheet.Column(1).Width = 22;
        }

        /// <summary>
        /// Sets a "Traces" count cell's value and colours it green when the count is non-zero and red-ish when it is
        /// zero, mirroring the IME export.
        /// </summary>
        /// <param name="cell">The <see cref="IXLCell" /> to write</param>
        /// <param name="count">The traces count</param>
        /// <param name="tracesFill">The fill for a non-zero count</param>
        /// <param name="noTraceFill">The fill for a zero count</param>
        private static void ApplyTraceCell(IXLCell cell, int count, XLColor tracesFill, XLColor noTraceFill)
        {
            cell.Value = count;
            cell.Style.Font.Italic = true;
            cell.Style.Fill.SetBackgroundColor(count > 0 ? tracesFill : noTraceFill);
        }

        /// <summary>
        /// Writes the "Relationships" worksheet: one row per current <see cref="BinaryRelationship" /> with its source,
        /// target, categories and owner.
        /// </summary>
        /// <param name="workbook">The <see cref="XLWorkbook" /> to add the worksheet to</param>
        private void WriteRelationshipsWorksheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Relationships");
            worksheet.TabColor = XLColor.FromHtml("#B0C4DE");

            worksheet.Cell(1, 1).Value = "Source";
            worksheet.Cell(1, 2).Value = "Relationship";
            worksheet.Cell(1, 3).Value = "Target";
            worksheet.Cell(1, 4).Value = "Categories";
            worksheet.Cell(1, 5).Value = "Owner";
            worksheet.Row(1).Style.Font.Bold = true;

            var relationships = this.payload.Relationships;

            for (var index = 0; index < relationships.Count; index++)
            {
                var relationship = relationships[index];
                var rowNumber = index + 2;

                worksheet.Cell(rowNumber, 1).Value = (relationship.Source as DefinedThing)?.Name ?? relationship.Source.UserFriendlyName;
                worksheet.Cell(rowNumber, 2).Value = this.payload.Rule?.ForwardRelationshipName;
                worksheet.Cell(rowNumber, 3).Value = (relationship.Target as DefinedThing)?.Name ?? relationship.Target.UserFriendlyName;
                worksheet.Cell(rowNumber, 4).Value = string.Join(", ", relationship.Category.Select(x => x.Name));
                worksheet.Cell(rowNumber, 5).Value = relationship.Owner?.ShortName;
            }

            worksheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Writes the "Configuration" worksheet: a label/value description of the model, iteration, rule and both axes.
        /// </summary>
        /// <param name="workbook">The <see cref="XLWorkbook" /> to add the worksheet to</param>
        private void WriteConfigurationWorksheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Configuration");

            (string Label, string Value)[] entries =
            [
                ("Engineering Model", this.payload.EngineeringModel.EngineeringModelSetup.Name),
                ("Iteration", this.payload.IterationNumber.ToString(CultureInfo.InvariantCulture)),
                ("Generated On", this.payload.GeneratedOn.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                ("Relationship Rule", this.payload.Rule?.Name),
                ("X Axis ClassKind", this.payload.ColumnConfiguration.SelectedClassKind?.ToString()),
                ("X Axis Categories", AxisCategories(this.payload.ColumnConfiguration)),
                ("Y Axis ClassKind", this.payload.RowConfiguration.SelectedClassKind?.ToString()),
                ("Y Axis Categories", AxisCategories(this.payload.RowConfiguration))
            ];

            for (var index = 0; index < entries.Length; index++)
            {
                worksheet.Cell(index + 1, 1).Value = entries[index].Label;
                worksheet.Cell(index + 1, 2).Value = entries[index].Value;
            }

            worksheet.Column(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Builds the axis-categories label for the given <paramref name="config" />, e.g. <c>(System Requirement)</c>
        /// or <c>(A OR B)</c>, joining the selected categories with the axis' boolean operator.
        /// </summary>
        /// <param name="config">The <see cref="ISourceConfigurationViewModel" /> of the axis</param>
        /// <returns>The parenthesised label, empty when the axis has no selected categories</returns>
        private static string AxisCategories(ISourceConfigurationViewModel config)
        {
            var categories = config.CategorySelector.SelectedCategories.Select(c => c.Name).ToList();

            if (categories.Count == 0)
            {
                return string.Empty;
            }

            return $"({string.Join($" {config.SelectedBooleanOperatorKind.ToString().ToUpperInvariant()} ", categories)})";
        }
    }
}
