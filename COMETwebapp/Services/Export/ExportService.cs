// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ExportService.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.Export
{
    using COMETwebapp.Services.Interoperability;

    /// <summary>
    /// The single service that runs an <see cref="IExporter" /> and offers its output for download through the
    /// <see cref="IJsUtilitiesService" />.
    /// </summary>
    public class ExportService : IExportService
    {
        /// <summary>
        /// The <see cref="IJsUtilitiesService" /> used to offer the exported file for download.
        /// </summary>
        private readonly IJsUtilitiesService jsUtilitiesService;

        /// <summary>
        /// Creates a new instance of <see cref="ExportService" />
        /// </summary>
        /// <param name="jsUtilitiesService">The <see cref="IJsUtilitiesService" /></param>
        public ExportService(IJsUtilitiesService jsUtilitiesService)
        {
            this.jsUtilitiesService = jsUtilitiesService;
        }

        /// <summary>
        /// Runs the given <paramref name="exporter" /> and offers its output for download under the exporter's
        /// <see cref="IExporter.FileName" />.
        /// </summary>
        /// <param name="exporter">The <see cref="IExporter" /> to run</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExportAndDownloadAsync(IExporter exporter)
        {
            using var stream = exporter.Export();
            stream.Position = 0;
            await this.jsUtilitiesService.DownloadFileFromStreamAsync(stream, exporter.FileName);
        }
    }
}
