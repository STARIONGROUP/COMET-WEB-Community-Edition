// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixConfigurationExporter.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RelationshipMatrix
{
    using System.Text;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model.RelationshipMatrix.Configuration;
    using COMETwebapp.Services.Export;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Exports a Relationship Matrix configuration to a JSON file whose shape matches the COMET-IME
    /// <c>RelationshipMatrixPluginSettings</c>, so that a file exported here can be read by the IME. Built from the two
    /// axis view models and the matrix options, capturing their state up front so it is decoupled from the view model.
    /// </summary>
    public class RelationshipMatrixConfigurationExporter : IExporter
    {
        /// <summary>
        /// The configuration file assembled from the current matrix state.
        /// </summary>
        private readonly MatrixConfigurationFile configurationFile;

        /// <summary>
        /// The name of the exported file, always ending in the <c>.json</c> extension.
        /// </summary>
        private readonly string fileName;

        /// <summary>
        /// Creates a new instance of <see cref="RelationshipMatrixConfigurationExporter" />
        /// </summary>
        /// <param name="rowConfiguration">The <see cref="ISourceConfigurationViewModel" /> of the rows (IME Y axis)</param>
        /// <param name="columnConfiguration">The <see cref="ISourceConfigurationViewModel" /> of the columns (IME X axis)</param>
        /// <param name="rule">The selected <see cref="BinaryRelationshipRule" />, or <see langword="null" /></param>
        /// <param name="showDirectionality">Whether the direction of relationships is shown</param>
        /// <param name="showRelatedOnly">Whether only related rows and columns are shown</param>
        /// <param name="showNonRelatedBackgroundColor">Whether cells without a relationship are highlighted</param>
        /// <param name="name">The optional name of the saved configuration</param>
        /// <param name="description">The optional description of the saved configuration</param>
        public RelationshipMatrixConfigurationExporter(ISourceConfigurationViewModel rowConfiguration, ISourceConfigurationViewModel columnConfiguration,
            BinaryRelationshipRule rule, bool showDirectionality, bool showRelatedOnly, bool showNonRelatedBackgroundColor, string name = null, string description = null)
        {
            var savedConfiguration = new MatrixSavedConfiguration
            {
                Id = Guid.NewGuid(),
                Name = name ?? "Relationship Matrix",
                Description = description ?? string.Empty,
                SourceConfigurationX = columnConfiguration.CaptureSnapshot(),
                SourceConfigurationY = rowConfiguration.CaptureSnapshot(),
                RelationshipConfiguration = new MatrixRelationshipConfiguration { SelectedRule = rule?.Iid },
                ShowDirectionality = showDirectionality,
                ShowRelatedOnly = showRelatedOnly,
                ShowNonRelatedBackgroundColor = showNonRelatedBackgroundColor
            };

            this.configurationFile = new MatrixConfigurationFile
            {
                SavedConfigurations = [savedConfiguration],
                PossibleClassKinds = rowConfiguration.PossibleClassKinds.ToList(),
                PossibleDisplayKinds = rowConfiguration.PossibleDisplayKinds.ToList()
            };

            var sanitized = new string((name ?? string.Empty).Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray()).Trim();
            var baseName = string.IsNullOrWhiteSpace(sanitized) ? "RelationshipMatrix" : sanitized;
            this.fileName = baseName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? baseName : $"{baseName}.json";
        }

        /// <summary>
        /// Gets the name of the exported file, always ending in <c>.json</c>.
        /// </summary>
        public string FileName => this.fileName;

        /// <summary>
        /// Serializes the configuration file to a UTF-8 JSON <see cref="Stream" />.
        /// </summary>
        /// <returns>The <see cref="Stream" /> holding the JSON bytes</returns>
        public Stream Export()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };

            var json = JsonConvert.SerializeObject(this.configurationFile, settings);
            return new MemoryStream(Encoding.UTF8.GetBytes(json)) { Position = 0 };
        }
    }
}
