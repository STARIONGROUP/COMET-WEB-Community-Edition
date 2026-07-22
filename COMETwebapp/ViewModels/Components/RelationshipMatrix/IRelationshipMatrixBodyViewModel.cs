// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRelationshipMatrixBodyViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.FileStore;

    /// <summary>
    /// Interface for the <see cref="RelationshipMatrixBodyViewModel" />, driving the Relationship Matrix application.
    /// </summary>
    public interface IRelationshipMatrixBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the <see cref="ISourceConfigurationViewModel" /> that configures the matrix rows.
        /// </summary>
        ISourceConfigurationViewModel RowConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="ISourceConfigurationViewModel" /> that configures the matrix columns.
        /// </summary>
        ISourceConfigurationViewModel ColumnConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule" />s available to pick from, defined by the reference data
        /// libraries of the current iteration.
        /// </summary>
        IEnumerable<BinaryRelationshipRule> AvailableRules { get; }

        /// <summary>
        /// Gets or sets the <see cref="BinaryRelationshipRule" /> that governs which <see cref="BinaryRelationship" />s
        /// are shown and created in the matrix.
        /// </summary>
        BinaryRelationshipRule SelectedRule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the direction of relationships is shown in the matrix cells.
        /// </summary>
        bool ShowDirectionality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether rows and columns without any relationship are hidden.
        /// </summary>
        bool ShowRelatedOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cells without a relationship are highlighted with a background colour.
        /// </summary>
        bool ShowNonRelatedBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration panel is collapsed.
        /// </summary>
        bool IsConfigurationPanelCollapsed { get; set; }

        /// <summary>
        /// Gets the <see cref="DefinedThing" />s shown as matrix rows, based on <see cref="RowConfiguration" />.
        /// </summary>
        IReadOnlyList<DefinedThing> RowThings { get; }

        /// <summary>
        /// Gets the <see cref="DefinedThing" />s shown as matrix columns, based on <see cref="ColumnConfiguration" />.
        /// </summary>
        IReadOnlyList<DefinedThing> ColumnThings { get; }

        /// <summary>
        /// Gets or sets the currently selected <see cref="MatrixCellViewModel" />. Set through <see cref="SelectCell" />.
        /// </summary>
        MatrixCellViewModel SelectedCell { get; set; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship" /> can be created from <see cref="SelectedCell" />'s row to its column.
        /// </summary>
        bool CanCreateRowToColumn { get; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship" /> can be created from <see cref="SelectedCell" />'s column to its row.
        /// </summary>
        bool CanCreateColumnToRow { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BinaryRelationship" />(s) of <see cref="SelectedCell" /> can be deleted.
        /// </summary>
        bool CanDelete { get; }

        /// <summary>
        /// Gets the <see cref="MatrixCellViewModel" /> for the given <paramref name="row" /> and <paramref name="column" />.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        /// <returns>The <see cref="MatrixCellViewModel" /></returns>
        MatrixCellViewModel GetCell(DefinedThing row, DefinedThing column);

        /// <summary>
        /// Determines whether the given <paramref name="row" /> has no relationship with any column.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <returns>true when no relationship touches the row</returns>
        bool IsRowUnrelated(DefinedThing row);

        /// <summary>
        /// Determines whether the given <paramref name="column" /> has no relationship with any row.
        /// </summary>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        /// <returns>true when no relationship touches the column</returns>
        bool IsColumnUnrelated(DefinedThing column);

        /// <summary>
        /// Selects the cell for the given <paramref name="row" /> and <paramref name="column" /> and recomputes the
        /// <see cref="CanCreateRowToColumn" />, <see cref="CanCreateColumnToRow" /> and <see cref="CanDelete" /> permissions.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" /></param>
        /// <param name="column">The column <see cref="DefinedThing" /></param>
        void SelectCell(DefinedThing row, DefinedThing column);

        /// <summary>
        /// Creates a <see cref="BinaryRelationship" /> between <see cref="SelectedCell" />'s row and column, in the given <paramref name="direction" />.
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipDirectionKind" /> of the <see cref="BinaryRelationship" /> to create</param>
        /// <returns>A <see cref="Task" /></returns>
        Task CreateRelationshipAsync(RelationshipDirectionKind direction);

        /// <summary>
        /// Deletes every <see cref="BinaryRelationship" /> of <see cref="SelectedCell" />.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task DeleteRelationshipAsync();

        /// <summary>
        /// Swaps the configuration of the rows and the columns.
        /// </summary>
        void SwapAxes();

        /// <summary>
        /// Exports the matrix to an Excel workbook and offers it for download.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ExportAsync();

        /// <summary>
        /// Exports the current matrix configuration to a COMET-IME-compatible JSON file and offers it for download.
        /// </summary>
        /// <param name="name">The name of the downloaded file (without extension); a default is used when empty</param>
        /// <returns>A <see cref="Task" /></returns>
        Task ExportConfigurationAsync(string name = null);

        /// <summary>
        /// Imports a matrix configuration from the given <paramref name="stream" /> (a COMET-IME-compatible JSON file)
        /// and applies it to the two axes, the relationship rule and the display options, then rebuilds the matrix.
        /// </summary>
        /// <param name="stream">The <see cref="Stream" /> holding the JSON configuration</param>
        /// <returns>A <see cref="Task" /></returns>
        Task ImportConfigurationAsync(Stream stream);

        /// <summary>
        /// Gets a value indicating whether the current model can hold matrix configurations in its file store, that is
        /// whether a JSON <see cref="FileType" /> is defined by the reference data.
        /// </summary>
        bool CanUseModelFileStore { get; }

        /// <summary>
        /// Gets the last outcome message of a save/load against the model file store, shown to the user. Empty when there is none.
        /// </summary>
        string FileStoreMessage { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration export/import dialog is open.
        /// </summary>
        bool IsConfigurationDialogVisible { get; set; }

        /// <summary>
        /// Gets a value indicating whether the requested <paramref name="storeType" /> exists on the current iteration.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to check</param>
        /// <returns><see langword="true" /> when the store exists, otherwise <see langword="false" /></returns>
        bool StoreExists(FileStoreType storeType);

        /// <summary>
        /// Gets the <see cref="Folder" />s of the requested <paramref name="storeType" /> the configuration can be saved into.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <returns>The available <see cref="Folder" />s</returns>
        IReadOnlyList<Folder> GetFolders(FileStoreType storeType);

        /// <summary>
        /// Creates the requested <paramref name="storeType" /> on the current iteration when it does not yet exist.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to create</param>
        /// <returns>A <see cref="Task" /> with <see langword="true" /> when the store was created</returns>
        Task<bool> CreateFileStoreAsync(FileStoreType storeType);

        /// <summary>
        /// Saves the current matrix configuration as a JSON file item into the model's file store.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to save into</param>
        /// <param name="name">The name of the saved configuration file (without extension); a default is used when empty</param>
        /// <param name="folder">The <see cref="Folder" /> to save into, or <see langword="null" /> for the store root</param>
        /// <returns>A <see cref="Task" /> with <see langword="true" /> when the configuration was saved</returns>
        Task<bool> SaveConfigurationToStoreAsync(FileStoreType storeType, string name = null, Folder folder = null);

        /// <summary>
        /// Gets the matrix configuration files stored in the requested store of the current iteration.
        /// </summary>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <returns>The stored configuration <see cref="File" />s</returns>
        IReadOnlyList<File> GetStoredConfigurations(FileStoreType storeType);

        /// <summary>
        /// Loads a matrix configuration from the given <paramref name="file" /> stored in the model's file store.
        /// </summary>
        /// <param name="file">The stored configuration <see cref="File" /></param>
        /// <returns>A <see cref="Task" /></returns>
        Task LoadConfigurationFromStoreAsync(File file);
    }
}
