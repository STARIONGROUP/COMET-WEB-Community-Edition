// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRelationshipDetail.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// One relationship a requirement participates in, resolved to a single matched <see cref="RelationshipRuleReference" />
    /// (or none), the direction relative to the requirement, the things at the other end and the relationship's
    /// categories. A relationship that matches several rules yields one detail per matched rule, so it lands in every
    /// matching rule's column; a relationship that matches no rule yields one detail with a null <see cref="Rule" />.
    /// </summary>
    public class RequirementRelationshipDetail
    {
        /// <summary>
        /// Gets the things at the other end of the relationship.
        /// </summary>
        public IReadOnlyList<Thing> RelatedThings { get; init; }

        /// <summary>
        /// Gets the direction of the relationship relative to the requirement.
        /// </summary>
        public RelationshipDirection Direction { get; init; }

        /// <summary>
        /// Gets the matched rule, or null when the relationship matches no rule.
        /// </summary>
        public RelationshipRuleReference Rule { get; init; }

        /// <summary>
        /// Gets the categories carried by the relationship.
        /// </summary>
        public IReadOnlyList<Category> Categories { get; init; }
    }
}
