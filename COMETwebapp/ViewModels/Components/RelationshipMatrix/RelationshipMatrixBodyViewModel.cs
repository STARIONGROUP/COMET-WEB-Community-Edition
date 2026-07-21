// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixBodyViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RelationshipMatrix
{
    using System.Globalization;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using ClosedXML.Excel;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the logic for the Relationship Matrix application: rows and columns are
    /// categorizable things picked through <see cref="RowConfiguration" /> and <see cref="ColumnConfiguration" />,
    /// and a cell click creates or deletes a categorized <see cref="BinaryRelationship" /> between them.
    /// </summary>
    public class RelationshipMatrixBodyViewModel : SingleIterationApplicationBaseViewModel, IRelationshipMatrixBodyViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedRule" />
        /// </summary>
        private BinaryRelationshipRule selectedRule;

        /// <summary>
        /// Backing field for <see cref="ShowDirectionality" />
        /// </summary>
        private bool showDirectionality = true;

        /// <summary>
        /// Backing field for <see cref="ShowRelatedOnly" />
        /// </summary>
        private bool showRelatedOnly;

        /// <summary>
        /// Backing field for <see cref="ShowNonRelatedBackgroundColor" />
        /// </summary>
        private bool showNonRelatedBackgroundColor;

        /// <summary>
        /// Backing field for <see cref="IsConfigurationPanelCollapsed" />
        /// </summary>
        private bool isConfigurationPanelCollapsed;

        /// <summary>
        /// Backing field for <see cref="RowThings" />
        /// </summary>
        private IReadOnlyList<DefinedThing> rowThings = [];

        /// <summary>
        /// Backing field for <see cref="ColumnThings" />
        /// </summary>
        private IReadOnlyList<DefinedThing> columnThings = [];

        /// <summary>
        /// Backing field for <see cref="SelectedCell" />
        /// </summary>
        private MatrixCellViewModel selectedCell;

        /// <summary>
        /// The currently built matrix cells, keyed by (row <see cref="Guid" />, column <see cref="Guid" />).
        /// </summary>
        private readonly Dictionary<(Guid Row, Guid Column), MatrixCellViewModel> cells = new();

        /// <summary>
        /// The <see cref="IJsUtilitiesService" /> used to offer the exported matrix for download.
        /// </summary>
        private readonly IJsUtilitiesService jsUtilitiesService;

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" /> used to log CRUD and export failures.
        /// </summary>
        private readonly ILogger<RelationshipMatrixBodyViewModel> logger;

        /// <summary>
        /// Creates a new instance of <see cref="RelationshipMatrixBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="jsUtilitiesService">The <see cref="IJsUtilitiesService" /></param>
        public RelationshipMatrixBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<RelationshipMatrixBodyViewModel> logger,
            IJsUtilitiesService jsUtilitiesService) : base(sessionService, messageBus)
        {
            this.logger = logger;
            this.jsUtilitiesService = jsUtilitiesService;

            var rowConfiguration = new SourceConfigurationViewModel(this.RebuildMatrix);
            this.Disposables.Add(rowConfiguration);
            this.RowConfiguration = rowConfiguration;

            var columnConfiguration = new SourceConfigurationViewModel(this.RebuildMatrix);
            this.Disposables.Add(columnConfiguration);
            this.ColumnConfiguration = columnConfiguration;

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.SelectedRule,
                    x => x.ShowRelatedOnly,
                    x => x.ShowNonRelatedBackgroundColor,
                    x => x.ShowDirectionality)
                .Subscribe(_ => this.RebuildMatrix()));

            this.InitializeSubscriptions([typeof(BinaryRelationship)]);
        }

        /// <summary>
        /// Gets the <see cref="ISourceConfigurationViewModel" /> that configures the matrix rows.
        /// </summary>
        public ISourceConfigurationViewModel RowConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="ISourceConfigurationViewModel" /> that configures the matrix columns.
        /// </summary>
        public ISourceConfigurationViewModel ColumnConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule" />s available to pick from, defined by the reference data
        /// libraries of the current iteration.
        /// </summary>
        public IEnumerable<BinaryRelationshipRule> AvailableRules { get; private set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="BinaryRelationshipRule" /> that governs which <see cref="BinaryRelationship" />s
        /// are shown and created in the matrix.
        /// </summary>
        public BinaryRelationshipRule SelectedRule
        {
            get => this.selectedRule;
            set => this.RaiseAndSetIfChanged(ref this.selectedRule, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the direction of relationships is shown in the matrix cells.
        /// </summary>
        public bool ShowDirectionality
        {
            get => this.showDirectionality;
            set => this.RaiseAndSetIfChanged(ref this.showDirectionality, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether rows and columns without any relationship are hidden.
        /// </summary>
        public bool ShowRelatedOnly
        {
            get => this.showRelatedOnly;
            set => this.RaiseAndSetIfChanged(ref this.showRelatedOnly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether cells without a relationship are highlighted with a background colour.
        /// </summary>
        public bool ShowNonRelatedBackgroundColor
        {
            get => this.showNonRelatedBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref this.showNonRelatedBackgroundColor, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration panel is collapsed.
        /// </summary>
        public bool IsConfigurationPanelCollapsed
        {
            get => this.isConfigurationPanelCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isConfigurationPanelCollapsed, value);
        }

        /// <summary>
        /// Gets the <see cref="DefinedThing" />s shown as matrix rows, based on <see cref="RowConfiguration" />.
        /// </summary>
        public IReadOnlyList<DefinedThing> RowThings
        {
            get => this.rowThings;
            private set => this.RaiseAndSetIfChanged(ref this.rowThings, value);
        }

        /// <summary>
        /// Gets the <see cref="DefinedThing" />s shown as matrix columns, based on <see cref="ColumnConfiguration" />.
        /// </summary>
        public IReadOnlyList<DefinedThing> ColumnThings
        {
            get => this.columnThings;
            private set => this.RaiseAndSetIfChanged(ref this.columnThings, value);
        }

        /// <summary>
        /// Gets or sets the currently selected <see cref="MatrixCellViewModel" />. Set through <see cref="SelectCell" />.
        /// </summary>
        public MatrixCellViewModel SelectedCell
        {
            get => this.selectedCell;
            set => this.RaiseAndSetIfChanged(ref this.selectedCell, value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship" /> can be created from <see cref="SelectedCell" />'s row to its column.
        /// </summary>
        public bool CanCreateRowToColumn { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship" /> can be created from <see cref="SelectedCell" />'s column to its row.
        /// </summary>
        public bool CanCreateColumnToRow { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BinaryRelationship" />(s) of <see cref="SelectedCell" /> can be deleted.
        /// </summary>
        public bool CanDelete { get; private set; }

        /// <summary>
        /// Gets the <see cref="MatrixCellViewModel" /> for the given <paramref name="row" /> and <paramref name="column" />,
        /// building and caching one on the fly when the matrix does not currently hold it.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        /// <returns>The <see cref="MatrixCellViewModel" /></returns>
        public MatrixCellViewModel GetCell(DefinedThing row, DefinedThing column)
        {
            if (this.cells.TryGetValue((row.Iid, column.Iid), out var cell))
            {
                return cell;
            }

            cell = this.BuildCell(row, column, this.GetCurrentRelationships());
            this.cells[(row.Iid, column.Iid)] = cell;
            return cell;
        }

        /// <summary>
        /// Determines whether the given <paramref name="row" /> has no relationship with any column.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <returns>true when no relationship touches the row</returns>
        public bool IsRowUnrelated(DefinedThing row)
        {
            return this.ColumnThings.All(column => this.GetCell(row, column).Direction == RelationshipDirectionKind.None);
        }

        /// <summary>
        /// Determines whether the given <paramref name="column" /> has no relationship with any row.
        /// </summary>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        /// <returns>true when no relationship touches the column</returns>
        public bool IsColumnUnrelated(DefinedThing column)
        {
            return this.RowThings.All(row => this.GetCell(row, column).Direction == RelationshipDirectionKind.None);
        }

        /// <summary>
        /// Selects the cell for the given <paramref name="row" /> and <paramref name="column" /> and recomputes the
        /// <see cref="CanCreateRowToColumn" />, <see cref="CanCreateColumnToRow" /> and <see cref="CanDelete" /> permissions.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        public void SelectCell(DefinedThing row, DefinedThing column)
        {
            this.SelectedCell = this.GetCell(row, column);

            var canWrite = this.CurrentThing != null && this.SessionService.Session.PermissionService.CanWrite(ClassKind.BinaryRelationship, this.CurrentThing);
            var direction = this.SelectedCell.Direction;
            var isSameThing = row.Iid == column.Iid;

            this.CanCreateRowToColumn = canWrite && !isSameThing && direction is not RelationshipDirectionKind.RowToColumn and not RelationshipDirectionKind.Bidirectional;
            this.CanCreateColumnToRow = canWrite && !isSameThing && direction is not RelationshipDirectionKind.ColumnToRow and not RelationshipDirectionKind.Bidirectional;
            this.CanDelete = canWrite && direction != RelationshipDirectionKind.None;
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationship" /> between <see cref="SelectedCell" />'s row and column, in the given <paramref name="direction" />.
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipDirectionKind" /> of the <see cref="BinaryRelationship" /> to create</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateRelationshipAsync(RelationshipDirectionKind direction)
        {
            if (this.SelectedCell == null || this.SelectedRule == null)
            {
                return;
            }

            try
            {
                this.IsLoading = true;

                var source = direction == RelationshipDirectionKind.RowToColumn ? this.SelectedCell.SourceRow : this.SelectedCell.SourceColumn;
                var target = direction == RelationshipDirectionKind.RowToColumn ? this.SelectedCell.SourceColumn : this.SelectedCell.SourceRow;

                var iterationClone = this.CurrentThing.Clone(false);
                var relationship = new BinaryRelationship { Iid = Guid.NewGuid(), Source = source, Target = target, Owner = this.CurrentDomain };
                relationship.Category.Add(this.SelectedRule.RelationshipCategory);
                iterationClone.Relationship.Add(relationship);

                var result = await this.SessionService.CreateOrUpdateThingsWithNotification(iterationClone, [iterationClone, relationship]);

                if (result.IsSuccess)
                {
                    this.RebuildMatrix();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while creating a relationship in the Relationship Matrix");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes every <see cref="BinaryRelationship" /> of <see cref="SelectedCell" />.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeleteRelationshipAsync()
        {
            if (this.SelectedCell == null || this.SelectedCell.Relationships.Count == 0)
            {
                return;
            }

            try
            {
                this.IsLoading = true;

                var iterationClone = this.CurrentThing.Clone(false);
                var relationshipClones = this.SelectedCell.Relationships.Select(x => x.Clone(false)).ToList();

                var result = await this.SessionService.DeleteThingsWithNotification(iterationClone, relationshipClones);

                if (result.IsSuccess)
                {
                    this.RebuildMatrix();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting a relationship in the Relationship Matrix");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Swaps the configuration of the rows and the columns.
        /// </summary>
        public void SwapAxes()
        {
            var rowSnapshot = this.RowConfiguration.CaptureSnapshot();
            var columnSnapshot = this.ColumnConfiguration.CaptureSnapshot();

            this.RowConfiguration.RestoreSnapshot(columnSnapshot);
            this.ColumnConfiguration.RestoreSnapshot(rowSnapshot);

            this.RebuildMatrix();
        }

        /// <summary>
        /// Exports the matrix to an Excel workbook (a Matrix, a Relationships and a Configuration worksheet, mirroring
        /// the COMET-IME export) and offers it for download.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExportAsync()
        {
            try
            {
                var generatedOn = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                using var workbook = new XLWorkbook();

                this.WriteMatrixWorksheet(workbook);
                this.WriteRelationshipsWorksheet(workbook);
                this.WriteConfigurationWorksheet(workbook, generatedOn);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                await this.jsUtilitiesService.DownloadFileFromStreamAsync(stream, "RelationshipMatrix.xlsx");
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while exporting the Relationship Matrix");
            }
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

            var tracesColumn = this.ColumnThings.Count + 2;
            var tracesRow = this.RowThings.Count + 2;
            var tracesFill = XLColor.FromHtml("#ACE1AF");
            var noTraceFill = XLColor.FromHtml("#FFE4E1");

            var cornerCell = worksheet.Cell(1, 1);
            cornerCell.Value = new string(' ', 17) + this.ColumnConfiguration.SelectedClassKind + new string('\n', 8) + this.RowConfiguration.SelectedClassKind + "\n";
            cornerCell.Style.Alignment.WrapText = true;
            cornerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            cornerCell.Style.Border.DiagonalDown = true;
            cornerCell.Style.Border.DiagonalBorder = XLBorderStyleValues.Thin;

            for (var columnIndex = 0; columnIndex < this.ColumnThings.Count; columnIndex++)
            {
                var headerCell = worksheet.Cell(1, columnIndex + 2);
                headerCell.Value = this.ColumnThings[columnIndex].Name;
                headerCell.Style.Alignment.TextRotation = 90;
            }

            for (var rowIndex = 0; rowIndex < this.RowThings.Count; rowIndex++)
            {
                var row = this.RowThings[rowIndex];
                worksheet.Cell(rowIndex + 2, 1).Value = row.Name;

                for (var columnIndex = 0; columnIndex < this.ColumnThings.Count; columnIndex++)
                {
                    var direction = this.GetCell(row, this.ColumnThings[columnIndex]).Direction;
                    var bodyCell = worksheet.Cell(rowIndex + 2, columnIndex + 2);

                    bodyCell.Value = direction switch
                    {
                        RelationshipDirectionKind.RowToColumn => "↑",
                        RelationshipDirectionKind.ColumnToRow => "←",
                        RelationshipDirectionKind.Bidirectional => "↔",
                        _ => string.Empty
                    };

                    bodyCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (this.ShowNonRelatedBackgroundColor && direction == RelationshipDirectionKind.None)
                    {
                        bodyCell.Style.Fill.SetBackgroundColor(noTraceFill);
                    }
                }

                var rowTraces = this.ColumnThings.Count(column => this.GetCell(row, column).Direction != RelationshipDirectionKind.None);
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

            for (var columnIndex = 0; columnIndex < this.ColumnThings.Count; columnIndex++)
            {
                var column = this.ColumnThings[columnIndex];
                var columnTraces = this.RowThings.Count(row => this.GetCell(row, column).Direction != RelationshipDirectionKind.None);
                ApplyTraceCell(worksheet.Cell(tracesRow, columnIndex + 2), columnTraces, tracesFill, noTraceFill);
            }

            var totalTraces = this.RowThings.Sum(row => this.ColumnThings.Count(column => this.GetCell(row, column).Direction != RelationshipDirectionKind.None));
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

            var relationships = this.GetCurrentRelationships();

            for (var index = 0; index < relationships.Count; index++)
            {
                var relationship = relationships[index];
                var rowNumber = index + 2;

                worksheet.Cell(rowNumber, 1).Value = (relationship.Source as DefinedThing)?.Name ?? relationship.Source.UserFriendlyName;
                worksheet.Cell(rowNumber, 2).Value = this.SelectedRule?.ForwardRelationshipName;
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
        /// <param name="generatedOn">The export timestamp label</param>
        private void WriteConfigurationWorksheet(XLWorkbook workbook, string generatedOn)
        {
            var worksheet = workbook.Worksheets.Add("Configuration");
            var engineeringModel = (EngineeringModel)this.CurrentThing.Container;

            (string Label, string Value)[] entries =
            [
                ("Engineering Model", engineeringModel.EngineeringModelSetup.Name),
                ("Iteration", this.CurrentThing.IterationSetup.IterationNumber.ToString(CultureInfo.InvariantCulture)),
                ("Generated On", generatedOn),
                ("Relationship Rule", this.SelectedRule?.Name),
                ("X Axis ClassKind", this.ColumnConfiguration.SelectedClassKind?.ToString()),
                ("X Axis Categories", AxisCategories(this.ColumnConfiguration)),
                ("Y Axis ClassKind", this.RowConfiguration.SelectedClassKind?.ToString()),
                ("Y Axis Categories", AxisCategories(this.RowConfiguration))
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

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            this.IsLoading = true;

            this.RowConfiguration.CurrentIteration = this.CurrentThing;
            this.ColumnConfiguration.CurrentIteration = this.CurrentThing;

            this.AvailableRules = this.CurrentThing == null
                ? []
                : this.CurrentThing.IterationSetup.GetContainerOfType<SiteDirectory>()
                    .AvailableReferenceDataLibraries()
                    .SelectMany(rdl => rdl.QueryRulesFromChainOfRdls())
                    .OfType<BinaryRelationshipRule>()
                    .Distinct()
                    .OrderBy(x => x.Name)
                    .ToList();

            this.SelectedRule = null;
            this.RebuildMatrix();

            this.IsLoading = false;
        }

        /// <summary>
        /// Handles the refresh of the current session by rebuilding the matrix when a relationship changed.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if (this.AddedThings.Count != 0 || this.UpdatedThings.Count != 0 || this.DeletedThings.Count != 0)
            {
                this.IsLoading = true;
                this.RebuildMatrix();
                this.ClearRecordedChanges();
                this.IsLoading = false;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the <c>SessionStatus.EndUpdate</c> message received, so that a relationship written by this or
        /// another open application is reflected here as well.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnEndUpdate()
        {
            return this.OnSessionRefreshed();
        }

        /// <summary>
        /// Gets the current <see cref="BinaryRelationship" />s categorized with <see cref="SelectedRule" />'s
        /// relationship category (or one of its sub-categories).
        /// </summary>
        /// <returns>The matching relationships, empty when no rule is selected or no iteration is loaded</returns>
        private List<BinaryRelationship> GetCurrentRelationships()
        {
            if (this.CurrentThing == null || this.SelectedRule == null)
            {
                return [];
            }

            return this.CurrentThing.Relationship.OfType<BinaryRelationship>()
                .Where(x => x.Category.Any(c => c == this.SelectedRule.RelationshipCategory || c.AllSuperCategories().Contains(this.SelectedRule.RelationshipCategory)))
                .ToList();
        }

        /// <summary>
        /// Builds the <see cref="MatrixCellViewModel" /> for the given <paramref name="row" /> and <paramref name="column" />,
        /// gathering the <paramref name="relationships" /> found between them in either direction.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        /// <param name="relationships">The current <see cref="BinaryRelationship" />s categorized with <see cref="SelectedRule" /></param>
        /// <returns>The built <see cref="MatrixCellViewModel" /></returns>
        private MatrixCellViewModel BuildCell(DefinedThing row, DefinedThing column, IReadOnlyList<BinaryRelationship> relationships)
        {
            var cellRelationships = relationships
                .Where(x => (x.Source.Iid == row.Iid && x.Target.Iid == column.Iid) || (x.Source.Iid == column.Iid && x.Target.Iid == row.Iid))
                .ToList();

            return new MatrixCellViewModel(row, column, cellRelationships, this.SelectedRule);
        }

        /// <summary>
        /// Recomputes <see cref="RowThings" /> and <see cref="ColumnThings" /> from the current iteration's
        /// categorizable things and the row/column configurations, and rebuilds the matrix cells. When
        /// <see cref="ShowRelatedOnly" /> is set, rows and columns without any relationship are dropped.
        /// </summary>
        private void RebuildMatrix()
        {
            this.cells.Clear();

            if (this.CurrentThing == null)
            {
                this.RowThings = [];
                this.ColumnThings = [];
                return;
            }

            var candidates = this.CurrentThing.QueryContainedThingsDeep()
                .OfType<DefinedThing>()
                .Where(x => x is ICategorizableThing)
                .Distinct()
                .ToList();

            var rows = this.RowConfiguration.QuerySourceThings(candidates);
            var columns = this.ColumnConfiguration.QuerySourceThings(candidates);
            var relationships = this.GetCurrentRelationships();

            foreach (var row in rows)
            {
                foreach (var column in columns)
                {
                    this.cells[(row.Iid, column.Iid)] = this.BuildCell(row, column, relationships);
                }
            }

            if (this.ShowRelatedOnly)
            {
                rows = rows.Where(row => columns.Any(column => this.cells[(row.Iid, column.Iid)].Direction != RelationshipDirectionKind.None)).ToList();
                columns = columns.Where(column => rows.Any(row => this.cells[(row.Iid, column.Iid)].Direction != RelationshipDirectionKind.None)).ToList();
            }

            this.RowThings = rows;
            this.ColumnThings = columns;
        }
    }
}
