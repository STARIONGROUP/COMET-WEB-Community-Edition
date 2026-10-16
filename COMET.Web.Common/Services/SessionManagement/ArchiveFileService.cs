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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="ArchiveFileService" /> stores an uploaded ECSS-E-TM-10-25 Annex C3 archive in a dedicated
    /// temporary directory for as long as the user's circuit lives
    /// </summary>
    public class ArchiveFileService : IArchiveFileService
    {
        /// <summary>
        /// The default for <see cref="MaximumArchiveSize" />, in megabytes, used when no configuration is supplied
        /// </summary>
        public const long DefaultMaximumArchiveSizeInMb = 512;

        /// <summary>
        /// The default for <see cref="MaximumTotalArchiveSize" />, in megabytes, used when no configuration is supplied
        /// </summary>
        public const long DefaultMaximumTotalArchiveSizeInMb = 8 * 1024;

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
        /// <param name="configuration">
        /// The <see cref="IConfiguration" /> the size limits are read from, under the <c>ArchiveStorage</c> section
        /// </param>
        public ArchiveFileService(ILogger<ArchiveFileService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.MaximumArchiveSize = ReadSizeInMb(configuration, "ArchiveStorage:MaximumArchiveSizeInMb", DefaultMaximumArchiveSizeInMb);
            this.MaximumTotalArchiveSize = ReadSizeInMb(configuration, "ArchiveStorage:MaximumTotalSizeInMb", DefaultMaximumTotalArchiveSizeInMb);
        }

        /// <summary>
        /// Reads a size in megabytes from configuration and returns it in bytes, falling back to the default when the
        /// configured value is absent or not a usable size
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration" /> to read from</param>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultInMb">The default size in megabytes</param>
        /// <returns>The size in bytes</returns>
        /// <remarks>
        /// Configuration is a trust boundary: a zero or negative value would otherwise refuse every upload with a
        /// message saying the maximum allowed size is 0 MB
        /// </remarks>
        private static long ReadSizeInMb(IConfiguration configuration, string key, long defaultInMb)
        {
            var configured = configuration.GetValue(key, defaultInMb);

            return (configured > 0 ? configured : defaultInMb) * 1024 * 1024;
        }

        /// <summary>
        /// Gets the largest archive, in bytes, that may be uploaded
        /// </summary>
        public long MaximumArchiveSize { get; }

        /// <summary>
        /// Gets the total size, in bytes, that all stored archives together may occupy. Uploads are accepted before the
        /// user is authenticated, so without a ceiling an anonymous client could open circuits until the server's
        /// temporary directory is full. A refused upload is reported to the user and leaves the stored archive alone
        /// </summary>
        public long MaximumTotalArchiveSize { get; }

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

            if (file.Size > this.MaximumArchiveSize)
            {
                return Result.Fail<string>($"The archive exceeds the maximum allowed size of {this.MaximumArchiveSize / (1024 * 1024)} MB");
            }

            string path = null;

            try
            {
                var directory = Directory.CreateDirectory(GetArchiveDirectory());

                // The quota is checked BEFORE the previously stored archive is touched, so a refused upload leaves the
                // user with the archive they already had. The archive this upload replaces does not count against the
                // quota, because it is deleted as soon as the new one is written.
                if (QueryStoredSize(directory) - this.QueryOwnStoredSize() + file.Size > this.MaximumTotalArchiveSize)
                {
                    this.logger.LogWarning("Refused an archive upload because the archive store is full");
                    return Result.Fail<string>("The server is temporarily out of space for archive uploads. Try again later");
                }

                path = Path.Combine(directory.FullName, $"{Guid.NewGuid()}.zip");

                await using (var source = file.OpenReadStream(file.Size))
                {
                    await using (var target = File.Create(path))
                    {
                        await source.CopyToAsync(target);
                    }
                }

                var replaced = this.ArchivePath;
                this.ArchivePath = path;

                if (replaced != null)
                {
                    TryDelete(replaced, this.logger);
                }

                return Result.Ok(path);
            }
            catch (OperationCanceledException exception)
            {
                TryDelete(path, this.logger);
                this.logger.LogWarning(exception, "The archive upload was cancelled");
                return Result.Fail<string>("The archive upload was cancelled");
            }
            catch (Exception exception)
            {
                // Deliberately broad: this runs from a Blazor event handler, and anything escaping it kills the circuit
                // rather than showing the user an error. Besides IOException, the browser stream can fail with
                // JSDisconnectedException or InvalidOperationException when the tab goes away mid-upload, and an
                // unwritable temporary directory surfaces as UnauthorizedAccessException.
                TryDelete(path, this.logger);
                this.logger.LogError(exception, "The uploaded archive could not be stored");
                return Result.Fail<string>("The uploaded archive could not be stored on the server");
            }
        }

        /// <summary>
        /// Queries the total size, in bytes, of the archives currently stored
        /// </summary>
        /// <param name="directory">The <see cref="DirectoryInfo" /> that uploaded archives are stored in</param>
        /// <returns>The total size in bytes</returns>
        private static long QueryStoredSize(DirectoryInfo directory)
        {
            return directory.EnumerateFiles("*.zip").Sum(x => x.Length);
        }

        /// <summary>
        /// Queries the size, in bytes, of the archive this service currently holds, which an upload replacing it frees
        /// again and which therefore must not count against the quota
        /// </summary>
        /// <returns>The size in bytes, or zero when no archive is stored</returns>
        private long QueryOwnStoredSize()
        {
            if (this.ArchivePath == null || !File.Exists(this.ArchivePath))
            {
                return 0;
            }

            return new FileInfo(this.ArchivePath).Length;
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
        public void Dispose()
        {
            this.Remove();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Removes archives that are older than the provided age. Circuits are normally cleaned up by
        /// <see cref="Dispose" />, this sweep exists to reclaim archives left behind by an abrupt shutdown of the
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
        /// <param name="path">The full path of the file to delete, which may be null when none was created yet</param>
        /// <param name="logger">The <see cref="ILogger" /> to report a failure on</param>
        private static void TryDelete(string path, ILogger logger)
        {
            // A null path means the upload failed before a file was created, for instance because the temporary
            // directory could not be created. File.Delete would raise an ArgumentNullException that this method does not
            // catch, which would escape PersistAsync and kill the circuit.
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

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
