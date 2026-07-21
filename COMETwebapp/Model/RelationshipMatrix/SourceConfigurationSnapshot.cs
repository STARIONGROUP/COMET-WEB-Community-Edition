// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SourceConfigurationSnapshot.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Model.RelationshipMatrix
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// An in-memory snapshot of one Relationship Matrix source axis' configuration (rows or columns). Used to
    /// exchange the two axes' configurations when swapping rows and columns.
    /// </summary>
    public class SourceConfigurationSnapshot
    {
        /// <summary>
        /// Gets or sets the captured <see cref="ClassKind" /> of the axis.
        /// </summary>
        public ClassKind? SelectedClassKind { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the captured selected <see cref="Category" />s.
        /// </summary>
        public List<Guid> SelectedCategories { get; set; } = [];

        /// <summary>
        /// Gets or sets the unique identifiers of the captured selected <see cref="DomainOfExpertise" /> owners.
        /// </summary>
        public List<Guid> SelectedOwners { get; set; } = [];

        /// <summary>
        /// Gets or sets the captured <see cref="CategoryBooleanOperatorKind" />.
        /// </summary>
        public CategoryBooleanOperatorKind SelectedBooleanOperatorKind { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sub-categories are included when matching the selected <see cref="Category" />s.
        /// </summary>
        public bool IncludeSubcategories { get; set; } = true;

        /// <summary>
        /// Gets or sets the captured <see cref="MatrixDisplayKind" /> used to label the axis.
        /// </summary>
        public MatrixDisplayKind SelectedDisplayKind { get; set; }

        /// <summary>
        /// Gets or sets the captured <see cref="MatrixDisplayKind" /> used to sort the axis.
        /// </summary>
        public MatrixDisplayKind SelectedSortKind { get; set; }

        /// <summary>
        /// Gets or sets the captured <see cref="MatrixSortOrder" />.
        /// </summary>
        public MatrixSortOrder SelectedSortOrder { get; set; }
    }
}
