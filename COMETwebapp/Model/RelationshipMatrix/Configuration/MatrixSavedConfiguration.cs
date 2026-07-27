// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixSavedConfiguration.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Model.RelationshipMatrix.Configuration
{
    /// <summary>
    /// A single saved Relationship Matrix configuration, mirroring the COMET-IME <c>SavedConfiguration</c>. The IME axis
    /// convention is X = columns, Y = rows, so <see cref="SourceConfigurationX" /> is this application's column
    /// configuration and <see cref="SourceConfigurationY" /> its row configuration.
    /// </summary>
    public class MatrixSavedConfiguration
    {
        /// <summary>
        /// Gets or sets the unique identifier of this configuration.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of this configuration.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the X axis (columns) <see cref="MatrixSourceConfiguration" />.
        /// </summary>
        public MatrixSourceConfiguration SourceConfigurationX { get; set; }

        /// <summary>
        /// Gets or sets the Y axis (rows) <see cref="MatrixSourceConfiguration" />.
        /// </summary>
        public MatrixSourceConfiguration SourceConfigurationY { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MatrixRelationshipConfiguration" /> holding the selected relationship rule.
        /// </summary>
        public MatrixRelationshipConfiguration RelationshipConfiguration { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the direction of relationships is shown.
        /// </summary>
        public bool ShowDirectionality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only related rows and columns are shown.
        /// </summary>
        public bool ShowRelatedOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cells without a relationship are highlighted with a background colour.
        /// </summary>
        public bool ShowNonRelatedBackgroundColor { get; set; }
    }
}
