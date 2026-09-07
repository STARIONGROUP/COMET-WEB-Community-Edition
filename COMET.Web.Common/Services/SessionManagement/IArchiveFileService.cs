// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IArchiveFileService.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// The <see cref="IArchiveFileService" /> provides temporary server-side storage for an uploaded ECSS-E-TM-10-25
    /// Annex C3 archive. An implementation is registered as a scoped service so that the stored archive is removed when
    /// the user's circuit ends, whether the model was closed explicitly or the browser was closed
    /// </summary>
    public interface IArchiveFileService : IAsyncDisposable
    {
        /// <summary>
        /// Gets the full path of the currently stored archive, or null when no archive is stored
        /// </summary>
        string ArchivePath { get; }

        /// <summary>
        /// Stores the content of the provided <see cref="IBrowserFile" /> in a temporary location on the server,
        /// discarding any previously stored archive
        /// </summary>
        /// <param name="file">The <see cref="IBrowserFile" /> that has been uploaded by the user</param>
        /// <returns>
        /// A <see cref="Task{T}" /> with the <see cref="Result{TValue}" /> of the operation, containing the full path of
        /// the stored archive in case of success
        /// </returns>
        Task<Result<string>> PersistAsync(IBrowserFile file);

        /// <summary>
        /// Removes the currently stored archive, if any
        /// </summary>
        void Remove();
    }
}
