// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionHandlerViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components.Forms;

    using System.Globalization;

    /// <summary>
    /// View model used to manage the files revisions in Filestores
    /// </summary>
    public class FileRevisionHandlerViewModel : ApplicationBaseViewModel, IFileRevisionHandlerViewModel
    {
        /// <summary>
        /// Gets the <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<FileRevisionHandlerViewModel> logger;

        /// <summary>
        /// Gets or sets the <see cref="IConfiguration"/>
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Gets or sets the directory where uploaded files for the current file are stored
        /// </summary>
        private readonly string uploadsDirectory;

        /// <summary>
        /// Gets or sets the current file store
        /// </summary>
        private FileStore CurrentFileStore { get; set; }

        /// <summary>
        /// The maximum file size to upload in megabytes
        /// </summary>
        private double MaxUploadFileSizeInMb => double.Parse(this.configuration.GetSection(Constants.MaxUploadFileSizeInMbConfigurationKey).Value!, CultureInfo.InvariantCulture);

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionHandlerViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        /// <param name="jsUtilitiesService">The <see cref="JsUtilitiesService"/></param>
        /// <param name="configuration">The <see cref="IConfiguration"/></param>
        public FileRevisionHandlerViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<FileRevisionHandlerViewModel> logger, IJsUtilitiesService jsUtilitiesService,
            IConfiguration configuration) : base(sessionService, messageBus)
        {
            this.JsUtilitiesService = jsUtilitiesService;
            this.logger = logger;
            this.configuration = configuration;

            this.InitializeSubscriptions([typeof(File), typeof(Folder)]);
            this.uploadsDirectory = $"wwwroot/uploads/{Guid.NewGuid()}";

            if (!Directory.Exists(this.uploadsDirectory))
            {
                Directory.CreateDirectory(this.uploadsDirectory);
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="File"/>
        /// </summary>
        public File CurrentFile { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IJsUtilitiesService"/>
        /// </summary>
        public IJsUtilitiesService JsUtilitiesService { get; private set; }

        /// <summary>
        /// Gets a collection of the selected <see cref="FileType"/>
        /// </summary>
        public IEnumerable<FileType> SelectedFileTypes { get; private set; } = Enumerable.Empty<FileType>();

        /// <summary>
        /// Gets a collection of the available <see cref="FileType"/>s
        /// </summary>
        public IEnumerable<FileType> FileTypes { get; private set; }

        /// <summary>
        /// The file revision that will be handled for both edit and add forms
        /// </summary>
        public FileRevision FileRevision { get; set; } = new();

        /// <summary>
        /// Gets or sets the error message that is displayed in the component
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Initializes the current <see cref="FileRevisionHandlerViewModel"/>
        /// </summary>
        /// <param name="file">The <see cref="File"/> to be set</param>
        /// <param name="fileStore"></param>
        public void InitializeViewModel(File file, FileStore fileStore)
        {
            this.CurrentFile = file;
            this.CurrentFileStore = fileStore;
            this.FileTypes = this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.FileType);
        }

        /// <summary>
        /// Downloads a file revision
        /// </summary>
        /// <param name="fileRevision">the file revision</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task DownloadFileRevision(FileRevision fileRevision)
        {
            this.logger.LogInformation("Starting File Revision download...");
            var fileRevisionNameWithExtension = $"{fileRevision.Name}.{string.Join(".", fileRevision.FileType.Select(x => x.Extension))}";

            try
            {
               var bytes = await this.SessionService.Session.ReadFile(fileRevision);
               var stream = new MemoryStream(bytes);
               await this.JsUtilitiesService.DownloadFileFromStreamAsync(stream, fileRevisionNameWithExtension);
               this.logger.LogInformation("Downloading PDF...");
            }
            catch (Exception ex)
            {
               this.logger.LogError(ex,"File Revision could not be downloaded") ;
            }
        }

        /// <summary>
        /// Uploads a file to server and creates a file revision
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task UploadFile(IBrowserFile file)
        {
            var maxUploadFileSizeInBytes = (long)(this.MaxUploadFileSizeInMb * 1024 * 1024);

            if (file.Size > maxUploadFileSizeInBytes)
            {
                this.ErrorMessage = $"The max file size is {this.MaxUploadFileSizeInMb} MB";
                return;
            }

            var uploadedFilePath = Path.Combine(this.uploadsDirectory, Guid.NewGuid().ToString());

            await using (var fileStream = new FileStream(uploadedFilePath, FileMode.Create))
            {
                await file.OpenReadStream(maxUploadFileSizeInBytes).CopyToAsync(fileStream);
            }

            var engineeringModel = this.CurrentFileStore.GetContainerOfType<EngineeringModel>();

            this.FileRevision.Name = Path.GetFileNameWithoutExtension(file.Name);
            this.FileRevision.LocalPath = uploadedFilePath;
            this.FileRevision.ContentHash = CalculateContentHash(this.FileRevision.LocalPath);
            this.FileRevision.Creator = engineeringModel.GetActiveParticipant(this.SessionService.Session.ActivePerson);
            this.FileRevision.CreatedOn = DateTime.UtcNow;

            var fileExtension = Path.GetExtension(file.Name);
            var fileType = this.FileTypes.FirstOrDefault(x => $".{x.Extension}" == fileExtension);

            if (fileType != null)
            {
                this.FileRevision.FileType.Add(fileType);
            }
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed() => Task.CompletedTask;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Directory.Exists(this.uploadsDirectory))
            {
                // Directory.Delete(this.uploadsDirectory, true);
            }
        }

        /// <summary>
        /// Calculates the hash of the file's content
        /// </summary>
        /// <param name="filePath">the path to the file</param>
        /// <returns>the hash of the content</returns>
        private static string CalculateContentHash(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return StreamToHashComputer.CalculateSha1HashFromStream(fileStream);
        }
    }
}
