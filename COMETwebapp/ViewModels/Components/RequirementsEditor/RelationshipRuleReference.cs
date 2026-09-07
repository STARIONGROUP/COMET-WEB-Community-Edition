// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipRuleReference.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// A lightweight, format-agnostic description of the <see cref="Rule" /> a relationship satisfies, used to lay out
    /// relationship columns in an export. A binary rule is directional (it has a forward and an inverse name); a multi
    /// rule is not.
    /// </summary>
    public class RelationshipRuleReference
    {
        /// <summary>
        /// Gets the identifier of the rule.
        /// </summary>
        public Guid Iid { get; init; }

        /// <summary>
        /// Gets the name of the rule, used as the column title for a non-directional rule.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the forward relationship name of a directional (binary) rule, used as the forward column title; null for
        /// a non-directional rule.
        /// </summary>
        public string ForwardName { get; init; }

        /// <summary>
        /// Gets the inverse relationship name of a directional (binary) rule, used as the inverse column title; null for
        /// a non-directional rule.
        /// </summary>
        public string InverseName { get; init; }

        /// <summary>
        /// Gets a value indicating whether the rule is directional, i.e. it distinguishes a forward from an inverse
        /// relationship.
        /// </summary>
        public bool IsDirectional => this.ForwardName != null;
    }
}
