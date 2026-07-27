// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixSourceConfiguration.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// One axis (rows or columns) of a <see cref="MatrixSavedConfiguration" />, mirroring the COMET-IME
    /// <c>SourceConfiguration</c> so a file written by either tool round-trips. Also used in-memory as the snapshot
    /// exchanged when swapping the row and column axes.
    /// </summary>
    public class MatrixSourceConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="ClassKind" /> of the things shown on this axis, or <see langword="null" /> when unset.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ClassKind? SelectedClassKind { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" /> used to label this axis.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MatrixDisplayKind SelectedDisplayKind { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" /> used to sort this axis.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MatrixDisplayKind SelectedSortKind { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the selected categories.
        /// </summary>
        public List<Guid> SelectedCategories { get; set; } = [];

        /// <summary>
        /// Gets or sets the unique identifiers of the selected owners.
        /// </summary>
        public List<Guid> SelectedOwners { get; set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="CategoryBooleanOperatorKind" /> used to combine the selected categories.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CategoryBooleanOperatorKind SelectedBooleanOperatorKind { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sub-categories of a selected category also match.
        /// </summary>
        public bool IncludeSubcategories { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="MatrixSortOrder" /> used to sort this axis.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MatrixSortOrder SortOrder { get; set; }
    }
}
