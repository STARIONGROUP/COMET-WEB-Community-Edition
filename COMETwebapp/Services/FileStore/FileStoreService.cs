// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileStoreService.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.FileStore
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using FluentResults;

    /// <summary>
    /// Generic, feature-agnostic service that saves and reads arbitrary file items into and from the common or domain
    /// file store of a CDP4-COMET model iteration.
    /// </summary>
    public class FileStoreService : IFileStoreService
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to reach the open session, its active domain and participant, and to
        /// read and write file items.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStoreService" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public FileStoreService(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Gets the first <see cref="FileType" /> defined by the iteration's reference data libraries whose
        /// <see cref="FileType.Extension" /> matches the given <paramref name="extension" /> (case-insensitive).
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose reference data is queried</param>
        /// <param name="extension">The file extension to match, without the leading dot (for example <c>json</c>)</param>
        /// <returns>The matching <see cref="FileType" />, or <see langword="null" /> when none is available</returns>
        public FileType GetFileType(Iteration iteration, string extension)
        {
            return iteration.IterationSetup.GetContainerOfType<SiteDirectory>()
                .AvailableReferenceDataLibraries()
                .SelectMany(rdl => rdl.FileType)
                .FirstOrDefault(x => string.Equals(x.Extension, extension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the <see cref="File" />s held by the requested store of the iteration, optionally limited to those whose
        /// <see cref="File.CurrentFileRevision" /> carries the given <paramref name="fileType" />.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is queried</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <param name="fileType">The optional <see cref="FileType" /> to filter by</param>
        /// <returns>The matching <see cref="File" />s, empty when the store is not available</returns>
        public IReadOnlyList<File> GetFiles(Iteration iteration, FileStoreType storeType, FileType fileType = null)
        {
            var store = this.ResolveStore(iteration, storeType);

            if (store == null)
            {
                return [];
            }

            if (fileType == null)
            {
                return store.File.ToList();
            }

            return store.File.Where(x => x.CurrentFileRevision != null && x.CurrentFileRevision.FileType.Contains(fileType)).ToList();
        }

        /// <summary>
        /// Reads the bytes of the current revision of the given <paramref name="file" />.
        /// </summary>
        /// <param name="file">The <see cref="File" /> to read</param>
        /// <returns>A <see cref="Task" /> with the file content as a <see cref="byte" /> array</returns>
        public Task<byte[]> ReadFileAsync(File file)
        {
            return this.sessionService.Session.ReadFile(file.CurrentFileRevision);
        }

        /// <summary>
        /// Gets a value indicating whether the requested store exists on the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is checked</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to check for</param>
        /// <returns><see langword="true" /> when the store exists, otherwise <see langword="false" /></returns>
        public bool StoreExists(Iteration iteration, FileStoreType storeType)
        {
            return this.ResolveStore(iteration, storeType) != null;
        }

        /// <summary>
        /// Gets the <see cref="Folder" />s held by the requested store of the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is queried</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <returns>The <see cref="Folder" />s, empty when the store is not available</returns>
        public IReadOnlyList<Folder> GetFolders(Iteration iteration, FileStoreType storeType)
        {
            return this.ResolveStore(iteration, storeType)?.Folder.ToList() ?? [];
        }

        /// <summary>
        /// Creates the requested store on the iteration when it does not yet exist. The common store is added to the
        /// engineering model, the domain store to the iteration; both are owned by the active domain of expertise.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to create the store on</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to create</param>
        /// <returns>A <see cref="Task" /> with the <see cref="Result" /> of the write</returns>
        public async Task<Result> CreateStoreAsync(Iteration iteration, FileStoreType storeType)
        {
            if (this.StoreExists(iteration, storeType))
            {
                return Result.Ok();
            }

            var activeDomain = this.sessionService.GetDomainOfExpertise(iteration);

            switch (storeType)
            {
                case FileStoreType.Common:
                {
                    var engineeringModel = iteration.GetContainerOfType<EngineeringModel>();
                    var modelClone = engineeringModel.Clone(false);
                    var store = new CommonFileStore { Iid = Guid.NewGuid(), Name = "Common File Store", Owner = activeDomain, CreatedOn = DateTime.UtcNow };
                    modelClone.CommonFileStore.Add(store);

                    return await this.sessionService.CreateOrUpdateThings(modelClone, [modelClone, store]);
                }

                case FileStoreType.Domain:
                {
                    var iterationClone = iteration.Clone(false);
                    var store = new DomainFileStore { Iid = Guid.NewGuid(), Name = $"{activeDomain.Name} File Store", Owner = activeDomain, CreatedOn = DateTime.UtcNow };
                    iterationClone.DomainFileStore.Add(store);

                    return await this.sessionService.CreateOrUpdateThings(iterationClone, [iterationClone, store]);
                }

                default:
                {
                    return Result.Fail($"Unknown file store type {storeType}");
                }
            }
        }

        /// <summary>
        /// Saves the given <paramref name="content" /> as a file item into the requested store of the iteration. When a
        /// <see cref="File" /> with the same name already exists a new <see cref="FileRevision" /> is added to it, otherwise
        /// a new <see cref="File" /> is created in the given <paramref name="folder" /> (or at the store root when it is null).
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store receives the file</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to write to</param>
        /// <param name="fileName">The name of the file item to save</param>
        /// <param name="fileType">The <see cref="FileType" /> of the file item</param>
        /// <param name="content">The bytes to persist</param>
        /// <param name="folder">The <see cref="Folder" /> to place the file in, or <see langword="null" /> for the store root</param>
        /// <returns>A <see cref="Task" /> with the <see cref="Result" /> of the write</returns>
        public async Task<Result> SaveFileAsync(Iteration iteration, FileStoreType storeType, string fileName, FileType fileType, byte[] content, Folder folder = null)
        {
            var store = this.ResolveStore(iteration, storeType);

            if (store == null)
            {
                return Result.Fail($"No {storeType} file store available");
            }

            var safeFileName = Path.GetFileName(fileName);

            if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName is "." or "..")
            {
                return Result.Fail("The file name is not valid");
            }

            fileName = safeFileName;

            var activeDomain = this.sessionService.GetDomainOfExpertise(iteration);
            var engineeringModel = iteration.GetContainerOfType<EngineeringModel>();
            var activeParticipant = engineeringModel.GetActiveParticipant(this.sessionService.Session.ActivePerson);

            var temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var temporaryPath = Path.Combine(temporaryDirectory, fileName);

            try
            {
                Directory.CreateDirectory(temporaryDirectory);

                await using (var fileStream = new FileStream(temporaryPath, FileMode.Create))
                {
                    await fileStream.WriteAsync(content);
                }

                var fileRevision = new FileRevision
                {
                    Iid = Guid.NewGuid(),
                    Name = fileName,
                    LocalPath = temporaryPath,
                    ContentHash = CalculateContentHash(temporaryPath),
                    Creator = activeParticipant,
                    CreatedOn = DateTime.UtcNow,
                    ContainingFolder = folder
                };

                fileRevision.FileType.Add(fileType);

                var storeClone = store.Clone(false);
                var existingFile = store.File.FirstOrDefault(x => x.CurrentFileRevision?.Name == fileName);
                var thingsToCreateOrUpdate = new List<Thing>();

                File fileClone;

                if (existingFile != null)
                {
                    fileClone = existingFile.Clone(false);
                    fileClone.FileRevision.Add(fileRevision);
                }
                else
                {
                    fileClone = new File { Iid = Guid.NewGuid(), Owner = activeDomain };
                    fileClone.FileRevision.Add(fileRevision);
                    storeClone.File.Add(fileClone);
                    thingsToCreateOrUpdate.Add(storeClone);
                }

                thingsToCreateOrUpdate.Add(fileClone);
                thingsToCreateOrUpdate.Add(fileRevision);

                return await this.sessionService.CreateOrUpdateThings(storeClone, thingsToCreateOrUpdate, [temporaryPath]);
            }
            finally
            {
                if (Directory.Exists(temporaryDirectory))
                {
                    Directory.Delete(temporaryDirectory, true);
                }
            }
        }

        /// <summary>
        /// Resolves the <see cref="FileStore" /> of the iteration for the requested <paramref name="storeType" />. The
        /// domain store is the one owned by the active domain of expertise, falling back to the first available one.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is resolved</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to resolve</param>
        /// <returns>The resolved <see cref="FileStore" />, or <see langword="null" /> when none is available</returns>
        private FileStore ResolveStore(Iteration iteration, FileStoreType storeType)
        {
            return storeType switch
            {
                FileStoreType.Common => iteration.GetContainerOfType<EngineeringModel>()?.CommonFileStore.FirstOrDefault(),
                FileStoreType.Domain => iteration.DomainFileStore.FirstOrDefault(x => x.Owner == this.sessionService.GetDomainOfExpertise(iteration)) ?? iteration.DomainFileStore.FirstOrDefault(),
                _ => null
            };
        }

        /// <summary>
        /// Calculates the SHA-1 content hash of the file at the given <paramref name="filePath" />.
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>The content hash, or <see langword="null" /> when the path is <see langword="null" /></returns>
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
