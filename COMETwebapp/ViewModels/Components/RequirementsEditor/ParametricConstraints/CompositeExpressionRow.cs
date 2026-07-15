// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompositeExpressionRow.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints
{
    /// <summary>
    /// A group of the parametric-constraint tree: an AND/OR/XOR of two or more child expressions. Maps to the SDK
    /// <see cref="CDP4Common.EngineeringModelData.AndExpression" /> / <see cref="CDP4Common.EngineeringModelData.OrExpression" />
    /// / <see cref="CDP4Common.EngineeringModelData.ExclusiveOrExpression" />.
    /// </summary>
    public class CompositeExpressionRow : BooleanExpressionRow
    {
        /// <summary>
        /// Gets or sets the logical operator combining the <see cref="Terms" />.
        /// </summary>
        public LogicalOperatorKind Operator { get; set; } = LogicalOperatorKind.And;

        /// <summary>
        /// Gets the child expressions of the group.
        /// </summary>
        public List<BooleanExpressionRow> Terms { get; } = [];

        /// <summary>
        /// Gets a value indicating whether the group has at least two children and all of them are valid.
        /// </summary>
        public override bool IsValid => this.Terms.Count >= 2 && this.Terms.All(x => x.IsValid);
    }
}
