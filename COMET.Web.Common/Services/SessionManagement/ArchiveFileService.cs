// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveFileService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Services.SessionManagement
{
    using FluentResults;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="ArchiveFileService" /> stores an uploaded ECSS-E-TM-10-25 Annex C3 archive in a dedicated
    /// temporary directory for as long as the user's circuit lives
    /// </summary>
    public class ArchiveFileService : IArchiveFileService
    {
        /// <summary>
        /// The largest archive, in bytes, that may be uploaded
        /// </summary>
        public const long MaximumArchiveSize = 512L * 1024 * 1024;

        /// <summary>
        /// The name of the directory, inside the system temporary directory, that uploaded archives are stored in
        /// </summary>
        private const string ArchiveDirectoryName = "comet-web-archives";

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<ArchiveFileService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFileService" /> class
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public ArchiveFileService(ILogger<ArchiveFileService> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets the full path of the currently stored archive, or null when no archive is stored
        /// </summary>
        public string ArchivePath { get; private set; }

        /// <summary>
        /// Stores the content of the provided <see cref="IBrowserFile" /> in a temporary location on the server,
        /// discarding any previously stored archive
        /// </summary>
        /// <param name="file">The <see cref="IBrowserFile" /> that has been uploaded by the user</param>
        /// <returns>
        /// A <see cref="Task{T}" /> with the <see cref="Result{TValue}" /> of the operation, containing the full path of
        /// the stored archive in case of success
        /// </returns>
        public async Task<Result<string>> PersistAsync(IBrowserFile file)
        {
            if (file == null)
            {
                return Result.Fail<string>("No file was provided");
            }

            if (file.Size > MaximumArchiveSize)
            {
                return Result.Fail<string>($"The archive exceeds the maximum allowed size of {MaximumArchiveSize / (1024 * 1024)} MB");
            }

            this.Remove();

            var directory = Directory.CreateDirectory(GetArchiveDirectory());
            var path = Path.Combine(directory.FullName, $"{Guid.NewGuid()}.zip");

            try
            {
                await using (var target = File.Create(path))
                {
                    await file.OpenReadStream(file.Size).CopyToAsync(target);
                }

                this.ArchivePath = path;
                return Result.Ok(path);
            }
            catch (IOException exception)
            {
                this.logger.LogError(exception, "The uploaded archive could not be stored");
                return Result.Fail<string>("The uploaded archive could not be stored on the server");
            }
        }

        /// <summary>
        /// Removes the currently stored archive, if any
        /// </summary>
        public void Remove()
        {
            if (this.ArchivePath == null)
            {
                return;
            }

            TryDelete(this.ArchivePath, this.logger);
            this.ArchivePath = null;
        }

        /// <summary>
        /// Disposes this service, removing the stored archive. This runs when the user's circuit ends, which covers the
        /// case where the browser is closed before the model is closed
        /// </summary>
        /// <returns>A <see cref="ValueTask" /></returns>
        public ValueTask DisposeAsync()
        {
            this.Remove();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Removes archives that are older than the provided age. Circuits are normally cleaned up by
        /// <see cref="DisposeAsync" />, this sweep exists to reclaim archives left behind by an abrupt shutdown of the
        /// server and is meant to be called once at startup
        /// </summary>
        /// <param name="maximumAge">The age beyond which a stored archive is considered orphaned</param>
        /// <param name="logger">The <see cref="ILogger" /> to report failures on</param>
        public static void RemoveOrphanedArchives(TimeSpan maximumAge, ILogger logger)
        {
            var directory = new DirectoryInfo(GetArchiveDirectory());

            if (!directory.Exists)
            {
                return;
            }

            var threshold = DateTime.UtcNow - maximumAge;

            foreach (var file in directory.EnumerateFiles("*.zip").Where(x => x.LastWriteTimeUtc < threshold))
            {
                TryDelete(file.FullName, logger);
            }
        }

        /// <summary>
        /// Gets the full path of the directory that uploaded archives are stored in
        /// </summary>
        /// <returns>The full path of the directory</returns>
        private static string GetArchiveDirectory()
        {
            return Path.Combine(Path.GetTempPath(), ArchiveDirectoryName);
        }

        /// <summary>
        /// Deletes the file at the provided path, logging rather than throwing when the deletion fails
        /// </summary>
        /// <param name="path">The full path of the file to delete</param>
        /// <param name="logger">The <see cref="ILogger" /> to report a failure on</param>
        private static void TryDelete(string path, ILogger logger)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException exception)
            {
                logger.LogWarning(exception, "The temporary archive {Path} could not be deleted", path);
            }
            catch (UnauthorizedAccessException exception)
            {
                logger.LogWarning(exception, "The temporary archive {Path} could not be deleted", path);
            }
        }
    }
}
