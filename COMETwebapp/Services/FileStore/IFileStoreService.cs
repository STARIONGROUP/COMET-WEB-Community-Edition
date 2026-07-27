// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IFileStoreService.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using FluentResults;

    /// <summary>
    /// Generic, feature-agnostic service that saves and reads arbitrary file items into and from the common or domain
    /// file store of a CDP4-COMET model iteration.
    /// </summary>
    public interface IFileStoreService
    {
        /// <summary>
        /// Gets the first <see cref="FileType" /> defined by the iteration's reference data libraries whose
        /// <see cref="FileType.Extension" /> matches the given <paramref name="extension" /> (case-insensitive).
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose reference data is queried</param>
        /// <param name="extension">The file extension to match, without the leading dot (for example <c>json</c>)</param>
        /// <returns>The matching <see cref="FileType" />, or <see langword="null" /> when none is available</returns>
        FileType GetFileType(Iteration iteration, string extension);

        /// <summary>
        /// Gets the <see cref="File" />s held by the requested store of the iteration, optionally limited to those whose
        /// <see cref="File.CurrentFileRevision" /> carries the given <paramref name="fileType" />.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is queried</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <param name="fileType">The optional <see cref="FileType" /> to filter by</param>
        /// <returns>The matching <see cref="File" />s, empty when the store is not available</returns>
        IReadOnlyList<File> GetFiles(Iteration iteration, FileStoreType storeType, FileType fileType = null);

        /// <summary>
        /// Reads the bytes of the current revision of the given <paramref name="file" />.
        /// </summary>
        /// <param name="file">The <see cref="File" /> to read</param>
        /// <returns>A <see cref="Task" /> with the file content as a <see cref="byte" /> array</returns>
        Task<byte[]> ReadFileAsync(File file);

        /// <summary>
        /// Gets a value indicating whether the requested store exists on the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is checked</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to check for</param>
        /// <returns><see langword="true" /> when the store exists, otherwise <see langword="false" /></returns>
        bool StoreExists(Iteration iteration, FileStoreType storeType);

        /// <summary>
        /// Gets the <see cref="Folder" />s held by the requested store of the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> whose store is queried</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to read from</param>
        /// <returns>The <see cref="Folder" />s, empty when the store is not available</returns>
        IReadOnlyList<Folder> GetFolders(Iteration iteration, FileStoreType storeType);

        /// <summary>
        /// Creates the requested store on the iteration when it does not yet exist. The common store is added to the
        /// engineering model, the domain store to the iteration; both are owned by the active domain of expertise.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to create the store on</param>
        /// <param name="storeType">The <see cref="FileStoreType" /> to create</param>
        /// <returns>A <see cref="Task" /> with the <see cref="Result" /> of the write</returns>
        Task<Result> CreateStoreAsync(Iteration iteration, FileStoreType storeType);

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
        Task<Result> SaveFileAsync(Iteration iteration, FileStoreType storeType, string fileName, FileType fileType, byte[] content, Folder folder = null);
    }
}
