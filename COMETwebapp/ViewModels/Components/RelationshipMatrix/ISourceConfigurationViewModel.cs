// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISourceConfigurationViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RelationshipMatrix
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Model.RelationshipMatrix.Configuration;

    /// <summary>
    /// View model for one Relationship Matrix source axis (rows or columns): a <see cref="ClassKind" />, a set of
    /// <see cref="Category" />s combined through a boolean operator, an owner filter, and the display/sort options
    /// that determine how the axis' <see cref="DefinedThing" />s are labelled and ordered.
    /// </summary>
    public interface ISourceConfigurationViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="Iteration" />.
        /// </summary>
        Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Gets the <see cref="ClassKind" />s that can be picked as this axis' <see cref="SelectedClassKind" />.
        /// </summary>
        IEnumerable<ClassKind> PossibleClassKinds { get; }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind" /> of the things shown on this axis.
        /// </summary>
        ClassKind? SelectedClassKind { get; set; }

        /// <summary>
        /// Gets the <see cref="IMultiCategorySelectorViewModel" /> used to select the <see cref="Category" />s that scope this axis.
        /// </summary>
        IMultiCategorySelectorViewModel CategorySelector { get; }

        /// <summary>
        /// Gets the <see cref="CategoryBooleanOperatorKind" />s that can be picked as this axis' <see cref="SelectedBooleanOperatorKind" />.
        /// </summary>
        IEnumerable<CategoryBooleanOperatorKind> PossibleBooleanOperatorKinds { get; }

        /// <summary>
        /// Gets or sets the <see cref="CategoryBooleanOperatorKind" /> used to combine the selected <see cref="Category" />s.
        /// </summary>
        CategoryBooleanOperatorKind SelectedBooleanOperatorKind { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether membership of a sub-category of a selected <see cref="Category" /> also matches.
        /// </summary>
        bool IncludeSubcategories { get; set; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" />s that can be picked as owner filters.
        /// </summary>
        IEnumerable<DomainOfExpertise> AvailableOwners { get; }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise" />s that a thing's <see cref="IOwnedThing.Owner" /> must be one of to be shown on this axis.
        /// </summary>
        IEnumerable<DomainOfExpertise> SelectedOwners { get; set; }

        /// <summary>
        /// Gets the <see cref="MatrixDisplayKind" />s that can be picked as this axis' <see cref="SelectedDisplayKind" />.
        /// </summary>
        IEnumerable<MatrixDisplayKind> PossibleDisplayKinds { get; }

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" /> used to label this axis' things.
        /// </summary>
        MatrixDisplayKind SelectedDisplayKind { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" /> used to sort this axis' things.
        /// </summary>
        MatrixDisplayKind SelectedSortKind { get; set; }

        /// <summary>
        /// Gets the <see cref="MatrixSortOrder" />s that can be picked as this axis' <see cref="SelectedSortOrder" />.
        /// </summary>
        IEnumerable<MatrixSortOrder> PossibleSortOrders { get; }

        /// <summary>
        /// Gets or sets the <see cref="MatrixSortOrder" /> used to sort this axis' things.
        /// </summary>
        MatrixSortOrder SelectedSortOrder { get; set; }

        /// <summary>
        /// Filters and sorts the given <paramref name="candidates" /> according to this axis' configuration.
        /// </summary>
        /// <param name="candidates">The candidate <see cref="DefinedThing" />s</param>
        /// <returns>The matching, ordered things shown on this axis</returns>
        IReadOnlyList<DefinedThing> QuerySourceThings(IEnumerable<DefinedThing> candidates);

        /// <summary>
        /// Captures this axis' configuration into a <see cref="MatrixSourceConfiguration" /> (used to swap axes and to export).
        /// </summary>
        /// <returns>The captured <see cref="MatrixSourceConfiguration" /></returns>
        MatrixSourceConfiguration CaptureSnapshot();

        /// <summary>
        /// Restores this axis' configuration from a <see cref="MatrixSourceConfiguration" />, silently dropping any
        /// <see cref="Category" /> or <see cref="DomainOfExpertise" /> that no longer resolves against <see cref="CurrentIteration" />.
        /// </summary>
        /// <param name="snapshot">The <see cref="MatrixSourceConfiguration" /> to restore</param>
        void RestoreSnapshot(MatrixSourceConfiguration snapshot);
    }
}
