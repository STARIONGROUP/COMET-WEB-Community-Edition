// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRequirementsExportViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model.RequirementsEditor.Export;

    /// <summary>
    /// Interface for the <see cref="RequirementsExportViewModel" />, driving the export of requirements to an Excel
    /// workbook.
    /// </summary>
    public interface IRequirementsExportViewModel
    {
        /// <summary>
        /// Gets the mutable configuration bound to the export dialog and read by <see cref="ExportAsync" />.
        /// </summary>
        RequirementsExportConfiguration ExportConfiguration { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the export configuration dialog is open.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets the non-deprecated <see cref="RequirementsSpecification" />s of the current iteration, offered as the
        /// default export scope and the specification picker choices.
        /// </summary>
        IEnumerable<RequirementsSpecification> AvailableSpecifications { get; }

        /// <summary>
        /// Sets the <see cref="Iteration" /> the export is performed against.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open</param>
        void SetIteration(Iteration iteration);

        /// <summary>
        /// Exports the requirements of the iteration to an Excel workbook, driven by <see cref="ExportConfiguration" />,
        /// and offers it for download. Closes the export dialog on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ExportAsync();

        /// <summary>
        /// Gets the distinct <see cref="ParameterType" />s used by the simple parameter values of every specification's
        /// requirements, offered as export column choices; ordered by short name.
        /// </summary>
        /// <returns>The exportable parameter types</returns>
        IReadOnlyList<ParameterType> GetExportableParameterTypes();

        /// <summary>
        /// Gets the distinct definition language codes used across every specification's requirements, offered as export
        /// language choices; ordered alphabetically.
        /// </summary>
        /// <returns>The exportable definition language codes</returns>
        IReadOnlyList<string> GetExportableDefinitionLanguages();

        /// <summary>
        /// Gets the distinct <see cref="Category" />s carried by the iteration's relationships, offered as export
        /// relationship-filter choices; ordered by name.
        /// </summary>
        /// <returns>The exportable relationship categories</returns>
        IReadOnlyList<Category> GetExportableRelationshipCategories();

        /// <summary>
        /// Gets a relationship detail for every <see cref="CDP4Common.EngineeringModelData.BinaryRelationship" /> and
        /// <see cref="CDP4Common.EngineeringModelData.MultiRelationship" /> of the iteration the given
        /// <paramref name="requirement" /> participates in, resolved to its matched rules so the export can lay out a
        /// column per rule and direction.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The relationship details</returns>
        IReadOnlyList<RequirementRelationshipDetail> GetRelationshipDetails(Requirement requirement);
    }
}
