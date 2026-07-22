// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixConfigurationDialog.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RelationshipMatrix
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.FileStore;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    using ReactiveUI;

    /// <summary>
    /// A dialog that exports the current Relationship Matrix configuration to a local download or the model file store,
    /// and imports a configuration from a local file or the model file store.
    /// </summary>
    public partial class MatrixConfigurationDialog : DisposableComponent
    {
        /// <summary>
        /// The destinations a configuration can be exported to, paired with their user-facing labels.
        /// </summary>
        private static readonly DestinationOption[] Destinations =
        [
            new(ConfigurationDestination.LocalDownload, "Local download"),
            new(ConfigurationDestination.CommonFileStore, "Model - Common file store (shared)"),
            new(ConfigurationDestination.DomainFileStore, "Model - Domain file store (private)")
        ];

        /// <summary>
        /// The file stores a stored configuration can be imported from.
        /// </summary>
        private static readonly FileStoreType[] ImportStores = [FileStoreType.Common, FileStoreType.Domain];

        /// <summary>
        /// Backing field for <see cref="Destination" />.
        /// </summary>
        private ConfigurationDestination destination = ConfigurationDestination.LocalDownload;

        /// <summary>
        /// Backing field for <see cref="ImportStore" />.
        /// </summary>
        private FileStoreType importStore = FileStoreType.Domain;

        /// <summary>
        /// The name of the exported configuration file.
        /// </summary>
        private string configurationName;

        /// <summary>
        /// The folder the configuration is saved into, or <see langword="null" /> for the store root.
        /// </summary>
        private Folder selectedFolder;

        /// <summary>
        /// The stored configuration selected to be loaded from the model file store.
        /// </summary>
        private File selectedStoredConfiguration;

        /// <summary>
        /// Gets or sets the <see cref="IRelationshipMatrixBodyViewModel" /> driving the dialog.
        /// </summary>
        [Parameter]
        public IRelationshipMatrixBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the selected export destination. Changing it clears the selected folder, which belongs to the
        /// previously targeted store.
        /// </summary>
        private ConfigurationDestination Destination
        {
            get => this.destination;
            set
            {
                this.destination = value;
                this.selectedFolder = null;
            }
        }

        /// <summary>
        /// Gets or sets the file store a stored configuration is imported from. Changing it clears the selected stored
        /// configuration, which belongs to the previously targeted store.
        /// </summary>
        private FileStoreType ImportStore
        {
            get => this.importStore;
            set
            {
                this.importStore = value;
                this.selectedStoredConfiguration = null;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileStoreType" /> matching the selected export <see cref="Destination" />.
        /// </summary>
        private FileStoreType StoreType => this.Destination == ConfigurationDestination.CommonFileStore ? FileStoreType.Common : FileStoreType.Domain;

        /// <summary>
        /// Gets a value indicating whether the export/save action can run: a local download always can, a model save only
        /// when the model defines a JSON file type (the store itself is created on demand).
        /// </summary>
        private bool CanExportOrSave => this.Destination == ConfigurationDestination.LocalDownload || this.ViewModel.CanUseModelFileStore;

        /// <summary>
        /// Gets the label of the export/save action button.
        /// </summary>
        private string ExportButtonText => this.Destination == ConfigurationDestination.LocalDownload ? "Download" : "Save to model";

        /// <summary>
        /// Registers the re-render subscription so the dialog reflects the open state and the file-store message.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsConfigurationDialogVisible, x => x.ViewModel.FileStoreMessage)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Gets the user-facing label of a <see cref="ConfigurationDestination" />.
        /// </summary>
        /// <param name="value">The <see cref="ConfigurationDestination" /></param>
        /// <returns>The label</returns>
        private static string DestinationLabel(ConfigurationDestination value)
        {
            return value switch
            {
                ConfigurationDestination.LocalDownload => "Local download",
                ConfigurationDestination.CommonFileStore => "Model - Common file store (shared)",
                ConfigurationDestination.DomainFileStore => "Model - Domain file store (private)",
                _ => value.ToString()
            };
        }

        /// <summary>
        /// Exports the configuration to a local download or saves it into the selected model file store, creating the
        /// store first when it does not exist. Closes the dialog only when the action succeeds so a failure stays visible.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnExportOrSaveAsync()
        {
            if (this.Destination == ConfigurationDestination.LocalDownload)
            {
                await this.ViewModel.ExportConfigurationAsync(this.configurationName);
                this.ViewModel.IsConfigurationDialogVisible = false;
                return;
            }

            if (!this.ViewModel.StoreExists(this.StoreType) && !await this.ViewModel.CreateFileStoreAsync(this.StoreType))
            {
                return;
            }

            if (await this.ViewModel.SaveConfigurationToStoreAsync(this.StoreType, this.configurationName, this.selectedFolder))
            {
                this.ViewModel.IsConfigurationDialogVisible = false;
            }
        }

        /// <summary>
        /// Reads the uploaded JSON file and asks the view model to import the configuration it holds.
        /// </summary>
        /// <param name="e">The <see cref="InputFileChangeEventArgs" /> of the upload</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnImportFileAsync(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
            {
                return;
            }

            using var memoryStream = new MemoryStream();
            await e.File.OpenReadStream().CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await this.ViewModel.ImportConfigurationAsync(memoryStream);
            this.ViewModel.IsConfigurationDialogVisible = false;
        }

        /// <summary>
        /// Loads the selected stored configuration from the model file store.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnLoadFromModelAsync()
        {
            if (this.selectedStoredConfiguration == null)
            {
                return;
            }

            await this.ViewModel.LoadConfigurationFromStoreAsync(this.selectedStoredConfiguration);
            this.ViewModel.IsConfigurationDialogVisible = false;
        }
    }
}
