// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRelationshipRow.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// A display row describing one <see cref="Relationship" /> a requirement participates in: its direction, the
    /// things at the other end, and the names of the <see cref="CDP4Common.SiteDirectoryData.Rule" />s that match the
    /// relationship's categories.
    /// </summary>
    public class RequirementRelationshipRow
    {
        /// <summary>
        /// Gets the <see cref="Relationship" /> this row describes.
        /// </summary>
        public Relationship Relationship { get; init; }

        /// <summary>
        /// Gets the direction of the relationship relative to the requirement.
        /// </summary>
        public RelationshipDirection Direction { get; init; }

        /// <summary>
        /// Gets the things at the other end of the relationship.
        /// </summary>
        public IReadOnlyList<Thing> RelatedThings { get; init; }

        /// <summary>
        /// Gets the names of the relationship rules that apply to the relationship's categories.
        /// </summary>
        public IReadOnlyList<string> RuleNames { get; init; }
    }
}
