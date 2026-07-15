// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BooleanExpressionRow.cs" company="Starion Group S.A.">
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
    /// Base row of the parametric-constraint expression tree edited in the UI. A row is either a
    /// <see cref="RelationalExpressionRow" /> (a leaf) or a <see cref="CompositeExpressionRow" /> (an AND/OR/XOR group);
    /// each row can be negated (NOT) and knows its parent group so the tree can be reordered and pruned.
    /// </summary>
    public abstract class BooleanExpressionRow
    {
        /// <summary>
        /// Gets the stable client-side identifier of the row, used as the Blazor key.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the identifier of the SDK <see cref="CDP4Common.EngineeringModelData.BooleanExpression" /> this row
        /// was loaded from, or null for a row added in the editor. It is reused when rebuilding so an edited expression
        /// keeps its identity (an in-place update) rather than being recreated with a new identity.
        /// </summary>
        public Guid? SourceIid { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the wrapping <see cref="CDP4Common.EngineeringModelData.NotExpression" /> this row
        /// was loaded from (when it was negated), or null. Reused so a negated expression keeps its NOT wrapper's identity.
        /// </summary>
        public Guid? SourceNotIid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the row is negated (wrapped in a NOT).
        /// </summary>
        public bool IsNegated { get; set; }

        /// <summary>
        /// Gets or sets the parent group of the row, or null when it is the root of the tree.
        /// </summary>
        public CompositeExpressionRow Parent { get; set; }

        /// <summary>
        /// Gets a value indicating whether the row (and its subtree) is a complete, valid expression.
        /// </summary>
        public abstract bool IsValid { get; }
    }
}
