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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Model.RelationshipMatrix.Configuration;
    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.FileStore;
    using COMETwebapp.Services.RelationshipMatrix;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

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
        /// Backing field for <see cref="FileStoreMessage" />
        /// </summary>
        private string fileStoreMessage;

        /// <summary>
        /// Backing field for <see cref="IsConfigurationDialogVisible" />
        /// </summary>
        private bool isConfigurationDialogVisible;

        /// <summary>
        /// The currently built matrix cells, keyed by (row <see cref="Guid" />, column <see cref="Guid" />).
        /// </summary>
        private readonly Dictionary<(Guid Row, Guid Column), MatrixCellViewModel> cells = new();

        /// <summary>
        /// The <see cref="IExportService" /> used to run an exporter and offer its output for download.
        /// </summary>
        private readonly IExportService exportService;

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" /> used to log CRUD and export failures.
        /// </summary>
        private readonly ILogger<RelationshipMatrixBodyViewModel> logger;

        /// <summary>
        /// The <see cref="IFileStoreService" /> used to save and load matrix configurations in the model's file store.
        /// </summary>
        private readonly IFileStoreService fileStoreService;

        /// <summary>
        /// Creates a new instance of <see cref="RelationshipMatrixBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="exportService">The <see cref="IExportService" /></param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService" /></param>
        public RelationshipMatrixBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<RelationshipMatrixBodyViewModel> logger,
            IExportService exportService, IFileStoreService fileStoreService) : base(sessionService, messageBus)
        {
            this.logger = logger;
            this.exportService = exportService;
            this.fileStoreService = fileStoreService;

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
                var payload = new RelationshipMatrixExportPayload(
                    this.RowThings, this.ColumnThings,
                    (row, column) => this.GetCell(row, column).Direction,
                    this.RowConfiguration, this.ColumnConfiguration,
                    this.SelectedRule, this.ShowNonRelatedBackgroundColor,
                    this.GetCurrentRelationships(),
                    (EngineeringModel)this.CurrentThing.Container,
                    this.CurrentThing.IterationSetup.IterationNumber,
                    DateTime.Now);

                await this.exportService.ExportAndDownloadAsync(new RelationshipMatrixExporter(payload));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while exporting the Relationship Matrix");
            }
        }

        /// <summary>
        /// Exports the current matrix configuration to a COMET-IME-compatible JSON file and offers it for download.
        /// </summary>
        /// <param name="name">The name of the downloaded file (without extension); a default is used when empty</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExportConfigurationAsync(string name = null)
        {
            try
            {
                var exporter = new RelationshipMatrixConfigurationExporter(
                    this.RowConfiguration, this.ColumnConfiguration, this.SelectedRule,
                    this.ShowDirectionality, this.ShowRelatedOnly, this.ShowNonRelatedBackgroundColor, name);

                await this.exportService.ExportAndDownloadAsync(exporter);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while exporting the Relationship Matrix configuration");
            }
        }

        /// <summary>
        /// Imports a matrix configuration from the given <paramref name="stream" /> (a COMET-IME-compatible JSON file)
        /// and applies it to the two axes, the relationship rule and the display options, then rebuilds the matrix.
        /// </summary>
        /// <param name="stream">The <see cref="Stream" /> holding the JSON configuration</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ImportConfigurationAsync(Stream stream)
        {
            try
            {
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };

                var configuration = JsonConvert.DeserializeObject<MatrixConfigurationFile>(json, settings)?.SavedConfigurations?.FirstOrDefault()
                                    ?? JsonConvert.DeserializeObject<MatrixSavedConfiguration>(json, settings);

                if (configuration?.SourceConfigurationX == null && configuration?.SourceConfigurationY == null)
                {
                    this.logger.LogWarning("The imported Relationship Matrix configuration file did not contain a usable configuration");
                    return;
                }

                this.RowConfiguration.RestoreSnapshot(configuration.SourceConfigurationY ?? new MatrixSourceConfiguration());
                this.ColumnConfiguration.RestoreSnapshot(configuration.SourceConfigurationX ?? new MatrixSourceConfiguration());

                this.SelectedRule = this.AvailableRules.FirstOrDefault(x => x.Iid == configuration.RelationshipConfiguration?.SelectedRule);
                this.ShowDirectionality = configuration.ShowDirectionality;
                this.ShowRelatedOnly = configuration.ShowRelatedOnly;
                this.ShowNonRelatedBackgroundColor = configuration.ShowNonRelatedBackgroundColor;

                this.RebuildMatrix();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while importing the Relationship Matrix configuration");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current model can hold matrix configurations in its file store, that is
        /// whether a JSON <see cref="FileType" /> is defined by the reference data.
        /// </summary>
        public bool CanUseModelFileStore => this.CurrentThing != null && this.fileStoreService.GetFileType(this.CurrentThing, "json") is not null;

        /// <summary>
        /// Gets the last outcome message of a save/load against the model file store, shown to the user. Empty when there is none.
        /// </summary>
        public string FileStoreMessage
        {
            get => this.fileStoreMessage;
            private set => this.RaiseAndSetIfChanged(ref this.fileStoreMessage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration export/import dialog is open.
        /// </summary>
        public bool IsConfigurationDialogVisible
        {
            get => this.isConfigurationDialogVisible;
            set => this.RaiseAndSetIfChanged(ref this.isConfigurationDialogVisible, value);
        }

        /// <summary>
        /// Gets a value indicating whether the requested <paramref name="storeType" /> exists on the current iteration.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to check</param>
        /// <returns><see langword="true" /> when the store exists, otherwise <see langword="false" /></returns>
        public bool StoreExists(FileStoreType storeType)
        {
            return this.CurrentThing != null && this.fileStoreService.StoreExists(this.CurrentThing, storeType);
        }

        /// <summary>
        /// Gets the <see cref="Folder" />s of the requested <paramref name="storeType" /> the configuration can be saved into.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <returns>The available <see cref="Folder" />s</returns>
        public IReadOnlyList<Folder> GetFolders(FileStoreType storeType)
        {
            return this.CurrentThing == null ? [] : this.fileStoreService.GetFolders(this.CurrentThing, storeType);
        }

        /// <summary>
        /// Creates the requested <paramref name="storeType" /> on the current iteration when it does not yet exist.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to create</param>
        /// <returns>A <see cref="Task" /> with <see langword="true" /> when the store was created</returns>
        public async Task<bool> CreateFileStoreAsync(FileStoreType storeType)
        {
            try
            {
                var result = await this.fileStoreService.CreateStoreAsync(this.CurrentThing, storeType);

                if (result.IsFailed)
                {
                    this.logger.LogWarning("Failed to create the {storeType} file store: {reasons}", storeType, string.Join(", ", result.Reasons.Select(x => x.Message)));
                    this.FileStoreMessage = $"Could not create the {storeType} file store: {string.Join(", ", result.Reasons.Select(x => x.Message))}";
                    return false;
                }

                this.FileStoreMessage = $"Created the {storeType} file store.";
                return true;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while creating the {storeType} file store", storeType);
                this.FileStoreMessage = $"An error occurred while creating the {storeType} file store.";
                return false;
            }
        }

        /// <summary>
        /// Saves the current matrix configuration as a JSON file item into the model's file store.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to save into</param>
        /// <param name="name">The name of the saved configuration file (without extension); a default is used when empty</param>
        /// <param name="folder">The <see cref="Folder" /> to save into, or <see langword="null" /> for the store root</param>
        /// <returns>A <see cref="Task" /> with <see langword="true" /> when the configuration was saved</returns>
        public async Task<bool> SaveConfigurationToStoreAsync(FileStoreType storeType, string name = null, Folder folder = null)
        {
            try
            {
                var jsonFileType = this.fileStoreService.GetFileType(this.CurrentThing, "json");

                if (jsonFileType == null)
                {
                    this.logger.LogWarning("Cannot save the Relationship Matrix configuration: no JSON file type is available in the model reference data");
                    this.FileStoreMessage = "No JSON file type is available in the model reference data.";
                    return false;
                }

                var exporter = new RelationshipMatrixConfigurationExporter(
                    this.RowConfiguration, this.ColumnConfiguration, this.SelectedRule,
                    this.ShowDirectionality, this.ShowRelatedOnly, this.ShowNonRelatedBackgroundColor, name);

                using var stream = exporter.Export();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                var result = await this.fileStoreService.SaveFileAsync(this.CurrentThing, storeType, exporter.FileName, jsonFileType, memoryStream.ToArray(), folder);

                if (result.IsFailed)
                {
                    this.logger.LogWarning("Failed to save the Relationship Matrix configuration to the model file store: {reasons}", string.Join(", ", result.Reasons.Select(x => x.Message)));
                    this.FileStoreMessage = $"Could not save to the {storeType} file store: {string.Join(", ", result.Reasons.Select(x => x.Message))}";
                    return false;
                }

                this.FileStoreMessage = $"Saved the configuration to the {storeType} file store.";
                return true;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while saving the Relationship Matrix configuration to the model file store");
                this.FileStoreMessage = "An error occurred while saving the configuration to the model file store.";
                return false;
            }
        }

        /// <summary>
        /// Gets the matrix configuration files stored in the requested store of the current iteration.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <returns>The stored configuration <see cref="File" />s</returns>
        public IReadOnlyList<File> GetStoredConfigurations(FileStoreType storeType)
        {
            if (this.CurrentThing == null)
            {
                return [];
            }

            var jsonFileType = this.fileStoreService.GetFileType(this.CurrentThing, "json");
            return this.fileStoreService.GetFiles(this.CurrentThing, storeType, jsonFileType);
        }

        /// <summary>
        /// Loads a matrix configuration from the given <paramref name="file" /> stored in the model's file store.
        /// </summary>
        /// <param name="file">The stored configuration <see cref="File" /></param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task LoadConfigurationFromStoreAsync(File file)
        {
            try
            {
                var bytes = await this.fileStoreService.ReadFileAsync(file);
                using var stream = new MemoryStream(bytes);
                await this.ImportConfigurationAsync(stream);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while loading the Relationship Matrix configuration from the model file store");
            }
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
