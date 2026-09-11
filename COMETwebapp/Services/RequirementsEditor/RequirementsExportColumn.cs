// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportColumn.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// One column of a requirements export: its header title, the cell value for a requirement in its specification, and
    /// whether it is a wide free-text column that should wrap. Used by the <see cref="RequirementsExcelExporter" /> to
    /// build a worksheet column by column.
    /// </summary>
    public sealed class RequirementsExportColumn
    {
        /// <summary>
        /// Creates a new instance of <see cref="RequirementsExportColumn" />
        /// </summary>
        /// <param name="title">The header title</param>
        /// <param name="value">The cell value for a requirement in its specification</param>
        /// <param name="wide">Whether the column is a wide, wrapping free-text column</param>
        public RequirementsExportColumn(string title, Func<Requirement, RequirementsSpecification, string> value, bool wide)
        {
            this.Title = title;
            this.Value = value;
            this.Wide = wide;
        }

        /// <summary>
        /// Gets or sets the header title; disambiguated after the columns are built.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the cell value for a requirement in its specification.
        /// </summary>
        public Func<Requirement, RequirementsSpecification, string> Value { get; }

        /// <summary>
        /// Gets a value indicating whether the column is a wide, wrapping free-text column.
        /// </summary>
        public bool Wide { get; }
    }
}
