// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementChange.cs" company="Starion Group S.A.">
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
    /// <summary>
    /// Represents a single change to a requirement-related element (a specification, group, requirement,
    /// one of its values, or a traceability link) between a baseline iteration and a newer iteration,
    /// as one row of the requirements changelog.
    /// </summary>
    public class RequirementChange
    {
        /// <summary>
        /// Gets the <see cref="RequirementChangeKind" /> that classifies this change.
        /// </summary>
        public RequirementChangeKind Kind { get; init; }

        /// <summary>
        /// Gets the human-readable kind of the changed element (for example "Requirement",
        /// "Requirements Specification", "Requirements Group" or "Binary Relationship").
        /// </summary>
        public string ElementKind { get; init; }

        /// <summary>
        /// Gets the <see cref="CDP4Common.CommonData.Thing.Iid" /> of the changed element, used to group its rows.
        /// </summary>
        public Guid ElementId { get; init; }

        /// <summary>
        /// Gets the name of the changed element.
        /// </summary>
        public string ElementName { get; init; }

        /// <summary>
        /// Gets the short name of the changed element.
        /// </summary>
        public string ElementShortName { get; init; }

        /// <summary>
        /// Gets the <see cref="CDP4Common.CommonData.Thing.Iid" /> of the owning
        /// <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" />, or
        /// <see cref="Guid.Empty" /> when the change has none.
        /// </summary>
        public Guid SpecificationId { get; init; }

        /// <summary>
        /// Gets the name of the owning <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" />, or an
        /// empty string when the change has none.
        /// </summary>
        public string SpecificationName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the short name of the owning <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" />,
        /// or an empty string when the change has none.
        /// </summary>
        public string SpecificationShortName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the changed field for a <see cref="RequirementChangeKind.Modified" /> change,
        /// or an empty string when the whole element was created, deleted, deprecated or restored.
        /// </summary>
        public string Field { get; init; } = string.Empty;

        /// <summary>
        /// Gets the value in the baseline iteration, or an empty string when there is none.
        /// </summary>
        public string OldValue { get; init; } = string.Empty;

        /// <summary>
        /// Gets the value in the newer iteration, or an empty string when there is none.
        /// </summary>
        public string NewValue { get; init; } = string.Empty;

        /// <summary>
        /// Gets the short name of the owning <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" /> of the
        /// changed element.
        /// </summary>
        public string Owner { get; init; } = string.Empty;

        /// <summary>
        /// Gets the display label combining <see cref="SpecificationShortName" /> and <see cref="SpecificationName" />,
        /// or a placeholder when the change has no owning specification, used to group the grid by specification.
        /// </summary>
        public string SpecificationLabel => this.SpecificationId == Guid.Empty ? "(no specification)" : $"{this.SpecificationShortName}: {this.SpecificationName}";
    }
}
