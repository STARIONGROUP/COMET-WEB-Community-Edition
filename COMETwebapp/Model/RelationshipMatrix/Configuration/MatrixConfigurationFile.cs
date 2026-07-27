// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixConfigurationFile.cs" company="Starion Group S.A.">
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
    /// The root of a Relationship Matrix configuration file, mirroring the COMET-IME
    /// <c>RelationshipMatrixPluginSettings</c> so that a file exported here can be read by the IME and vice versa.
    /// </summary>
    public class MatrixConfigurationFile
    {
        /// <summary>
        /// Gets or sets the saved configurations held in this file.
        /// </summary>
        public List<MatrixSavedConfiguration> SavedConfigurations { get; set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="ClassKind" />s that can be picked as an axis' class kind.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<ClassKind> PossibleClassKinds { get; set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" />s that can be picked as an axis' display or sort kind.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<MatrixDisplayKind> PossibleDisplayKinds { get; set; } = [];
    }
}
