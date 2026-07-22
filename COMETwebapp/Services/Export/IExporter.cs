// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IExporter.cs" company="Starion Group S.A.">
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
    /// <summary>
    /// Defines an exporter that produces a downloadable file as a <see cref="Stream" />. Implementations own the file
    /// format and content; new exporters are added by implementing this interface, without any additional dependency
    /// injection registration.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Gets the name of the file offered for download, including its extension.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Produces the file content as a readable <see cref="Stream" />.
        /// </summary>
        /// <returns>The <see cref="Stream" /> holding the exported content</returns>
        Stream Export();
    }
}
