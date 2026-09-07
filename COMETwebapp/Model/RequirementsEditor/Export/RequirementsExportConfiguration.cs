// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportConfiguration.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Model.RequirementsEditor.Export
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The user's choices for a requirements export, expressed independently of any file format so the same
    /// configuration drives the Excel export today and future PDF or Word exporters. Almost every column and section can
    /// be turned on or off, so the export is as generic as the underlying model.
    /// </summary>
    public class RequirementsExportConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the exported file, without extension; null or empty means the default
        /// "Requirements".
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsSpecification" />s to export; an empty selection means every
        /// specification of the iteration.
        /// </summary>
        public IReadOnlyList<RequirementsSpecification> SelectedSpecifications { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether each specification is written to its own worksheet; when false all
        /// specifications share a single worksheet.
        /// </summary>
        public bool SpecificationPerSheet { get; set; } = true;

        /// <summary>
        /// Gets or sets whether identities (requirements, groups, columns, categories, owners) are written by short name,
        /// by name, or by both.
        /// </summary>
        public RequirementsExportNamingMode NamingMode { get; set; } = RequirementsExportNamingMode.Both;

        /// <summary>
        /// Gets or sets a value indicating whether deprecated things are included in the export.
        /// </summary>
        public bool IncludeDeprecated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner column is written.
        /// </summary>
        public bool IncludeOwner { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the categories column is written.
        /// </summary>
        public bool IncludeCategories { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether requirements are grouped: a "Group" column carries each requirement's
        /// immediate group name and the rows are indented and outlined (collapsible/expandable) by group depth. When
        /// false the requirements are written as a flat list.
        /// </summary>
        public bool GroupRequirements { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the definitions column is written.
        /// </summary>
        public bool IncludeDefinitions { get; set; } = true;

        /// <summary>
        /// Gets or sets the language code of the definitions to export; when null or empty every definition, regardless of
        /// language, is written.
        /// </summary>
        public string DefinitionLanguageCode { get; set; }

        /// <summary>
        /// Gets or sets whether the simple parameter value columns include every parameter type, only the selected ones,
        /// or none.
        /// </summary>
        public RequirementsExportSelectionMode SimpleParameterValues { get; set; } = RequirementsExportSelectionMode.All;

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" />s whose simple parameter values are exported when
        /// <see cref="SimpleParameterValues" /> is <see cref="RequirementsExportSelectionMode.Selection" />.
        /// </summary>
        public IReadOnlyList<ParameterType> SelectedParameterTypes { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the parametric constraints column is written.
        /// </summary>
        public bool IncludeParametricConstraints { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether each parametric constraint also shows the element and value linked to
        /// its relational expressions.
        /// </summary>
        public bool IncludeConstraintLinkedElementAndValue { get; set; } = true;

        /// <summary>
        /// Gets or sets whether relationships are written (as a pair of columns per directional rule and one column per
        /// non-directional rule): every relationship, only those matching the selected categories, or none.
        /// </summary>
        public RequirementsExportSelectionMode Relationships { get; set; } = RequirementsExportSelectionMode.All;

        /// <summary>
        /// Gets or sets the <see cref="Category" />s a relationship must carry to be exported when
        /// <see cref="Relationships" /> is <see cref="RequirementsExportSelectionMode.Selection" />.
        /// </summary>
        public IReadOnlyList<Category> SelectedRelationshipCategories { get; set; } = [];
    }
}
